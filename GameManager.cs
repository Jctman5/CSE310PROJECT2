using Godot;

public partial class GameManager : Node
{
	[Export] public PackedScene EnemyScene { get; set; }
	[Export] public float SpawnInterval { get; set; } = 3.0f;
	[Export] public float SpawnDistance { get; set; } = 400.0f;
	[Export] public int MaxEnemies { get; set; } = 10;

	private float _spawnTimer;

	public override void _Ready()
	{
		_spawnTimer = SpawnInterval;
	}

	public override void _Process(double delta)
	{
		_spawnTimer -= (float)delta;
		if (_spawnTimer > 0) return;
		_spawnTimer = SpawnInterval;

		if (GetTree().GetNodesInGroup("enemy").Count >= MaxEnemies) return;

		var player = GetTree().GetFirstNodeInGroup("player") as Node2D;
		if (player == null) return;

		Enemy enemy = EnemyScene.Instantiate<Enemy>();
		GetTree().CurrentScene.AddChild(enemy);

		// Random spot on a circle around the player
		Vector2 offset = Vector2.Right.Rotated((float)GD.RandRange(0, Mathf.Tau)) * SpawnDistance;
		enemy.GlobalPosition = player.GlobalPosition + offset;
	}
}
