using Godot;

public partial class Global : Node
{
    // Static reference accessible from any script
    public static CharacterBody2D Player { get; set; }
    public static int Floor { get; set; } = 1;
}