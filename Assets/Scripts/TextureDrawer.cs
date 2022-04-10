using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class TextureDrawer : MonoBehaviour
{

    private Texture2D outputTexture;
    private Stopwatch sw = new Stopwatch();
    [SerializeField] private RawImage rawImage;
    [SerializeField] private Text dateText;

    [SerializeField] private TilesManager tilesManager;
    private ulong lastTimestamp;
    private int currentIndex;

    private void Awake()
    {
        tilesManager.OnLoadStart += Setup;
    }

    private void OnDestroy()
    {
        tilesManager.OnLoadStart -= Setup;
    }

    private void Setup()
    {
        SetupTexture();
        //outputRenderTexture = GetRenderTexture();
        StartCoroutine(WaitLoading());
    }

    private void SetupTexture()
    {
        if (outputTexture != null) {
            DestroyImmediate(outputTexture);
        }
        outputTexture = new Texture2D(tilesManager.Dataset.imageResolution, tilesManager.Dataset.imageResolution, TextureFormat.RGB24, false);
        outputTexture.filterMode = FilterMode.Point;
        outputTexture.anisoLevel = 16;
        rawImage.texture = outputTexture;
    }

    private IEnumerator WaitLoading()
    {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();
        while (tilesManager.IsLoading) {
            yield return wait;
        }
        UnityEngine.Debug.Log($"Loading finished in {Time.time}sec");
        lastTimestamp = tilesManager.EndTime;
        DrawAt(tilesManager.StartTime);
    }

    public void Draw(float progress)
    {
        ulong timestamp = (ulong)(tilesManager.StartTime + (tilesManager.EndTime - tilesManager.StartTime) * progress);
        DrawAt(timestamp);
    }

    private void DrawAt(ulong timestamp)
    {
        if (!tilesManager.IsLoading) {
            //sw.Restart();
            DateTime moment = DateTime.FromFileTimeUtc((long)timestamp);
            dateText.text = moment.ToShortDateString() + " " + moment.ToLongTimeString();
            if (timestamp < lastTimestamp) { // Going backward
                                             // recompute since the begining.
                //UnityEngine.Debug.Log($"DrawAt use ApplyMapTextureAt");
                currentIndex = tilesManager.ApplyMapTextureAt(timestamp, outputTexture);
            } else {
                // recompute only the difference with actual frame.
                //UnityEngine.Debug.Log($"DrawAt use ApplyMapTextureDiff");
                currentIndex = tilesManager.ApplyMapTextureDiff(currentIndex, timestamp, outputTexture);
            }
            lastTimestamp = timestamp;
            //sw.Stop();
            //UnityEngine.Debug.Log($"DrawAt took {sw.Elapsed}");
            //sw.Reset();
        }
    }

    private void DrawToIndex(int index)
    {
        if (!tilesManager.IsLoading) {
            tilesManager.ApplyMapTextureToTileIndex(index, outputTexture);
        }
    }
}
