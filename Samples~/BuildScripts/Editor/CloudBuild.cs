using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.Linq;
using System.IO;

namespace UnityCloudBuild
{
    public class CloudBuild
    {
        public static void BuildAll()
        {
            string buildTarget = Environment.GetEnvironmentVariable("BUILD_TARGET"); // e.g., "StandaloneWindows64", "StandaloneOSX", "Android"
            string buildPath = Environment.GetEnvironmentVariable("BUILD_OUTPUT_PATH");

            if (string.IsNullOrEmpty(buildTarget))
            {
                Debug.LogError("BUILD_TARGET not set.");
                EditorApplication.Exit(1);
            }

            if (string.IsNullOrEmpty(buildPath))
            {
                // Default path if not set: Project/Build/Automated Builds/Latest/[Target]
                buildPath = Path.Combine(Directory.GetCurrentDirectory(), "Build", "Automated Builds", "Latest", buildTarget);
            }

            Debug.Log($"Starting build for {buildTarget} to {buildPath}");
            
            BuildTarget target = (BuildTarget)Enum.Parse(typeof(BuildTarget), buildTarget);
            string extension = GetExtension(target);
            string executableName = Application.productName + extension;
            string locationPathName = Path.Combine(buildPath, executableName);

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(locationPathName));

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = locationPathName,
                target = target,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build SUCCEEDED! {summary.totalSize / 1024 / 1024} MB");
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError($"Build FAILED! Errors: {summary.totalErrors}");
                EditorApplication.Exit(1);
            }
        }

        private static string GetExtension(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return ".exe";
                case BuildTarget.StandaloneOSX:
                    return ".app";
                case BuildTarget.Android:
                    return ".apk"; // or .aab
                case BuildTarget.iOS:
                    return ""; // iOS builds a folder project
                case BuildTarget.StandaloneLinux64:
                    return ".x86_64";
                default:
                    return "";
            }
        }
    }
}
