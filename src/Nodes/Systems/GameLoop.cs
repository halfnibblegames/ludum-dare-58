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
    for (var i = 0; i < 3; i++)
      Animations.Animations.DoDelayed(
          rng.RandfRange(0, simulationDuration),
          () => taskManager.AllocateProgram(new Program(randomColor(), "asd"), rng.RandiRange(3, 8)))
        ;
    Animations.Animations.DoDelayed(simulationDuration, startGarbageCollecting);
  }

  private static Color randomColor() {
    var hue = rng.Randf();
    return Color.FromHsv(hue, 1, 1);
  }
}
