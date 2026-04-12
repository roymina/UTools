using System.IO;
using UnityEngine;

namespace UTools
{
    public static class UFileUtilities
    {
        public static string ReadFromPersistentDataPath(string fileName, bool createNewIfNotExist = true)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Debug.LogWarning("File name is null or empty.");
                return string.Empty;
            }

            string fullPath = Path.Combine(Application.persistentDataPath, fileName);
            EnsureParentDirectoryExists(fullPath);

            if (!File.Exists(fullPath))
            {
                if (!createNewIfNotExist)
                {
                    Debug.LogWarning($"File not found: {fullPath}");
                    return string.Empty;
                }

                using (File.Create(fullPath))
                {
                }
            }

            return File.ReadAllText(fullPath);
        }

        public static void WriteToPersistentDataPath(string content, string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Debug.LogWarning("File name is null or empty.");
                return;
            }

            string fullPath = Path.Combine(Application.persistentDataPath, fileName);
            EnsureParentDirectoryExists(fullPath);
            File.WriteAllText(fullPath, content ?? string.Empty);
        }

        public static string GetProjectRootFolder()
        {
            DirectoryInfo assetsDirectory = Directory.GetParent(Application.dataPath);
            return assetsDirectory?.FullName ?? Application.dataPath;
        }

        private static void EnsureParentDirectoryExists(string fullPath)
        {
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
