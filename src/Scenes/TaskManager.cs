using System.Collections.Generic;
using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Nodes;

namespace HalfNibbleGame.Scenes;

public sealed record Program(Color Color, string Name);

public interface ITaskManager {
  void AllocateProgram(Program program, int memoryNeeded);
  void KillProcess(Program program);
  IReadOnlyList<Program> GetPrograms();
}

public partial class TaskManager : Node2D, ITaskManager {
  private readonly List<Program> programs = [];
  private VBoxContainer programListContainer;

  public void AllocateProgram(Program program, int memoryNeeded) {
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
    programs.Remove(program);
  }

  public IReadOnlyList<Program> GetPrograms() {
    return programs;
  }

  public override void _Ready() {
    base._Ready();
    Global.Services.ProvidePersistent<ITaskManager>(this);
    programListContainer = GetNode<VBoxContainer>("../../../../ProgramList");
  }
}
