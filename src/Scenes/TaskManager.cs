using System.Collections.Generic;
using System.Linq;
using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Nodes;
using HalfNibbleGame.Nodes.Systems;

namespace HalfNibbleGame.Scenes;

public interface ITaskManager {
  IReadOnlyList<Program> Programs { get; }
  float MemoryUsage { get; }

  Color GetNextColor();
  void AllocateProgram(Program program, int memoryNeeded);
  void AddMemoryToProcess(Program program, int memoryAdded);
  void KillProcess(Program program);
  void CrashProcess(Program program);
}

public partial class TaskManager : Node2D, ITaskManager {
  private static readonly List<Color> availableProgramColors = [
    Color.FromString("#FFADAD", Colors.Red),
    Color.FromString("#FFD6A5", Colors.Orange),
    Color.FromString("#FDFFB6", Colors.Yellow),
    Color.FromString("#CAFFBF", Colors.Green),
    Color.FromString("#9BF6FF", Colors.Cyan),
    Color.FromString("#A0C4FF", Colors.Blue),
    Color.FromString("#BDB2FF", Colors.Purple),
    Color.FromString("#FFC6FF", Colors.Pink),
    Colors.Red,
    Colors.Orange,
    Colors.Yellow,
    Colors.Green,
    Colors.Cyan,
    Colors.Blue,
    Colors.Purple,
    Colors.Pink
  ];

  private readonly List<Program> programs = [];
  private VBoxContainer programListContainer = null!;

  private MemoryGrid memoryGrid => Global.Services.Get<MemoryGrid>();

  public IReadOnlyList<Program> Programs => programs;
  public float MemoryUsage => memoryGrid.MemoryUsage;

  public Color GetNextColor() {
    // Pick the first available color from the available pool.
    var color = availableProgramColors.First();
    availableProgramColors.RemoveAt(0);
    return color;
  }

  public void AllocateProgram(Program program, int memoryNeeded) {
    // Add program to the internal list.
    programs.Add(program);

    // Attempt to allocate memory for the program. The game might end right here.
    AddMemoryToProcess(program, memoryNeeded);

    // Add program to the UI.
    var programListEntry = Global.Prefabs.ProgramListEntry.Instantiate<ProgramListEntry>();
    programListEntry.SetProgram(program);
    programListContainer.AddChild(programListEntry);

    Global.Services.Get<SoundPlayer>().PlayProgram();
  }

  public void AddMemoryToProcess(Program program, int memoryAdded) {
    memoryGrid.AllocateProgram(program, memoryAdded);
  }

  public void KillProcess(Program program) {
    // Remove from internal list.
    var index = programs.IndexOf(program);
    programs.RemoveAt(index);

    // Add the program's color back to the list of available colors. Putting it at the end prevents reuse and confusion.
    availableProgramColors.Add(program.Color);

    // Remove from the UI.
    var childToRemove = programListContainer.GetChildren()[index];
    programListContainer.RemoveChild(childToRemove);

    Global.Services.Get<SoundPlayer>().PlayProgram();
  }

  public void CrashProcess(Program program) {
    KillProcess(program);
    Global.Services.Get<GameLoop>().InterruptGarbageCollecting();
  }

  public override void _Ready() {
    base._Ready();
    Global.Services.ProvidePersistent<ITaskManager>(this);
    programListContainer = GetNode<VBoxContainer>("../../../../ProgramList");
  }
}
