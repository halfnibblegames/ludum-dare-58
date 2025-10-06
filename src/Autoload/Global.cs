using Godot;

namespace HalfNibbleGame.Autoload;

public sealed partial class Global : Node {
  private static Prefabs? prefabs;

  private readonly ServiceProvider services = new();

  [ExportCategory("Experiments")] [Export]
  public bool DimFreeMemory;

  [Export] public bool FreeAdjacentMemory = true;

  public static IServiceProvider Services => Instance.services;

  public static Prefabs Prefabs {
    get {
      prefabs ??= Services.Get<Prefabs>();
      return prefabs;
    }
  }

  // This will be set in _Ready, and since Global is automatically loaded, this will always be true.
  public static Global Instance { get; private set; } = null!;

  public override void _Ready() {
    Instance = this;
  }

  public void SwitchScene(string path) {
    GetTree().ChangeSceneToFile(path);
    services.OnSceneChanging();
  }
}
