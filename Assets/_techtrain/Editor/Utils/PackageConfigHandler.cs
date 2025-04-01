using UnityEngine;
using UnityEditor;
using System.IO;
using NugetForUnity;
using System.Xml;

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

        private static readonly PackageInfo[] RequiredPackages = new PackageInfo[]
        {
            new PackageInfo("Nett", "0.15.0"),
        };

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
                CreateNewPackageConfig();
            }
            else
            {
                UpdateExistingPackageConfig();
            }
        }

        private static void CreateNewPackageConfig()
        {
            XmlDocument doc = new XmlDocument();
            
            // Create declaration
            XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(declaration);
            
            // Create root element
            XmlElement root = doc.CreateElement("packages");
            doc.AppendChild(root);
            
            // Add all required packages
            foreach (var package in RequiredPackages)
            {
                AddPackage(doc, root, package);
            }
            
            // Save the XML document
            doc.Save(PackageConfigPath);
            Debug.Log($"Created new packages.config file with {RequiredPackages.Length} package entries.");
            AssetDatabase.Refresh();
        }

        private static void UpdateExistingPackageConfig()
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
                foreach (var package in RequiredPackages)
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
