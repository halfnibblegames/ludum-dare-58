using System;
using Godot;
using HalfNibbleGame.Autoload;

namespace HalfNibbleGame.Nodes;

public partial class GlitchShader : ColorRect {
  private const double decayTime = 2;
  private const float oneOffGlitchAmount = 0.8f;

  [Export] private ShaderMaterial? glitchShader;

  private float glitchAmount;
  private bool suppressDecay;

  public override void _Ready() {
    Global.Services.ProvideInScene(this);
  }

  public override void _Process(double delta) {
    if (!suppressDecay) {
      glitchAmount = Math.Max(0, glitchAmount - (float) (delta / decayTime));
    }

    glitchShader?.SetShaderParameter("overall_amount", glitchAmount);
  }

  public void OneOffGlitch() {
    glitchAmount = Math.Min(1, glitchAmount + oneOffGlitchAmount);
  }

  public void GlitchPermanently() {
    glitchAmount = 1f;
    suppressDecay = true;
  }

  public void StopGlitching() {
    glitchAmount = 0f;
    suppressDecay = false;
  }
}
