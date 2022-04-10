using System;
using System.Globalization;
using UnityEngine;

[Serializable]
public struct Tile
{
    public const int SIZE = 13; // 8 + 2 + 2 + 1
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

    public byte[] ToByteArray()
    {
        byte[] result = new byte[SIZE];
        Array.Copy(BitConverter.GetBytes(timeStamp), 0, result, 0, 8);
        Array.Copy(BitConverter.GetBytes(x), 0, result, 8, 2);
        Array.Copy(BitConverter.GetBytes(y), 0, result, 10, 2);
        result[12] = colorCode;
        return result;
    }

    public override string ToString()
    {
        return $"{base.ToString()} {{{timeStamp} ({DateTime.FromFileTimeUtc((long)timeStamp)}), {x:D4}, {y:D4}, {colorCode}}}";
    }

    public static Tile FromCSV(string line)
    {
        string[] fields = line.Split(',');
        if (fields.Length == 4) {
            // timestamp, x_coordinate, y_coordinate, color
            return new Tile(DateTime.Parse(fields[0], CultureInfo.InvariantCulture).ToFileTimeUtc(), ushort.Parse(fields[1]), ushort.Parse(fields[2]), byte.Parse(fields[3]));
        } else if (fields.Length == 5) {
            // timestamp, user_hash, x_coordinate, y_coordinate, color
            return new Tile(DateTime.Parse(fields[0], CultureInfo.InvariantCulture).ToFileTimeUtc(), ushort.Parse(fields[2]), ushort.Parse(fields[3]), byte.Parse(fields[4]));
        }
        throw new FormatException($"Got {fields.Length} fields instead of 4 or 5");
    }
}
