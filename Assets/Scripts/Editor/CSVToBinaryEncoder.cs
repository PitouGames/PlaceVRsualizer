using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class CSVToBinaryEncoder : Editor
{
    [SerializeField] private string fileName;

    [MenuItem("place/Convert CSV to binary")]
    public static void StartConversion()
    {
        string path = EditorUtility.OpenFilePanel("Select input CSV file", "", "csv");
        if (path.Length != 0) {
            Thread tilesLoadThread = new Thread(() => {
                ConvertCSVToBinary(path);
            });
            tilesLoadThread.Start();
        }
    }

    /// <summary>
    /// Convert the CSV data to a binary file.
    /// Note: tiles placement should be sorted by timestamp ascending.
    /// </summary>
    private static void ConvertCSVToBinary(string path)
    {
        string csvFileName = Path.GetFileNameWithoutExtension(path);
        int progressId = Progress.Start("Convert CSV to binary");
        using (StreamReader reader = new StreamReader(path))
        using (FileStream fileStream = new FileStream($@"Data/{csvFileName}.bin", FileMode.Create, FileAccess.Write)) {
            reader.ReadLine(); // skip header line
            int numberOfLines = 0;
            while (!reader.EndOfStream) {
                reader.ReadLine(); // skip line
                numberOfLines++;
            }
            reader.BaseStream.Position = 0; // Restart from the begining
            reader.DiscardBufferedData();

            reader.ReadLine(); // skip header line
            Tile t;
            string line;
            int lineIndex = 0;
            while (!reader.EndOfStream) {
                line = reader.ReadLine();
                t = Tile.FromCSV(line);
                fileStream.Write(t.ToByteArray(), 0, Tile.SIZE);
                lineIndex++;
                if (lineIndex % 10000 == 0) {
                    Progress.Report(progressId, lineIndex / (float)numberOfLines);
                }
            }
        }
        Progress.Remove(progressId);
    }
}
