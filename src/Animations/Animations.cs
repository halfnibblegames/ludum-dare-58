using System;
using Godot;
using HalfNibbleGame.Autoload;

namespace HalfNibbleGame.Animations;

static class Animations {
  // Delegates to animation runner
  private static AnimationRunner runner => Global.Services.Get<AnimationRunner>();

  public static void DoDelayed(double delaySeconds, Action action) {
    runner.DoDelayed(delaySeconds, action);
  }

  // Other helpers
  public static void PlayAndThen(AnimatedSprite2D sprite, string animation, Action onFinished) {
    sprite.Play(animation);
    sprite.AnimationFinished += onAnimationFinished;
    return;

    void onAnimationFinished() {
      sprite.AnimationFinished -= onAnimationFinished;
      onFinished();
    }
  }
}
