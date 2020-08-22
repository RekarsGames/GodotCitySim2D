using Godot;
using System;

public class ActionPanel : CanvasLayer
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	[Signal]
	public delegate void ResidentialZonePanelClick ();

	[Signal]
	public delegate void BusinessZonePanelClick ();

	[Signal]
	public delegate void IndustryZonePanelClick ();

	[Signal]
	public delegate void BulldozePanelClick ();

	[Signal]
	public delegate void RoadPanelClick ();

	[Signal]
	public delegate void PolicePanelClick ();

	[Signal]
	public delegate void FirePanelClick ();

	[Signal]
	public delegate void UtilPanelClick ();

	[Signal]
	public delegate void ParkPanelClick ();

	[Signal]
	public delegate void MouseOver ();

	[Signal]
	public delegate void MouseLeave ();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready ()
	{

	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }

	private void _on_btnResidential_pressed ()
	{
		EmitSignal ("ResidentialZonePanelClick");
	}

	private void _on_btnBusiness_pressed ()
	{
		EmitSignal ("BusinessZonePanelClick");
	}

	private void _on_btnIndustry_pressed ()
	{
		EmitSignal ("IndustryZonePanelClick");
	}

	private void _on_btnRoad_pressed ()
	{
		EmitSignal ("RoadPanelClick");
	}

	private void _on_btnPolice_pressed ()
	{
		EmitSignal ("PolicePanelClick");
	}

	private void _on_btnFire_pressed ()
	{
		EmitSignal ("FirePanelClick");
	}

	private void _on_btnUtil_pressed ()
	{
		EmitSignal ("UtilPanelClick");
	}

	private void _on_btnPark_pressed ()
	{
		EmitSignal ("ParkPanelClick");
	}

	private void _on_btnDemolish_pressed ()
	{
		EmitSignal ("BulldozePanelClick");
	}

	private void _on_Sidebar_mouse_entered ()
	{
		EmitSignal ("MouseOver");
	}

	private void _on_Sidebar_mouse_exited ()
	{
		EmitSignal ("MouseLeave");
	}
}
