using System.Linq;
using Godot;
using HalfNibbleGame.Nodes.Systems;

namespace HalfNibbleGame.Scenes;

public partial class ProgramListEntry : Control {
  private RandomNumberGenerator rng = new();
  private Program? program;
  private Label programNameLabel => GetNode<Label>("Name");
  private ColorRect programColorLabel => GetNode<ColorRect>("Color");

  private double nextNameUpdate;
  private bool nameIsOverriden;

  private static readonly string symbols = "!@#$%^&*_+";

  public void SetProgram(Program p) {
    program = p;
    programNameLabel.Text = p.Name;
    programColorLabel.Color = p.Color;
    nameIsOverriden = false;
  }

  public override void _Process(double delta) {
    nextNameUpdate -= delta;
    if (nextNameUpdate > 0 || program is null) return;

    if (nameIsOverriden) {
      programNameLabel.Text = program.Name;
      nextNameUpdate = rng.RandfRange(1, 3);
      nameIsOverriden = false;
      return;
    }

    if (program is Virus) {
      var overriddenName = string.Concat(program.Name.Select(c => char.IsSymbol(c) || rng.Randf() < 0.5 ? c : randomSymbol()));
      programNameLabel.Text = overriddenName;
      nextNameUpdate = rng.RandfRange(0.2f, 0.6f);
      nameIsOverriden = true;
    }
  }

  private char randomSymbol() {
    return symbols[rng.RandiRange(0, symbols.Length - 1)];
  }
}
