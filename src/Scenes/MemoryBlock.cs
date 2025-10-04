using Godot;

namespace HalfNibbleGame.Scenes;

public partial class MemoryBlock : Area2D {
  public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx) {
	GD.Print(@event);

	if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } me) {
	  GD.Print(me.Position);
	  freeMemory();
	}

	if (@event is InputEventMouseMotion me2) {
	  GD.Print(me2.Position);
	}
  }

  private void freeMemory() {
	GetNode<ColorRect>("ColorRect").Color = new Color(255, 0, 0);
  }

  public override void _Ready() {
	GD.Print("MemoryBlock incoming connections:");
	foreach (var c in GetIncomingConnections()) {
	  foreach (var cc in c) {
		GD.Print(cc);
	  }
	}
  }
}
