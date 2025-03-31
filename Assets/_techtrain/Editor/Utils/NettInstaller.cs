using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using NugetForUnity;

namespace TechtrainExtension.Utils
{
    [InitializeOnLoad]
    public class PackageConfigHandler
    {
        private const string PackageConfigPath = "Assets/packages.config";
        private const string NettPackageLine = "<package id=\"Nett\" version=\"0.15.0\" manuallyInstalled=\"true\" />";

        static PackageConfigHandler()
        {
            // This constructor will be called when Unity loads/reloads scripts
            CheckAndUpdatePackageConfig();
        }

        private static void CheckAndUpdatePackageConfig()
        {
            if (!File.Exists(PackageConfigPath))
            {
                // Create new packages.config file if it doesn't exist
                string newContent = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<packages>\n  " + NettPackageLine + "\n</packages>";
                File.WriteAllText(PackageConfigPath, newContent);
                Debug.Log("Created new packages.config file with Nett package entry.");
                AssetDatabase.Refresh();
            }
            else
            {
                string content = File.ReadAllText(PackageConfigPath);

                if (!content.Contains("id=\"Nett\""))
                {
                    if (content.Contains("</packages>"))
                    {
                        content = Regex.Replace(content, "</packages>", "  " + NettPackageLine + "\n</packages>");
                        File.WriteAllText(PackageConfigPath, content);
                        Debug.Log("Added Nett package to existing packages.config file.");
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        Debug.LogError("packages.config exists but has invalid format (missing </packages> tag)");
                    }
                }
                else
                {
                    Debug.Log("Nett package already exists in packages.config");
                }
            }
        }
    }
}
