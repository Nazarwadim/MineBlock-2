using Godot;

// Summary:
//     This is Chunk container resource that can be saved.

[GlobalClass]
public partial class ChunkResource : Resource
{
    public Vector2I Position;
    public Block[,,] Data;
    public ushort[,] Height;
    //
    // Summary:
    //     Create chunk without blocks
    public ChunkResource()
    {
        short chunkSize = ChunkDataGenerator.CHUNK_SIZE;
        Data = new Block[chunkSize, ChunkDataGenerator.CHUNK_HEIGHT, chunkSize];
        
        for(int i = 0; i < chunkSize;++i)
        {
            for(int j = 0; j < ChunkDataGenerator.CHUNK_HEIGHT; ++j)
            {
                for(int k = 0; k < chunkSize; ++k)
                {
                    Data[i,j,k] = new Block();
                }
            }
        }
    }
    public ChunkResource(Block[,,] data)
    {
        Data = data;
        Position = new Vector2I();
    }

    public ChunkResource(Block[,,] data, Vector2I position)
    {
        Data = data;
        Position = position;
    }

    public ChunkResource(Vector2I position) : this()
    {
        Position = position;
    }

    public ChunkResource(ushort[,,] data, Vector2I position) : this(position)
    {
        for(int i = 0; i < ChunkDataGenerator.CHUNK_SIZE; ++i)
        {
            for(int j = 0; j < ChunkDataGenerator.CHUNK_HEIGHT; ++j)
            {
                for(int k =0 ;k < ChunkDataGenerator.CHUNK_SIZE; ++k)
                {
                    Data[i,j,k].type = (Block.Type)data[i,j,k];
                }
            }
        }
    }
}