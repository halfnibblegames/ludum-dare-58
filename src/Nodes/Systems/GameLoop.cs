using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Scenes;

namespace HalfNibbleGame.Nodes.Systems;

public sealed partial class GameLoop : Node {
  private static readonly RandomNumberGenerator rng = new();

  [Export] public float GarbageCollectionDuration = 4f;
  private SceneTreeTimer? garbageCollectTimer;
  [Export] private float simulationDuration = 4f;

  [Export] private Graph? memoryGraph;

  private ScoreTracker? scoreTracker;
  private int cycleNumber;

  public bool IsGarbageCollecting { get; private set; }
  public double GarbageCollectingTimeLeft => garbageCollectTimer?.TimeLeft ?? 0;

  public override void _Ready() {
    Global.Services.ProvideInScene(this);
    Global.Services.ProvideInScene(scoreTracker = new ScoreTracker());
    CallDeferred(nameof(startComputerSimulation));
  }

  public override void _Process(double delta) {
    if (IsGarbageCollecting && GarbageCollectingTimeLeft <= 0) {
      checkEndOfCycleScore();
      startComputerSimulation();
    }
  }

  private void checkEndOfCycleScore() {
    if (scoreTracker is null) return;
    scoreTracker.CycleCompleted();

    var perfect = true;
    var grid = Global.Services.Get<MemoryGrid>();
    foreach (var block in grid) {
      if (block.AssignedProgram is { IsDead: true } or Virus) {
        perfect = false;
        break;
      }
    }

    if (perfect) {
      scoreTracker.PerfectCycleCompleted();
    }
  }

  public void InterruptGarbageCollecting() {
    // TODO: error sound
    startComputerSimulation();
  }

  private void startGarbageCollecting() {
    IsGarbageCollecting = true;
    garbageCollectTimer = GetTree().CreateTimer(GarbageCollectionDuration);
  }

  private void startComputerSimulation() {
    cycleNumber++;

    var taskManager = Global.Services.Get<ITaskManager>();

    memoryGraph?.PushDataPoint(taskManager.MemoryUsage);

    IsGarbageCollecting = false;
    garbageCollectTimer?.Dispose();
    garbageCollectTimer = null;

    planCycle(taskManager);

    Animations.Animations.DoDelayed(simulationDuration, startGarbageCollecting);
  }

  private static Color randomColor() {
    var hue = rng.Randf();
    return Color.FromHsv(hue, 1, 1);
  }

  private void planCycle(ITaskManager taskManager) {
    // The minimum number of programs we want active is 2 + sqrt(cycle)
    var minPrograms = 2 + (int) Math.Sqrt(cycleNumber);

    planExistingPrograms(taskManager, minPrograms, out var closingCount);
    planNewPrograms(taskManager, minPrograms, closingCount);
  }

  private void planExistingPrograms(ITaskManager taskManager, int minPrograms, out int closingCount)
  {
    var existingPrograms = new List<Program>(taskManager.Programs);
    Random.Shared.Shuffle(CollectionsMarshal.AsSpan(existingPrograms));

    closingCount = existingPrograms.Count <= minPrograms
      ? 0
      // Close at least one, at most 4 or the maximum number we can close to get to our min program count.
      : rng.RandiRange(1, Math.Min(4, existingPrograms.Count - minPrograms));

    // Close the first closeCount programs, the rest will be simulated for a chance to modify their memory footprint.
    var programsToClose = existingPrograms.Take(closingCount).ToList();
    var programsToSimulate = existingPrograms.Skip(closingCount).ToList();

    foreach (var p in programsToClose) {
      Animations.Animations.DoDelayed(rng.RandfRange(0, simulationDuration), () => p.Kill());
    }
    foreach (var p in programsToSimulate) {
      Animations.Animations.DoDelayed(rng.RandfRange(0, simulationDuration), () => p.SimulateCycle(rng));
    }
  }

  private void planNewPrograms(ITaskManager taskManager, int minPrograms, int closingCount) {
    var openAfterClosing = taskManager.Programs.Count - closingCount;

    // Open at least 1 program, and at least enough to get us to min programs (mostly relevant for first cycle).
    var minProgramsToOpen = Math.Max(1, minPrograms - openAfterClosing);
    var maxProgramsToOpen = minPrograms - (taskManager.Programs.Count / 2);
    // Reduce the probability of more programs opening as the memory gets more full.
    var memoryUsage = taskManager.MemoryUsage;
    if (memoryUsage >= 0.75 && rng.Randf() < 0.5) maxProgramsToOpen--;
    if (memoryUsage >= 0.85 && rng.Randf() < 0.6) maxProgramsToOpen--;
    if (memoryUsage >= 0.90 && rng.Randf() < 0.7) maxProgramsToOpen--;
    if (memoryUsage >= 0.94 && rng.Randf() < 0.8) maxProgramsToOpen--;
    if (memoryUsage >= 0.98 && rng.Randf() < 0.9) maxProgramsToOpen--;
    // Ensure we always open at least enough programs to meet min programs.
    maxProgramsToOpen = Math.Max(minPrograms - openAfterClosing, maxProgramsToOpen);

    var openingCount = rng.RandiRange(minProgramsToOpen, maxProgramsToOpen);
    var programsToOpen = new List<Program>();

    // One in five times, if there is no virus yet, add a virus.
    if (openingCount > 0 && !taskManager.Programs.Any(p => p is Virus) && rng.Randf() < 0.2) {
      openingCount--;
      programsToOpen.Add(new Virus(taskManager, taskManager.GetNextColor()));
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
}
