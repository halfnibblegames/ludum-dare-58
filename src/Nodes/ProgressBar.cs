using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Nodes.Systems;

namespace HalfNibbleGame.Nodes;

public partial class ProgressBar : ReferenceRect {
  public override void _Process(double delta) {
    var gameLoop = Global.Services.Get<GameLoop>();
    var timeLeft = gameLoop.GarbageCollectingTimeLeft;
    var ratio = (float) timeLeft / gameLoop.GarbageCollectionDuration;
    var rect = GetNode<ColorRect>("Progress");
    rect.Size = new Vector2(ratio * Size.X, Size.Y);
  }
}
