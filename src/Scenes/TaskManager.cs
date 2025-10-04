using System.Collections.Generic;
using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Nodes;

namespace HalfNibbleGame.Scenes;

public sealed record Program(Color Color, string Name);

public interface ITaskManager {
  void AllocateProgram(string programName, int memoryNeeded);
  void KillProcess(Program program);
  IReadOnlyList<Program> GetPrograms();
}

public partial class TaskManager : Node2D, ITaskManager {
  private static readonly RandomNumberGenerator rng = new();

  private static readonly List<Color> availableProgramColors = [
    Color.FromString("#FFADAD", Colors.Red),
    Color.FromString("#FFD6A5", Colors.Orange),
    Color.FromString("#FDFFB6", Colors.Yellow),
    Color.FromString("#CAFFBF", Colors.Green),
    Color.FromString("#9BF6FF", Colors.Cyan),
    Color.FromString("#A0C4FF", Colors.Blue),
    Color.FromString("#BDB2FF", Colors.Purple),
    Color.FromString("#FFC6FF", Colors.Pink)
  ];

  private readonly List<Program> programs = [];
  private VBoxContainer programListContainer;

  public IReadOnlyList<Program> GetPrograms() {
    return programs;
  }

  public void AllocateProgram(string programName, int memoryNeeded) {
    // Pick a random color and remove it from the available pool.
    var colorIndex = rng.RandiRange(0, availableProgramColors.Count - 1);
    var color = availableProgramColors[colorIndex];
    availableProgramColors.RemoveAt(colorIndex);

    // Crate the program using the selected color.
    var program = new Program(color, programName);

    // Add program to the internal list.
    programs.Add(program);

    // Attempt to allocate memory for the program. The game might end right here.s
    Global.Services.Get<MemoryGrid>().AllocateProgram(program, memoryNeeded);

    // Add program to the UI.
    var programListEntry = Global.Prefabs.ProgramListEntry.Instantiate<ProgramListEntry>();
    programListEntry.SetProgram(program);
    programListContainer.AddChild(programListEntry);
  }

  public void KillProcess(Program program) {
    // Remove from internal list.
    var index = programs.IndexOf(program);
    programs.RemoveAt(index);

    // Add the program's color back to the list of available colors.
    availableProgramColors.Add(program.Color);

    // Remove from the UI.
    var childToRemove = programListContainer.GetChildren()[index];
    programListContainer.RemoveChild(childToRemove);
  }

  public override void _Ready() {
    base._Ready();
    Global.Services.ProvidePersistent<ITaskManager>(this);
    programListContainer = GetNode<VBoxContainer>("../../../../ProgramList");
  }
}
