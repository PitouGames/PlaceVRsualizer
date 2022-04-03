using System;
using System.Globalization;
using UnityEngine;

public struct Tile
{
    public ulong timeStamp;
    public ushort x;
    public ushort y;
    public byte colorCode;

    public Tile(ulong timeStamp, ushort x, ushort y, byte colorCode) : this()
    {
        this.timeStamp = timeStamp;
        this.x = x;
        this.y = y;
        this.colorCode = colorCode;
    }

    public Tile(long timeStamp, ushort x, ushort y, byte colorCode) : this((ulong)timeStamp, x, y, colorCode) { }

    public static int Size
    {
        get { return sizeof(ulong) + sizeof(ushort) + sizeof(ushort) + sizeof(byte); }
    }

    public Color Color
    {
        get { return ColorFromCode(colorCode); }
    }

    public override string ToString()
    {
        return $"{base.ToString()} {{{timeStamp} ({DateTime.FromFileTimeUtc((long)timeStamp)}), {x:D4}, {y:D4}, {colorCode:D2}}})";
    }

    public static Tile FromCSV(string line)
    {
        string[] fields = line.Split(',');
        if (fields.Length != 4) {
            throw new FormatException($"Got {fields.Length} fields instead of 4.");
        }
        return new Tile(DateTime.Parse(fields[0], CultureInfo.InvariantCulture).ToFileTimeUtc(), ushort.Parse(fields[1]), ushort.Parse(fields[2]), byte.Parse(fields[3]));
    }

    public static Color ColorFromCode(byte colorCode)
    {
        switch (colorCode) {
            case 0:
                return Color.white;
            case 1:
                return new Color32(228, 228, 228, 255);
            case 2:
                return new Color32(136, 136, 136, 255);
            case 3:
                return new Color32(34, 34, 34, 255); // black
            case 4:
                return new Color32(255, 167, 209, 255); // rose
            case 5:
                return new Color32(229, 0, 0, 255); // red
            case 6:
                return new Color32(229, 149, 0, 255); // orange
            case 7:
                return new Color32(160, 106, 66, 255); // brown
            case 8:
                return new Color32(229, 217, 0, 255); //yellow
            case 9:
                return new Color32(148, 224, 68, 255); // light green
            case 10:
                return new Color32(2, 190, 1, 255); // green
            case 11:
                return new Color32(0, 211, 221, 255); // light blue
            case 12:
                return new Color32(0, 131, 199, 255); // blue
            case 13:
                return new Color32(0, 0, 234, 255); // dark blue
            case 14:
                return new Color32(207, 110, 228, 255); // violet rose
            case 15:
                return new Color32(130, 0, 128, 255); // violet
            default:
                return Color.white;
        }
    }
}
