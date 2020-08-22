using Godot;
using System;
using System.Globalization;

public class HUD : CanvasLayer
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	[Signal]
	public delegate void PauseGameTime ();

	[Signal]
	public delegate void AddResidentialZone ();

	[Signal]
	public delegate void AddBusinessZone ();

	[Signal]
	public delegate void AddIndustryZone ();

	[Signal]
	public delegate void BulldozeAction ();

	[Signal]
	public delegate void RoadAction ();

	[Signal]
	public delegate void PoliceAction ();

	[Signal]
	public delegate void FireAction ();

	[Signal]
	public delegate void UtilAction ();

	[Signal]
	public delegate void ParkAction ();

	[Signal]
	public delegate void MouseOver ();

	[Signal]
	public delegate void MouseLeave ();

	private Label timeLabel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready ()
	{
		timeLabel = GetNode<Label> ("CalendarText");
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }

	private void _on_GameController_UpdateClock (string newTime)
	{
		timeLabel.Text = DateTimeFormatInfo.CurrentInfo.GetMonthName (DateTime.Parse (newTime).Month) + ", " + DateTime.Parse (newTime).Year;
	}

	private void _on_TopBar_mouse_entered ()
	{
		EmitSignal ("MouseOver");
	}

	private void _on_TopBar_mouse_exited ()
	{
		EmitSignal ("MouseLeave");
	}

	private void _on_ActionPanel_BulldozePanelClick ()
	{
		EmitSignal ("BulldozeAction");
	}

	private void _on_ActionPanel_BusinessZonePanelClick ()
	{
		EmitSignal ("AddBusinessZone");
	}

	private void _on_ActionPanel_FirePanelClick ()
	{
		EmitSignal ("FireAction");
	}

	private void _on_ActionPanel_IndustryZonePanelClick ()
	{
		EmitSignal ("AddIndustryZone");
	}

	private void _on_ActionPanel_MouseLeave ()
	{
		EmitSignal ("MouseLeave");
	}

	private void _on_ActionPanel_MouseOver ()
	{
		EmitSignal ("MouseOver");
	}

	private void _on_ActionPanel_ParkPanelClick ()
	{
		EmitSignal ("ParkAction");
	}

	private void _on_ActionPanel_PolicePanelClick ()
	{
		EmitSignal ("PoliceAction");
	}

	private void _on_ActionPanel_ResidentialZonePanelClick ()
	{
		EmitSignal ("AddResidentialZone");
	}

	private void _on_ActionPanel_RoadPanelClick ()
	{
		EmitSignal ("RoadAction");
	}

	private void _on_ActionPanel_UtilPanelClick ()
	{
		EmitSignal ("UtilAction");
	}
}
