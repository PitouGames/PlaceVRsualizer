using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class TilesManager : MonoBehaviour
{
    public enum LoadingState { LoadingFile, ComputeKeyframes, Finished }
    public LoadingState CurrentLoadingState { get; private set; }
    public Action OnLoadStart;
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

    //public const ushort IMAGE_RESOLUTION = 1000;
    //public const uint IMAGE_RESOLUTION_SQUARED = IMAGE_RESOLUTION * IMAGE_RESOLUTION;

    //public const int KEYFRAMES_INTERVAL = 1_000_000;

    public Dataset Dataset { get; private set; }


    private void Start()
    {
        SetDataset(true);
    }

    private void OnDestroy()
    {
        tiles = null; // free memory
        GC.Collect();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataset2022"> True to load 2022 data, false to load 2017 data. </param>
    public void SetDataset(bool dataset2022)
    {
        Dataset datasetToLoad = null;
        if (dataset2022 && !(Dataset is Dataset2022)) {
            datasetToLoad = new Dataset2022();
        } else if (!(Dataset is Dataset2017)) {
            datasetToLoad = new Dataset2017();
        }

        if (datasetToLoad != null) {
            Dataset = datasetToLoad;
            if (tilesLoadThread != null && tilesLoadThread.IsAlive) {
                tilesLoadThread.Abort();
            }
            CurrentLoadingState = LoadingState.LoadingFile;
            tilesLoadThread = new Thread(() => {
                ReadFromBinary(Dataset.fileName);
                CurrentLoadingState = LoadingState.ComputeKeyframes;
                GenerateKeyframes();
                CurrentLoadingState = LoadingState.Finished;
            });
            tilesLoadThread.Start();
            OnLoadStart?.Invoke();
        }
    }

    private void GenerateKeyframes()
    {
        KeyframesCount = (int)(TilesCount / Dataset.keyframeInterval + 1);
        Keyframes = new Keyframe[KeyframesCount];
        for (int i = 0; i < Keyframes.Length; i++) {
            Keyframes[i].index = i * Dataset.keyframeInterval;
            Keyframes[i].colors = new Color[Dataset.imageResolutionSquared];
            if (i == 0) {
                for (ushort x = 0; x < Dataset.imageResolution; x++) {
                    for (ushort y = 0; y < Dataset.imageResolution; y++) {
                        Keyframes[i].colors[y * Dataset.imageResolution + x] = Color.white;
                    }
                }
                Keyframes[i].timestamp = StartTime;
            } else {
                Keyframes[i - 1].colors.CopyTo(Keyframes[i].colors, 0);
                Keyframes[i].timestamp = ApplyMapColorsDiff((i - 1) * Dataset.keyframeInterval, i * Dataset.keyframeInterval, Keyframes[i].colors);
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
                res = Dataset.colors[t.colorCode];
            }
        }
        return res;
    }

    public Color[] GetMapColorsAt(ulong time)
    {
        Color[] result = new Color[Dataset.imageResolutionSquared];
        foreach (Tile t in tiles) {
            if (t.timeStamp > time) {
                break;
            }
            result[t.y * Dataset.imageResolution + t.x] = Dataset.colors[t.colorCode];
        }
        return result;
    }

    public (ulong, Color[]) GetMapColorsAtIndex(int index)
    {
        Color[] result = new Color[Dataset.imageResolutionSquared];
        for (ushort x = 0; x < Dataset.imageResolution; x++) {
            for (ushort y = 0; y < Dataset.imageResolution; y++) {
                result[y * Dataset.imageResolution + x] = Color.white;
            }
        }
        int i = 0;
        ulong lastTimestamp = 0;
        foreach (Tile t in tiles) {
            if (i > index) {
                break;
            }
            if (t.y < Dataset.imageResolution && t.x < Dataset.imageResolution) {
                result[t.y * Dataset.imageResolution + t.x] = Dataset.colors[t.colorCode];
                lastTimestamp = t.timeStamp;
            }
            i++;
        }
        return (lastTimestamp, result);
    }

    public ulong ApplyMapColorsDiff(int startIndex, int endIndex, Color[] result)
    {
        Debug.Assert(result.Length == Dataset.imageResolutionSquared);

        endIndex = Mathf.Min(endIndex, (int)TilesCount);
        ulong lastTimestamp = 0;
        Tile t;
        for (int i = startIndex; i < endIndex; i++) {
            t = tiles[i];
            if (t.y < Dataset.imageResolution && t.x < Dataset.imageResolution) {
                result[t.y * Dataset.imageResolution + t.x] = Dataset.colors[t.colorCode];
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
            texture.SetPixel(t.x, t.y, Dataset.colors[t.colorCode]);
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
            texture.SetPixel(t.x, t.y, Dataset.colors[t.colorCode]);
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

        if (keyFrameIndex * Dataset.keyframeInterval > startIndex) {
            startIndex = keyFrameIndex * Dataset.keyframeInterval;
            texture.SetPixels(Keyframes[keyFrameIndex].colors);
        }

        int numberOfTiles = tiles.Length;
        int i;
        for (i = startIndex; i < numberOfTiles; i++) {
            if (tiles[i].timeStamp > timestamp) {
                break;
            }
            texture.SetPixel(tiles[i].x, tiles[i].y, Dataset.colors[tiles[i].colorCode]);
        }
        texture.Apply();
        return i;
    }

    private void ClearTexture(Texture2D texture)
    {
        for (ushort x = 0; x < Dataset.imageResolution; x++) {
            for (ushort y = 0; y < Dataset.imageResolution; y++) {
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
    private void ReadFromBinary(string binaryFileName)
    {
        using (FileStream reader = new FileStream($@"Data/{binaryFileName}.bin", FileMode.Open, FileAccess.Read)) {
            MemoryStream memoryStream = new MemoryStream();
            reader.CopyTo(memoryStream);
            TilesCount = memoryStream.Length / Tile.SIZE;
            tiles = new Tile[TilesCount];
            memoryStream.Seek(0, SeekOrigin.Begin);
            BinaryReader binaryReader = new BinaryReader(memoryStream);
            for (int i = 0; i < TilesCount; i++) {
                tiles[i] = new Tile(binaryReader.ReadUInt64(), binaryReader.ReadUInt16(), binaryReader.ReadUInt16(), binaryReader.ReadByte());
                if (tiles[i].colorCode < 0 || tiles[i].colorCode > 31) {
                    Debug.Log(tiles[i]);
                }
            }
            StartTime = TilesCount > 0 ? tiles[0].timeStamp : 0;
            EndTime = TilesCount > 0 ? tiles[TilesCount - 1].timeStamp : 0;
            Debug.Log($"Loaded {TilesCount} tiles.");
        }
    }
}
