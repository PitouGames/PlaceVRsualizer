using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class FileDownloader : MonoBehaviour
{
    [MenuItem("place/Open binary files download page")]
    public static void OpenDownloadPage()
    {
        Process.Start(new ProcessStartInfo {
            FileName = "https://drive.google.com/drive/folders/1EV5CrzvZtq_z63eh48XjqnQahSkEy84_",
            UseShellExecute = true
        });
        UnityEngine.Debug.Log("Your web browser should have openned in a google drive folder. Download and place these binary files in the folder 'Data'." +
            "You can also generate these files with the code provided, but this will take you mush more time.");
    }
}
