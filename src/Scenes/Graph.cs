using System;
using System.Collections.Generic;
using Godot;

namespace HalfNibbleGame.Scenes;

public partial class Graph : Node2D {
  [Export] private int horizontalSteps = 16;
  [Export] private Color graphColor = new(1, 0, 0);

  private readonly List<float> dataPoints = [];

  private Line2D? dataLine;
  private ReferenceRect? reference;

  public override void _Ready() {
    dataLine = GetNode<Line2D>("DataLine");
    reference = GetNode<ReferenceRect>("Reference");
    dataLine.DefaultColor = graphColor;
  }

  public void PushDataPoint(float point) {
    point = Math.Clamp(point, 0, 1);
    dataPoints.Add(point);
    while (dataPoints.Count > horizontalSteps) {
      dataPoints.RemoveAt(0);
    }

    redraw();
  }

  private void redraw() {
    if (dataLine == null || reference == null) {
      GD.PrintErr("Expected data line and reference rect, but found none. Not redrawing graph");
      return;
    }

    var horizontalDiff = reference.Size.X / (horizontalSteps - 1);
    var verticalFactor = reference.Size.Y;

    var points = new Vector2[dataPoints.Count];
    for (var i = 0; i < dataPoints.Count; i++) {
      points[i] = new Vector2(Mathf.Round(horizontalDiff * i), reference.Size.Y - Mathf.Round(verticalFactor * dataPoints[i]));
    }
    dataLine.SetPoints(points);
  }
}
