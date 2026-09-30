using Godot;
using System.Collections.Generic;
public partial class Room : Node2D
{
	[Export] public PackedScene[] EnemyScenes { get; set; }
	[Export] public Area2D EntryTrigger { get; set; }

	private int _enemiesAlive;
	private bool _started;
	public bool IsExit { get; set; }
	public static bool Leaving { get; set; }

	private Area2D _portal;
	public override void _Ready()
	{
		EntryTrigger.BodyEntered += OnBodyEntered;
		SetDoorsLocked(false);
		Leaving = false;
		if (IsExit) CreatePortal();
	}

private void OnBodyEntered(Node2D body)
	{
		if (!body.IsInGroup("player")) return;

		// Always snap, even for rooms already cleared
		var cam = GetViewport().GetCamera2D();
		if (cam != null)
		cam.GlobalPosition = GlobalPosition;

		// Only start the fight once
		if (_started) return;
		_started = true;

		if (GetNode("Spawns").GetChildCount() == 0) return;
		CallDeferred(nameof(StartFight));
	}
	private void StartFight()
	{
	SetDoorsLocked(true);
	SpawnEnemies();
	}
	private void SpawnEnemies()
	{
		foreach (Node child in GetNode("Spawns").GetChildren())
		{
			if (child is not Marker2D marker) continue;

			var scene = EnemyScenes[GD.Randi() % EnemyScenes.Length];
			var enemy = scene.Instantiate<Enemy>();
			int bonus = Global.Floor - 1;
			enemy.Health += bonus;
			enemy.Speed += 10 * bonus;
			enemy.FireInterval = Mathf.Max(0.6f, enemy.FireInterval - 0.2f * bonus);
			AddChild(enemy);
			enemy.GlobalPosition = marker.GlobalPosition;

			_enemiesAlive++;
			enemy.TreeExited += OnEnemyGone; // fires when QueueFree() removes it
		}
	}

	private void OnEnemyGone()
	{
		if (Leaving) return; // enemies being freed by the scene reload
		_enemiesAlive--;
		if (_enemiesAlive <= 0)
		{
			SetDoorsLocked(false);
			if (IsExit) ShowPortal();
		}
	}

	private static readonly Color WallColor = new Color("2e2e2e"); // match your wall color
	private static readonly Color LockedColor = Colors.White;

	private void SetDoorsLocked(bool locked)
	{
		foreach (Node door in GetNode("Doors").GetChildren())
		{
			if (door is not StaticBody2D body) continue;

			bool isSealed = _sealed.Contains(body.Name.ToString());
			bool closed = locked || isSealed;

			body.Visible = closed;
			body.GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", !closed);

			// Sealed doors look like wall; fight-locked doors stay white
			body.Modulate = isSealed ? WallColor : LockedColor;
		}
	}
	private readonly HashSet<string> _sealed = new();

	// Called by the generator BEFORE the room is added to the tree
	public void SetOpenDoors(bool up, bool down, bool left, bool right)
	{
		_sealed.Clear();
		if (!up) _sealed.Add("Top");
		if (!down) _sealed.Add("Bottom");
		if (!left) _sealed.Add("Left");
		if (!right) _sealed.Add("Right");
	}
	private void CreatePortal()
{
	_portal = new Area2D();
	_portal.CollisionLayer = 0;
	_portal.CollisionMask = 2;   // player's layer (layer 2 = value 2)
	_portal.Monitoring = false;  // off until the room is cleared
	_portal.Visible = false;

	_portal.AddChild(new CollisionShape2D { Shape = new CircleShape2D { Radius = 30 } });
	_portal.AddChild(new Polygon2D
	{
		Color = Colors.Purple,
		Polygon = new Vector2[] { new(-25, -25), new(25, -25), new(25, 25), new(-25, 25) }
	});

	AddChild(_portal);
	_portal.BodyEntered += OnPortalEntered;
}

private void ShowPortal()
{
	if (_portal == null) return;
	_portal.Visible = true;
	_portal.SetDeferred("monitoring", true); // deferred: we're in a physics-related callback
}

private void OnPortalEntered(Node2D body)
{
	if (!body.IsInGroup("player")) return;
	CallDeferred(nameof(NextFloor));
}

private void NextFloor()
{
	Leaving = true;
	Global.Floor++;
	GetTree().ReloadCurrentScene();
}
}
