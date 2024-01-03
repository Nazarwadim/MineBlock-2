using Godot;

namespace ChunksSerialization
{
    [GlobalClass]
    public partial class GameSerializer : Node
    {
        public const string BASE_WORLDS_PATH = "user://worlds/";
        public const string PLAYER_DIR = "Player";
        public const string CHUNKS_DIR = "Chunks";

        [Export] private VoxelWorld _voxelWorld;
        [Export] private CharacterBody3D _player;
        [Export] public bool IsWorking{get; private set;}
        public override void _Ready()
        {
            if(_voxelWorld == null)
            {
                GD.PrintErr("Set VoxelWorld in ChunkSerializer");
                GetTree().Quit();
            }
            if(_player == null)
            {
                GD.PrintErr("Set player in ChunkSerializer");
                GetTree().Quit();
            }
            CreateGameDirectoriesOrNothing();
            _LoadPlayer();
        }
        
        private void _LoadPlayer()
        {
            string _savePath = BASE_WORLDS_PATH + _voxelWorld.WorldName + '/';
            _player.Call("loadPlayer", _savePath + PLAYER_DIR + '/');
        }

        private void _onSaveTimerTimeout()
        {
            string _savePath = BASE_WORLDS_PATH + _voxelWorld.WorldName + '/';
            _voxelWorld.Save(_savePath + CHUNKS_DIR + '/');
            _player.Call("save", _savePath + PLAYER_DIR + '/');
        }


        public void CreateGameDirectoriesOrNothing()
        {
            if(!DirAccess.DirExistsAbsolute(BASE_WORLDS_PATH.Remove(BASE_WORLDS_PATH.Length - 1)))
            {
                DirAccess.MakeDirAbsolute(BASE_WORLDS_PATH.Remove(BASE_WORLDS_PATH.Length - 1));
            }

            if(!DirAccess.DirExistsAbsolute(BASE_WORLDS_PATH + _voxelWorld.WorldName))
            {
                DirAccess.MakeDirAbsolute(BASE_WORLDS_PATH + _voxelWorld.WorldName);
                DirAccess.MakeDirAbsolute(BASE_WORLDS_PATH + _voxelWorld.WorldName + "/" + PLAYER_DIR);
                DirAccess.MakeDirAbsolute(BASE_WORLDS_PATH + _voxelWorld.WorldName + "/" + CHUNKS_DIR);
            }
        }

    }
}