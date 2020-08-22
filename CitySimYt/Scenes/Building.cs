using Godot;
using System;

public class Building : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	public int MaxWorkers { get; set; }
	public int ID { get; set; } = 0;
	public EnumBuildingTypes BuildingType {get;set;}
	public decimal Cost { get; set; } = 0;
	public int AreaOfAffect { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}

public enum EnumBuildingTypes
{
	Police,
	Road,
	Util,
	Park,
	Fire
}
