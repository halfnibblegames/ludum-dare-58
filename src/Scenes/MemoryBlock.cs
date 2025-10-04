using System;
using System.Diagnostics.CodeAnalysis;
using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Nodes.Systems;

namespace HalfNibbleGame.Scenes;

public partial class MemoryBlock : Area2D {
  public Program? AssignedProgram { get; private set; }

  [Export] private Color freeColor = new(0.5f, 0.5f, 0.5f);

  private bool isCorrupted;
  public bool IsFree => AssignedProgram is null && !isCorrupted;

  public override void _Ready() {
    setColor(freeColor);
  }

  public override void _Process(double delta) {
    if (Global.Instance.DimFreeMemory && AssignedProgram is { IsDead: true }) {
      setColor(c => new Color(c.R, c.G, c.B, 0.5f));
    }
  }

  public bool TrySimulateClick(Vector2 position, [NotNullWhen(true)] out Program? freedProgram) {
    freedProgram = null;

    if (AssignedProgram is null) return false;

    var hitbox = GetNode<CollisionShape2D>("MouseCollision").Shape.GetRect().Grow(1);
    if (!hitbox.HasPoint(position - GlobalPosition)) return false;

    freedProgram = AssignedProgram;
    FreeMemory();
    return true;
  }

  public void FreeMemory() {
    setColor(freeColor);
    AssignedProgram!.OnMemoryFreed(this);
    AssignedProgram = null;
  }

  public void AssignProgram(Program program) {
    if (AssignedProgram is not null) throw new InvalidOperationException();

    AssignedProgram = program;
    program.OnMemoryAllocated(this);
    setColor(program.Color);
  }

  public void Corrupt() {
    isCorrupted = true;
    setColor(new Color(0, 0, 0));
  }

  private void setColor(Color c) {
    setColor(_ => c);
  }

  private void setColor(Func<Color, Color> f) {
    var rect = GetNode<ColorRect>("ColorRect");
    var color = f(rect.Color);
    rect.Color = color;
  }
}
