using System.Collections.Generic;
using System.Linq;
using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Nodes.Systems;
using HalfNibbleGame.Scenes;

namespace HalfNibbleGame.Nodes;

public partial class MemoryGrid : Node2D {
  private const int width = 10;
  private const int height = 7;
  private const float blockSize = 18;
  private const float blockMargin = 1;

  private MemoryBlock[] blocks = [];

  private MemoryBlock this[int x, int y] {
    get => blocks[x + y * width];
    set => blocks[x + y * width] = value;
  }

  public float MemoryUsage { get; private set; }

  public override void _Ready() {
    Global.Services.ProvideInScene(this);
    blocks = new MemoryBlock[width * height];
    for (var y = 0; y < height; y++)
    for (var x = 0; x < width; x++) {
      var node = Global.Prefabs.MemoryBlock.Instantiate<MemoryBlock>();
      node.Position = new Vector2((x + 0.5f) * (blockSize + blockMargin), (y + 0.5f) * (blockSize + blockMargin));
      this[x, y] = node;
      AddChild(node);
    }
  }

  public override void _Process(double delta) {
    MemoryUsage = (float) blocks.Count(b => !b.IsFree) / blocks.Length;
  }

  public override void _Input(InputEvent @event) {
    if (!Global.Services.Get<GameLoop>().IsGarbageCollecting) return;

    if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left } mouseButton)
      simulateClickAt(mouseButton.Position);

    if (@event is InputEventMouseMotion mouseMotion && mouseMotion.ButtonMask.HasFlag(MouseButtonMask.Left) &&
        // Disable dragging if the adjacent memory experiment is on.
        !Global.Instance.FreeAdjacentMemory) {
      simulateClickAt(mouseMotion.Position);
    }
  }

  private void simulateClickAt(Vector2 position) {
    for (var i = 0; i < blocks.Length; i++) {
      if (!blocks[i].TrySimulateClick(position, out var freedProgram)) continue;

      if (Global.Instance.FreeAdjacentMemory) {
        freeSurroundingMemory(i, freedProgram);
      }

      break;
    }
  }

  private void freeSurroundingMemory(int from, Program program) {
    var q = new Queue<int>();
    q.Enqueue(from);
    var seen = new HashSet<int> { from };

    while (q.TryDequeue(out var idx)) {
      var block = blocks[idx];
      if (idx != from && block.AssignedProgram != program) {
        // Don't propagate further if the block has a different program (exception for the initial tile).
        continue;
      }

      if (!block.IsFree) block.FreeMemory();

      foreach (var neighbor in adjacentIndices(idx)) {
        // Queue neighbouring tiles.
        if (!seen.Contains(neighbor)) {
          q.Enqueue(neighbor);
          seen.Add(neighbor);
        }
      }
    }
  }

  private IEnumerable<int> adjacentIndices(int idx) {
    // Above
    if (idx >= width) yield return idx - width;
    // Right
    if (idx % width < width - 1) yield return idx + 1;
    // Below
    if (idx < blocks.Length - width) yield return idx + width;
    // Left
    if (idx % width > 0) yield return idx - 1;
  }

  public void AllocateProgram(Program program, int size) {
    var leftToAllocate = size;
    foreach (var t in blocks)
    {
      if (!t.IsFree) continue;
      t.AssignProgram(program);
      leftToAllocate--;
      if (leftToAllocate == 0) break;
    }

    if (leftToAllocate > 0) GD.Print("game over");
  }
}
