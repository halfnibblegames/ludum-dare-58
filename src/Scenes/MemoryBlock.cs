using Godot;

namespace HalfNibbleGame.Scenes;

public partial class MemoryBlock : Area2D {

  public override void _Input(InputEvent @event) {
    // 😭
    if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouseEvent) {
      if (GetNode<CollisionShape2D>("MouseCollision").Shape.GetRect().HasPoint(mouseEvent.Position - GlobalPosition)) {
        freeMemory();
      }
    }
  }

  private void freeMemory() {
	GetNode<ColorRect>("ColorRect").Color = new Color(255, 0, 0);
  }
}
