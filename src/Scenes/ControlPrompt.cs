using Godot;

namespace HalfNibbleGame.Scenes;

[Tool]
public partial class ControlPrompt : Control {
  private const int tileSize = 16;
  private const double animationSpeed = 3;

  private Texture2D? imageTexture;
  private int iconsPerRow;

  private AnimatedSprite2D? animatedSprite;
  private SpriteFrames spriteFrames = new();
  private ControlInput shownInput;

  [Export]
  public ControlInput ShownInput {
    get => shownInput;
    set {
      shownInput = value;
      updateAnimation();
    }
  }

  public override void _Ready() {
    animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite");
    animatedSprite.SpriteFrames = spriteFrames;
  }

  private void updateAnimation() {
    var animationName = animationKey(shownInput);
    if (spriteFrames.HasAnimation(animationName) || loadAnimation(shownInput)) {
      animatedSprite?.Play(animationName);
    }
  }

  private bool loadAnimation(ControlInput input) {
    imageTexture ??= loadTexture(out iconsPerRow);
    if (imageTexture is null) { // Texture might not be loaded if we're not on the main thread.
      return false;
    }

    var key = animationKey(input);
    var data = createData(input);

    var normalFrame = atlas(data.FrameNo);
    var pressedFrame = atlas(data.PressedFrameNo);

    spriteFrames.AddAnimation(key);
    spriteFrames.SetAnimationSpeed(key, animationSpeed);
    spriteFrames.AddFrame(key, data.Invert ? pressedFrame : normalFrame, 2);
    spriteFrames.AddFrame(key, data.Invert ? normalFrame : pressedFrame);

    return true;
  }

  private static Texture2D loadTexture(out int iconsPerRow) {
    var texture = GD.Load<Texture2D>("uid://b8builnlxpm41");
    iconsPerRow = texture.GetWidth() / tileSize;
    return texture;
  }

  private AtlasTexture atlas(int frameNo) => new() { Atlas = imageTexture!, Region = regionForFrame(frameNo) };

  private Rect2 regionForFrame(int frameNo) {
    var row = frameNo / iconsPerRow;
    var col = frameNo % iconsPerRow;
    return new Rect2(col * tileSize, row * tileSize, tileSize, tileSize);
  }

  private static string animationKey(ControlInput input) => input.ToString();
}
