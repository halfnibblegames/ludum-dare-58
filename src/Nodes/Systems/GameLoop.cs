using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;
using HalfNibbleGame.Animations;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Scenes;

namespace HalfNibbleGame.Nodes.Systems;

public sealed partial class GameLoop : Node {
  private static readonly RandomNumberGenerator rng = new();

  [Export] public float GarbageCollectionDuration = 4f;
  private SceneTreeTimer? garbageCollectTimer;
  [Export] private float simulationDuration = 4f;
  public const int CyclesPerDefrag = 8;

  [Export] private Graph? memoryGraph;

  private ScoreTracker? scoreTracker;
  private TutorialPhase activeTutorial;
  private int cycleNumber;

  public bool IsGarbageCollecting { get; private set; }
  public double GarbageCollectingTimeLeft => garbageCollectTimer?.TimeLeft ?? 0;
  public bool CanDefrag => IsGarbageCollecting && DefragsAvailable > 0;
  public int DefragsAvailable { get; private set; }

  public override void _Ready() {
    Global.Services.ProvideInScene(this);
    Global.Services.ProvideInScene(scoreTracker = new ScoreTracker());
    CallDeferred(nameof(startComputerSimulation));
  }

  public override void _Process(double delta) {
    if (IsGarbageCollecting && GarbageCollectingTimeLeft <= 0) {
      endGarbageCollecting();
    }
  }

  public override void _Input(InputEvent @event) {
    if (activeTutorial == TutorialPhase.None) return;
    if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) return;

    var tutorial = Global.Services.Get<Tutorial>();
    var taskManager = Global.Services.Get<ITaskManager>();

    switch (activeTutorial) {
      case TutorialPhase.List:
        // Close a random program that's not a virus to give the player something to do.
        var nonVirusPrograms = taskManager.Programs.Where(p => p is not Virus).ToList();
        var randomProgram = nonVirusPrograms[rng.RandiRange(0, nonVirusPrograms.Count - 1)];
        randomProgram.Kill();
        tutorial.ShowFreeMemoryExplanation();
        activeTutorial = TutorialPhase.Memory;
        break;
      case TutorialPhase.Memory:
        tutorial.ShowCorruptionExplanation();
        activeTutorial = TutorialPhase.Corruption;
        break;
      case TutorialPhase.Corruption:
      case TutorialPhase.Defrag:
        // Tutorial is over
        tutorial.Hide();
        startGarbageCollecting();
        activeTutorial = TutorialPhase.None;
        break;
      case TutorialPhase.None:
      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  private void endGarbageCollecting() {
    var perfect = checkPerfect();

    updateScore(perfect);

    var soundPlayer = Global.Services.Get<SoundPlayer>();
    if (perfect) {
      soundPlayer.PlayEndPerfect();
    }
    else {
      soundPlayer.PlayEnd();
    }

    startComputerSimulation();
  }

  private bool checkPerfect() {
    var grid = Global.Services.Get<MemoryGrid>();
    foreach (var block in grid) {
      if (block.AssignedProgram is { IsDead: true } or Virus) {
        return false;
      }
    }

    return true;
  }

  private void updateScore(bool perfect) {
    if (scoreTracker is null) return;
    scoreTracker.CycleCompleted();
    if (perfect) {
      scoreTracker.PerfectCycleCompleted();
    }
  }

  public void InterruptGarbageCollecting() {
    if (!IsGarbageCollecting) return;
    var soundPlayer = Global.Services.Get<SoundPlayer>();
    soundPlayer.PlayError();
    Global.Services.Get<ShakeCamera2D>().Shake(1);
    Global.Services.Get<GlitchShader>().OneOffGlitch();
    startComputerSimulation();
  }

  private void startGarbageCollecting() {
    IsGarbageCollecting = true;
    garbageCollectTimer = GetTree().CreateTimer(GarbageCollectionDuration);
  }

  private void startComputerSimulation() {
    cycleNumber++;
    GetNode<Label>("../../../../UIManager/CurrentCycle").Text = cycleNumber.ToString();

    var taskManager = Global.Services.Get<ITaskManager>();

    memoryGraph?.PushDataPoint(taskManager.MemoryUsage);

    IsGarbageCollecting = false;
    garbageCollectTimer?.Dispose();
    garbageCollectTimer = null;

    planCycle(taskManager);

    Animations.Animations.DoDelayed(simulationDuration, finishComputerSimulation);
  }

  private void finishComputerSimulation() {
    if (cycleNumber % CyclesPerDefrag == 0) {
      DefragsAvailable++;

      // First defrag? Show tutorial
      if (cycleNumber / CyclesPerDefrag == 1) {
        Global.Services.Get<Tutorial>().ShowDefragExplanation();
        activeTutorial = TutorialPhase.Defrag;
        return;
      }
    }

    if (cycleNumber != 1) {
      // No more tutorial needed
      startGarbageCollecting();
      return;
    }

    Global.Services.Get<Tutorial>().ShowApplicationListExplanation();
    activeTutorial = TutorialPhase.List;
  }

  private void planCycle(ITaskManager taskManager) {
    // The minimum number of programs we want active is 2 + sqrt(cycle)
    var minPrograms = 2 + (int) Math.Sqrt(cycleNumber);

    planExistingPrograms(taskManager, minPrograms, out var closingCount);
    planNewPrograms(taskManager, minPrograms, closingCount);
  }

  private void planExistingPrograms(ITaskManager taskManager, int minPrograms, out int closingCount) {
    var existingPrograms = new List<Program>(taskManager.Programs);
    Random.Shared.Shuffle(CollectionsMarshal.AsSpan(existingPrograms));

    closingCount = existingPrograms.Count <= 0
      ? 0
      // Close at least one, at most 4 or the maximum number we can close to get to our min program count.
      : rng.RandiRange(1, Math.Min(4, Math.Max(1, existingPrograms.Count - minPrograms)));

    // Close the first closeCount programs, the rest will be simulated for a chance to modify their memory footprint.
    var programsToClose = existingPrograms.Take(closingCount).ToList();
    var programsToSimulate = existingPrograms.Skip(closingCount).ToList();

    foreach (var p in programsToClose.Where(p => p is not Virus)) {
      Animations.Animations.DoDelayed(rng.RandfRange(0, simulationDuration), () => p.Kill());
    }

    foreach (var p in programsToSimulate) {
      Animations.Animations.DoDelayed(rng.RandfRange(0, simulationDuration), () => p.SimulateCycle(rng));
    }
  }

  private void planNewPrograms(ITaskManager taskManager, int minPrograms, int closingCount) {
    var openAfterClosing = taskManager.Programs.Count - closingCount;

    // Open at least 1 program, and at least enough to get us to min programs + 1 (mostly relevant for first cycle).
    var minProgramsToOpen = Math.Max(1, 1 + minPrograms - openAfterClosing);
    var maxProgramsToOpen = minPrograms - taskManager.Programs.Count / 2;
    // Reduce the probability of more programs opening as the memory gets more full.
    var memoryUsage = taskManager.MemoryUsage;
    if (memoryUsage >= 0.75 && rng.Randf() < 0.5) maxProgramsToOpen--;
    if (memoryUsage >= 0.85 && rng.Randf() < 0.6) maxProgramsToOpen--;
    if (memoryUsage >= 0.90 && rng.Randf() < 0.7) maxProgramsToOpen--;
    if (memoryUsage >= 0.94 && rng.Randf() < 0.8) maxProgramsToOpen--;
    if (memoryUsage >= 0.98 && rng.Randf() < 0.9) maxProgramsToOpen--;
    // Ensure we always open at least enough programs to meet min programs + 1.
    maxProgramsToOpen = Math.Max(1 + minPrograms - openAfterClosing, maxProgramsToOpen);

    var openingCount = rng.RandiRange(minProgramsToOpen, maxProgramsToOpen);

    if (openAfterClosing + openingCount > 12) {
      openingCount = 12 - openAfterClosing;
    }

    var programsToOpen = new List<Program>();

    // One in five times, if there is no virus yet, add a virus.
    if (openingCount > 0 && !taskManager.Programs.Any(p => p is Virus) && rng.Randf() < 0.2 && cycleNumber > 1) {
      openingCount--;
      programsToOpen.Add(new Virus(taskManager, selectRandomVirusName(taskManager), taskManager.GetNextColor()));
    }

    programsToOpen.AddRange(
      selectRandomNames(taskManager, openingCount)
        .Select(name => new Program(taskManager, name, taskManager.GetNextColor())));

    foreach (var p in programsToOpen) {
      Animations.Animations.DoDelayed(rng.RandfRange(0, simulationDuration),
        () => taskManager.AllocateProgram(p, rng.RandiRange(2, 5)));
    }
  }

  private IList<string> selectRandomNames(ITaskManager taskManager, int count) {
    var existingNames = taskManager.Programs.Select(p => p.Name).ToHashSet();
    var availableNames = Program.PossibleNames.Where(n => !existingNames.Contains(n)).ToList();
    Random.Shared.Shuffle(CollectionsMarshal.AsSpan(availableNames));

    if (availableNames.Count >= count) {
      return availableNames[..count];
    }

    return availableNames.Concat(Enumerable.Range(0, count - availableNames.Count).Select(i => $"Program{i}")).ToList();
  }

  private string selectRandomVirusName(ITaskManager taskManager) {
    var existingNames = taskManager.Programs.Select(p => p.Name).ToHashSet();
    var availableNames = Program.VirusNames.Where(n => !existingNames.Contains(n)).ToList();
    if (availableNames.Count == 0) {
      return "Rick Astley"; // The probability that this happens...
    }

    return availableNames[rng.RandiRange(0, availableNames.Count)];
  }

  public void Defrag() {
    if (!CanDefrag) {
      GD.PushError("Defrag: can't defrag");
      return;
    }

    Global.Services.Get<ITaskManager>().Defrag();
    DefragsAvailable--;

    // Immediately stop garbage collection
    startComputerSimulation();
  }

  enum TutorialPhase {
    None,
    List,
    Memory,
    Corruption,
    Defrag
  }
}
