using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class GameController : Node2D
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";
	[Signal]
	public delegate void UpdateClock (string newTime);

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

	public DateTime GameTime = DateTime.Parse ("01/01/2000");

	private Vector2 _screenSize;

	private Sprite SelectRect;

	private float _elapsedGameTimeSinceUpdate;
	public bool IsMouseOverHUD { get; set; } = false;

	////
	//Zoning related variables
	private bool ZoneMode = false;
	private bool IsZoning;
	private EnumZoneTypes zoneType;
	private Zone _zone;
	public List<Zone> Zones { get; set; }
	public List<Node> TempZoneArea { get; set; }
	public Vector2 TempStartPositonZone { get; set; }
	public Vector2 TempLastEndPositonZone { get; set; }
	////

	////
	//Building related variables
	private bool BuildMode = false;
	private bool IsBuilding;
	private Building _building;
	public List<Building> Buildings { get; set; }
	private EnumBuildingTypes buildingType;
	////

	////
	//Road related variables
	private bool RoadMode = false;
	private bool IsLayingRoad;
	private Road _road;
	public List<Road> Roads { get; set; }
	////

	private bool EraseMode = false;
	private Node SelectedObject;
	private Node ObjectUnderMouse;
	public List<Node> BuildQueue { get; set; } = new List<Node> ();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready ()
	{
		//Connect a large portion of HUD signals
		var n = GetNode<CanvasLayer> ("../CanvasLayer/HUD");
		n.Connect ("FireAction", this, nameof (OnFireAction));
		n.Connect ("PoliceAction", this, nameof (OnPoliceAction));
		n.Connect ("RoadAction", this, nameof (OnRoadAction));
		n.Connect ("ParkAction", this, nameof (OnParkAction));
		n.Connect ("UtilAction", this, nameof (OnUtilAction));
		n.Connect ("AddBusinessZone", this, nameof (OnBusinessAction));
		n.Connect ("AddIndustryZone", this, nameof (OnIndustryAction));
		n.Connect ("AddResidentialZone", this, nameof (OnResidentialAction));
		n.Connect ("BulldozeAction", this, nameof (OnBulldozeAction));
		n.Connect ("MouseLeave", this, nameof (OnMouseLeave));
		n.Connect ("MouseOver", this, nameof (OnMouseOver));

		Zones = new List<Zone> ();
		Buildings = new List<Building> ();
		Roads = new List<Road> ();

		GridSize = new Vector2 (32, 32);
		SelectRect = GetNode<Sprite> ("Mouse");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process (float delta)
	{
		SelectRect.Position = CalculateGridMovement ();
		UpdateTime (delta);
		ObjectUnderMouse = GetObjectUnderMouse ();

		CompleteQueueItem ();

		if (!IsMouseOverHUD)
		{
			//Select an object under the mouse
			if (!EraseMode && !BuildMode && !ZoneMode && Input.IsActionJustPressed ("leftclick"))
			{
				try
				{
					if (ObjectUnderMouse is Zone _zone)
					{
						var _z = Zones.SingleOrDefault (a => a.ID == _zone.ID);
						SelectedObject = _z;
					}
					else if (ObjectUnderMouse is Building _building)
					{
						var _b = Buildings.SingleOrDefault (a => a.ID == _building.ID);
						SelectedObject = _b;
					}
				}
				catch (System.Exception)
				{
					SelectedObject = null;
				}
			}

			//Start Zoning if not already
			if (!IsZoning && ZoneMode && Input.IsActionPressed ("leftclick"))
			{
				IsZoning = true;
				TempStartPositonZone = SelectRect.Position;
			}

			//If build mode, build selected building type
			if (BuildMode && !IsBuilding && Input.IsActionJustPressed ("leftclick"))
			{
				IsBuilding = true;
				Node _b = new Node ();

				if (buildingType == EnumBuildingTypes.Fire)
				{
					_b = FireStationScene.Instance ();
				}
				else if (buildingType == EnumBuildingTypes.Park)
				{
					_b = ParkStationScene.Instance ();
				}
				else if (buildingType == EnumBuildingTypes.Police)
				{
					_b = PoliceStationScene.Instance ();
				}
				else if (buildingType == EnumBuildingTypes.Road)
				{
					_b = RoadScene.Instance ();
				}
				else if (buildingType == EnumBuildingTypes.Util)
				{
					_b = UtilStationScene.Instance ();
				}

				Build (_b, SelectRect.Position);

				BuildMode = false;
				IsBuilding = false;

				_building.QueueFree ();
				_building = null;
			}

			//Lay road if not already
			if (RoadMode && !IsLayingRoad && Input.IsActionJustPressed ("leftclick"))
			{
				IsLayingRoad = true;
				var _r = RoadScene.Instance();
				Build(_r, SelectRect.Position);

				RoadMode = false;
				IsLayingRoad = false;

				_road.QueueFree();
				_road = null;
			}

			if (ZoneMode && _zone != null)
			{
				_zone.Position = SelectRect.Position;
			}

			if (BuildMode && _building != null)
			{
				_building.Position = SelectRect.Position;
			}

			if (RoadMode && _road != null)
			{
				_road.Position = SelectRect.Position;
			}

			if (ZoneMode && Input.IsActionPressed("rightclick"))
			{
				ZoneMode = false;
				IsZoning = false;
				_zone.QueueFree();
				_zone = null;

				if (TempZoneArea != null && TempZoneArea.Count() > 0)
				{
					for (int i = TempZoneArea.Count - 1; i >= 0 ; i--)
					{
						TempZoneArea[i].Free();
						TempZoneArea.RemoveAt(i);
					}
				}
			}

			if (IsZoning && Input.IsActionPressed("leftclick"))
			{
				if (zoneType == EnumZoneTypes.Residential)
				{
					if (TempLastEndPositonZone != SelectRect.Position)
					{
						if (TempZoneArea == null)
						{
							TempZoneArea = new List<Node>();
						}
						if (TempZoneArea.Count > 0)
						{
							for (int i = TempZoneArea.Count - 1; i >= 0 ; i--)
							{
								TempZoneArea[i].Free();
								TempZoneArea.RemoveAt(i);
							}
						}

						ZoneArea(TempStartPositonZone, SelectRect.Position, EnumZoneTypes.Residential);

						TempLastEndPositonZone = SelectRect.Position;
					}
				}
				else if (zoneType == EnumZoneTypes.Business)
				{
					if (TempLastEndPositonZone != SelectRect.Position)
					{
						if (TempZoneArea == null)
						{
							TempZoneArea = new List<Node>();
						}
						if (TempZoneArea.Count > 0)
						{
							for (int i = TempZoneArea.Count - 1; i >= 0 ; i--)
							{
								TempZoneArea[i].Free();
								TempZoneArea.RemoveAt(i);
							}
						}

						ZoneArea(TempStartPositonZone, SelectRect.Position, EnumZoneTypes.Business);

						TempLastEndPositonZone = SelectRect.Position;
					}
				}
				else if (zoneType == EnumZoneTypes.Industry)
				{
					if (TempLastEndPositonZone != SelectRect.Position)
					{
						if (TempZoneArea == null)
						{
							TempZoneArea = new List<Node>();
						}
						if (TempZoneArea.Count > 0)
						{
							for (int i = TempZoneArea.Count - 1; i >= 0 ; i--)
							{
								TempZoneArea[i].Free();
								TempZoneArea.RemoveAt(i);
							}
						}

						ZoneArea(TempStartPositonZone, SelectRect.Position, EnumZoneTypes.Industry);

						TempLastEndPositonZone = SelectRect.Position;
					}
				}
			}

			if (IsZoning && Input.IsActionJustReleased("leftclick"))
			{
				if (TempZoneArea.Count > 0)
				{
					for (int i = TempZoneArea.Count - 1; i >= 0 ; i--)
					{
						var temp = TempZoneArea[i];
						if (temp is ResidentialZone)
						{
							var zone = new ResidentialZone();
							zone.Position = (temp as ResidentialZone).Position;
							zone.ZoneType = EnumZoneTypes.Residential;
							BuildQueue.Add(zone);
						}
						else if (temp is BusinessZone)
						{
							var zone = new BusinessZone();
							zone.Position = (temp as BusinessZone).Position;
							zone.ZoneType = EnumZoneTypes.Residential;
							BuildQueue.Add(zone);
						}
						else if (temp is IndustryZone)
						{
							var zone = new IndustryZone();
							zone.Position = (temp as IndustryZone).Position;
							zone.ZoneType = EnumZoneTypes.Residential;
							BuildQueue.Add(zone);
						}
						TempZoneArea[i].Free();
						TempZoneArea.RemoveAt(i);
					}
				}
				ZoneMode = false;
				IsZoning = false;

				_zone.QueueFree();
				_zone = null;
			}

			if (EraseMode && Input.IsActionPressed("leftclick"))
			{
				if (ObjectUnderMouse != null)
				{
					try
					{
						DeleteObject(ObjectUnderMouse);
						SelectedObject = null;
						EraseMode = false;
					}
					catch (System.Exception)
					{
						EraseMode = false;
					}
				}
			}
		}
	}

	private Node GetObjectUnderMouse ()
	{
		var currentLoc = SelectRect.Position;

		if (currentLoc != null)
		{
			var zoneUnder = Zones.FirstOrDefault (a => a.Position == currentLoc);
			var buildingUnder = Buildings.FirstOrDefault (a => a.Position == currentLoc);
			var roadUnder = Roads.FirstOrDefault (a => a.Position == currentLoc);
			if (zoneUnder != null)
			{
				return zoneUnder as Node;
			}
			else if (buildingUnder != null)
			{
				return buildingUnder as Node;
			}
			else if (roadUnder != null)
			{
				return roadUnder as Node;
			}
			else
			{
				return null;
			}
		}
		else
		{
			return null;
		}
	}

	private async Task<bool> CompleteQueueItem ()
	{
		var item = BuildQueue.LastOrDefault ();
		if (item != null)
		{
			if (item is Zone)
			{
				if (item is ResidentialZone)
				{
					var zone = ResidentialZoneScene.Instance ();
					GD.Print (((Zone) item).Position);
					await AddZone (zone, ((Zone) item).Position);
					BuildQueue.Remove (item);
				}
				else if (item is BusinessZone)
				{
					var zone = BusinessZoneScene.Instance ();
					GD.Print (((Zone) item).Position);
					await AddZone (zone, ((Zone) item).Position);
					BuildQueue.Remove (item);
				}
				else if (item is IndustryZone)
				{
					var zone = IndustryZoneScene.Instance ();
					GD.Print (((Zone) item).Position);
					await AddZone (zone, ((Zone) item).Position);
					BuildQueue.Remove (item);
				}
			}
		}
		return await Task.FromResult (true).ConfigureAwait (false);
	}

	private async void ZoneArea (Vector2 StartLocation, Vector2 EndLocation, EnumZoneTypes Type)
	{
		var NumX = Mathf.Abs (StartLocation.x - EndLocation.x) / GridSize.x;
		var NumY = Mathf.Abs (StartLocation.y - EndLocation.y) / GridSize.y;
		for (int i = 0; i < NumX + 1; i++)
		{
			for (int ii = 0; ii < NumY + 1; ii++)
			{
				Vector2 pos = new Vector2 ();
				float thisX;
				float thisY;

				if (StartLocation.x > EndLocation.x)
				{
					thisX = StartLocation.x - (GridSize.x * i);
				}
				else
				{
					thisX = (GridSize.x * i) + StartLocation.x;
				}

				if (StartLocation.y > EndLocation.y)
				{
					thisY = StartLocation.y - (GridSize.y * ii);
				}
				else
				{
					thisY = (GridSize.y * ii) + StartLocation.y;
				}

				pos = new Vector2 (thisX, thisY);

				await AddTempZone (pos, Type).ConfigureAwait (false);
			}
		}
	}

	private async Task<bool> AddTempZone (Vector2 position, EnumZoneTypes Type)
	{
		if (Type == EnumZoneTypes.Residential)
		{
			var zone = ResidentialZoneScene.Instance ();
			Node z1 = zone as Node;
			CallDeferred ("add_child", zone);
			var _z = z1 as ResidentialZone;
			_z.Position = position;
			TempZoneArea.Add (_z);
		}
		else if (Type == EnumZoneTypes.Business)
		{
			var zone = BusinessZoneScene.Instance ();
			Node z1 = zone as Node;
			CallDeferred ("add_child", zone as Node);
			var _z = z1 as BusinessZone;
			_z.Position = position;
			TempZoneArea.Add (_z);
		}
		else if (Type == EnumZoneTypes.Industry)
		{
			var zone = IndustryZoneScene.Instance ();
			Node z1 = zone as Node;
			CallDeferred ("add_child", zone as Node);
			var _z = z1 as IndustryZone;
			_z.Position = position;
			TempZoneArea.Add (_z);
		}

		return await Task.FromResult (true).ConfigureAwait (false);
	}

	private async Task<bool> GetUnbuildableTile (Vector2 position)
	{
		var tMap = GetParent ().GetNode<TileMap> ("TileMap");
		var mapLoc = tMap.WorldToMap (position);
		var tileIndex = tMap.GetCellv (mapLoc);

		if (tileIndex == 2)
		{
			return await Task.FromResult (false).ConfigureAwait (false);
		}
		else
		{
			return await Task.FromResult (true).ConfigureAwait (false);
		}
	}

	private bool Build<T> (T building, Vector2 Location)
	{
		var existingZone = Zones.FirstOrDefault (a => a.Position == Location);
		var existingBuilding = Buildings.FirstOrDefault (a => a.Position == Location);
		var existingRoad = Roads.FirstOrDefault (a => a.Position == Location);
		if (existingZone != null || existingBuilding != null)
		{
			if (existingZone != null)
			{
				DeleteObject (existingZone);
			}
			else if (existingBuilding != null)
			{
				DeleteObject (existingBuilding);
			}
			else if (existingRoad != null)
			{
				DeleteObject (existingRoad);
			}
		}

		if (building is PoliceStation)
		{
			Node z1 = building as Node;
			AddChild (z1);
			var _z = z1 as PoliceStation;
			_z.Position = Location;
			_z.BuildingType = EnumBuildingTypes.Police;
			_z.ID = (Buildings.Count () < 1) ? 1 : Buildings.Select (a => a.ID).DefaultIfEmpty (0).Max () + 1;
			Buildings.Add (_z);
		}
		else if (building is UtilityStation)
		{
			Node z1 = building as Node;
			AddChild (building as Node);
			var _z = z1 as UtilityStation;
			_z.Position = Location;
			_z.BuildingType = EnumBuildingTypes.Util;
			_z.ID = (Buildings.Count () < 1) ? 1 : Buildings.Select (a => a.ID).DefaultIfEmpty (0).Max () + 1;
			Buildings.Add (_z);
		}
		else if (building is FireStation)
		{
			Node z1 = building as Node;
			AddChild (building as Node);
			var _z = z1 as FireStation;
			_z.Position = Location;
			_z.BuildingType = EnumBuildingTypes.Fire;
			_z.ID = (Buildings.Count () < 1) ? 1 : Buildings.Select (a => a.ID).DefaultIfEmpty (0).Max () + 1;
			Buildings.Add (_z);
		}
		else if (building is Park)
		{
			Node z1 = building as Node;
			AddChild (building as Node);
			var _z = z1 as Park;
			_z.Position = Location;
			_z.BuildingType = EnumBuildingTypes.Park;
			_z.ID = (Buildings.Count () < 1) ? 1 : Buildings.Select (a => a.ID).DefaultIfEmpty (0).Max () + 1;
			Buildings.Add (_z);
		}
		else if (building is Road)
		{
			Node z1 = building as Node;
			AddChild (building as Node);
			var _z = z1 as Road;
			_z.Position = Location;
			_z.ID = (Roads.Count () < 1) ? 1 : Roads.Select (a => a.ID).DefaultIfEmpty (0).Max () + 1;
			Roads.Add (_z);
		}
		return true;
	}

	private async Task<bool> AddZone<T> (T zone, Vector2 Location)
	{
		var existingZone = Zones.FirstOrDefault (a => a.Position == Location);
		var existingBuilding = Buildings.FirstOrDefault (a => a.Position == Location);
		var existingRoad = Roads.FirstOrDefault (a => a.Position == Location);
		if (existingZone != null || existingBuilding != null)
		{
			if (existingZone != null)
			{
				DeleteObject (existingZone);
			}
			else if (existingBuilding != null)
			{
				DeleteObject (existingBuilding);
			}
			else if (existingRoad != null)
			{
				DeleteObject (existingRoad);
			}
		}

		if (zone is ResidentialZone)
		{
			Node z1 = zone as Node;
			CallDeferred ("add_child", zone as Node);
			var _z = z1 as ResidentialZone;
			_z.Position = Location;
			_z.ID = (Zones.Count () < 1) ? 1 : Zones.Select (a => a.ID).DefaultIfEmpty (0).Max () + 1;
			Zones.Add (_z);
		}
		else if (zone is BusinessZone)
		{
			Node z1 = zone as Node;
			CallDeferred ("add_child", zone as Node);
			var _z = z1 as BusinessZone;
			_z.Position = Location;
			_z.ID = (Zones.Count () < 1) ? 1 : Zones.Select (a => a.ID).DefaultIfEmpty (0).Max () + 1;
			Zones.Add (_z);
		}
		else if (zone is IndustryZone)
		{
			Node z1 = zone as Node;
			CallDeferred ("add_child", zone as Node);
			var _z = z1 as IndustryZone;
			_z.Position = Location;
			_z.ID = (Zones.Count () < 1) ? 1 : Zones.Select (a => a.ID).DefaultIfEmpty (0).Max () + 1;
			Zones.Add (_z);
		}
		return await Task.FromResult (true).ConfigureAwait (false);
	}
	private bool DeleteObject<T> (T node)
	{
		if (node is Zone zone)
		{
			try
			{
				var _z = Zones.SingleOrDefault (a => a.ID == zone.ID);

				_z.KillMe ();

				RemoveChild (zone);
				Zones.Remove (_z);

				return true;
			}
			catch (System.Exception)
			{
				return false;
			}
		}
		else if (node is Building building)
		{
			try
			{
				var _z = Buildings.SingleOrDefault (a => a.Position == building.Position);

				_z.QueueFree ();

				RemoveChild (building);
				Buildings.Remove (_z);

				return true;
			}
			catch (System.Exception)
			{
				return false;
			}
		}
		else if (node is Road road)
		{
			try
			{
				var _z = Roads.SingleOrDefault (a => a.Position == road.Position);

				_z.QueueFree ();

				RemoveChild (road);
				Roads.Remove (_z);

				return true;
			}
			catch (System.Exception)
			{
				return false;
			}
		}
		else
		{
			return false;
		}
	}
	public bool UpdateTime (float deltaTime)
	{
		_elapsedGameTimeSinceUpdate += deltaTime * GameSpeed;

		if (TimeSpan.FromMilliseconds (_elapsedGameTimeSinceUpdate) >= TimeSpan.FromMinutes (1))
		{
			GameTime = GameTime.AddMonths (1);
			_elapsedGameTimeSinceUpdate = 0;
			EmitSignal ("UpdateClock", GameTime.ToString ());
		}

		return true;
	}

	private Vector2 CalculateGridMovement ()
	{
		var x = (float) Math.Round (GetLocalMousePosition ().x / GridSize.x);
		var y = (float) Math.Round (GetLocalMousePosition ().y / GridSize.y);

		var newPos = new Vector2 (x, y);

		if (newPos == GridPosition)
		{
			return GridPosition * GridSize;
		}

		GridPosition = newPos;
		return GridPosition * GridSize;
	}

	private void OnRoadAction ()
	{
		if (_road != null)
		{
			_road.QueueFree ();
			_road = null;
		}

		RoadMode = true;
		var z = RoadScene.Instance ();
		AddChild (z);
		_road = z as Road;
	}

	private void OnPoliceAction ()
	{
		if (_building != null)
		{
			_building.QueueFree ();
			_building = null;
		}

		BuildMode = true;
		buildingType = EnumBuildingTypes.Police;
		var z = PoliceStationScene.Instance ();
		AddChild (z);
		_building = z as PoliceStation;
	}

	private void OnFireAction ()
	{
		if (_building != null)
		{
			_building.QueueFree ();
			_building = null;
		}

		BuildMode = true;
		buildingType = EnumBuildingTypes.Fire;
		var z = FireStationScene.Instance ();
		AddChild (z);
		_building = z as FireStation;
	}

	private void OnUtilAction ()
	{
		if (_building != null)
		{
			_building.QueueFree ();
			_building = null;
		}

		BuildMode = true;
		buildingType = EnumBuildingTypes.Util;
		var z = UtilStationScene.Instance ();
		AddChild (z);
		_building = z as UtilityStation;
	}

	private void OnParkAction ()
	{
		if (_building != null)
		{
			_building.QueueFree ();
			_building = null;
		}

		BuildMode = true;
		buildingType = EnumBuildingTypes.Park;
		var z = ParkStationScene.Instance ();
		AddChild (z);
		_building = z as Park;
	}

	private void OnBusinessAction ()
	{
		if (_zone != null)
		{
			_zone.QueueFree ();
			_zone = null;
		}

		ZoneMode = true;
		zoneType = EnumZoneTypes.Business;
		var z = BusinessZoneScene.Instance ();
		AddChild (z);
		_zone = z as BusinessZone;
	}

	private void OnIndustryAction ()
	{
		if (_zone != null)
		{
			_zone.QueueFree ();
			_zone = null;
		}

		ZoneMode = true;
		zoneType = EnumZoneTypes.Industry;
		var z = IndustryZoneScene.Instance ();
		AddChild (z);
		_zone = z as IndustryZone;
	}

	private void OnResidentialAction ()
	{
		if (_zone != null)
		{
			_zone.QueueFree ();
			_zone = null;
		}
		ZoneMode = true;
		zoneType = EnumZoneTypes.Residential;
		var z = ResidentialZoneScene.Instance ();
		AddChild (z);
		_zone = z as ResidentialZone;
	}

	private void OnMouseOver ()
	{
		IsMouseOverHUD = true;
	}

	private void OnMouseLeave ()
	{
		IsMouseOverHUD = false;
	}

	private void OnBulldozeAction ()
	{
		if (_zone != null)
		{
			_zone.QueueFree ();
			_zone = null;
		}
		ZoneMode = false;
		IsZoning = false;
		EraseMode = true;
	}
}
