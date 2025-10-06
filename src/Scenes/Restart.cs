using Godot;
using HalfNibbleGame.Autoload;

namespace HalfNibbleGame.Scenes;

public partial class Restart : Node2D {
  private bool hasRestarted;

  public override void _Process(double delta) {
    if (hasRestarted) return;

    Global.Instance.SwitchScene("uid://bn1xhxvduovxo");
    hasRestarted = true;
  }
}
