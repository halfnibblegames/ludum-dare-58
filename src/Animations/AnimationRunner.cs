using System;
using System.Collections.Generic;
using Godot;
using HalfNibbleGame.Autoload;

namespace HalfNibbleGame.Animations;

public sealed partial class AnimationRunner : Node {
  private double totalTime;
  private readonly List<DelayedAction> delayedActions = [];

  public override void _Ready() {
    Global.Services.ProvideInScene(this);
  }

  public override void _Process(double delta) {
    totalTime += delta;
    var i = 0;
    while (delayedActions.Count > i && delayedActions[i].Timestamp <= totalTime) {
      delayedActions[i].Action();
      i++;
    }

    delayedActions.RemoveRange(0, i);
  }

  public void DoDelayed(double delaySeconds, Action action) {
    delayedActions.Add(new DelayedAction(totalTime + delaySeconds, action));
    // Oh, oh, oh so bad to sort every time, but this list will never be long and this makes for easy to read code.
    delayedActions.Sort((left, right) => left.Timestamp.CompareTo(right.Timestamp));
  }

  private readonly record struct DelayedAction(double Timestamp, Action Action);
}
