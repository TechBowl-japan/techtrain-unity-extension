#nullable enable

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TechtrainExtension.Utils
{
    /**
     * This class handles installation of Git packages by modifying the project's manifest.json file.
     * It reads Git dependencies from package.json and adds them to the project manifest.
     */
    [InitializeOnLoad]
    public class GitPackageInstaller
    {
        private const string ManifestPath = "Packages/manifest.json";

        // We'll resolve the actual path at runtime
        private static readonly string PackageJsonPath;

        // Structure to define Git package information
        private struct GitPackageInfo
        {
            public string Name;        // Package name (e.g., "com.company.package")
            public string GitUrl;      // Git repository URL

            public GitPackageInfo(string name, string gitUrl)
            {
                Name = name;
                GitUrl = gitUrl;
            }
        }

        static GitPackageInstaller()
        {
            // Resolve package.json path relative to this script
            var manifestPath = PackageUtils.ResolvePackageJsonPath();
            if (string.IsNullOrEmpty(manifestPath) || manifestPath == null)
            {
                throw new System.Exception("Could not resolve package.json path");
            }
            PackageJsonPath = manifestPath;

            // This constructor will be called when Unity loads/reloads scripts
            CheckAndUpdateManifest();
        }

        private static List<GitPackageInfo> ReadGitDependenciesFromPackageJson()
        {
            var gitPackages = new List<GitPackageInfo>();

            if (!File.Exists(PackageJsonPath))
            {
                Debug.LogWarning("package.json not found at: " + PackageJsonPath);
                return gitPackages;
            }

            try
            {
                string jsonContent = File.ReadAllText(PackageJsonPath);
                JObject packageJson = JObject.Parse(jsonContent);

                var _gitDeps = packageJson["gitDependencies"];

                // Check if gitDependencies section exists
                if (_gitDeps == null)
                {
                    Debug.Log("No gitDependencies found in package.json");
                    return gitPackages;
                }

                var gitDeps = (JObject)_gitDeps;

                foreach (var prop in gitDeps.Properties())
                {
                    string packageName = prop.Name;
                    string url = prop.Value.ToString();

                    if (!string.IsNullOrEmpty(url))
                    {
                        gitPackages.Add(new GitPackageInfo(packageName, url));
                    }
                    else
                    {
                        Debug.LogWarning($"Missing URL for Git package {packageName} in package.json");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error reading package.json: {ex.Message}");
            }

            return gitPackages;
        }

        private static void CheckAndUpdateManifest()
        {
            if (!File.Exists(ManifestPath))
            {
                Debug.LogError("manifest.json not found at expected path: " + ManifestPath);
                return;
            }

            // Read Git dependencies from package.json
            var requiredGitPackages = ReadGitDependenciesFromPackageJson();

            if (requiredGitPackages.Count == 0)
            {
                Debug.Log("No Git packages to install.");
                return;
            }

            try
            {
                // Read the manifest file
                string jsonContent = File.ReadAllText(ManifestPath);

                // Parse the JSON
                JObject manifestJson = JObject.Parse(jsonContent);

                // Get or create dependencies object
                var dependenciesObj = manifestJson["dependencies"];
                JObject dependencies;
                if (dependenciesObj == null)
                {
                    dependencies = new JObject();
                    manifestJson["dependencies"] = dependencies;
                }
                else
                {
                    dependencies = (JObject)dependenciesObj;
                }

                bool manifestChanged = false;

                // Add each required Git package if it doesn't exist
                foreach (var package in requiredGitPackages)
                {
                    if (dependencies[package.Name] == null)
                    {
                        // Package doesn't exist, add it
                        dependencies[package.Name] = package.GitUrl;
                        Debug.Log($"Adding Git package: {package.Name} from {package.GitUrl}");
                        manifestChanged = true;
                    }
                    else
                    {
                        Debug.Log($"Git package already exists: {package.Name}");
                    }
                }

                if (manifestChanged)
                {
                    // Write the updated manifest back to disk
                    File.WriteAllText(ManifestPath, manifestJson.ToString(Formatting.Indented));
                    Debug.Log("Updated manifest.json with required Git packages");

                    // Refresh AssetDatabase to apply changes
                    AssetDatabase.Refresh();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error updating manifest.json: {ex.Message}");
            }
        }
    }
}
