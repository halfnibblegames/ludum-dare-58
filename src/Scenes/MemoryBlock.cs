using System;
using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Nodes.Systems;

namespace HalfNibbleGame.Scenes;

public partial class MemoryBlock : Area2D {

  [Export] private Color freeColor = new(0.5f, 0.5f, 0.5f);

  private IProgram? assignedProgram;

  public bool IsFree => assignedProgram is null;

  public override void _Ready() {
    setColor(freeColor);
  }

  public override void _Input(InputEvent @event) {
    // 😭
    if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouseEvent) {
      if (GetNode<CollisionShape2D>("MouseCollision").Shape.GetRect().HasPoint(mouseEvent.Position - GlobalPosition) &&
          Global.Services.Get<GameLoop>().IsGarbageCollecting) {
        freeMemory();
      }
    }
  }

  private void freeMemory() {
    if (assignedProgram is null) return;

    assignedProgram.MemoryFreed();
	  setColor(freeColor);
    assignedProgram = null;
  }

  public void AssignProgram(IProgram program) {
    if (assignedProgram is not null) throw new InvalidOperationException();

    assignedProgram = program;
    setColor(program.Color);
  }

  private void setColor(Color c) => GetNode<ColorRect>("ColorRect").Color = c;
}

public interface IProgram {
  Color Color { get; }
  void MemoryFreed();
}
