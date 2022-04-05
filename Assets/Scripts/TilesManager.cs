using System.IO;
using System.Threading;
using UnityEngine;

public class TilesManager : MonoBehaviour
{
    public enum LoadingState { LoadingFile, ComputeKeyframes, Finished }
    public LoadingState CurrentLoadingState { get; private set; }
    public int KeyframesProgress { get; private set; }

    //private List<Tile> tiles = new List<Tile>();
    public long TilesCount { get; private set; }
    private Tile[] tiles;
    private Thread tilesLoadThread;


    public int KeyframesCount { get; private set; }
    public Keyframe[] Keyframes { get; private set; }

    public bool IsLoading { get { return tilesLoadThread != null && tilesLoadThread.IsAlive; } }

    public ulong StartTime { get; private set; }
    public ulong EndTime { get; private set; }

    public const ushort IMAGE_RESOLUTION = 1000;
    public const uint IMAGE_RESOLUTION_SQUARED = IMAGE_RESOLUTION * IMAGE_RESOLUTION;

    public const int KEYFRAMES_INTERVAL = 50_000;


    private void Start()
    {
        //ConvertCSVToBinary();
        //UnityEngine.Debug.Log($"tiles.Count: {tiles.Count}");
        tilesLoadThread = new Thread(() => {
            CurrentLoadingState = LoadingState.LoadingFile;
            ReadFromBinary();
            CurrentLoadingState = LoadingState.ComputeKeyframes;
            GenerateKeyframes();
            CurrentLoadingState = LoadingState.Finished;
        });
        tilesLoadThread.Start();
    }

    private void GenerateKeyframes()
    {
        KeyframesCount = (int)(TilesCount / KEYFRAMES_INTERVAL + 1);
        Keyframes = new Keyframe[KeyframesCount];
        for (int i = 0; i < Keyframes.Length; i++) {
            Keyframes[i].index = i * KEYFRAMES_INTERVAL;
            Keyframes[i].colors = new Color[IMAGE_RESOLUTION_SQUARED];
            if (i == 0) {
                for (ushort x = 0; x < IMAGE_RESOLUTION; x++) {
                    for (ushort y = 0; y < IMAGE_RESOLUTION; y++) {
                        Keyframes[i].colors[y * IMAGE_RESOLUTION + x] = Color.white;
                    }
                }
                Keyframes[i].timestamp = StartTime;
            } else {
                Keyframes[i - 1].colors.CopyTo(Keyframes[i].colors, 0);
                Keyframes[i].timestamp = ApplyMapColorsDiff((i - 1) * KEYFRAMES_INTERVAL, i * KEYFRAMES_INTERVAL, Keyframes[i].colors);
            }
            KeyframesProgress = i + 1;
        }
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

    public Color[] GetMapColorsAt(ulong time)
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

    public (ulong, Color[]) GetMapColorsAtIndex(int index)
    {
        Color[] result = new Color[IMAGE_RESOLUTION_SQUARED];
        for (ushort x = 0; x < IMAGE_RESOLUTION; x++) {
            for (ushort y = 0; y < IMAGE_RESOLUTION; y++) {
                result[y * IMAGE_RESOLUTION + x] = Color.white;
            }
        }
        int i = 0;
        ulong lastTimestamp = 0;
        foreach (Tile t in tiles) {
            if (i > index) {
                break;
            }
            if (t.y < IMAGE_RESOLUTION && t.x < IMAGE_RESOLUTION) {
                result[t.y * IMAGE_RESOLUTION + t.x] = t.Color;
                lastTimestamp = t.timeStamp;
            }
            i++;
        }
        return (lastTimestamp, result);
    }

    public ulong ApplyMapColorsDiff(int startIndex, int endIndex, Color[] result)
    {
        Debug.Assert(result.Length == IMAGE_RESOLUTION_SQUARED);

        endIndex = Mathf.Min(endIndex, (int)TilesCount);
        ulong lastTimestamp = 0;
        Tile t;
        for (int i = startIndex; i < endIndex; i++) {
            t = tiles[i];
            if (t.y < IMAGE_RESOLUTION && t.x < IMAGE_RESOLUTION) {
                result[t.y * IMAGE_RESOLUTION + t.x] = t.Color;
            }
            lastTimestamp = t.timeStamp;
        }
        return lastTimestamp;
    }

    public int ApplyMapTextureAt(ulong timestamp, Texture2D texture)
    {
        int keyFrameIndex = FindPreviousKeyframeIndex(timestamp);
        texture.SetPixels(Keyframes[keyFrameIndex].colors);

        int j;
        for (j = Keyframes[keyFrameIndex].index; j < TilesCount; j++) {
            Tile t = tiles[j];
            if (t.timeStamp > timestamp) {
                break;
            }
            texture.SetPixel(t.x, t.y, t.Color);
        }
        texture.Apply();
        return j;
    }

    public ulong ApplyMapTextureToTileIndex(int index, Texture2D texture)
    {
        ClearTexture(texture);
        int i = 0;
        ulong lastTimestamp = 0;
        foreach (Tile t in tiles) {
            if (i > index) {
                break;
            }
            texture.SetPixel(t.x, t.y, t.Color);
            lastTimestamp = t.timeStamp;
            i++;
        }
        texture.Apply();
        return lastTimestamp;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="timestamp"></param>
    /// <param name="texture"></param>
    /// <returns> The end index. </returns>
    public int ApplyMapTextureDiff(int startIndex, ulong timestamp, Texture2D texture)
    {
        int keyFrameIndex = FindPreviousKeyframeIndex(timestamp);

        if (keyFrameIndex * KEYFRAMES_INTERVAL > startIndex) {
            startIndex = keyFrameIndex * KEYFRAMES_INTERVAL;
            texture.SetPixels(Keyframes[keyFrameIndex].colors);
        }

        int numberOfTiles = tiles.Length;
        int i;
        for (i = startIndex; i < numberOfTiles; i++) {
            if (tiles[i].timeStamp > timestamp) {
                break;
            }
            texture.SetPixel(tiles[i].x, tiles[i].y, tiles[i].Color);
        }
        texture.Apply();
        return i;
    }

    private void ClearTexture(Texture2D texture)
    {
        for (ushort x = 0; x < IMAGE_RESOLUTION; x++) {
            for (ushort y = 0; y < IMAGE_RESOLUTION; y++) {
                texture.SetPixel(x, y, Color.white);
            }
        }
    }

    private int FindPreviousKeyframeIndex(ulong timestamp)
    {
        int index = 1;
        while (index < KeyframesCount) {
            if (timestamp > Keyframes[index].timestamp) {
                index++;
            } else {
                break;
            }
        }
        return index - 1;
    }

    /// <summary>
    /// Load a binary file to the tiles array.
    /// </summary>
    private void ReadFromBinary()
    {
        using (FileStream reader = new FileStream(@"Data/2017_place_tiles_sorted_no_user.bin", FileMode.Open, FileAccess.Read)) {
            MemoryStream memoryStream = new MemoryStream();
            reader.CopyTo(memoryStream);
            TilesCount = memoryStream.Length / Tile.Size;
            tiles = new Tile[TilesCount];
            memoryStream.Seek(0, SeekOrigin.Begin);
            BinaryReader binaryReader = new BinaryReader(memoryStream);
            for (int i = 0; i < TilesCount; i++) {
                tiles[i] = new Tile(binaryReader.ReadUInt64(), binaryReader.ReadUInt16(), binaryReader.ReadUInt16(), binaryReader.ReadByte());
            }
            StartTime = TilesCount > 0 ? tiles[0].timeStamp : 0;
            EndTime = TilesCount > 0 ? tiles[TilesCount - 1].timeStamp : 0;
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
        using (StreamReader reader = new StreamReader(@"Data/2017_place_tiles_sorted_no_user.csv")) {
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
            using (FileStream fileStream = new FileStream(@"Data/2017_place_tiles_sorted_no_user.bin", FileMode.Create, FileAccess.Write)) {
                memoryStream.CopyTo(fileStream);
            }
        }
    }
}
