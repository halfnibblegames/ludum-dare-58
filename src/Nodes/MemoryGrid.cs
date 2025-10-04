using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Scenes;

namespace HalfNibbleGame.Nodes;

public partial class MemoryGrid : Node2D {

  private const int width = 8;
  private const int height = 8;
  private const float blockSize = 16;
  private const float blockMargin = 3;

  private MemoryBlock[] blocks = [];

  public override void _Ready() {
    Global.Services.ProvideInScene(this);
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

  public override void _Input(InputEvent @event) {
    if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left } mouseButton) {
      simulateClickAt(mouseButton.Position);
    }

    if (@event is InputEventMouseMotion mouseMotion && mouseMotion.ButtonMask.HasFlag(MouseButtonMask.Left)) {
      simulateClickAt(mouseMotion.Position);
    }
  }

  private void simulateClickAt(Vector2 position) {
    foreach (var b in blocks) {
      b.TrySimulateClick(position);
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
