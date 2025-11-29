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
                "This will copy build scripts and workflow templates to your project. Existing files may be overwritten. Continue?", 
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

            // 1. Copy PowerShell Scripts to Unity-CI-Builder/Scripts
            // Scripts are deployment helpers, typically in root/Scripts, but user wants them under a specific folder.
            // Since these are run by CI outside Unity, typically they are at root.
            // We'll put them in ProjectRoot/Unity-CI-Builder/Scripts
            
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            string builderRoot = Path.Combine(projectRoot, "Unity-CI-Builder");
            string scriptsDestDir = Path.Combine(builderRoot, "Scripts");
            
            CopyDirectory(
                Path.Combine(packageRoot, "Scripts"), 
                scriptsDestDir
            );

            // 2. Copy Workflow Template
            string workflowSrc = Path.Combine(packageRoot, ".github/workflows/main_build.yml.template");
            string workflowDestDir = Path.Combine(projectRoot, ".github/workflows");
            string workflowDest = Path.Combine(workflowDestDir, "main_build.yml");

            if (File.Exists(workflowSrc))
            {
                Directory.CreateDirectory(workflowDestDir);
                if (!File.Exists(workflowDest) || EditorUtility.DisplayDialog("Overwrite Workflow?", "main_build.yml already exists. Overwrite?", "Yes", "No"))
                {
                    // We need to read the content to update the script paths, since we moved them to Unity-CI-Builder/Scripts
                    string content = File.ReadAllText(workflowSrc);
                    content = content.Replace(@".\Scripts\", @".\Unity-CI-Builder\Scripts\");
                    File.WriteAllText(workflowDest, content);
                    Debug.Log($"Installed workflow to: {workflowDest}");
                }
            }
            else
            {
                Debug.LogError($"Workflow template not found at {workflowSrc}");
            }

            // 3. Copy Sample Build Script to Assets/Unity-CI-Builder/Editor/CloudBuild.cs
            string scriptSrc = Path.Combine(packageRoot, "Samples~/BuildScripts/Editor/CloudBuild.cs");
            string editorDestDir = Path.Combine(Application.dataPath, "Unity-CI-Builder/Editor");
            string scriptDest = Path.Combine(editorDestDir, "CloudBuild.cs");

            if (File.Exists(scriptSrc))
            {
                Directory.CreateDirectory(editorDestDir);
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
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(CloudBuildSetup).Assembly);
            if (packageInfo != null) return packageInfo.resolvedPath;
            if (Directory.Exists("Packages/" + PackageName)) return Path.GetFullPath("Packages/" + PackageName);
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
