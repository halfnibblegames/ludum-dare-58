using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot;
using HalfNibbleGame.Autoload;
using HalfNibbleGame.Scenes;

namespace HalfNibbleGame.Nodes.Systems;

public class Program(ITaskManager taskManager, string name, Color color) {
  public static readonly ImmutableArray<string> PossibleNames = [
    "Facebork",
    "Insta-ham",
    "Blickclok",
    "RegretIt",
    "Discourt",
    "WorkBook",
    "SinkedIn",
    "WhassUp",
    "RockBlox",
    "FightNight",
    "SusBus",
    "Block game",
    "Legend of L0nk",
    "Underwatch",
    "Vaporrant",
    "Condense",
    "Epiq Shames",
    "Googol Dorks",
    "Microword",
    "X-Cell",
    "SnoozeDeck",
    "Snack",
    "Zoomers Meet",
    "Banana",
    "Jungle cart",
    "DoorCrash",
    "BnBarely",
    "Chroam",
    "Safurry",
    "FireFerret",
    "Edgy",
    "GitGud",
    "Windy ohs",
    "SnackOS",
    "ByteMuch",
    "Sweatify",
    "Spendr",
    "Tender",
    "ToastIQ"
  ];

  public static readonly ImmutableArray<string> VirusNames = [
    "N3URAL",
    "System32",
    "RAMZilla",
    "420BlazeIt.sys",
    "Dankware",
    "Byterot",
    "Horsey.exe",
    "FreeStuff.zip"
  ];

  protected readonly HashSet<MemoryBlock> AllocatedMemory = [];
  public bool IsDead { get; private set; }

  // Should this program crash when memory is freed?
  protected virtual bool ShouldCrashOnFree => !IsDead;
  protected ITaskManager TaskManager => taskManager;

  public string Name => name;
  public Color Color => color;
  public int MemoryUsage => AllocatedMemory.Count;

  public virtual void SimulateCycle(RandomNumberGenerator rng) {
    if (rng.Randf() < 0.75) taskManager.AddMemoryToProcess(this, rng.RandiRange(1, 3));
  }

  public void ReplaceMemory(ICollection<MemoryBlock> newMemory) {
    if (newMemory.Count != AllocatedMemory.Count) GD.PushError("Different memory usage");

    AllocatedMemory.Clear();
    foreach (var block in newMemory) AllocatedMemory.Add(block);
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
}

public class Virus(ITaskManager taskManager, string name, Color color) : Program(taskManager, name, color) {
  // We keep running, even if we lose memory.
  protected override bool ShouldCrashOnFree => false;

  public override void SimulateCycle(RandomNumberGenerator rng) {
    // We copy into a list: better to not loop over a hash set anyway, but we're actually modifying the hash set!
    foreach (var memoryBlock in AllocatedMemory.ToList()) {
      var adjacentBlocks = memoryBlock.AdjacentBlocks.ToList();
      if (adjacentBlocks.Count == 0) GD.PushWarning("There should always be adjacent blocks");

      var pickedBlock = adjacentBlocks[rng.RandiRange(0, adjacentBlocks.Count - 1)];
      attemptToSpread(pickedBlock);
    }
  }

  private void attemptToSpread(MemoryBlock block) {
    // Viruses are nice to each other.
    if (block.AssignedProgram is Virus) return;

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
      if (block.IsFree) block.AssignProgram(this);
    }
  }

  public override void OnMemoryFreed(MemoryBlock memoryBlock) {
    base.OnMemoryFreed(memoryBlock);
    if (AllocatedMemory.Count == 0 && !IsDead) {
      TaskManager.KillProcess(this);
      Global.Services.Get<ScoreTracker>().VirusKilled();
    }
  }
}
