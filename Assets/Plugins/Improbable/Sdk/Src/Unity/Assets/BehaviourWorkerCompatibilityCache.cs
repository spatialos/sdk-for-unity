// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using Improbable.Unity.Visualizer;
using UnityEngine;

namespace Improbable.Unity.Assets
{
    class BehaviourWorkerCompatibilityCache
    {
        private readonly HashSet<Type> compatibleBehaviours;

        public BehaviourWorkerCompatibilityCache(WorkerPlatform platform)
        {
            compatibleBehaviours = new HashSet<Type>();
            var allTypes = AppDomain
                           .CurrentDomain
                           .GetAssemblies()
                           .SelectMany(assembly => assembly.GetTypes());
            foreach (var type in allTypes)
            {
                var attributes = type.GetCustomAttributes(typeof(WorkerTypeAttribute), false);
                if (typeof(MonoBehaviour).IsAssignableFrom(type))
                {
                    if (IsPlatformCompatible(attributes, platform))
                    {
                        compatibleBehaviours.Add(type);
                    }
                }
                else if (attributes.Length > 0)
                {
                    Debug.LogWarningFormat("{0} uses EngineTypeAttribute but is not MonoBehavoiur. The attribute will be ignored.", type.FullName);
                }
            }
        }

        public bool IsCompatibleBehaviour(Type behaviourType)
        {
            return compatibleBehaviours.Contains(behaviourType);
        }

        private static bool IsPlatformCompatible(object[] workerTypes, WorkerPlatform platform)
        {
            WorkerPlatform workerPlatformMask = 0;
            for (int i = 0; i < workerTypes.Length; i++)
            {
                workerPlatformMask |= ((WorkerTypeAttribute) workerTypes[i]).WorkerPlatform;
            }

            return workerTypes.Length == 0 || (workerPlatformMask & platform) != 0;
        }
    }
}
