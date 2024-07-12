extends TileMap

# Called when the node enters the scene tree for the first time.
func _ready():
	var areaSize = Vector2i(64, 64) #Floor will be auto set based on size of maze generated. For now, a static 64x64 will do.
	for x in range(areaSize.x):
		for y in range(areaSize.y):
			set_cell(0, Vector2i(x, y), 0, Vector2i(1, 1)) 
