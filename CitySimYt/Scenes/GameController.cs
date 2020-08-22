using Godot;
using System;
using System.Collections.Generic;

public class GameController : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	[Signal]
	public delegate void UpdateClock(string newTime);

	[Export]
	public PackedScene Ground;

	[Export]
	public PackedScene ResidentialZoneScene;

	[Export]
	public PackedScene BusinessZoneScene;

	[Export]
	public PackedScene IndustryZoneScene;

	[Export]
	public PackedScene RoadScene;

	[Export]
	public PackedScene UtilStationScene;

	[Export]
	public PackedScene PoliceStationScene;

	[Export]
	public PackedScene FireStationScene;

	[Export]
	public PackedScene ParkStationScene;


	[Export]
	public Vector2 GridSize;

	[Export]
	public Vector2 GridPosition;

	[Export]
	public float GameSpeed = 5000; //1000 is normal speed

	public DateTime GameTime = DateTime.Parse("01/01/2000");

	private Vector2 _screenSize;

	private Sprite SelectRect;

	private float _elapsedGameTimeSinceUpdate;
	public bool IsMouseOverHUD { get; set; } = false;

	private bool ZoneMode = false;
	private bool IsZoning = false;

	private bool BuildMode = false;
	private bool IsBuilding = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GridSize = new Vector2(32,32);
		SelectRect = GetNode<Sprite>("Mouse");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
	  SelectRect.Position = CalculateGridMovement();
	  UpdateTime(delta);
	}

	public bool UpdateTime(float deltaTime)
	{
		_elapsedGameTimeSinceUpdate += deltaTime * GameSpeed;

		if (TimeSpan.FromMilliseconds(_elapsedGameTimeSinceUpdate) >= TimeSpan.FromMinutes(1))
		{
			GameTime = GameTime.AddMonths(1);
			_elapsedGameTimeSinceUpdate = 0;
			EmitSignal("UpdateClock", GameTime.ToString());
		}

		return true;
	}

	private Vector2 CalculateGridMovement()
	{
		var x = (float)Math.Round(GetLocalMousePosition().x / GridSize.x);
		var y = (float)Math.Round(GetLocalMousePosition().y / GridSize.y);

		var newPos = new Vector2(x,y);

		if (newPos == GridPosition)
		{
			return GridPosition * GridSize;
		}

		GridPosition = newPos;
		return GridPosition * GridSize;
	}
}
