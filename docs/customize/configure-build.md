# Configuring the Unity build process

> A new build system (currently in **beta**) was introduced in SpatialOS 12.2. This page is about the system that existed before that. To try out the new system, see [Using the new build system](../customize/minimal-build.md).

## Auto-patching workers

When `Improbable -> Autopatch workers on editor reload` is enabled, any workers that you have already built
are automatically repackaged when you make changes in your code editor and then return to Unity (triggering a Unity
Editor script reload). This saves the need to run a full build.

If you change any assets (for example: prefabs, textures, sounds), you'll need to run the build commands mentioned above.

## Build configuration

The build configuration file (`player-build-config.json` in the Unity Assets directory) describes:

* the global Unity build configuration
* which Unity workers are built for Development
* which Unity workers are built for Deployment

This file is generated when you select `Improbable -> Build development workers` or
`Improbable -> Build deployment workers`.

By default, the generated file looks like this:

```json
{
  "Deploy": {
    "UnityWorker": {
      "Targets": [
        "StandaloneLinux64?EnableHeadlessMode"
      ],
      "Assets": "Streaming"
    },
    "UnityClient": {
      "Targets": [
        "StandaloneWindows",
        "StandaloneOSXIntel64"
      ],
      "Assets": "Streaming"
    }
  },
  "Develop": {
    "UnityWorker": {
      "Targets": [
        "Current"
      ],
      "Assets": "Streaming"
    },
    "UnityClient": {
      "Targets": [
        "Current"
      ],
      "Assets": "Streaming"
    }
  }
}
```

What the sections mean:

* `Global` specifies where the Improbable plugins for Unity go: in Unity's root plugin
directory (`Assets/Plugins`), or in its platform sub-directories (`x86` or `x86_64`) when appropriate.

    See the [Unity docs on Plugins for Desktop](https://docs.unity3d.com/Manual/PluginsForDesktop.html) for more information.

* `Develop` specifies (for local development) which workers will be built, for which OS, and which asset database is used.
The default is:
    * build a single UnityWorker for the *current* OS (either Windows or macOS)
    * use the streaming asset database (see [Loading assets](#loading-assets) below)

* `Deploy` specifies (for cloud deployment) which workers will be built, for which OS, and which asset database is used.
The default is:
    * build a single UnityWorker for the current OS

     > SpatialOS currently only works with a 64-bit Linux headless environment for
     UnityWorkers in the cloud, and Windows and macOS for UnityClients.

    * use the streaming asset database (see [Loading assets](#loading-assets) below)

### Options

The following options are available for `Targets` and `Assets`.

`Targets`:

* Current - build for the current OS
* StandaloneLinux64
* StandaloneWindows
* StandaloneWindows64
* StandaloneOSXIntel64

To pass a [Unity BuildOption](https://docs.unity3d.com/ScriptReference/BuildOptions.html) to a target, add it with
a question mark as a delimiter. For example:

```json
 "Targets": [ "StandaloneLinux64?EnableHeadlessMode" ],
```

`Assets`:

* Streaming
* Local

We recommend switching to the `Local` option. See [Loading assets](#loading-assets) below for an explanation.

## Customizing how workers are built and packaged

To change how workers are built and packaged, derive from and implement
the `Improbable.Unity.EditorTools.Build.IPlayerBuildEvents` interface to perform custom behaviour.

To install custom build behaviour, provide a function that creates a new instance.

For example:
```c#
using UnityEditor;
using Improbable.Unity.EditorTools.Build;

[InitializeOnLoad]
internal class CustomPlayerBuildEvents : IPlayerBuildEvents
{
    #region Implement IPlayerBuildEvents
    // ...
    #endregion

    static CustomPlayerBuildEvents()
    {
        // Install the custom event handler here.
        SimpleBuildSystem.CreatePlayerBuildEventsAction = () => new CustomPlayerBuildEvents();
    }
}
```

Each time you use `Build development workers` or `Build deployment workers`, `SimpleBuildSystem.CreatePlayerBuildEventsAction`
is called to create a new event handler.

The following methods of your class are called at specific points in the build process:

* `BeginBuild()` - Called before any workers are built. Use to do preliminary work such as saving scenes.
* `EndBuild()` - Called after all workers are built, even if errors occurred. Use to clean up anything done in `BeginBuild`.

Then, between the calls to `BeginBuild()` and `EndBuild()`, the following are called for each type of worker being built:

* `GetScenes()` - Load and optionally modify scenes, then return an array of scene paths to be built into the worker.
For information about how the returned array of scenes is used by Unity's build process, see the reference for
`UnityEditor.BuildPlayerOptions.scenes`.
* `BeginPackage()` - Do work such as copying additional files that need to be packaged with the worker.

### Default worker building

If you have a simple setup and don't want to do anything complicated with building or packaging, use
the `Improbable.Unity.EditorTools.Build.SingleScenePlayerBuildEvents` class as a basis for customization.

> `SingleScenePlayerBuildEvents` is the behaviour used by default.

It will open a single scene for each worker type, with the following defaults:

* `UnityClient` will open `Assets/UnityClient.unity`
* `UnityWorker` will open `Assets/UnityWorker.unity`

You can change the scenes that are opened by modifying `SingleScenePlayerBuildEvents.WorkerToScene`:

```c#
using Improbable.Unity.EditorTools.Build;

using UnityEditor;
using Improbable.Unity.EditorTools.Build;
using Improbable.Unity.Util;

[InitializeOnLoad]
internal class CustomSinglePlayerBuildEvents : IPlayerBuildEvents
{
    #region Implement IPlayerBuildEvents
    // ...
    #endregion

    static CustomSinglePlayerBuildEvents()
    {
        SimpleBuildSystem.CreatePlayerBuildEventsAction = () =>
        {
            var singleScene = new SingleScenePlayerBuildEvents();
            singleScene.WorkerToScene[WorkerPlatform.UnityClient] = PathUtil.Combine("Assets", "Client", "UnityClientMainMenu.unity");
            singleScene.WorkerToScene[WorkerPlatform.UnityWorker] = PathUtil.Combine("Assets", "Worker", "UnityWorker.unity");

            return singleScene;
        };
    }
}
```

You can also provide your own behaviour by overriding methods on `SingleScenePlayerBuildEvents`:

```c#
using Improbable.Unity.EditorTools.Build;
internal class SceneProcessor : SingleScenePlayerBuildEvents
{
    #region Implement IPlayerBuildEvents
    // ...
    #endregion

    public override void ProcessScene(string sceneName)
    {
        // ... Modify the scene ...
    }
    public override void BeginPackage(WorkerPlatform workerType, BuildTarget target, Config config, string packagePath)
    {
        // ... Add additional files to the packagePath
    }
}
```

For an example, see [Loading from multiple scenes](../customize/multiple-scenes.md).

## Exporting entity prefabs into the assembly

The menu `Improbable -> Prefabs -> Export All EntityPrefabs` runs the entity export method.

This method puts files into the `<project root>/build/assembly/` directory; when you run `spatial cloud upload`, the
contents of this directory are uploaded so they can be used by a
[cloud deployment (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#cloud-deployment). For more information
on deploying, see [Deploying to the cloud (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/deploy/deploy-cloud).

### Customizing the exporting of prefabs

By default, prefabs are simply bundled up. If you need to do more with your prefabs, customize the
asset cleaning and exporting pipeline by assigning your own behaviour to
`EntityPrefabExportMenus.OnExportEntityPrefabs` and `EntityPrefabExportMenus.OnCleanEntityPrefabs`.

For example, you might want to modify exported UnityWorker assets to remove references to textures and render meshes.

Do this customization at static loadtime in Unity, so that your custom behaviour will be invoked properly
by any external build systems. Place your custom behaviour under `Assets/Editor`.

```csharp
    // This attribute ensures that the class is initialized when the editor starts up.
    [InitializeOnLoad]
    public static class CustomPrefabExporter
    {
        static CustomPrefabExporter()
        {
            var defaultHandler = EntityPrefabExportMenus.OnExportEntityPrefabs;

            EntityPrefabExportMenus.OnExportEntityPrefabs = () =>
            {
                Debug.Log("Invoking the custom prefab exporter...");
                defaultHandler();
            };
        }
    }
```

## The assembly and its contents

When you build (eg if you run `spatial worker build`), this creates an assembly, which is used by SpatialOS to run a deployment. It
includes meshes, textures and game object definitions (prefabs), world schemas, and packaged executables of your workers.

Some of the assets in the assembly are needed by SpatialOS (like the executable package for a worker), others by a UnityWorker
itself (like colliders for the game objects), and others only by UnityClients (like most textures). Some assets
will be needed in more than one place.

### Assets used by SpatialOS

The main asset used by SpatialOS is a worker executable, of which there are two types for Unity: a `DownloadableUnityWorkerEngine`
and a `DownloadableUnityClientEngine`.  When SpatialOS needs to start a new UnityWorker, it ensures that one has been downloaded
from a URL and runs it.

### Loading assets

UnityClients and UnityWorkers need to load assets (for example, Unity prefabs, or materials). The UnitySDK stores these assets
within an asset database, which is loaded at runtime.

The UnitySDK provides two basic ways of serving your assets:

#### Local asset database

If you use the `Local` asset database, workers will load asset bundles from a directory
on disk. Assets are bundled into the worker executable, which means workers don't need to download prefab information
at entity spawn time.

Set this in the [launch section of the worker configuration file (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/worker-configuration/launch-configuration)
(eg `spatialos.UnityClient.worker.json`) in the `arguments` section, for example:

```
"arguments": [
    "+assetDatabaseStrategy",
    "Local",
```

To configure where the asset database is located, add `+localAssetDatabasePath path_to_asset_database` as an argument
in the same file.

#### Streaming asset database

If you use the `Streaming` asset database, workers will download asset bundles over HTTP from a configured
URL. This has the advantage that assets can be deployed separately to the workers, reducing the size of the worker executable.
However, it might take longer to load initially, depending on internet speed, and might be more costly in certain cases.

This is the default asset database, but we recommend switching to the local asset database.

To explicitly use the streaming asset database, set this in the
[launch section of the worker configuration file (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/worker-configuration/launch-configuration)
(eg `spatialos.UnityClient.worker.json`) in the `arguments` section, for example:

```
"arguments": [
    "+assetDatabaseStrategy",
    "Streaming",
```

#### Configuring the DefaultEntityTemplateProvider

If you want to configure the `DefaultTemplateProvider` directly rather than via the command line, you can add it to
the root `GameObject` (the one passed to `SpatialOS.Connect()`) and apply any configuration necessary. This
has to be done before `SpatialOS.Connect()` is called. Note that any values provided by the command line will
override values set directly on the `DefaultEntityTemplateProvider` object.

### Custom asset loading strategies

Both of the above asset database types work for deployed UnityClients and managed UnityWorkers, running in the cloud
and in development. However, the Unity asset bundles that the asset databases serve up contain less information
than the Unity prefabs that exist in the editor. Sometimes you'll want to use these prefabs rather than the served assets, or
customize the asset loading strategy in a different way.

To customize the instantiation of `GameObject`s for entities and where to obtain assets and prefabs, implement an
`IEntityTemplateProvider` and set `SpatialOS.TemplateProvider` accordingly. Add the implementation as an instance to the
root `GameObject` passed to `SpatialOS.Connect()`.

The `IEntityTemplateProvider` provides functionality for loading and obtaining loaded prefabs. The interface
provides two methods:

```csharp
void PrepareTemplate(EntityAssetId assetId, Action onSuccess, Action<Exception> onError);
GameObject GetEntityTemplate(EntityAssetId assetId);
```

`PrepareTemplate` will always be called before `GetEntityTemplate` with the same `EntityAssetId`.
Note that it is correct to call `PrepareTemplate` once, followed by an arbitrary number of calls to `GetEntity` with
the same argument.
If you are calling an `IEntityTemplateProvider` yourself, you must also adhere to this protocol.

The `GetEntityTemplate` method can be used to obtain a loaded prefab template for instantiation. If it is unable to get
the entity prefab template, it will throw an exception; otherwise, it returns a prefab which can be subsequently
instantiated for use in game. To place the asset into your scene, you should use
[`Object.Instantiate(prefabGameObject)`](http://docs.unity3d.com/ScriptReference/Object.Instantiate.html).
