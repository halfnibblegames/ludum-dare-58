using System;
using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Nodes.Systems;

namespace HalfNibbleGame.Scenes;

public partial class UiManager : Control {
  [Export] private Label? scoreLabel;
  [Export] private Label? scoreNoticeLabel;
  [Export] private Button? defragButton;
  [Export] private TextureButton? muteButton;
  [Export] private AudioStreamPlayer? audioPlayer;
  [Export] private Texture2D? mutedTexture;
  [Export] private Texture2D? playingTexture;

  private int lastKnownScore;
  private double nextScoreUpdate;

  public override void _Process(double delta) {
    nextScoreUpdate -= delta;
    var tracker = Global.Services.Get<ScoreTracker>();

    if (nextScoreUpdate <= 0 && scoreLabel is not null) {
      var currentScore = tracker.Score;
      var interpolate = (int) (0.5 * currentScore + 0.5 * lastKnownScore);
      if (Math.Abs(interpolate - currentScore) < 5) interpolate = currentScore;
      lastKnownScore = interpolate;
      scoreLabel.Text = lastKnownScore.ToString();

      // Only update the score 20 times per second;
      nextScoreUpdate = 0.05;
    }

    if (scoreNoticeLabel is not null) {
      scoreNoticeLabel.Text = tracker.ScoreNotice;
    }

    if (defragButton is not null) {
      var gameLoop = Global.Services.Get<GameLoop>();
      defragButton.Disabled = !gameLoop.CanDefrag;
      defragButton.Text = $"{gameLoop.DefragsAvailable} available";
    }
  }

  public void OnDefragButtonClicked() {
    Global.Services.Get<GameLoop>().Defrag();
  }

  public void OnRestartButtonClicked() {
    // We use an intermediate scene to make sure we have a frame to clean up all the services.
    // Ideally the game over screen would have been a separate scene but... path of least resistance
    Global.Instance.SwitchScene("uid://veqm5h1saokm");
    // GetNode("/root/Main").Free();
  }

  public void MuteButtonClicked() {
    if (audioPlayer is null) return;
    if (audioPlayer.Playing) {
      audioPlayer.Stop();
      muteButton!.TextureNormal = mutedTexture;
    }
    else {
      audioPlayer.Play();
      muteButton!.TextureNormal = playingTexture;
    }
  }
}
