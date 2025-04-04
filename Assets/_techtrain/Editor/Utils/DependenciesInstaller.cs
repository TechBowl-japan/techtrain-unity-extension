#nullable enable

using UnityEngine;
using UnityEditor;
using System;

namespace TechtrainExtension.Utils
{
    /**
     * This class coordinates the installation of all dependencies (both Git and NuGet packages)
     * and provides a single entry point for package installation process.
     */
    [InitializeOnLoad]
    public class DependenciesInstaller
    {
        public static event Action? OnAllDependenciesInstalled;
        
        private static bool _gitDependenciesInstalled = false;
        private static bool _nugetDependenciesInstalled = false;
        
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
                OnAllDependenciesInstalled?.Invoke();
            }
        }
    }
}
