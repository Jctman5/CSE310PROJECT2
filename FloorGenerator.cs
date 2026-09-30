using Godot;
using System.Collections.Generic;

public partial class FloorGenerator : Node
{
	[Export] public PackedScene StartRoom { get; set; }
	[Export] public PackedScene[] RoomScenes { get; set; }
	[Export] public int RoomCount { get; set; } = 8;
	[Export] public Vector2 RoomSize { get; set; } = new Vector2(1152, 648);

	public override void _Ready()
	{
		if (StartRoom == null || RoomScenes == null || RoomScenes.Length == 0)
		{
			GD.PrintErr("FloorGenerator: assign StartRoom and RoomScenes in the inspector");
			return;
		}

		// Left/right only, because the rooms only have side doorways.
		// Add Up and Down back once your rooms have top/bottom doorways.
		var dirs = new[] { Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right };
		var cells = new List<Vector2I> { Vector2I.Zero };
		var pos = Vector2I.Zero;

		int target = RoomCount + Global.Floor - 1;
		int safety = 0;
		while (cells.Count < target && safety++ < 1000)
		{
			pos += dirs[GD.Randi() % dirs.Length];
			if (!cells.Contains(pos)) cells.Add(pos);
		}

		GD.Print("Floor ", Global.Floor, " RoomCount=", RoomCount, " target=", target, " cells=", cells.Count);

		for (int i = 0; i < cells.Count; i++)
		{
			var scene = i == 0 ? StartRoom : RoomScenes[GD.Randi() % RoomScenes.Length];
			var room = scene.Instantiate<Room>();
			var c = cells[i];

			room.SetOpenDoors(
				cells.Contains(c + Vector2I.Up),
				cells.Contains(c + Vector2I.Down),
				cells.Contains(c + Vector2I.Left),
				cells.Contains(c + Vector2I.Right));
			room.IsExit = i == cells.Count - 1;
			room.Position = new Vector2(c.X * RoomSize.X, c.Y * RoomSize.Y);
			AddChild(room);
		}

		// Floor label, created once
		var layer = new CanvasLayer();
		var label = new Label { Text = $"Floor {Global.Floor}", Position = new Vector2(20, 20) };
		label.AddThemeFontSizeOverride("font_size", 32);
		layer.AddChild(label);
		AddChild(layer);
	}
}
