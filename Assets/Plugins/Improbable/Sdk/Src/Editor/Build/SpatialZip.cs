// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Diagnostics;
using System.IO;
using Improbable.Unity.Editor;
using Improbable.Unity.Util;

namespace Improbable.Unity.EditorTools.Build
{
    static class SpatialZip
    {
        private const string ZipSubCommand = "file zip";

        internal static void Zip(string spatialCommand, string zipAbsolutePath, string basePath, string subFolder, string filePattern, PlayerCompression useCompression)
        {
            var zipFileFullPath = Path.GetFullPath(zipAbsolutePath);
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                FileName = spatialCommand,
                Arguments = ZipArgs(basePath, subFolder, filePattern, zipFileFullPath, useCompression),
                CreateNoWindow = true
            };

            var zipProcess = SpatialRunner.RunCommandWithSpatialInThePath(spatialCommand, startInfo);

            var output = zipProcess.StandardOutput.ReadToEnd();
            var errOut = zipProcess.StandardError.ReadToEnd();
            zipProcess.WaitForExit();
            if (zipProcess.ExitCode != 0)
            {
                throw new Exception(string.Format("Could not package the folder {0}/{1}. The following error occurred: {2}, {3}\n", basePath, subFolder, output, errOut));
            }
        }

        private static string ZipArgs(string basePath, string subFolder, string filePattern, string zipFileFullPath, PlayerCompression useCompression)
        {
            filePattern = string.IsNullOrEmpty(filePattern) ? "**" : filePattern;
            return string.Format("{0} --output=\"{1}\" --basePath=\"{2}\" --relativePath=. \"{3}\" --compression={4}",
                                 ZipSubCommand,
                                 zipFileFullPath,
                                 Path.GetFullPath(basePath),
                                 PathUtil.EnsureTrailingSlash(subFolder) + filePattern,
                                 useCompression == PlayerCompression.Enabled);
        }
    }
}
