using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Scenes;

namespace HalfNibbleGame.Nodes.Systems;

public sealed partial class GameLoop : Node {

  private static RandomNumberGenerator rng = new();

  [Export] private float garbageCollectionDuration = 4f;
  [Export] private float simulationDuration = 4f;

  public bool IsGarbageCollecting { get; private set; }

  public override void _Ready() {
    Global.Services.ProvideInScene(this);
    CallDeferred(nameof(startComputerSimulation));
  }

  private void startGarbageCollecting() {
    IsGarbageCollecting = true;
    Animations.Animations.DoDelayed(garbageCollectionDuration, startComputerSimulation);
  }

  private void startComputerSimulation() {
    IsGarbageCollecting = false;
    Global.Services.Get<MemoryGrid>().AllocateProgram(new ProgramPlaceholder(randomColor()), 6);
    // Animations.Animations.DoDelayed(SimulationDuration, startGarbageCollecting);
    startGarbageCollecting();
  }

  private record ProgramPlaceholder(Color Color) : IProgram {
    public void MemoryFreed() { }
  }

  private static Color randomColor() {
    var hue = rng.Randf();
    return Color.FromHsv(hue, 1, 1);
  }
}
