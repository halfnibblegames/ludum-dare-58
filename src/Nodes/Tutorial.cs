using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Nodes.Systems;
using HalfNibbleGame.Scenes;

namespace HalfNibbleGame.Nodes;

public partial class Tutorial : Control {
  private ReferenceRect reference = null!;
  private Label text = null!;
  private ControlPrompt controllerPrompt = null!;
  private ColorRect left = null!;
  private ColorRect right = null!;
  private ColorRect top = null!;
  private ColorRect bottom = null!;

  private double timeUntilHint;

  public override void _Ready() {
    reference = GetNode<ReferenceRect>("ReferenceRect");
    text = GetNode<Label>("Label");
    left = GetNode<ColorRect>("MaskRects/Left");
    right = GetNode<ColorRect>("MaskRects/Right");
    top = GetNode<ColorRect>("MaskRects/Top");
    bottom = GetNode<ColorRect>("MaskRects/Bottom");
    controllerPrompt = GetNode<ControlPrompt>("ControllerPrompt");

    Global.Services.ProvideInScene(this);
  }

  public override void _Process(double delta) {
    if (!Visible) return;
    if (controllerPrompt.Visible) return;

    timeUntilHint -= delta;
    if (timeUntilHint <= 0) {
      controllerPrompt.Show();
      // Reset the shown input to reset the animation
      controllerPrompt.ShownInput = ControlPrompt.ControlInput.ControllerButtonA;
      controllerPrompt.ShownInput = ControlPrompt.ControlInput.MouseButtonLeft;
    }
  }

  public void ShowApplicationListExplanation() {
    var processList = GetNode<Control>("../ProgramList");
    reference.Position = processList.Position;
    reference.Size = processList.Size;
    text.Text = explanationApplicationList;
    updatePositions();

    controllerPrompt.Hide();
    timeUntilHint = 5;

    Show();
  }

  public void ShowFreeMemoryExplanation() {
    var mem = GetNode<Control>("MemoryReference");
    reference.Position = mem.Position;
    reference.Size = mem.Size;
    text.Text = explanationFreeMemory;
    updatePositions();

    controllerPrompt.Hide();
    timeUntilHint = 5;

    Show();
  }

  public void ShowCorruptionExplanation() {
    var mem = GetNode<Control>("MemoryReference");
    reference.Position = mem.Position;
    reference.Size = mem.Size;
    text.Text = explanationCorruption;
    updatePositions();

    controllerPrompt.Hide();
    timeUntilHint = 5;

    Show();
  }

  public void ShowDefragExplanation() {
    var def = GetNode<Control>("DefragReference");
    reference.Position = def.Position;
    reference.Size = def.Size;
    text.Text = explanationDefrag;
    updatePositions();

    controllerPrompt.Hide();
    timeUntilHint = 5;

    Show();
  }

  private void updatePositions() {
    var l = reference.Position.X;
    var r = reference.Position.X + reference.Size.X;
    var t = reference.Position.Y;
    var b = reference.Position.Y + reference.Size.Y;

    left.Size = new Vector2(l, 720);
    left.Position = Vector2.Zero;
    right.Size = new Vector2(1280 - r, 720);
    right.Position = new Vector2(r, 0);

    top.Size = new Vector2(r - l, t);
    top.Position = new Vector2(l, 0);
    bottom.Size = new Vector2(r - l, 720 - b);
    bottom.Position = new Vector2(l, b);

    if (text.Size.X < 1280 - r) {
      text.Position = new Vector2(r + 4, t);
    }
    else {
      text.Position = new Vector2(l - 4 - text.Size.X, t);
    }

    controllerPrompt.Position = new Vector2(text.Position.X + 0.5f * text.Size.X, text.Position.Y + text.Size.Y + 4);
  }

  private static readonly string explanationApplicationList =
    "Open applications are shown in this list. Applications that are open should never have their memory garbage collected.";

  private static readonly string explanationFreeMemory =
    "When an application closes, its memory should be freed to make room for other applications. During the garbage collection cycle, click the memory blocks that are out of use.";

  private static readonly string explanationCorruption =
    "Freeing memory that's in use causes that block to corrupt and the cycle to end. Viruses should always be freed, even while they are running.";

  private static readonly string explanationDefrag =
    $"Every {GameLoop.CyclesPerDefrag} cycles, you get access to a defragmentation. This groups all memory of the same programs together, but immediately ends the cycle.";
}
