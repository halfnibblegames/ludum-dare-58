using Godot;
using HalfNibbleGame.Autoload;

namespace HalfNibbleGame.Scenes;

public sealed partial class Splash : Control {
  private const float animationDelay = 0.7f;
  private const float textWipeDuration = 0.8f;
  private const float fadeOutDelay = 0.2f;
  private const float fadeOutDuration = 0.8f;

  public override void _Ready() {
    Animations.Animations.DoDelayed(animationDelay, startAnimations);
  }

  private void startAnimations() {
    GetNode<AudioStreamPlayer>("BiteSound").Play();
    Animations.Animations.PlayAndThen(GetNode<AnimatedSprite2D>("Strawberry"), "Bite", onBiteAnimationFinished);
  }

  private void onBiteAnimationFinished() {
    var bg = GetNode<ColorRect>("Background");

    var tween = CreateTween();
    tween.TweenProperty(GetNode("Copyright"), "visible_ratio", 1, textWipeDuration);
    tween.TweenProperty(GetNode("FadeRect"), "color", bg.Color, fadeOutDuration).SetDelay(fadeOutDelay);
    tween.TweenCallback(Callable.From(onAnimationsFinished));
  }

  private void onAnimationsFinished() {
    Global.Instance.SwitchScene("uid://bn1xhxvduovxo");
  }
}
