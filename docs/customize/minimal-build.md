> The new build system is currently in **beta**, and isn't fully tested yet.
Feel free to try it out and let us know how you get on.

# Using the new build system

The new build system (the minimal build system) is a full rewrite of our current system. It is intended to help quick iteration, and to be simple enough to serve as an example of how to create your own build system.

It has the following differences compared to the old system:

* Faster builds
  * Asset bundles are no longer enforced for entity prefabs, allowing Unity to build everything in one single pass.
  * This means that shared assets are no longer duplicated between multiple entity prefabs.
* Better runtime performance
  * Prefab processors are run at build time, allowing you to strip assets and components per worker type.
  * No need to precache asset bundles, as everything is packed with the worker.
* More secure
  * [Worker-specific symbols](#worker-symbols) allow for excluding sensitive bits of code from specific workers.
* Simpler configuration

This system does have a few downsides:

* Not compatible with the SpatialOS Window (yet).
* Not compatible with [autopatching of workers](../customize/configure-build.md#auto-patching-workers).
* No hooks to change steps in the build process (see [Customising the build system](#customising-the-build-system)).

> If you switch to using the new build system, you'll easily be able to switch back while the new system is in beta.

## Getting started

To set up Unity to use the new build system:

1. In the `spatialos.*.worker.json` files for both the UnityClient and UnityWorker, change the value of [`tasks_filename` (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/worker-configuration/worker-build#using-custom-build-scripts):
  * For `spatialos.UnityClient.worker.json`, change `spatialos.unity.client.build.json` to `Improbable/build_script/spatialos.unity.client.build.experimental.json`
  * For `spatialos.UnityWorker.worker.json`, change `spatialos.unity.worker.build.experimental.json` to `Improbable/build_script/spatialos.unity.worker.build.experimental.json`
1. In the root folder of your project, run [`spatial codegen` (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/spatial-cli/spatial-worker-codegen#spatial-worker-codegen).
1. Open your project in Unity.
1. Open the UnityClient scene, and select the "Bootstrap" or "GameEntry" `GameObject` in the hierarchy.
1. Remove the `DefaultTemplateProvider` component if it is there, then add the `BasicTemplateProvider` component.
1. Repeat steps 4 and 5 for the UnityWorker scene.
1. In the Unity menu, go to **Assets -> Create -> SpatialOS -> Build Configuration**.
1. Open the newly created asset in the Unity inspector and ensure that, in both UnityClient and UnityWorker, every scene you want Unity to build is in the "Scenes to include (in order)" column. Scene 0 will be loaded first.

## Building your workers

There are two ways to build your workers:

* In the Unity menu, click **Improbable -> Experimental build -> Build ...**
  * You can pick which worker(s) you want to build, and the environment you want to build for.
  * The console will print a log after the build has completed.

* From a command prompt in your project folder
  * With Unity closed, run [`spatial worker build --target=local` (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/spatial-cli/spatial-worker-build#spatial-worker-build)

## Configuring your build

You can use the build configuration asset created in [Getting started](#getting-started) to configure how the workers should be built.

### Local and cloud environment

Use the local and cloud environments to have different build settings for [local (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#local-deployment) and [cloud (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#cloud-deployment) deployments. Both environments will produce the same results if they have the same settings, with the exception of the compression on the final zip file: to save build time, the local environment will not enable compression.

> 'local' and 'cloud' environments were previously called 'development' and 'deployment'.

### Scenes

Both the UnityClient and UnityWorker sections have a list of scenes that will be included in the final build. These scenes are ordered similarly to how the standard [Unity build settings](https://docs.unity3d.com/Manual/BuildSettings.html) work.
You can use the arrow buttons next to each scene to reorder them.

### Build platforms

Use the build platform section in the Unity inspector to specify which platforms a worker will be built for. By default you will only build for the platform you're developing on. You may want to change this if, for example, you were building a UnityWorker to run in the cloud, where you would usually just build for Linux.

You can select multiple platforms at the same time, but you can only select one Windows target, because SpatialOS can't differentiate between 32-bit and 64-bit Windows in the Launcher.

The `Current` platform will automatically build for the platform your machine is running.

### Build options

Build options are all the available build options provided by Unity. The [Unity Reference](https://docs.unity3d.com/ScriptReference/BuildOptions.html) lists all the available options.

## Reference information

### Worker symbols

When building, we provide the following symbols:

* `IMPROBABLE_WORKERTYPE_UNITYCLIENT`
* `IMPROBABLE_WORKERTYPE_UNITYWORKER`

When building we will automatically define a symbol during compilation, per worker type that is built. If you have a section of code that should only be included for a specific worker type, you can exclude it from all others.

This code example show you two methods for excluding a section:

```csharp
[Conditional("UNITY_EDITOR"), Conditional("IMPROBABLE_WORKERTYPE_UNITYWORKER")]
void SomeSecretFunction() {
    // ...
}

void SomeOtherSecretFunction() {
#if UNITY_EDITOR || IMPROBABLE_WORKERTYPE_UNITYWORKER
    // ...
#endif
}
```

> Always include the `UNITY_EDITOR` symbol in these checks, as the worker-specific symbols are not set when running code from the editor.

### Prefab export processors

Prefab export processors are special `MonoBehaviour` components that can be added on prefabs in the `EntityPrefab` folder, and will allow you to modify the prefab when building for a specific worker.
You can create these processors by implementing the `Improbable.Unity.Export.IPrefabExportProcessor` interface.
The processor itself will not be present on the final processed prefab unless tagged with the `Improbable.Unity.Export.KeepOnExportedPrefab` attribute.

For example, you could use a processor to remove all `Renderer` components of your entity prefabs when building a UnityWorker.

```csharp
using Improbable.Unity.Export;

public class RendererProcessor : MonoBehaviour, IPrefabExportProcessor {
    public void ExportProcess(WorkerPlatform worker) {
        if (worker == WorkerPlatform.UnityWorker) {
            var renderers = GetComponents<MeshRenderer>();
            foreach (var renderer in renderers) {
                DestroyImmediate(renderer);
            }
        }
    }
}
```

### Customising the build system

Currently you can only customise the build system by editing the source code. Copy it from the SDK folder and remove `Improbable.MinimalBuildSystem` as a dependency from the `spatialos_worker_packages.json`. After copying, any changes you make won't be overwritten when building or running codegen.

> Changes to the system might require changes in the `spatialos.*.build.json` files of your Unity workers as well, as these will invoke the provided build functions.

## Reverting to the old system

You can revert to the old system using the following steps:

1. Delete the build configuration asset that was created in step 7 of [Getting Started](#getting-started).
1. Revert the changes to the UnityClient and UnityWorker scenes.
1. Revert the changes to the `spatialos.*.worker.json` files.
1. Run [`spatial codegen` (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/spatial-cli/spatial-worker-codegen#spatial-worker-codegen) in your project's root folder.
