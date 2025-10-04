using System;
using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Nodes.Systems;

namespace HalfNibbleGame.Scenes;

public partial class MemoryBlock : Area2D {
  private Program? assignedProgram;

  [Export] private Color freeColor = new(0.5f, 0.5f, 0.5f);

  public bool IsFree => assignedProgram is null;

  public override void _Ready() {
    setColor(freeColor);
  }

  public void TrySimulateClick(Vector2 position) {
    if (GetNode<CollisionShape2D>("MouseCollision").Shape.GetRect().Grow(1).HasPoint(position - GlobalPosition) &&
        Global.Services.Get<GameLoop>().IsGarbageCollecting)
      freeMemory();
  }

  private void freeMemory() {
    if (assignedProgram is null) return;

    setColor(freeColor);
    assignedProgram = null;
  }

  public void AssignProgram(Program program) {
    if (assignedProgram is not null) throw new InvalidOperationException();

    assignedProgram = program;
    setColor(program.Color);
  }

  private void setColor(Color c) {
    GetNode<ColorRect>("ColorRect").Color = c;
  }
}
