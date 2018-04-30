// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.IO;
using Improbable.Unity.Util;
using UnityEngine;

namespace Improbable.Unity.Core
{
    /// <summary>
    ///     Represents a subset of the information in a SpatialOS project's spatialos.json.
    /// </summary>
    public class ProjectDescriptor
    {
        public static readonly string ProjectDescriptorPath = PathUtil.Combine(Application.dataPath, "..", "..", "..", "spatialos.json");
        public static readonly string DefaultFieldValue = "invalid";

#pragma warning disable 649
        [SerializeField] private string name;
#pragma warning restore 649
        public string Name
        {
            get { return name; }
        }

#pragma warning disable 649
        // ReSharper disable once InconsistentNaming
        [SerializeField] private string sdk_version;
#pragma warning restore 649
        public string SdkVersion
        {
            get { return sdk_version; }
        }

        public ProjectDescriptor(string name, string sdkVersion)
        {
            this.name = name;
            this.sdk_version = sdkVersion;
        }

        public static ProjectDescriptor Load()
        {
            if (File.Exists(ProjectDescriptorPath))
            {
                var descriptorText = File.ReadAllText(ProjectDescriptorPath);
                return JsonUtility.FromJson<ProjectDescriptor>(descriptorText);
            }

            return new ProjectDescriptor(DefaultFieldValue, DefaultFieldValue);
        }
    }
}
