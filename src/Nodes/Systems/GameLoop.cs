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
    for (var i = 0; i < 3; i++) {
      var diceRoll = rng.Randf();
      if (diceRoll < 0.4f) {
        // Create a new program.
        Animations.Animations.DoDelayed(
          rng.RandfRange(0, simulationDuration),
          () => taskManager.AllocateProgram(new Program(randomColor(), "asd"), rng.RandiRange(3, 8)));
      }
      else if (diceRoll < 0.65f) {
        // Increase memory footprint of an existing program.
      }
      else if (diceRoll < 0.9f) {
        // Close a program.
        var programs = taskManager.GetPrograms();
        if (programs.Count > 0) {
          var programToClose = programs[rng.RandiRange(0, programs.Count - 1)];

          Animations.Animations.DoDelayed(
            rng.RandfRange(0, simulationDuration),
            () => taskManager.KillProcess(programToClose));
        }
      }
      // Do nothing

      var a = 1;
    }

    Animations.Animations.DoDelayed(simulationDuration, startGarbageCollecting);
  }

  private static Color randomColor() {
    var hue = rng.Randf();
    return Color.FromHsv(hue, 1, 1);
  }
}
