using Godot;
using HalfNibbleGame.Autoload;

namespace HalfNibbleGame.Animations;

public sealed partial class ShakeCamera2D : Camera2D {
  private const float maxShakeOffset = 5;
  private const float noiseSpeed = 60;
  private const float decayPerSecond = 2.0f;

  private readonly FastNoiseLite noise = new();
  private float amount;
  private float t;

  public override void _Ready() {
    GD.Randomize();
    noise.Seed = (int) GD.Randi();
    noise.Frequency = 0.25f;
    noise.FractalOctaves = 2;
    Global.Services.ProvideInScene(this);
  }

  public override void _Process(double delta) {
    amount -= decayPerSecond * (float) delta;
    if (amount <= 0) {
      amount = 0;
      // Avoid t getting too big by just resetting it.
      t = 0;
      return;
    }

    t += noiseSpeed * (float) delta;

    Offset = new Vector2(
      maxShakeOffset * amount * noise.GetNoise1D(t),
      maxShakeOffset * amount * noise.GetNoise2D(noise.Seed, t));
  }

  public void Shake(float intensity) {
    amount = Mathf.Min(amount + intensity, 1);
  }
}
