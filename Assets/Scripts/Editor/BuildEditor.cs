using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

public class BuildEditor
{

    [PostProcessBuild(10)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        string playerPath = Path.GetDirectoryName(pathToBuiltProject);
        string dataPath = playerPath + "/Data";
        DirectoryCopy("Data", dataPath, true);
    }

    /// <summary>
    /// https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
    /// </summary>
    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists) {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();

        // If the destination directory doesn't exist, create it.       
        Directory.CreateDirectory(destDirName);

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files) {
            string tempPath = Path.Combine(destDirName, file.Name);
            if (!File.Exists(tempPath)) {
                file.CopyTo(tempPath, false);
            }
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs) {
            foreach (DirectoryInfo subdir in dirs) {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
            }
        }
    }
}
