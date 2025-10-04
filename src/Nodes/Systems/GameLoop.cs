using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Scenes;

namespace HalfNibbleGame.Nodes.Systems;

public sealed partial class GameLoop : Node {

  private static readonly RandomNumberGenerator rng = new();

  [Export] public float GarbageCollectionDuration = 4f;
  [Export] private float simulationDuration = 4f;

  public bool IsGarbageCollecting { get; private set; }
  public double GarbageCollectingTimeLeft => garbageCollectTimer?.TimeLeft ?? 0;
  private SceneTreeTimer? garbageCollectTimer;

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

    var grid = Global.Services.Get<MemoryGrid>();
    for (var i = 0; i < 3; i++) {
      Animations.Animations.DoDelayed(
        rng.RandfRange(0, simulationDuration),
        () => grid.AllocateProgram(new ProgramPlaceholder(randomColor()), rng.RandiRange(3, 8)));
    }
    Animations.Animations.DoDelayed(simulationDuration, startGarbageCollecting);
  }

  private record ProgramPlaceholder(Color Color) : IProgram {
    public void MemoryFreed() { }
  }

  private static Color randomColor() {
    var hue = rng.Randf();
    return Color.FromHsv(hue, 1, 1);
  }
}
