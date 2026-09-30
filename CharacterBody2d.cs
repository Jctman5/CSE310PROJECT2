using Godot;
using System;

public partial class CharacterBody2d : CharacterBody2D
{
	private void Restart() => GetTree().ReloadCurrentScene();
	public const float Speed = 300.0f;
	public const float JumpVelocity = -400.0f;
	[Export] 
	public PackedScene ProjectileScene { get; set; }
	[Export] public int Health { get; set; } = 3;
	public void ShootProjectile()
	{
	Projectile projectile = ProjectileScene.Instantiate<Projectile>();
	projectile.Direction = GlobalPosition.DirectionTo(GetGlobalMousePosition());
	projectile.BulletColor = Colors.Blue;
	projectile.CollisionMask = 1 | 4; // hits walls + enemies
	GetTree().CurrentScene.AddChild(projectile);
	projectile.GlobalPosition = GlobalPosition;
	}
	public override void _Ready()
	{
		Global.Player = this; // Register this instance globally
		AddToGroup("player");
	}
	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		
		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("left", "right", "up", "down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Y = direction.Y * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Y = Mathf.MoveToward(Velocity.Y, 0, Speed);
		}
		if (Input.IsActionJustPressed("shoot"))
		{
			ShootProjectile();
			GD.Print("Shoot");
		}
		Velocity = velocity;
		MoveAndSlide();
	}
	public void TakeDamage(int amount)
	{
		Health -= amount;
		if (Health <= 0)
		{
			Global.Floor = 1;
			Room.Leaving = true; // stops old rooms reacting to freed enemies
			CallDeferred(nameof(Restart));
		}
	}
}
