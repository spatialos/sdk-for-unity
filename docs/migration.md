# Unity SDK:
# Upgrading to SpatialOS 13.0
# Migrating to the SpatialOS Unity SDK 1.0.0

GitHub repository: [github.com/spatialos/UnitySDK](https://github.com/spatialos/UnitySDK)

**NOTE**: The steps below tell you how to migrate from an earlier version of the SpatialOS Unity SDK
(before SpatialOS 13.0). If you are installing SpatialOS and the Unity SDK for the first time,
see: [Introduction to the SpatialOS Unity SDK](introduction.md).

SpatialOS 13.0 splits SpatialOS and the SpatialOS Unity SDK to make SpatialOS available as a separate product to the SpatialOS Unity SDK.
At the time of initial release, the functionality of SpatialOS 13.0 and the SpatialOS Unity SDK 1.0.0 is the same as the combined SpatialOS and SpatialOS Unity SDK 12.2.1. Subsequent patch releases may make the functionality of the versions slightly different.
If you want to use the SpatialOS Unity SDK 1.0.0, you need to upgrade your SpatialOS version to 13.0 and
migrate your Unity SDK version to 1.0.0.

Follow the steps below for every SpatialOS Unity project you want to upgrade and migrate.

## Quick guide
1. Run `spatial clean`.
1. Clone the [SpatialOS Unity SDK GitHub repository](https://github.com/spatialos/UnitySDK).
1. From the cloned repository, move the `Assets` and `Improbable` directories into your project’s Unity worker directory (this is usually `<root>/workers/unity`).
	If you are asked whether to merge or replace the `Assets` folder, select merge, otherwise you will delete the assets that your game depends upon.
1. Still in the Unity worker directory, edit the `spatialos.UnityClient.worker.json` file to remove `"generated_build_scripts_type":"unity"` completely
and to replace `"spatialos.unity.client.build.json"` with `"Improbable/build_script/spatialos.unity.client.build.json".`
1. Edit the `spatialos.UnityWorker.worker.json` file to remove  `"generated_build_scripts_type":"unity"` completely
and to replace `"spatialos.unity.worker.build.json"` with `"Improbable/build_script/spatialos.unity.worker.build.json".`
1. Delete the `spatialos_worker_packages.json` file.
1. In the root of your project, edit the `spatialos.json` file in two places so that `"version"` is `“13.0.0”`.
1. Run `spatial clean` (again).
1. Run `spatial worker build`.
1. In your project’s root directory and `workers/unity` directory, edit the following lines in the version control system (VCS) ignore files:
    * Delete
        * `Assets/Plugins/Improbable`
        * `Improbable.meta`
        * `spatialos.*.build.json`
    * Add 
        * `workers/unity/Assets/Plugins/Improbable/Editor/Generated/`
        * `workers/unity/Assets/Plugins/Improbable/Editor/Generated.meta`
        * `workers/unity/Assets/Plugins/Improbable/Generated/`
        * `workers/unity/Assets/Plugins/Improbable/Generated.meta`
        * `workers/unity/Assets/Plugins/Improbable/Sdk/Dll/Generated.Code.*`

**Note:** All our starter projects the VCS ignore files are not set as above, so if your project is based on any of these starter projects, you need to edit the VCS ignore files as described in step 10.


## Detailed guide

### 1. Clean up your existing project
1. Open a terminal window and `cd` to the root directory of your project.
2. Run `spatial clean`.
Note: It’s very important you start by running `spatial clean`. If you don’t, files won’t be cleaned up properly and
may cause issues with the new version.

### 2. Clone or download the SpatialOS Unity SDK (1.0.0)
Get the SpatialOS Unity SDK by cloning the [SpatialOS Unity SDK GitHub repository](https://github.com/spatialos/UnitySDK).
&nbsp;
Either follow the **Clone or download** instructions on the web page or clone using the command line.
(See the [git-scm](https://git-scm.com/book/en/v2/Git-Basics-Getting-a-Git-Repository) website for
guidance on setting up and using command-line git.)
&nbsp;

For command-line git, in a terminal window run:
`git clone github.com/spatialos/UnitySDK`

### 3. Upgrade your Unity project
1. From the cloned repository, copy the contents of the `Assets` and `Improbable` directories into the directory
which contains your Unity project’s workers. (It is the directory which contains the `spatialos_worker_packages.json` file.)
&nbsp;
For example:
`~/mySpatialOSgame/workers/unity`

2. In the same directory, edit the `spatialos.UnityClient.worker.json` file to remove `"generated_build_scripts_type":"unity"` completely
and to replace `"spatialos.unity.client.build.json"` with `"Improbable/build_script/spatialos.unity.client.build.json".`
This part of the file should now look like this:
```
"build": {
    "tasks_filename": "Improbable/build_script/spatialos.unity.client.build.json"
  },
```

3. In the same directory, edit the `spatialos.UnityWorker.worker.json` file to remove  `"generated_build_scripts_type":"unity"` completely
and to replace `"spatialos.unity.worker.build.json"` with `"Improbable/build_script/spatialos.unity.worker.build.json".`
This part of the file should now look like this:
```
{
  "build": {
    "tasks_filename": "Improbable/build_script/spatialos.unity.worker.build.json",
  },
  ```
  
4. Delete `spatialos_worker_packages.json` as `spatial` no longer uses this file.

5. Navigate two directories up to find the `spatialos.json` file.
Edit the `spatialos.json` file so that the `"version"` is `“13.0.0”` and save the file. Note that there are two
places to edit the version.
The file should now look similar to this:
```
{
    "name": "mySpatialOSgame",
    "project_version": "1.0.0",
    "sdk_version": "13.0.0",
    "dependencies": [
        {"name": "standard_library", "version": "13.0.0"}
    ]
}
```

6. In the root directory of your project, run `spatial clean` (again).

### 5. Update the version control ignore files

Make sure your version control system (VCS) is set to; **stop ignoring** the Unity SDK directories you have copied, specifically the directories under `Assets/Plugins/Improbable`, and **ignore** some auto-generated files.
1. Locate the VCS ignore files in your project’s root directory and `workers\unity` directory. 
* For example, the VCS ignore files to check on the Pirates project on GitHub are:
    * in the `workers/unity` directory - [github.com/spatialos/PiratesTutorial/blob/master/workers/unity/.gitignore](https://github.com/spatialos/PiratesTutorial/blob/master/workers/unity/.gitignore)
    * in the root directory - [github.com/spatialos/PiratesTutorial/blob/master/.gitignore](https://github.com/spatialos/PiratesTutorial/blob/master/.gitignore)

2. Delete the following lines from the ignore files:
    * `Assets/Plugins/Improbable`
    * `Improbable.meta`
    * `spatialos.*.build.json`

3. Add the following lines to the ignore files:
    * `workers/unity/Assets/Plugins/Improbable/Editor/Generated/`
    * `workers/unity/Assets/Plugins/Improbable/Editor/Generated.meta`
    * `workers/unity/Assets/Plugins/Improbable/Generated/`
    * `workers/unity/Assets/Plugins/Improbable/Generated.meta`
    * `workers/unity/Assets/Plugins/Improbable/Sdk/Dll/Generated.Code.*`

You need to do this for every project you migrate.

**Note:** In all our starter projects (including Wizards and Pirates) the version control ignore files are not set as above, so if your project is based on any of these projects, you need to edit the VCS ignore files.

### 6. Check it worked
In the root directory of your project, check that the upgrade and migration worked by running:
`spatial worker build`.

It’s worked when you see `'spatial build UnityWorker UnityClient' succeeded` (or `'spatial.exe build UnityWorker UnityClient' succeeded`).

Please note that, due to outdated references in your `<root>\workers\unity\Library` folder, you may see errors reported despite your build ultimately succeeding. If you run `spatial worker build` again these should no longer appear.

Let us know how the migration went on the [SpatialOS forums](https://forums.improbable.io/)!
