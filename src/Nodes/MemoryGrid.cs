using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Scenes;

namespace HalfNibbleGame.Nodes;

public partial class MemoryGrid : Node2D {

  [Export] private int width = 10;
  [Export] private int height = 10;
  [Export] private float blockSize = 12;
  [Export] private float blockMargin = 4;

  private MemoryBlock[] blocks = [];

  // Instantiate memory blocks

  public override void _Ready() {
    blocks = new MemoryBlock[width * height];
    for (var x = 0; x < width; x++) {
      for (var y = 0; y < height; y++) {
        var node = Global.Prefabs.MemoryBlock.Instantiate<MemoryBlock>();
        node.Position = new Vector2((x + 0.5f) * (blockSize + blockMargin),  (y + 0.5f) * (blockSize + blockMargin));
        this[x, y] = node;
        AddChild(node);
      }
    }
  }

  private MemoryBlock this[int x, int y] {
    get => blocks[x + y * width];
    set => blocks[x + y * width] = value;
  }
}
