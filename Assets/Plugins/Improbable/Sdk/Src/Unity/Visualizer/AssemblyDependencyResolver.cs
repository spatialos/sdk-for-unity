// Copyright (c) Improbable Worlds Ltd, All Rights Reserved

using System.Collections.Generic;
using System.Reflection;

namespace Improbable.Unity.Visualizer
{
    class AssemblyDependencyResolver
    {
        /// <summary>
        ///     Given a rootAssembly and a list of all assemblies to consider, builds a dictionary that maps each assembly to
        ///     a boolean indicating whether or not it depends on the rootAssembly.
        /// </summary>
        internal static IDictionary<string, bool> GetAssemblyDependencyDictionary(Assembly rootAssembly, IList<Assembly> allAssemblies)
        {
            var dependenceDictionary = new Dictionary<string, bool>();
            var assemblyNameToAssemblyMap = CreateNameToAssemblyDictionary(allAssemblies);
            dependenceDictionary.Add(rootAssembly.GetName().FullName, true);
            for (int i = 0; i < allAssemblies.Count; ++i)
            {
                DependsOnRootAssembly(allAssemblies[i], dependenceDictionary, assemblyNameToAssemblyMap);
            }

            return dependenceDictionary;
        }

        private static IDictionary<string, Assembly> CreateNameToAssemblyDictionary(IList<Assembly> assemblies)
        {
            var assemblyNameToAssemblyMap = new Dictionary<string, Assembly>();
            for (int i = 0; i < assemblies.Count; ++i)
            {
                if (!assemblyNameToAssemblyMap.ContainsKey(assemblies[i].GetName().FullName))
                {
                    assemblyNameToAssemblyMap.Add(assemblies[i].GetName().FullName, assemblies[i]);
                }
            }

            return assemblyNameToAssemblyMap;
        }

        private static bool DependsOnRootAssembly(Assembly assembly, IDictionary<string, bool> dependenceDictionary, IDictionary<string, Assembly> assemblyNameToAssembly)
        {
            if (!dependenceDictionary.ContainsKey(assembly.GetName().FullName))
            {
                AnalyzeDependence(assembly, dependenceDictionary, assemblyNameToAssembly);
            }

            return dependenceDictionary[assembly.GetName().FullName];
        }

        private static void AnalyzeDependence(Assembly assembly, IDictionary<string, bool> dependenceDictionary, IDictionary<string, Assembly> assemblyNameToAssembly)
        {
            var assemblyName = assembly.GetName().FullName;
            dependenceDictionary.Add(assemblyName, false);
            var assemblyDependencies = assembly.GetReferencedAssemblies();
            var dependsOn = false;
            for (int i = 0; i < assemblyDependencies.Length; i++)
            {
                var assemblyDependencyName = assemblyDependencies[i].FullName;
                if (assemblyNameToAssembly.ContainsKey(assemblyDependencyName) && assemblyDependencyName != assemblyName)
                {
                    dependsOn |= DependsOnRootAssembly(assemblyNameToAssembly[assemblyDependencyName], dependenceDictionary, assemblyNameToAssembly);
                }
            }

            dependenceDictionary[assemblyName] = dependsOn;
        }
    }
}
