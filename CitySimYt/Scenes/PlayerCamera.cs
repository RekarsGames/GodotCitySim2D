using Godot;
using System;

public class PlayerCamera : Area2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	[Export]
	public float MoveSpeed = 300;

	[Export]
	public float MoveVectorAmount = 2f;

	private Camera2D _playerCamera;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_playerCamera = (Camera2D)GetNode<Position2D>("Position").GetChild(0);
	}

  	// Called every frame. 'delta' is the elapsed time since the previous frame.
  	public override void _Process(float delta)
 	{
	  var velocity = new Vector2();

	  if (Input.IsActionPressed("ui_right"))
	  {
		  velocity.x += MoveVectorAmount;
	  }
	  if (Input.IsActionPressed("ui_up"))
	  {
		  velocity.y -= MoveVectorAmount;
	  }
	  if (Input.IsActionPressed("ui_down"))
	  {
		  velocity.y += MoveVectorAmount;
	  }
	  if (Input.IsActionPressed("ui_left"))
	  {
		  velocity.x -= MoveVectorAmount;
	  }

	  if (velocity.Length() > 0)
	  {
		  velocity = velocity.Normalized() * MoveSpeed;
	  }

	  Position += velocity * delta;
  	}

	  public override void _UnhandledInput(InputEvent @event)
	  {
		  if (@event is InputEventMouseButton)
		  {
			  InputEventMouseButton emb = (InputEventMouseButton)@event;

			  if (emb.IsPressed())
			  {
				  if (emb.ButtonIndex == (int)ButtonList.WheelUp)
				  {
					  _playerCamera.Zoom -= new Vector2(0.1f,0.1f);
				  }
				  if (emb.ButtonIndex == (int)ButtonList.WheelDown)
				  {
					  _playerCamera.Zoom += new Vector2(0.1f,0.1f);
				  }

				  if (emb.ButtonIndex == (int)ButtonList.Middle)
				  {
					  _playerCamera.Zoom = new Vector2(1f,1f);
				  }

				  _playerCamera.Zoom = new Vector2(x: Mathf.Clamp(_playerCamera.Zoom.x, 0.2f, 25), y: Mathf.Clamp(_playerCamera.Zoom.y, 0.2f,25));
			  }
		  }
	  }
}
