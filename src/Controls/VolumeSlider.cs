using Godot;

namespace HalfNibbleGame.Controls;

[Tool]
public sealed partial class VolumeSlider : Control {
  private int busIndex;
  private double volume;
  private Slider? slider;

  [Export(PropertyHint.Range, "0,1,0.05")]
  public double Volume {
    get => volume;
    set {
      volume = value;
      if (slider is not null) {
        slider.Value = volume;
      }
    }
  }

  // Called when the node enters the scene tree for the first time.
  public override void _EnterTree() {
    base._EnterTree();

    ChildEnteredTree += onChildAdded;
    ChildExitingTree += onChildRemoved;
  }

  public override void _ExitTree() {
    ChildEnteredTree -= onChildAdded;
    ChildExitingTree -= onChildRemoved;

    base._ExitTree();
  }

  public override void _Ready() {
    base._Ready();
    busIndex = AudioServer.GetBusIndex("Master");
    volume = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(busIndex));
  }

  private void onChildAdded(Node child) {
    if (child is not Slider s) return;
    if (slider is not null) GD.PushWarning("Second slider added as child to volume slider. Ignoring.");

    setSlider(s);
  }

  private void setSlider(Slider s) {
    slider = s;
    slider.Value = volume;
    slider.ValueChanged += onSliderValueChanged;

    if (Engine.IsEditorHint()) UpdateConfigurationWarnings();
  }

  private void onChildRemoved(Node child) {
    if (child != slider) return;

    unsetSlider();
  }

  private void unsetSlider() {
    if (slider is null) return;
    slider.ValueChanged -= onSliderValueChanged;
    slider = null;

    if (Engine.IsEditorHint()) UpdateConfigurationWarnings();
  }

  private void onSliderValueChanged(double newValue) {
    volume = newValue;
    AudioServer.SetBusVolumeDb(busIndex, (float) Mathf.LinearToDb(newValue));
  }

  public override string[] _GetConfigurationWarnings() {
    return slider is null ? ["Volume slider requires a slider as child to work."] : [];
  }
}
