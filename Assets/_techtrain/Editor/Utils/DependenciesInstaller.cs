#nullable enable

using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace TechtrainExtension.Utils
{
    /**
     * This class coordinates the installation of all dependencies (both Git and NuGet packages)
     * and provides a single entry point for package installation process.
     */
    [InitializeOnLoad]
    public class DependenciesInstaller
    {
        private static bool _gitDependenciesInstalled = false;
        private static bool _nugetDependenciesInstalled = false;
        
        // Use relative paths for folder renaming
        private static string? _dotExtensionFolderPath;
        private static string? _finalExtensionFolderPath;
        
        static DependenciesInstaller()
        {
            // This constructor will be called when Unity loads/reloads scripts
            EditorApplication.delayCall += StartInstallation;
        }
        
        public static void StartInstallation()
        {
            Debug.Log("Starting dependency installation process...");
            
            // Reset status
            _gitDependenciesInstalled = false;
            _nugetDependenciesInstalled = false;
            
            // Start both installation processes
            GitPackageInstaller.CheckAndInstallPackages();
            PackageConfigHandler.CheckAndUpdatePackageConfig();
        }
        
        public static void NotifyGitDependenciesInstalled()
        {
            _gitDependenciesInstalled = true;
            CheckAllDependenciesInstalled();
        }
        
        public static void NotifyNugetDependenciesInstalled()
        {
            _nugetDependenciesInstalled = true;
            CheckAllDependenciesInstalled();
        }
        
        private static void CheckAllDependenciesInstalled()
        {
            if (_gitDependenciesInstalled && _nugetDependenciesInstalled)
            {
                Debug.Log("All dependencies (Git and NuGet packages) have been successfully installed!");
                RenameTechtrainExtensionFolder();
            }
        }
        
        private static void RenameTechtrainExtensionFolder()
        {
            // Determine paths based on this script's location
            ResolveFolderPaths();
            
            // Validate paths were resolved successfully
            if (string.IsNullOrEmpty(_dotExtensionFolderPath) || string.IsNullOrEmpty(_finalExtensionFolderPath))
            {
                Debug.LogError("Failed to resolve extension folder paths");
                return;
            }
            
            try
            {
                Debug.Log($"Renaming {_dotExtensionFolderPath} to {_finalExtensionFolderPath}...");
                
                // Use AssetDatabase to move assets properly within Unity
                Directory.Move(_dotExtensionFolderPath, _finalExtensionFolderPath);
                
                // Refresh the AssetDatabase to reflect changes
                AssetDatabase.Refresh();
                Debug.Log("Successfully renamed .TechtrainExtension folder to TechtrainExtension");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error renaming .TechtrainExtension folder: {ex.Message}");
            }
        }
        
        private static void ResolveFolderPaths()
        {
            // Find the DependenciesInstaller script by name using AssetDatabase
            string[] guids = AssetDatabase.FindAssets($"t:MonoScript {nameof(DependenciesInstaller)}");

            if (guids.Length == 0)
            {
                Debug.LogError("Could not find DependenciesInstaller script");
                return;
            }

            string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            
            if (string.IsNullOrEmpty(scriptPath))
            {
                Debug.LogError("Could not resolve path for DependenciesInstaller script");
                return;
            }
            
            // Get the directory of this script
            string scriptDirectory = Path.GetDirectoryName(scriptPath);
            if (string.IsNullOrEmpty(scriptDirectory))
            {
                Debug.LogError("Could not resolve directory for DependenciesInstaller script");
                return;
            }
            
            // Navigate up to the Editor folder
            string? editorDirectory = Path.GetDirectoryName(scriptDirectory);
            if (string.IsNullOrEmpty(editorDirectory))
            {
                Debug.LogError("Could not resolve Editor directory");
                return;
            }
            
            // Construct paths to the target folders
            _dotExtensionFolderPath = Path.Combine(editorDirectory, ".TechtrainExtension");
            _finalExtensionFolderPath = Path.Combine(editorDirectory, "TechtrainExtension");
            
            // Convert to Unity-style paths
            _dotExtensionFolderPath = _dotExtensionFolderPath.Replace('\\', '/');
            _finalExtensionFolderPath = _finalExtensionFolderPath.Replace('\\', '/');
            
            Debug.Log($"Resolved source folder: {_dotExtensionFolderPath}");
            Debug.Log($"Resolved target folder: {_finalExtensionFolderPath}");
        }
    }
}
