using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
  void Defrag();
}

public partial class TaskManager : Node2D, ITaskManager {
  private readonly List<Color> availableProgramColors = [
    Color.FromHtml("2f4f4f"), // dark slate gray
    Color.FromHtml("228b22"), // forest green
    Color.FromHtml("7b68ee"), // medium slate blue
    Color.FromHtml("8b4513"), // saddle brown
    Color.FromHtml("9acd32"), // yellow green
    Color.FromHtml("00ff7f"), // spring green
    Color.FromHtml("dc143c"), // crimson
    Color.FromHtml("ffa07a"), // light salmon
    Color.FromHtml("1e90ff"), // dodger blue
    Color.FromHtml("ff1493"), // deep pink
    Color.FromHtml("eee8aa"), // pale golden rod
    Color.FromHtml("dda0dd"), // plum
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
    if (Global.Services.Get<GameLoop>().GameIsOver) return;

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
    _ = memoryGrid.AllocateProgram(program, memoryAdded).ToList();
  }

  public void KillProcess(Program program) {
    if (Global.Services.Get<GameLoop>().GameIsOver) return;

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
    Random.Shared.Shuffle(CollectionsMarshal.AsSpan(availableProgramColors));
    Global.Services.ProvideInScene<ITaskManager>(this);
    programListContainer = GetNode<VBoxContainer>("../../../../ProgramList");
  }

  public void Defrag() {
    var programsInMemory = memoryGrid.ProgramsInMemory;
    var memoryUsagePerProgram = programsInMemory.Select(p => (p, p.MemoryUsage)).ToDictionary();

    memoryGrid.ResetAll();
    foreach (var program in programsInMemory) {
      var memoryUsage = memoryUsagePerProgram[program];
      var newMemory = memoryGrid.AllocateProgram(program, memoryUsage, true).ToList();
      program.ReplaceMemory(newMemory);
    }
  }
}
