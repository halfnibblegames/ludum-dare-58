using Godot;
using HalfNibbleGame.Nodes.Systems;

namespace HalfNibbleGame.Scenes;

public partial class ProgramListEntry : Control {
  private Label programNameLabel => GetNode<Label>("Name");
  private ColorRect programColorLabel => GetNode<ColorRect>("Color");

  public void SetProgram(Program program) {
    programNameLabel.Text = program.Name;
    programColorLabel.Color = program.Color;
  }
}
