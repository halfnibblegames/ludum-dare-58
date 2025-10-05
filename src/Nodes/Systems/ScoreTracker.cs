namespace HalfNibbleGame.Nodes.Systems;

public sealed class ScoreTracker {
  public int Score { get; private set; }
  public string ScoreNotice { get; private set; } = "";

  public void MemoryFreed() {
    Score += 5;
  }

  public void VirusKilled() {
    Score += 30;
    ScoreNotice = "Virus killed";
  }

  public void CycleCompleted() {
    Score += 20;
    ScoreNotice = "";
  }

  public void PerfectCycleCompleted() {
    Score += 100;
    ScoreNotice = "Perfect!";
  }
}
