using Godot;

public partial class Enemy : CharacterBody2D
{
	[Export] public PackedScene ProjectileScene { get; set; }
	[Export] public float Speed { get; set; } = 100.0f;
	[Export] public float FireInterval { get; set; } = 2.0f;
	[Export] public int Health { get; set; } = 3;

	private Node2D _player;
	private float _fireTimer;

	public override void _Ready()
	{
		_fireTimer = FireInterval;
		// Swap this line for however your Global.cs exposes the player
		_player = GetTree().GetFirstNodeInGroup("player") as Node2D;
		AddToGroup("enemy");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!IsInstanceValid(_player)) return;

		Velocity = GlobalPosition.DirectionTo(_player.GlobalPosition) * Speed;
		MoveAndSlide();

		_fireTimer -= (float)delta;
		if (_fireTimer <= 0)
		{
			Shoot();
			_fireTimer = FireInterval;
		}
	}

	private void Shoot()
	{
		Projectile projectile = ProjectileScene.Instantiate<Projectile>();
		projectile.Direction = GlobalPosition.DirectionTo(_player.GlobalPosition);
		projectile.BulletColor = Colors.Red;
		projectile.CollisionMask = 1 | 2; // hits walls + player
		GetTree().CurrentScene.AddChild(projectile);
		projectile.GlobalPosition = GlobalPosition;
	}

	public void TakeDamage(int amount)
	{
		Health -= amount;
		if (Health <= 0)
			QueueFree();
	}
}
