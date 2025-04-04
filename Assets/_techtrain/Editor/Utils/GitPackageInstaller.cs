#nullable enable

using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TechtrainExtension.Utils
{
    /**
     * This class handles installation of Git packages using Unity's Package Manager API.
     * It reads Git dependencies from package.json and adds them to the project.
     */
    public class GitPackageInstaller
    {
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

        private static AddRequest? _currentRequest;
        private static List<GitPackageInfo> _pendingPackages = new List<GitPackageInfo>();
        private static int _currentPackageIndex = 0;
        private static ListRequest? _listRequest;

        static GitPackageInstaller()
        {
            // Resolve package.json path relative to this script
            var manifestPath = PackageUtils.ResolvePackageJsonPath();
            if (string.IsNullOrEmpty(manifestPath) || manifestPath == null)
            {
                throw new System.Exception("Could not resolve package.json path");
            }
            PackageJsonPath = manifestPath;
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

        public static void CheckAndInstallPackages()
        {
            // Read Git dependencies from package.json
            var requiredGitPackages = ReadGitDependenciesFromPackageJson();

            if (requiredGitPackages.Count == 0)
            {
                Debug.Log("No Git packages to install.");
                DependenciesInstaller.NotifyGitDependenciesInstalled();
                return;
            }

            // Get list of installed packages to check what needs to be installed
            _listRequest = Client.List(true);
            EditorApplication.update += OnListRequestUpdate;
        }

        private static void OnListRequestUpdate()
        {
            if (_listRequest == null || !_listRequest.IsCompleted)
                return;

            // Important: Remove this handler first to prevent multiple executions
            EditorApplication.update -= OnListRequestUpdate;
            
            if (_listRequest.Status == StatusCode.Success)
            {
                // Find packages that need to be installed
                var requiredGitPackages = ReadGitDependenciesFromPackageJson();
                foreach (var package in requiredGitPackages)
                {
                    bool isInstalled = false;
                    foreach (var installedPackage in _listRequest.Result)
                    {
                        if (installedPackage.name == package.Name)
                        {
                            Debug.Log($"Git package already exists: {package.Name}");
                            isInstalled = true;
                            break;
                        }
                    }

                    if (!isInstalled)
                    {
                        _pendingPackages.Add(package);
                    }
                }

                // Start installing packages if any are needed
                if (_pendingPackages.Count > 0)
                {
                    Debug.Log($"Found {_pendingPackages.Count} git packages to install");
                    InstallNextPackage();
                }
                else
                {
                    Debug.Log("All git packages are already installed.");
                    DependenciesInstaller.NotifyGitDependenciesInstalled();
                }
            }
            else
            {
                Debug.LogError($"Failed to list packages: {_listRequest.Error.message}");
            }
        }

        private static void InstallNextPackage()
        {
            if (_currentPackageIndex >= _pendingPackages.Count)
            {
                // All packages have been processed
                Debug.Log("All git packages have been installed");
                _pendingPackages.Clear();
                _currentPackageIndex = 0;
                DependenciesInstaller.NotifyGitDependenciesInstalled();
                return;
            }

            var package = _pendingPackages[_currentPackageIndex];
            Debug.Log($"Installing git package: {package.Name} from {package.GitUrl}");
            
            _currentRequest = Client.Add(package.GitUrl);
            EditorApplication.update += OnAddRequestUpdate;
        }

        private static void OnAddRequestUpdate()
        {
            if (_currentRequest == null || !_currentRequest.IsCompleted)
                return;

            // Important: Remove the handler first to prevent multiple executions
            EditorApplication.update -= OnAddRequestUpdate;

            if (_currentRequest.Status == StatusCode.Success)
            {
                Debug.Log($"Successfully installed package: {_currentRequest.Result.displayName}");
            }
            else
            {
                Debug.LogError($"Failed to install package: {_currentRequest.Error.message}");
            }

            // Move to the next package
            _currentPackageIndex++;
            _currentRequest = null;
            
            // Process the next package
            InstallNextPackage();
        }
    }
}
