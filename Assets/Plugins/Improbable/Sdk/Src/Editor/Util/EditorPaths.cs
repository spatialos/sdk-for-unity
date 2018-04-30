// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


using Improbable.Unity.Util;

namespace Improbable.Unity.EditorTools.Util
{
    /// <summary>
    ///     Contains common directories related to building assets and players.
    /// </summary>
    /// <remarks>
    ///     All directories should be in Unity path format e.g. "Foo/Bar".
    /// </remarks>
    public static class EditorPaths
    {
        public static readonly string OrganizationName = "Improbable";

        public static readonly string PluginDirectory = PathUtil.Combine("Assets", "Plugins").ToUnityPath();

        public static readonly string DataDirectory = PathUtil.Combine("..", "..", "build").ToUnityPath();
        public static readonly string AssetDatabaseDirectory = PathUtil.Combine(DataDirectory, "assembly").ToUnityPath();
        public static readonly string AssetDirectory = PathUtil.Combine(PluginDirectory, OrganizationName).ToUnityPath();

        public static readonly string PrefabCompileDirectory = PathUtil.Combine(AssetDirectory, "EntityPrefabs").ToUnityPath();
        public static readonly string PrefabExportDirectory = PathUtil.Combine(AssetDatabaseDirectory, "unity").ToUnityPath();
        public static readonly string PrefabResourcesDirectory = PathUtil.Combine("Assets", "Resources", "EntityPrefabs").ToUnityPath();
        public static readonly string PrefabSourceDirectory = PathUtil.Combine("Assets", "EntityPrefabs").ToUnityPath();

        public static readonly string ScriptAssembliesDirectory = PathUtil.Combine("Library", "ScriptAssemblies").ToUnityPath();

        public static readonly string CodeGeneratorScratchDirectory = ".spatialos";
    }
}
