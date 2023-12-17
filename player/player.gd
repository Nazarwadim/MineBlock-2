extends CharacterBody3D

const EYE_HEIGHT_STAND = 1.6
const EYE_HEIGHT_CROUCH = 1.4
const MOVEMENT_SPEED_GROUND = 0.6
const MOVEMENT_SPEED_AIR = 0.11
const MOVEMENT_SPEED_CROUCH_MODIFIER = 0.5
const MOVEMENT_FRICTION_GROUND = 0.9
const MOVEMENT_FRICTION_AIR = 0.98

var _mouse_motion = Vector2()
var _selected_block = 6

@onready var gravity = ProjectSettings.get_setting("physics/3d/default_gravity")

@onready var head = $Head
@onready var raycast = $Head/RayCast3D
@onready var camera_attributes = $Head/Camera3D.attributes
@onready var selected_block_texture = $SelectedBlock


func _ready():
	
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	


func _process(_delta):
	# Mouse movement.
	
	_mouse_motion.y = clamp(_mouse_motion.y, -709, 709)
	transform.basis = Basis.from_euler(Vector3(0, _mouse_motion.x * -0.0022, 0))
	head.transform.basis = Basis.from_euler(Vector3(_mouse_motion.y * -0.0022, 0, 0))

	# Block selection.
	var ray_position = raycast.get_collision_point()
	var ray_normal = raycast.get_collision_normal()

func _input(event):
	if event is InputEventMouseMotion:
		if Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
			_mouse_motion += event.relative


func _on_voxel_world_child_entered_tree(node:ChunkStaticBody):
	ResourceSaver.save( node.MeshInstance.mesh, "res://mesh.res")
