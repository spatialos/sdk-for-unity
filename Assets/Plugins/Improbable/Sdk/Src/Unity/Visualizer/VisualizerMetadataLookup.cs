// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System;
using System.Collections.Generic;
using Improbable.Entity.Component;
using Improbable.Util.Injection;
using UnityEngine;

namespace Improbable.Unity.Visualizer
{
    sealed class VisualizerMetadataLookup
    {
        private static readonly VisualizerMetadataLookup ObjectInstance = new VisualizerMetadataLookup();

        private readonly InjectionCache visualizerInjectionCache = new InjectionCache(typeof(RequireAttribute));
        private readonly HashSet<Type> visualizers = new HashSet<Type>();
        private readonly HashSet<Type> readers = new HashSet<Type>();
        private readonly HashSet<Type> writers = new HashSet<Type>();
        private readonly Dictionary<Type, IList<IMemberAdapter>> visualizerRequiredReadersWriters = new Dictionary<Type, IList<IMemberAdapter>>();
        private readonly Dictionary<Type, IList<IMemberAdapter>> visualizerRequiredWriters = new Dictionary<Type, IList<IMemberAdapter>>();
        private readonly Dictionary<Type, IList<uint>> visualizerRequiredReaderStateIds = new Dictionary<Type, IList<uint>>();
        private readonly HashSet<Type> visualizersToNotAutoEnableOnStart = new HashSet<Type>();
        private readonly HashSet<Type> visualizersWithNonInjectableRequiredFields = new HashSet<Type>();
        private HashSet<Type> requiredTypesSeen;

        public static VisualizerMetadataLookup Instance
        {
            get { return ObjectInstance; }
        }

        private VisualizerMetadataLookup()
        {
            InitializeMetadata();
        }

        public IEnumerable<Type> AllRequiredTypes
        {
            get { return requiredTypesSeen; }
        }

        public bool IsVisualizer(Type visualizerType)
        {
            return visualizers.Contains(visualizerType);
        }

        public IList<IMemberAdapter> GetRequiredWriters(Type visualizerType)
        {
            return visualizerRequiredWriters[visualizerType];
        }

        public bool AreAllRequiredFieldsInjectable(Type visualizer)
        {
            return !visualizersWithNonInjectableRequiredFields.Contains(visualizer);
        }

        public IList<uint> GetRequiredReaderComponentIds(Type visualizerType)
        {
            return visualizerRequiredReaderStateIds[visualizerType];
        }

        public bool IsWriter(IMemberAdapter fieldInfo)
        {
            return writers.Contains(fieldInfo.TypeOfMember);
        }

        public bool IsReader(IMemberAdapter fieldInfo)
        {
            return readers.Contains(fieldInfo.TypeOfMember);
        }

        public IList<IMemberAdapter> GetRequiredReadersWriters(Type visualizerType)
        {
            return visualizerRequiredReadersWriters[visualizerType];
        }

        public bool DontEnableOnStart(Type visualizerType)
        {
            return visualizersToNotAutoEnableOnStart.Contains(visualizerType);
        }

        public IMemberAdapter GetFieldInfo(Type stateType, Type visualizerType)
        {
            return visualizerInjectionCache.GetAdapterForType(visualizerType, stateType);
        }

        private void InitializeMetadata()
        {
            var types = GetAssemblyTypes();
            requiredTypesSeen = new HashSet<Type>();
            for (int i = 0; i < types.Count; ++i)
            {
                InitializeMetadataForType(requiredTypesSeen, types[i]);
            }
        }

        private void InitializeMetadataForType(HashSet<Type> requiredTypesSeen, Type type)
        {
            if (IsVisualizerInternal(type))
            {
                visualizers.Add(type);
                if (Attribute.IsDefined(type, typeof(DontAutoEnableAttribute), true))
                {
                    visualizersToNotAutoEnableOnStart.Add(type);
                }

                InitializeMetadataForMembers(requiredTypesSeen, type, visualizerInjectionCache.GetAdaptersForType(type));
            }
            else
            {
                CheckForVisualizerOnlyAttributes(type);
            }
        }

        private void InitializeMetadataForMembers(HashSet<Type> requiredTypesSeen, Type type, IList<IMemberAdapter> requiredMembers)
        {
            var maxCount = requiredMembers == null ? 0 : requiredMembers.Count;
            List<IMemberAdapter> requiredReadersWriters = new List<IMemberAdapter>(maxCount);
            List<IMemberAdapter> requiredWriters = new List<IMemberAdapter>(maxCount);
            var readersStateIds = new List<uint>(maxCount);

            visualizerRequiredReadersWriters.Add(type, requiredReadersWriters);
            visualizerRequiredWriters.Add(type, requiredWriters);
            visualizerRequiredReaderStateIds.Add(type, readersStateIds);

            if (requiredMembers == null)
            {
                return;
            }

            for (int j = 0; j < requiredMembers.Count; ++j)
            {
                var requiredMember = requiredMembers[j];
                InitializeMetadataForMember(requiredTypesSeen, type, requiredMember, requiredReadersWriters, requiredWriters, readersStateIds);
            }
        }

        private void InitializeMetadataForMember(HashSet<Type> requiredTypesSeen, Type type, IMemberAdapter requiredMember, List<IMemberAdapter> requiredReadersWriters, List<IMemberAdapter> requiredWriters, List<uint> readersStateIds)
        {
            CacheReadersAndWriters(requiredTypesSeen, requiredMember);

            if (IsWriter(requiredMember))
            {
                requiredReadersWriters.Add(requiredMember);
                requiredWriters.Add(requiredMember);
            }
            else if (IsReader(requiredMember))
            {
                requiredReadersWriters.Add(requiredMember);
                var canonicalNameAttribute = Attribute.GetCustomAttribute(requiredMember.TypeOfMember, typeof(ComponentIdAttribute), false);
                if (canonicalNameAttribute != null)
                {
                    readersStateIds.Add(((ComponentIdAttribute) canonicalNameAttribute).Id);
                }
                else
                {
                    Debug.LogErrorFormat("Could not find state metadata for Reader {0}. This might cause issues with the state not being synchronised to the worker.", requiredMember.TypeOfMember.FullName);
                }
            }
            else
            {
                visualizersWithNonInjectableRequiredFields.Add(type);
                Debug.LogErrorFormat("The [Require] attribute can only be used on state Readers and Writers. {0} {1} is not one of those in visualizer {2}. The visualizer won't be enabled.",
                                     requiredMember.TypeOfMember.FullName, requiredMember.Member.Name, type.FullName);
            }
        }

        private void CacheReadersAndWriters(HashSet<Type> requiredTypesSeen, IMemberAdapter requiredMember)
        {
            if (requiredTypesSeen.Contains(requiredMember.TypeOfMember))
            {
                return;
            }

            requiredTypesSeen.Add(requiredMember.TypeOfMember);
            if (Attribute.IsDefined(requiredMember.TypeOfMember, typeof(WriterInterfaceAttribute), false))
            {
                writers.Add(requiredMember.TypeOfMember);
            }

            if (Attribute.IsDefined(requiredMember.TypeOfMember, typeof(ReaderInterfaceAttribute), false))
            {
                readers.Add(requiredMember.TypeOfMember);
            }
        }

        private void CheckForVisualizerOnlyAttributes(Type type)
        {
            if (Attribute.IsDefined(type, typeof(DontAutoEnableAttribute), false))
            {
                Debug.LogWarningFormat("{0} uses DontAutoEnableAttribute but is not a managed behaviour as it has no [Require] fields. The attribute will be ignored.", type.FullName);
            }
        }

        private IList<Type> GetAssemblyTypes()
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assembliesPotentiallyUsingRequire =
                AssemblyDependencyResolver.GetAssemblyDependencyDictionary(typeof(RequireAttribute).Assembly, allAssemblies);
            var types = new List<Type>();
            for (int i = 0; i < allAssemblies.Length; ++i)
            {
                if (!assembliesPotentiallyUsingRequire[allAssemblies[i].GetName().FullName])
                {
                    continue;
                }

                var assemblyTypes = allAssemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j)
                {
                    types.Add(assemblyTypes[j]);
                }
            }

            return types;
        }

        private bool IsVisualizerInternal(Type visualizerType)
        {
            if (visualizerType.IsAbstract || visualizerType.IsInterface)
            {
                return false;
            }

            var registered = TryRegisterVisualizer(visualizerType);
            return
                !registered //If registering failed, must have been a visualizer
                || visualizerInjectionCache.GetAdaptersForType(visualizerType) != null; // Any type with either worker or Required, or Data attributes
        }

        private bool TryRegisterVisualizer(Type visualizer)
        {
            try
            {
                visualizerInjectionCache.RegisterType(visualizer);
                return true;
            }
            catch (ArgumentException e)
            {
                Debug.LogErrorFormat(e.Message);
                visualizersWithNonInjectableRequiredFields.Add(visualizer);
                return false;
            }
        }

        public static IList<MonoBehaviour> ExtractVisualizers(GameObject gameObject)
        {
            var foundVisualizers = new List<MonoBehaviour>();
            if (gameObject != null)
            {
                var componentsInChildren = gameObject.GetComponentsInChildren<MonoBehaviour>();
                if (componentsInChildren == null)
                {
                    Debug.LogErrorFormat("GetComponentsInChildren returned null for GameObject: {0}", gameObject.name);
                }
                else
                {
                    for (int index = 0; index < componentsInChildren.Length; index++)
                    {
                        var visualizer = componentsInChildren[index];
                        if (visualizer == null)
                        {
                            Debug.LogErrorFormat("GetComponentsInChildren returned a null element for GameObject {0}", gameObject.name);
                            continue;
                        }

                        var visualizerType = visualizer.GetType();
                        if (Instance.IsVisualizer(visualizerType))
                        {
                            foundVisualizers.Add(visualizer);
                        }
                    }
                }
            }

            return foundVisualizers;
        }
    }
}
