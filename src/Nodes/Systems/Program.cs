using System.Collections.Generic;
using System.Collections.Immutable;
using Godot;
using HalfNibbleGame.Scenes;

namespace HalfNibbleGame.Nodes.Systems;

public class Program(ITaskManager taskManager, string name, Color color) {
  protected bool IsDead;
  protected readonly HashSet<MemoryBlock> AllocatedMemory = [];

  // Should this program crash when memory is freed?
  protected virtual bool ShouldCrashOnFree => !IsDead;

  public string Name => name;
  public Color Color => color;

  public virtual void SimulateCycle(RandomNumberGenerator rng) {
    if (rng.Randf() < 0.5) {
      taskManager.AddMemoryToProcess(this, rng.RandiRange(1, 3));
    }
    // Add memory
  }

  public void OnMemoryAllocated(MemoryBlock memoryBlock) {
    AllocatedMemory.Add(memoryBlock);
  }

  public void Kill() {
    markAsDead();
    taskManager.KillProcess(this);
  }

  public virtual void OnMemoryFreed(MemoryBlock memoryBlock) {
    AllocatedMemory.Remove(memoryBlock);
    if (ShouldCrashOnFree) {
      memoryBlock.Corrupt();
      markAsDead();
      taskManager.CrashProcess(this);
    }
  }

  private void markAsDead() {
    IsDead = true;
  }

  public static readonly ImmutableArray<string> PossibleNames = [
    "PhotoStore",
    "Goggle Ride",
    "Goggle Vroom",
    "Disharmony",
    "EarthMammal",
    "Gopoint",
    "Paint4D",
    "Watervapor",
    "Manufacturio",
    "Underwatch",
    "Recycling bin",
    "VisageTome Messenger",
    "Cosmic Critter Chess"
  ];
}

public class Virus(ITaskManager taskManager, Color color) : Program(taskManager, "TrOjAn HoRsE", color) {
  // We keep running, even if we lose memory.
  protected override bool ShouldCrashOnFree => false;

  public override void SimulateCycle(RandomNumberGenerator rng) {
    taskManager.AddMemoryToProcess(this, AllocatedMemory.Count);
  }

  public override void OnMemoryFreed(MemoryBlock memoryBlock) {
    base.OnMemoryFreed(memoryBlock);
    if (AllocatedMemory.Count == 0 && !IsDead) {
      taskManager.KillProcess(this);
    }
  }
}
