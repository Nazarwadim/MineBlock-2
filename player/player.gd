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
@export var is_serialisatable:bool = false
const SAVE_PATH = "user://PlayerTransform.bin"
@onready var gravity = ProjectSettings.get_setting("physics/3d/default_gravity")
@onready var _voxel_world :VoxelWorld= $"../VoxelWorld"
@onready var head = $Head
@onready var raycast :RayCast3D= $Head/RayCast3D
@onready var selected_block_texture = $SelectedBlock
@onready var position_before:Vector2i = Vector2i(position.x,  position.z)

var can_move:bool = false
signal position_XZ_changed(position:Vector2i)
func _ready():
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	_voxel_world.CurrentRenderDistanseChanged.connect(_on_current_render_distance_changed)


func _enter_tree():
	if(FileAccess.file_exists(SAVE_PATH) and is_serialisatable):
		transform = bytes_to_var( FileAccess.get_file_as_bytes(SAVE_PATH))
		
func _exit_tree():
	if is_serialisatable:
		save()	
		
func save():
	var transform_bynary = var_to_bytes(transform)
	var file = FileAccess.open(SAVE_PATH, FileAccess.WRITE)
	file.store_buffer(transform_bynary)
	
var _block_pos_set_before:Vector3i = Vector3i.ZERO
func _physics_process(delta):
	if(Vector2i(position.x, position.z) != position_before):
		position_before = Vector2i(position.x, position.z)
		position_XZ_changed.emit(position_before)
		
	if raycast.is_colliding():
		var ray_normal = raycast.get_collision_normal()
		var ray_position :Vector3= raycast.get_collision_point()
		var block_pos_watch = Vector3i((ray_position - ray_normal / 2).floor())
		var block_pos_set = Vector3i((ray_position + ray_normal / 2).floor())
		$BlockToPlaseArea.position = block_pos_set
		$BlockToRemoveArea.position = block_pos_watch 
		$BlockToRemoveArea.visible = true
		$BlockToPlaseArea.visible = true
		if Input.is_action_just_pressed("remove_block"):
			_voxel_world.SetBlockTypeInGlobalPosition(block_pos_watch, 0)
			
		if Input.is_action_just_pressed("place_block"):
			if block_pos_set != _block_pos_set_before:
				await get_tree().create_timer(delta + 0.01).timeout
			if not $BlockToPlaseArea.overlaps_body(self):
				_voxel_world.SetBlockTypeInGlobalPosition(block_pos_set, 13)
		_block_pos_set_before = block_pos_set
	else :
		$BlockToRemoveArea.visible = false	
		$BlockToPlaseArea.visible = false
			
func _process(_delta):
	if !can_move:
		return
	# Mouse movement.
	_mouse_motion.y = clamp(_mouse_motion.y, -709, 709)
	transform.basis = Basis.from_euler(Vector3(0, _mouse_motion.x * -0.0022, 0))
	head.transform.basis = Basis.from_euler(Vector3(_mouse_motion.y * -0.0022, 0, 0))

		
	if not is_on_floor():
		velocity.y -= gravity * _delta
	
	if is_on_floor() and Input.is_action_pressed("ui_accept"):
		velocity.y = 5;
	
	var movement_vec2 := Input.get_vector("move_left", "move_right", "move_forward", "move_back")
	var movement = transform.basis * (Vector3(movement_vec2.x, 0, movement_vec2.y))
	
	velocity.x = movement.x * 5
	velocity.z = movement.z * 5
	move_and_slide()
	
func _input(event):
	if event is InputEventMouseMotion:
		if Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
			_mouse_motion += event.relative



func _on_current_render_distance_changed(value:int):
	if(value > 0):
		can_move = true
	else :
		can_move = false
