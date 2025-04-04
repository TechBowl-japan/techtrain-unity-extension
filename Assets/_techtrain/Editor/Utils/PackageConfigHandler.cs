#nullable enable

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TechtrainExtension.Utils
{
    /**
     * https://github.com/GlitchEnzo/NuGetForUnity
     * This class handles the creation and updating of a packages.config file which is used by NuGetForUnity to manage package dependencies in Unity projects.
    */
    [InitializeOnLoad]
    public class PackageConfigHandler
    {
        private const string PackageConfigPath = "Assets/packages.config";
        
        // We'll resolve the actual path at runtime
        private static readonly string PackageJsonPath;
        
        private struct PackageInfo
        {
            public string Id;
            public string Version;
            public bool ManuallyInstalled;

            public PackageInfo(string id, string version, bool manuallyInstalled = true)
            {
                Id = id;
                Version = version;
                ManuallyInstalled = manuallyInstalled;
            }
        }

        static PackageConfigHandler()
        {
            // Resolve package.json path relative to this script
            var manifestPath = PackageUtils.ResolvePackageJsonPath();
            if (string.IsNullOrEmpty(manifestPath) || manifestPath == null)
            {
                throw new System.Exception("Could not resolve package.json path");
            }
            PackageJsonPath = manifestPath;

            // This constructor will be called when Unity loads/reloads scripts
            CheckAndUpdatePackageConfig();
        }

        private static List<PackageInfo> ReadNugetDependenciesFromPackageJson()
        {
            var nugetPackages = new List<PackageInfo>();

            if (!File.Exists(PackageJsonPath))
            {
                Debug.LogWarning("package.json not found at: " + PackageJsonPath);
                return nugetPackages;
            }

            try
            {
                string jsonContent = File.ReadAllText(PackageJsonPath);
                JObject packageJson = JObject.Parse(jsonContent);

                var _nugetDeps = packageJson["nugetDependencies"];

                // Check if nugetDependencies section exists
                if (_nugetDeps == null)
                {
                    Debug.Log("No nugetDependencies found in package.json");
                    return nugetPackages;
                }

                var nugetDeps = (JObject)_nugetDeps;

                foreach (var prop in nugetDeps.Properties())
                {
                    string packageId = prop.Name;
                    string version = prop.Value.ToString();

                    if (!string.IsNullOrEmpty(version))
                    {
                        nugetPackages.Add(new PackageInfo(packageId, version));
                    }
                    else
                    {
                        Debug.LogWarning($"Missing version for NuGet package {packageId} in package.json");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error reading package.json: {ex.Message}");
            }

            return nugetPackages;
        }

        private static void CheckAndUpdatePackageConfig()
        {
            // Get required packages from package.json
            List<PackageInfo> requiredPackages = ReadNugetDependenciesFromPackageJson();
            
            if (requiredPackages.Count == 0)
            {
                Debug.Log("No NuGet packages to install.");
                return;
            }

            if (!File.Exists(PackageConfigPath))
            {
                // Create new packages.config file if it doesn't exist
                CreateNewPackageConfig(requiredPackages);
            }
            else
            {
                UpdateExistingPackageConfig(requiredPackages);
            }
        }

        private static void CreateNewPackageConfig(List<PackageInfo> requiredPackages)
        {
            XmlDocument doc = new XmlDocument();
            
            // Create declaration
            XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(declaration);
            
            // Create root element
            XmlElement root = doc.CreateElement("packages");
            doc.AppendChild(root);
            
            // Add all required packages
            foreach (var package in requiredPackages)
            {
                AddPackage(doc, root, package);
            }
            
            // Save the XML document
            doc.Save(PackageConfigPath);
            Debug.Log($"Created new packages.config file with {requiredPackages.Count} package entries.");
            AssetDatabase.Refresh();
        }

        private static void UpdateExistingPackageConfig(List<PackageInfo> requiredPackages)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(PackageConfigPath);
                
                XmlElement root = doc.DocumentElement; // Get <packages> element
                if (root == null)
                {
                    Debug.LogError("packages.config exists but has invalid format (missing root element)");
                    return;
                }
                
                int addedCount = 0;

                // Check each required package
                foreach (var package in requiredPackages)
                {
                    // Check if package already exists
                    XmlNodeList existingPackages = doc.SelectNodes($"//package[@id='{package.Id}']");
                    
                    if (existingPackages.Count == 0)
                    {
                        // Package doesn't exist, add it
                        AddPackage(doc, root, package);
                        addedCount++;
                    }
                    else
                    {
                        Debug.Log($"Package {package.Id} already exists in packages.config");
                    }
                }

                if (addedCount > 0)
                {
                    doc.Save(PackageConfigPath);
                    Debug.Log($"Added {addedCount} package(s) to existing packages.config file.");
                    AssetDatabase.Refresh();
                }
            }
            catch (XmlException ex)
            {
                Debug.LogError($"Error parsing packages.config: {ex.Message}");
            }
        }

        private static void AddPackage(XmlDocument doc, XmlElement root, PackageInfo package)
        {
            XmlElement packageElement = doc.CreateElement("package");
            packageElement.SetAttribute("id", package.Id);
            packageElement.SetAttribute("version", package.Version);
            packageElement.SetAttribute("manuallyInstalled", package.ManuallyInstalled.ToString().ToLower());
            root.AppendChild(packageElement);
        }
    }
}
