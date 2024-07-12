extends Node2D

@onready var maze_generator = $LabyrinthGenerator


# Called when the node enters the scene tree for the first time.
func _ready():
	var collumns = 64
	var rows = 64
	maze_generator.GenerateMaze(collumns, rows)
	
	var player_spawn_room = preload("res://Maze/Rooms/player_spawn_room.tscn").instantiate()
	player_spawn_room.position = Vector2i(128, 128)
	add_child(player_spawn_room)
	


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta):
	pass
