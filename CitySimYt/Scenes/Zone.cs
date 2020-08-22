using Godot;
using System;

public class Zone : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	public EnumZoneTypes ZoneType {get;set;}
	public int MaxPeople { get; set; }
	public int ID { get; set; } = 0;
	public decimal Cost { get; set; } = 0;
	public decimal LandValue { get; set; } = 1000;
	public int RawMatNeedPerMonth { get; set; } = 0;
	public int FinishedGoodsProducedPerMonth { get; set; } = 0;
	public int PollutionLevel { get; set; } = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	public virtual bool KillMe()
	{
		this.QueueFree();
		return true;
	}
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

public enum EnumZoneTypes
{
	Residential,
	Business,
	Industry
}
