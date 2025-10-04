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

  public bool IsGarbageCollecting { get; private set; }
  public double GarbageCollectingTimeLeft => garbageCollectTimer?.TimeLeft ?? 0;

  public override void _Ready() {
    Global.Services.ProvideInScene(this);
    CallDeferred(nameof(startComputerSimulation));
  }

  public override void _Process(double delta) {
    if (IsGarbageCollecting && GarbageCollectingTimeLeft <= 0) {
      startComputerSimulation();
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
    IsGarbageCollecting = false;
    garbageCollectTimer?.Dispose();
    garbageCollectTimer = null;

    var taskManager = Global.Services.Get<ITaskManager>();
    planCycle(taskManager);

    Animations.Animations.DoDelayed(simulationDuration, startGarbageCollecting);
  }

  private static Color randomColor() {
    var hue = rng.Randf();
    return Color.FromHsv(hue, 1, 1);
  }

  private void planCycle(ITaskManager taskManager) {
    var programs = new List<Program>(taskManager.Programs);
    Random.Shared.Shuffle(CollectionsMarshal.AsSpan(programs));
    var closeCount = rng.RandiRange(1, Math.Min(4, programs.Count - 1));
    var openCount = rng.RandiRange(Math.Max(0, 3 - programs.Count), 5 - closeCount);

    // Existing programs
    var programsToClose = programs.Take(closeCount).ToList();
    var programsToSimulate = programs.Skip(closeCount).ToList();

    // New programs
    var programsToOpen = new List<Program>();
    if (openCount > 0 && !programs.Any(p => p is Virus) && rng.Randf() < 0.1) {
      openCount--;
      programsToOpen.Add(new Virus(taskManager, taskManager.GetNextColor()));
    }

    programsToOpen.AddRange(
      selectRandomNames(taskManager, openCount)
        .Select(name => new Program(taskManager, name, taskManager.GetNextColor())));

    foreach (var p in programsToClose) {
      Animations.Animations.DoDelayed(rng.RandfRange(0, simulationDuration), () => p.Kill());
    }

    foreach (var p in programsToSimulate) {
      Animations.Animations.DoDelayed(rng.RandfRange(0, simulationDuration), () => p.SimulateCycle(rng));
    }

    foreach (var p in programsToOpen) {
      Animations.Animations.DoDelayed(rng.RandfRange(0, simulationDuration),
        () => taskManager.AllocateProgram(p, rng.RandiRange(3, 8)));
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
