using UnityEditor;
using UnityEngine;
using System.IO;

namespace UnityCloudBuild.Editor
{
    public class CloudBuildSetup
    {
        private const string PackageName = "com.fernantastic.unity-ci-builder";

        [MenuItem("Tools/Unity CI Builder/Install Config Files")]
        public static void InstallConfigFiles()
        {
            if (!EditorUtility.DisplayDialog("Install CI/CD Config", 
                "This will copy build scripts and workflow templates to your project root. Existing files may be overwritten. Continue?", 
                "Yes", "No"))
            {
                return;
            }

            string packageRoot = GetPackageRootPath();
            if (string.IsNullOrEmpty(packageRoot))
            {
                EditorUtility.DisplayDialog("Error", $"Could not find package '{PackageName}'.", "OK");
                return;
            }

            // 1. Copy PowerShell Scripts
            CopyDirectory(
                Path.Combine(packageRoot, "Scripts"), 
                Path.Combine(Application.dataPath, "../Scripts")
            );

            // 2. Copy Workflow Template
            string workflowSrc = Path.Combine(packageRoot, ".github/workflows/main_build.yml.template");
            string workflowDestDir = Path.Combine(Application.dataPath, "../.github/workflows");
            string workflowDest = Path.Combine(workflowDestDir, "main_build.yml");

            if (File.Exists(workflowSrc))
            {
                Directory.CreateDirectory(workflowDestDir);
                // Don't overwrite if exists, or ask? For now, we follow the prompt warning.
                if (!File.Exists(workflowDest) || EditorUtility.DisplayDialog("Overwrite Workflow?", "main_build.yml already exists. Overwrite?", "Yes", "No"))
                {
                    File.Copy(workflowSrc, workflowDest, true);
                    Debug.Log($"Installed workflow to: {workflowDest}");
                }
            }
            else
            {
                Debug.LogError($"Workflow template not found at {workflowSrc}");
            }

            // 3. Copy Sample Build Script (if not already in Assets)
            string scriptSrc = Path.Combine(packageRoot, "Samples~/BuildScripts/Editor/CloudBuild.cs");
            string scriptDestDir = Path.Combine(Application.dataPath, "Editor");
            string scriptDest = Path.Combine(scriptDestDir, "CloudBuild.cs");

            if (File.Exists(scriptSrc))
            {
                Directory.CreateDirectory(scriptDestDir);
                if (!File.Exists(scriptDest))
                {
                    File.Copy(scriptSrc, scriptDest);
                    Debug.Log($"Installed build script to: {scriptDest}");
                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.LogWarning($"Build script already exists at {scriptDest}. Skipping copy to avoid overwriting changes.");
                }
            }

            Debug.Log("Unity CI/CD Builder setup complete.");
        }

        private static string GetPackageRootPath()
        {
            // Try to find the package in the Package Manager
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(CloudBuildSetup).Assembly);
            if (packageInfo != null)
            {
                return packageInfo.resolvedPath;
            }

            // Fallback for development (local assets)
            if (Directory.Exists("Packages/" + PackageName))
            {
                return Path.GetFullPath("Packages/" + PackageName);
            }
            
            // Fallback if checked out directly in Assets (not recommended but possible)
            return Path.GetFullPath("."); 
        }

        private static void CopyDirectory(string sourceDir, string destDir)
        {
            if (!Directory.Exists(sourceDir))
            {
                Debug.LogError($"Source directory not found: {sourceDir}");
                return;
            }

            Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
                Debug.Log($"Installed: {destFile}");
            }
        }
    }
}
