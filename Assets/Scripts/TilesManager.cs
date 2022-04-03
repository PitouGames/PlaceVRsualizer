using System.IO;
using UnityEngine;

public class TilesManager : MonoBehaviour
{
    //private List<Tile> tiles = new List<Tile>();
    private Tile[] tiles;
    public ulong StartTime { get; private set; }
    public ulong EndTime { get; private set; }

    public const ushort IMAGE_RESOLUTION = 1000;
    public const uint IMAGE_RESOLUTION_SQUARED = IMAGE_RESOLUTION * IMAGE_RESOLUTION;

    private void Start()
    {
        //ConvertCSVToBinary();
        ReadFromBinary();
        //UnityEngine.Debug.Log($"tiles.Count: {tiles.Count}");
    }

    private Tile? GetTileAFter(ulong time)
    {
        foreach (Tile t in tiles) {
            if (t.timeStamp > time) {
                return t;
            }
        }
        return null;
    }

    public Color GetTileColorAt(ulong time, ushort x, ushort y)
    {
        Color res = Color.white;
        foreach (Tile t in tiles) {
            if (t.timeStamp > time) {
                break;
            }
            if (t.x == x && t.y == y) {
                res = t.Color;
            }
        }
        return res;
    }

    public Color[] GetMapColorAt(ulong time)
    {
        Color[] result = new Color[IMAGE_RESOLUTION_SQUARED];
        foreach (Tile t in tiles) {
            if (t.timeStamp > time) {
                break;
            }
            result[t.y * IMAGE_RESOLUTION + t.x] = t.Color;
        }
        return result;
    }
    public int ApplyMapTextureAt(ulong time, Texture2D texture)
    {
        for (ushort x = 0; x < IMAGE_RESOLUTION; x++) {
            for (ushort y = 0; y < IMAGE_RESOLUTION; y++) {
                texture.SetPixel(x, y, Color.white);
            }
        }
        int i = 0;
        foreach (Tile t in tiles) {
            if (t.timeStamp > time) {
                break;
            }
            texture.SetPixel(t.x, t.y, t.Color);
            i++;
        }
        texture.Apply();
        return i;
    }

    public void ApplyMapTextureToTileIndex(int index, Texture2D texture)
    {
        for (ushort x = 0; x < IMAGE_RESOLUTION; x++) {
            for (ushort y = 0; y < IMAGE_RESOLUTION; y++) {
                texture.SetPixel(x, y, Color.white);
            }
        }
        int i = 0;
        foreach (Tile t in tiles) {
            if (i > index) {
                break;
            }
            texture.SetPixel(t.x, t.y, t.Color);
            i++;
        }
        texture.Apply();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="time"></param>
    /// <param name="texture"></param>
    /// <returns> The end index. </returns>
    public int ApplyMapTextureDiff(int startIndex, ulong time, Texture2D texture)
    {
        int numberOfTiles = tiles.Length;
        int i;
        for (i = startIndex; i < numberOfTiles; i++) {
            if (tiles[i].timeStamp > time) {
                break;
            }
            texture.SetPixel(tiles[i].x, tiles[i].y, tiles[i].Color);
        }
        texture.Apply();
        return i;
    }

    /// <summary>
    /// Load a binary file to the tiles array.
    /// </summary>
    private void ReadFromBinary()
    {
        using (FileStream reader = new FileStream(@"Data/place_tiles_sorted_no_user.bin", FileMode.Open, FileAccess.Read)) {
            MemoryStream memoryStream = new MemoryStream();
            reader.CopyTo(memoryStream);
            long numberOfTiles = memoryStream.Length / Tile.Size;
            tiles = new Tile[numberOfTiles];
            memoryStream.Seek(0, SeekOrigin.Begin);
            BinaryReader binaryReader = new BinaryReader(memoryStream);
            for (int i = 0; i < numberOfTiles; i++) {
                tiles[i] = new Tile(binaryReader.ReadUInt64(), binaryReader.ReadUInt16(), binaryReader.ReadUInt16(), binaryReader.ReadByte());
            }
            StartTime = numberOfTiles > 0 ? tiles[0].timeStamp : 0;
            EndTime = numberOfTiles > 0 ? tiles[numberOfTiles - 1].timeStamp : 0;
        }
    }

    /// <summary>
    /// Used to convert the CSV data to a binary file.
    /// Note: tiles placement should be sorted by timestamp ascending.
    /// </summary>
    private void ConvertCSVToBinary()
    {
        using (MemoryStream memoryStream = new MemoryStream())
        using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
        using (StreamReader reader = new StreamReader(@"Data/place_tiles_sorted_no_user.csv")) {
            reader.ReadLine(); // skip header line
            Tile t;
            string line;
            while (!reader.EndOfStream) {
                line = reader.ReadLine();
                t = Tile.FromCSV(line);
                binaryWriter.Write(t.timeStamp);
                binaryWriter.Write(t.x);
                binaryWriter.Write(t.y);
                binaryWriter.Write(t.colorCode);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            using (FileStream fileStream = new FileStream(@"Data/place_tiles_sorted_no_user.bin", FileMode.Create, FileAccess.Write)) {
                memoryStream.CopyTo(fileStream);
            }
        }
    }
}
