using Godot;

public partial class Projectile : Area2D
{
	public const float Speed = 300.0f;

	public Vector2 Direction { get; set; } = Vector2.Zero;
	public Color BulletColor { get; set; } = Colors.White;
	public int Damage { get; set; } = 1;

	private float _lifetime = 4.0f;

	public override void _Ready()
	{
		Modulate = BulletColor;
		BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		Position += Direction * Speed * (float)delta;

		_lifetime -= (float)delta;
		if (_lifetime <= 0)
			QueueFree();
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body.HasMethod("TakeDamage"))
			body.Call("TakeDamage", Damage);

		QueueFree();
	}
}
