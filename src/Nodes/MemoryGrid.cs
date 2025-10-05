using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Nodes.Systems;
using HalfNibbleGame.Scenes;

namespace HalfNibbleGame.Nodes;

public partial class MemoryGrid : Node2D, IEnumerable<MemoryBlock> {
  public const int Width = 10;
  public const int Height = 7;
  private const float blockWidth = 18;
  private const float blockHeight = 17;
  private const float blockMargin = 1;

  private MemoryBlock[] blocks = [];

  private int streak;
  private double streakCooldown;

  public MemoryBlock this[int x, int y] {
    get => blocks[x + y * Width];
    private set => blocks[x + y * Width] = value;
  }

  public float MemoryUsage { get; private set; }

  public override void _Ready() {
    Global.Services.ProvideInScene(this);
    blocks = new MemoryBlock[Width * Height];
    for (var y = 0; y < Height; y++)
    for (var x = 0; x < Width; x++) {
      var node = Global.Prefabs.MemoryBlock.Instantiate<MemoryBlock>();
      node.AssignToGrid(this, (x, y));
      node.Position = new Vector2((x + 0.5f) * (blockWidth + blockMargin), (y + 0.5f) * (blockHeight + blockMargin));
      this[x, y] = node;
      AddChild(node);
    }
  }

  public override void _Process(double delta) {
    MemoryUsage = (float) blocks.Count(b => !b.IsFree) / blocks.Length;
    streakCooldown -= delta;
    if (streakCooldown <= 0) {
      streak = 0;
      streakCooldown = 0;
    }
  }

  public override void _Input(InputEvent @event) {
    if (!Global.Services.Get<GameLoop>().IsGarbageCollecting) return;

    if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left } mouseButton)
      simulateClickAt(mouseButton.Position);

    if (@event is InputEventMouseMotion mouseMotion && mouseMotion.ButtonMask.HasFlag(MouseButtonMask.Left) &&
        // Disable dragging if the adjacent memory experiment is on.
        !Global.Instance.FreeAdjacentMemory)
      simulateClickAt(mouseMotion.Position);
  }

  private void simulateClickAt(Vector2 position) {
    for (var i = 0; i < blocks.Length; i++) {
      if (!blocks[i].TrySimulateClick(position, out var freedProgram)) continue;

      var scoreTracker = Global.Services.Get<ScoreTracker>();
      if (blocks[i].IsFree) {
        scoreTracker.MemoryFreed();
        streak++;
        streakCooldown = 0.3;
        Global.Services.Get<SoundPlayer>().PlayConfirm(streak);
      }

      if (Global.Instance.FreeAdjacentMemory) freeSurroundingMemory(i, freedProgram, scoreTracker);

      break;
    }
  }

  private void freeSurroundingMemory(int from, Program program, ScoreTracker scoreTracker) {
    var q = new Queue<int>();
    q.Enqueue(from);
    var seen = new HashSet<int> { from };

    while (q.TryDequeue(out var idx)) {
      var block = blocks[idx];
      if (idx != from && block.AssignedProgram != program)
        // Don't propagate further if the block has a different program (exception for the initial tile).
        continue;

      if (!block.IsFree) {
        block.FreeMemory();
        scoreTracker.MemoryFreed();
      }

      foreach (var neighbor in adjacentIndices(idx))
        // Queue neighbouring tiles.
        if (!seen.Contains(neighbor)) {
          q.Enqueue(neighbor);
          seen.Add(neighbor);
        }
    }
  }

  private IEnumerable<int> adjacentIndices(int idx) {
    // Above
    if (idx >= Width) yield return idx - Width;
    // Right
    if (idx % Width < Width - 1) yield return idx + 1;
    // Below
    if (idx < blocks.Length - Width) yield return idx + Width;
    // Left
    if (idx % Width > 0) yield return idx - 1;
  }

  public void AllocateProgram(Program program, int size) {
    var leftToAllocate = size;
    foreach (var t in blocks) {
      if (!t.IsFree) continue;
      t.AssignProgram(program);
      leftToAllocate--;
      if (leftToAllocate == 0) break;
    }

    if (leftToAllocate > 0) GD.Print("game over");
  }

  public IEnumerator<MemoryBlock> GetEnumerator() {
    return blocks.AsEnumerable().GetEnumerator();
  }

  IEnumerator IEnumerable.GetEnumerator() {
    return GetEnumerator();
  }
}
