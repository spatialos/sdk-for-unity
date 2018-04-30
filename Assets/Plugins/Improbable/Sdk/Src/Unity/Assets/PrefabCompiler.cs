// Copyright (c) Improbable Worlds Ltd, All Rights Reserved


using Improbable.Unity.Export;
using Improbable.Unity.Visualizer;
using UnityEngine;

namespace Improbable.Unity.Assets
{
    public class PrefabCompiler
    {
        private readonly BehaviourWorkerCompatibilityCache compatibilityCache;
        private readonly WorkerPlatform workerPlatform;

        public PrefabCompiler(WorkerPlatform workerPlatform)
        {
            this.workerPlatform = workerPlatform;
            compatibilityCache = new BehaviourWorkerCompatibilityCache(workerPlatform);
        }

        public void Compile(GameObject prefab)
        {
            CompileRecursively(prefab);
        }

        private void CompileRecursively(GameObject prefab)
        {
            InvokePrefabExportProcessors(prefab);
            DisableVisualizers(prefab);
            DisableWrongPlatformMonoBehaviours(prefab);
            ExportProcessChildren(prefab);
        }

        private void DisableWrongPlatformMonoBehaviours(GameObject prefab)
        {
            var components = prefab.GetComponents<MonoBehaviour>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] != null && !compatibilityCache.IsCompatibleBehaviour(components[i].GetType()))
                {
                    GameObject.DestroyImmediate(components[i], true);
                }
            }
        }

        private void InvokePrefabExportProcessors(GameObject prefab)
        {
            var components = prefab.GetComponents<MonoBehaviour>();

            foreach (var component in components)
            {
                if (component is IPrefabExportProcessor)
                {
                    var processor = component as IPrefabExportProcessor;
                    processor.ExportProcess(workerPlatform);

                    if (ShouldRemoveFromPrefab(processor))
                    {
                        Object.DestroyImmediate(component, true);
                    }
                }
            }
        }

        private static bool ShouldRemoveFromPrefab(object exportProcessor)
        {
            var attributes = exportProcessor.GetType().GetCustomAttributes(typeof(KeepOnExportedPrefabAttribute), true);
            return attributes.Length == 0;
        }

        private static void DisableVisualizers(GameObject prefab)
        {
            var components = prefab.GetComponents<MonoBehaviour>();

            foreach (var component in components)
            {
                if (component == null)
                {
                    continue;
                }

                if (VisualizerMetadataLookup.Instance.IsVisualizer(component.GetType()))
                {
                    component.enabled = false;
                }
            }
        }

        private void ExportProcessChildren(GameObject prefab)
        {
            var prefabTransform = prefab.transform;
            if (prefabTransform.childCount > 0)
            {
                foreach (Transform childTransform in prefabTransform)
                {
                    CompileRecursively(childTransform.gameObject);
                }
            }
        }
    }
}
