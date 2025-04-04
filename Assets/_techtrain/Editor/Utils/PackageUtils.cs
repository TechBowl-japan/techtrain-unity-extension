using UnityEngine;
using UnityEditor;
using System.IO;

namespace TechtrainExtension.Utils
{
    public static class PackageUtils
    {
        private static string FindPackageRoot()
        {
            // Find the PackageUtils script by name using AssetDatabase
            string[] guids = AssetDatabase.FindAssets($"t:MonoScript {typeof(PackageUtils).Name}");

            if (guids.Length == 0)
            {
                Debug.LogError("Could not find PackageUtils script");
                return null;
            }

            string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);

            if (string.IsNullOrEmpty(scriptPath))
            {
                Debug.LogError("Could not resolve path for PackageUtils script");
                return null;
            }

            string scriptDirectory = Path.GetDirectoryName(scriptPath);

            // Navigate up from Editor/Utils to _techtrain folder
            return Path.GetDirectoryName(Path.GetDirectoryName(scriptDirectory));
        }

        public static string ResolvePackageJsonPath()
        {
            var packageRoot = FindPackageRoot();
            if (string.IsNullOrEmpty(packageRoot))
            {
                Debug.LogError("Could not resolve package root");
                return null;
            }

            string packageJsonPath = Path.Combine(packageRoot, "package.json");
            Debug.Log($"Resolved package.json path: {packageJsonPath}");

            return packageJsonPath;
        }
    }
}
