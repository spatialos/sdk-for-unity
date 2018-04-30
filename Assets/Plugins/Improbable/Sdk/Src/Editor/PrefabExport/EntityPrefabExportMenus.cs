// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using UnityEditor;

namespace Improbable.Unity.EditorTools.PrefabExport
{
    public class EntityPrefabExportMenus
    {
        /// <summary>
        ///     This is called whenever entity prefabs need to be exported for all build targets.
        ///     This can be done from within the editor, or from external sources like build systems.
        ///     By default its value is the baseline behaviour, which can be saved off and invoked
        ///     as part of a custom chain of events.
        /// </summary>
        public static Action OnExportAllEntityPrefabs = EntityPrefabExporter.ExportAllEntityPrefabs;

        /// <summary>
        ///     This is called whenever entity prefabs need to be exported for development targets.
        ///     This can be done from within the editor, or from external sources like build systems.
        ///     By default its value is the baseline behaviour, which can be saved off and invoked
        ///     as part of a custom chain of events.
        /// </summary>
        public static Action OnExportDevelopmentEntityPrefabs = EntityPrefabExporter.ExportDevelopmentEntityPrefabs;

        /// <summary>
        ///     This is called when we need to export the entity prefabs that are currently selected in the Project view.
        /// </summary>
        public static Action OnExportSelectedEntityPrefabs = EntityPrefabExporter.ExportSelectedEntityPrefabs;

        /// <summary>
        ///     This is called when we need to export the entity prefabs that are currently selected in the Project view.
        /// </summary>
        public static Action OnExportSelectedDevelopmentEntityPrefabs = EntityPrefabExporter.ExportSelectedDevelopmentEntityPrefabs;

        /// <summary>
        ///     This is called whenever entity prefabs need to be cleaned.
        ///     This can be done from within the editor, or from external sources like build systems.
        ///     By default its value is the baseline behaviour, which can be saved off and invoked
        ///     as part of a custom chain of events.
        /// </summary>
        public static Action OnCleanEntityPrefabs = EntityPrefabDirectoryCleaner.CleanPrefabTargetDirectories;

        [MenuItem("Improbable/Prefabs/Clean entity prefabs")]
        public static void CleanAllEntityPrefabs()
        {
            OnCleanEntityPrefabs();
        }

        [MenuItem("Improbable/Prefabs/Export all entity prefabs %&#E")]
        [MenuItem("Assets/Export all entity prefabs %&#E")]
        public static void ExportAllEntityPrefabs()
        {
            OnExportAllEntityPrefabs();
        }

        [MenuItem("Improbable/Prefabs/Export development entity prefabs")]
        [MenuItem("Assets/Export development entity prefabs")]
        public static void ExportDevelopmentEntityPrefabs()
        {
            OnExportDevelopmentEntityPrefabs();
        }

        [MenuItem("Improbable/Prefabs/Export selected entity prefabs %&#S")]
        [MenuItem("Assets/Export selected entity prefabs %&#S")]
        public static void ExportSelectedEntityPrefabs()
        {
            OnExportSelectedEntityPrefabs();
        }

        [MenuItem("Improbable/Prefabs/Export selected development entity prefabs")]
        [MenuItem("Assets/Export selected development entity prefabs")]
        public static void ExportSelectedDevelopmentEntityPrefabs()
        {
            OnExportSelectedDevelopmentEntityPrefabs();
        }

        [MenuItem("Improbable/Prefabs/Export selected entity prefabs %&#S", true)]
        [MenuItem("Assets/Export selected entity prefabs %&#S", true)]
        private static bool ExportSelectedEntityPrefabsValidation()
        {
            return EntityPrefabExporter.AnyPrefabsSelected();
        }
    }
}
