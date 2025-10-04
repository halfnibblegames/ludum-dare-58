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

  private void startGarbageCollecting() {
    IsGarbageCollecting = true;
    garbageCollectTimer = GetTree().CreateTimer(GarbageCollectionDuration);
    Animations.Animations.DoDelayed(GarbageCollectionDuration, startComputerSimulation);
  }

  private void startComputerSimulation() {
    IsGarbageCollecting = false;
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
    var modifyCount = rng.RandiRange(0, Math.Min(programs.Count - closeCount, 5 - openCount - closeCount));

    var programsToClose = programs.Take(closeCount).ToList();
    var programsToModify = programs.Skip(closeCount).Take(modifyCount).ToList();
    var programsToOpen =
      Enumerable.Range(0, openCount).Select(_ => new ProgramDescription("asd", rng.RandiRange(3, 8)));

    foreach (var p in programsToClose) {
      Animations.Animations.DoDelayed(rng.RandfRange(0, simulationDuration), () => taskManager.KillProcess(p));
    }

    foreach (var p in programsToModify) {
      Animations.Animations.DoDelayed(rng.RandfRange(0, simulationDuration),
        () => taskManager.AddMemoryToProcess(p, rng.RandiRange(1, 3)));
    }

    foreach (var p in programsToOpen) {
      Animations.Animations.DoDelayed(rng.RandfRange(0, simulationDuration),
        () => taskManager.AllocateProgram(p.Name, p.MemoryFootprint));
    }
  }

  private record ProgramDescription(string Name, int MemoryFootprint);
}
