using System;
using Godot;
using HalfNibbleGame.Autoload;

namespace HalfNibbleGame.Scenes;

public partial class SoundPlayer : Node {

  private AudioStreamPlayer? player;

  [Export] private AudioStream? confirm;
  [Export] private AudioStream? error;
  [Export] private AudioStream? end;
  [Export] private AudioStream? endPerfect;
  [Export] private AudioStream? program;

  public override void _Ready() {
    Global.Services.ProvideInScene(this);
    player = GetNode<AudioStreamPlayer>("Player");
  }

  public void PlayConfirm(int streak) {
    play(confirm, streak);
  }

  public void PlayError() {
    play(error);
  }

  public void PlayEnd() {
    play(end);
  }

  public void PlayEndPerfect() {
    play(endPerfect);
  }

  public void PlayProgram() {
    play(program);
  }

  private void play(AudioStream? stream, int streak = 0) {
    if (player is null || stream is null) return;

    player.Stop();

    if (streak > 1) {
      player.PitchScale = 1 + Math.Min(5, streak - 1) * 0.2f;
    }
    else {
      player.PitchScale = 1;
    }

    player.Stream = stream;
    player.Play();
  }
}
