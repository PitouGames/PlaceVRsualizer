using UnityEngine;

public interface Dataset
{
    string fileName { get; }
    int imageResolution { get; }
    int imageResolutionSquared { get; }
    int keyframeInterval { get; }
    Color[] colors { get; }
}

public class Dataset2017 : Dataset
{
    public string fileName => "2017_place_tiles_sorted_no_user";
    public int imageResolution => 1000;
    public int imageResolutionSquared => 1_000_000;
    public int keyframeInterval => 50_000;
    public Color[] colors => _colors;
    private readonly Color[] _colors = new Color[] {
        Color.white,
        new Color32(228, 228, 228, 255),
        new Color32(136, 136, 136, 255),
        new Color32(34, 34, 34, 255),
        new Color32(255, 167, 209, 255),
        new Color32(229, 0, 0, 255),
        new Color32(229, 149, 0, 255),
        new Color32(160, 106, 66, 255),
        new Color32(229, 217, 0, 255),
        new Color32(148, 224, 68, 255),
        new Color32(2, 190, 1, 255),
        new Color32(0, 211, 221, 255),
        new Color32(0, 131, 199, 255),
        new Color32(0, 0, 234, 255),
        new Color32(207, 110, 228, 255),
        new Color32(130, 0, 128, 255)
    };
}

public class Dataset2022 : Dataset
{
    public string fileName => "2022_place_tiles_sorted_no_user";
    public int imageResolution => 2000;
    public int imageResolutionSquared => 4_000_000;
    public int keyframeInterval => 1_000_000;
    public Color[] colors => _colors;
    private Color[] _colors = new Color[] {
        new Color32(109, 0, 26, 255),
        new Color32(190, 0, 57, 255),
        new Color32(255, 69, 0, 255),
        new Color32(255, 168, 0, 255),
        new Color32(255, 214, 53, 255),
        new Color32(255, 248, 184, 255),
        new Color32(0, 163, 104, 255),
        new Color32(0, 204, 120, 255),
        new Color32(126, 237, 86, 255),
        new Color32(0, 117, 111, 255),
        new Color32(0, 158, 170, 255),
        new Color32(0, 204, 192, 255),
        new Color32(36, 80, 164, 255),
        new Color32(54, 144, 234, 255),
        new Color32(81, 233, 244, 255),
        new Color32(73, 58, 193, 255),
        new Color32(106, 92, 255, 255),
        new Color32(148, 179, 255, 255),
        new Color32(129, 30, 159, 255),
        new Color32(180, 74, 192, 255),
        new Color32(228, 171, 255, 255),
        new Color32(222, 16, 127, 255),
        new Color32(255, 56, 129, 255),
        new Color32(255, 153, 170, 255),
        new Color32(109, 72, 47, 255),
        new Color32(156, 105, 38, 255),
        new Color32(255, 180, 112, 255),
        new Color32(0, 0, 0, 255),
        new Color32(81, 82, 82, 255),
        new Color32(137, 141, 144, 255),
        new Color32(212, 215, 217, 255),
        new Color32(255, 255, 255, 255)
    };
}
