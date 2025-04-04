using UnityEngine;
using UnityEditor;
using System.IO;

namespace TechtrainExtension.Utils
{
    public static class PackageUtils
    {

        public static string ResolvePackageJsonPath()
        {
            // Find the GitPackageInstaller script by name using AssetDatabase
            string[] guids = AssetDatabase.FindAssets($"t:MonoScript {typeof(GitPackageInstaller).Name}");

            if (guids.Length == 0)
            {
                Debug.LogError("Could not find GitPackageInstaller script");
                return null;
            }

            string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);

            if (string.IsNullOrEmpty(scriptPath))
            {
                Debug.LogError("Could not resolve path for GitPackageInstaller script");
                return null;
            }

            string scriptDirectory = Path.GetDirectoryName(scriptPath);

            // Navigate up from Editor/Utils to _techtrain folder
            string packageRoot = Path.GetDirectoryName(Path.GetDirectoryName(scriptDirectory));

            string packageJsonPath = Path.Combine(packageRoot, "package.json");
            Debug.Log($"Resolved package.json path: {packageJsonPath}");

            return packageJsonPath;
        }
    }
}
