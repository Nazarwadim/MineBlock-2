extends Node

@export var use_serealisation:bool = false

@onready var timer:Timer = $SaveTimer


func _ready():
	timer.timeout.connect(_on_save_timer_timeout)
	timer.start()
	
func _on_save_timer_timeout():
	if use_serealisation:
		if $Player.is_serialisatable:
			$Player.save()
		if $VoxelWorld.IsSerialization:
			$VoxelWorld.Save()
