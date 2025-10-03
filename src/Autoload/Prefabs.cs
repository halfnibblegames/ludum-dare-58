using Godot;

namespace HalfNibbleGame.Autoload;

public sealed partial class Prefabs : Node {
  public override void _Ready() {
    Global.Services.ProvidePersistent(this);
  }
}
