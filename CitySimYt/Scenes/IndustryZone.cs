using Godot;
using System;

public class IndustryZone : Zone
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ZoneType = EnumZoneTypes.Industry;
		Cost = 100;
		LandValue = 2500;
		RawMatNeedPerMonth = 0;
		FinishedGoodsProducedPerMonth = 7;
		PollutionLevel = 5;
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}


