using System;
using UnityEngine;
using UnityEngine.UI;

public class TextureDrawer : MonoBehaviour
{

    private Texture2D outputTexture;
    private RenderTexture outputRenderTexture;
    [SerializeField] private RawImage rawImage;
    [SerializeField] private Text dateText;

    [SerializeField] private TilesManager tilesManager;
    private ulong lastTimestamp;
    private int currentIndex;

    private void Start()
    {
        outputTexture = new Texture2D(TilesManager.IMAGE_RESOLUTION, TilesManager.IMAGE_RESOLUTION, TextureFormat.ARGB32, false);
        outputTexture.filterMode = FilterMode.Point;
        //outputRenderTexture = GetRenderTexture();
        rawImage.texture = outputTexture;
        Invoke("Draw", 0.2f);
    }
    private void Draw()
    {
        //DrawAt(131356345873720000);
        DrawAt(131357123357390000);
        //DrawToIndex(10000);
    }

    /*private RenderTexture GetRenderTexture()
    {
        RenderTexture ret = new RenderTexture(TilesManager.IMAGE_RESOLUTION, TilesManager.IMAGE_RESOLUTION, 0, RenderTextureFormat.ARGB32);
        ret.filterMode = FilterMode.Point;
        ret.wrapMode = TextureWrapMode.Clamp;
        ret.enableRandomWrite = true;
        ret.Create();
        return ret;
    }*/

    public void Draw(float progress)
    {
        ulong timestamp = (ulong)(tilesManager.StartTime + (tilesManager.EndTime - tilesManager.StartTime) * progress);
        DrawAt(timestamp);
    }

    private void DrawAt(ulong timestamp)
    {
        DateTime moment = DateTime.FromFileTimeUtc((long)timestamp);
        dateText.text = moment.ToShortDateString() + " " + moment.ToLongTimeString();
        if (lastTimestamp > timestamp) {
            currentIndex = tilesManager.ApplyMapTextureAt(timestamp, outputTexture); // recompute since the begining.
        } else {
            currentIndex = tilesManager.ApplyMapTextureDiff(currentIndex, timestamp, outputTexture); // recompute only the difference.
        }
        lastTimestamp = timestamp;
    }

    private void DrawToIndex(int index)
    {
        tilesManager.ApplyMapTextureToTileIndex(index, outputTexture);
    }
}
