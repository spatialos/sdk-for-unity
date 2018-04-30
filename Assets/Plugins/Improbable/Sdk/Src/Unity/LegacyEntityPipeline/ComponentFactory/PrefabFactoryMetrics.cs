// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using Improbable.Unity.Entity;
using Improbable.Unity.Metrics;
using UnityEngine;

namespace Improbable.Unity.ComponentFactory
{
    /// <summary>
    ///     A Proxy to wrap a IPrefabFactroy such that we report metrics about
    ///     the number of entities in a worker by prefab
    ///     metrics are named "prefab.{prefabName}.count"
    /// </summary>
    public class PrefabFactoryMetrics : IPrefabFactory<GameObject>
    {
        private readonly IPrefabFactory<GameObject> prefabFactory;
        private readonly WorkerMetrics metrics;

        public PrefabFactoryMetrics(IPrefabFactory<GameObject> prefabFactory, WorkerMetrics metrics)
        {
            this.prefabFactory = prefabFactory;
            this.metrics = metrics;
        }

        /// <inheritdoc />
        public GameObject MakeComponent(GameObject prefabGameObject, string prefabName)
        {
            var name = GetPrefabsGauge(prefabName);
            metrics.IncrementGauge(name);

            return prefabFactory.MakeComponent(prefabGameObject, prefabName);
        }

        /// <inheritdoc />
        public void DespawnComponent(GameObject gameObject, string prefabName)
        {
            var name = GetPrefabsGauge(prefabName);
            metrics.DecrementGauge(name);

            prefabFactory.DespawnComponent(gameObject, prefabName);
        }

        private string GetPrefabsGauge(string prefabName)
        {
            return "prefab." + prefabName + ".count";
        }
    }
}
