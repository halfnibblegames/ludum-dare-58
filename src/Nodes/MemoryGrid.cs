using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Scenes;

namespace HalfNibbleGame.Nodes;

public partial class MemoryGrid : Node2D {

  [Export] private int width = 10;
  [Export] private int height = 10;
  [Export] private float blockSize = 12;
  [Export] private float blockMargin = 3;

  private MemoryBlock[] blocks = [];

  public override void _Ready() {
    blocks = new MemoryBlock[width * height];
    for (var y = 0; y < width; y++) {
      for (var x = 0; x < height; x++) {
        var node = Global.Prefabs.MemoryBlock.Instantiate<MemoryBlock>();
        node.Position = new Vector2((x + 0.5f) * (blockSize + blockMargin),  (y + 0.5f) * (blockSize + blockMargin));
        this[x, y] = node;
        AddChild(node);
      }
    }
  }

  public void AllocateProgram(IProgram program, int size) {
    var leftToAllocate = size;
    for (var i = 0; i < blocks.Length; i++) {
      if (!blocks[i].IsFree) continue;
      blocks[i].AssignProgram(program);
      leftToAllocate--;
      if (leftToAllocate == 0) break;
    }

    if (leftToAllocate > 0) {
      GD.Print("game over");
    }
  }

  private MemoryBlock this[int x, int y] {
    get => blocks[x + y * width];
    set => blocks[x + y * width] = value;
  }
}
