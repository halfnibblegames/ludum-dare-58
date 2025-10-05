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

  public void PlayConfirm() {
    play(confirm);
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

  private void play(AudioStream? stream) {
    if (player is null || stream is null) return;

    player.Stop();
    player.Stream = stream;
    player.Play();
  }
}
