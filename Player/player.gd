extends CharacterBody2D

var movement_speed = 40.0
var hp = 80
@onready var sprite = $Sprite2D
@onready var walkTimer = get_node("walkTimer")

#Physics process goes based on physics frams or something. Every 1 60th of a second I think?
func _physics_process(_delta):
	movement()
	
func movement():
	var mov = Input.get_vector("left","right","up","down")
	if mov.x > 0:
		sprite.flip_h = false
	elif mov.x < 0:
		sprite.flip_h = true
		
	if mov != Vector2.ZERO:
		if walkTimer.is_stopped():
			if sprite.frame >= 2:
				sprite.frame = 1
			else:
				sprite.frame += 1
			walkTimer.start()
	
	#Normalized does some math to make diaganal movement more similar to horizontal and vertical. TLDR: Diaginalas in x y cords are longer because triangles. Fix the triangels.
	velocity = mov.normalized() * movement_speed
	move_and_slide() #Built in movement thing
