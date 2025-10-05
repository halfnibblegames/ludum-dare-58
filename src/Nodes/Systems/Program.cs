using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using HalfNibbleGame.Scenes;

namespace HalfNibbleGame.Nodes.Systems;

public class Program(ITaskManager taskManager, string name, Color color) {
  public bool IsDead { get; private set; }
  protected readonly HashSet<MemoryBlock> AllocatedMemory = [];

  // Should this program crash when memory is freed?
  protected virtual bool ShouldCrashOnFree => !IsDead;
  protected ITaskManager TaskManager => taskManager;

  public string Name => name;
  public Color Color => color;

  public virtual void SimulateCycle(RandomNumberGenerator rng) {
    if (rng.Randf() < 0.75) {
      taskManager.AddMemoryToProcess(this, rng.RandiRange(1, 3));
    }
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
    // We copy into a list: better to not loop over a hash set anyway, but we're actually modifying the hash set!
    foreach (var memoryBlock in AllocatedMemory.ToList()) {
      var adjacentBlocks = memoryBlock.AdjacentBlocks.ToList();
      if (adjacentBlocks.Count == 0) {
        GD.PushWarning("There should always be adjacent blocks");
      }
      var pickedBlock = adjacentBlocks[rng.RandiRange(0, adjacentBlocks.Count - 1)];
      attemptToSpread(pickedBlock);
    }
  }

  private void attemptToSpread(MemoryBlock block) {
    // Viruses are nice to each other.
    if (block.AssignedProgram is Virus) {
      return;
    }
    // Spread into an empty block.
    if (block.IsFree) {
      block.AssignProgram(this);
      return;
    }
    // Block is already assigned to a different program, see if there is at least two virus tiles adjacent before
    // corrupting.
    var adjacentVirusCount = block.AdjacentBlocks.Count(b => b.AssignedProgram is Virus);
    if (adjacentVirusCount >= 2) {
      block.FreeMemory();
      // If it wasn't corrupted, the program wasn't running anymore. Great, we'll occupy it instead.
      if (block.IsFree) {
        block.AssignProgram(this);
      }
    }
  }

  public override void OnMemoryFreed(MemoryBlock memoryBlock) {
    base.OnMemoryFreed(memoryBlock);
    if (AllocatedMemory.Count == 0 && !IsDead) {
      TaskManager.KillProcess(this);
    }
  }
}
