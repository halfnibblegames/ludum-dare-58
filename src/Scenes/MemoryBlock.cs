using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Nodes;
using HalfNibbleGame.Nodes.Systems;

namespace HalfNibbleGame.Scenes;

public partial class MemoryBlock : Area2D {
  public Program? AssignedProgram { get; private set; }

  [Export] private Color freeColor = new(0.5f, 0.5f, 0.5f);
  [Export] private Color corruptedColor = new(0.2f, 0.2f, 0.2f);
  [Export] private AnimatedSprite2D? background;
  [Export] private ColorRect? colorRect;
  [Export] private Sprite2D? lightSprite;

  private MemoryGrid? grid;
  private (int X, int Y) coordinates;
  public bool IsCorrupted { get; private set; }
  public bool IsFree => AssignedProgram is null && !IsCorrupted;

  public override void _Ready() {
    setColor(freeColor);
  }

  public void AssignToGrid(MemoryGrid g, (int X, int Y) coords) {
    grid = g;
    coordinates = coords;
  }

  public IEnumerable<MemoryBlock> AdjacentBlocks {
    get {
      if (grid == null) yield break;
      if (coordinates.X < MemoryGrid.Width - 1) yield return grid[coordinates.X + 1, coordinates.Y];
      if (coordinates.Y < MemoryGrid.Height - 1) yield return grid[coordinates.X, coordinates.Y + 1];
      if (coordinates.X > 0) yield return grid[coordinates.X - 1, coordinates.Y];
      if (coordinates.Y > 0) yield return grid[coordinates.X, coordinates.Y - 1];
    }
  }

  public override void _Process(double delta) {
    updateColor();

    var canBeClicked = Global.Services.Get<GameLoop>().IsGarbageCollecting;
    var shouldLightUp = canBeClicked && !IsCorrupted;
    if (background is not null) {
      if (!shouldLightUp) {
        background.Frame = 0;
      }
      else if (AssignedProgram is Virus) {
        background.Frame = 2;
      }
      else {
        background.Frame = 1;
      }
    }
    if (lightSprite is not null) {
      lightSprite.Visible = shouldLightUp;
    }
  }

  private void updateColor() {
    if (IsCorrupted) {
      setColor(corruptedColor);
      return;
    }

    var color = IsCorrupted ? corruptedColor : AssignedProgram?.Color ?? freeColor;
    if (Global.Instance.DimFreeMemory && AssignedProgram is { IsDead: true }) {
      color = new Color(color.R, color.G, color.B, 0.5f);
    }
    setColor(color);
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
    AssignedProgram!.OnMemoryFreed(this);
    Reset();
  }

  public void Reset() {
    setColor(freeColor);
    AssignedProgram = null;
  }

  public void AssignProgram(Program program, bool suppressProgramNotification = false) {
    if (AssignedProgram is not null) throw new InvalidOperationException();

    AssignedProgram = program;
    if (!suppressProgramNotification) program.OnMemoryAllocated(this);
    setColor(program.Color);
  }

  public void Corrupt() {
    IsCorrupted = true;
    setColor(new Color(0, 0, 0));
  }

  private void setColor(Color c) {
    if (background is not null) {
      background.Modulate = c;
    }
    if (colorRect is not null) {
      colorRect.Color = c;
    }
  }
}
