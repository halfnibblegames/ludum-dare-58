using Godot;

namespace HalfNibbleGame.Autoload;

public sealed partial class Global : Node {

  [ExportCategory("Experiments")]
  [Export] public bool DimFreeMemory;
  [Export] public bool FreeAdjacentMemory;

  public static IServiceProvider Services => Instance.services;

  private static Prefabs? prefabs;

  public static Prefabs Prefabs {
    get {
      prefabs ??= Services.Get<Prefabs>();
      return prefabs;
    }
  }

  // This will be set in _Ready, and since Global is automatically loaded, this will always be true.
  public static Global Instance { get; private set; } = null!;

  private readonly ServiceProvider services = new();

  public override void _Ready() {
    Instance = this;
  }

  public void SwitchScene(string path) {
    GetTree().ChangeSceneToFile(path);
    services.OnSceneChanging();
  }
}
