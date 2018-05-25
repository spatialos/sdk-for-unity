# Unity worker project structure

Code for the [Unity workers](../introduction.md) - the UnityWorker and the UnityClient -
should be placed in the `workers/` directory as follows:

* [`workers/` (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/reference/project-structure#workers)
    * `unity/` (you may use a different name)
        * [`spatialos.[worker_type].worker.json`](#spatialos-worker-type-worker-json)
        * `.gitignore` *(optional)*
        * [`Assets/EntityPrefabs`](#assets-entityprefabs)
        * The rest of your Unity content in `Assets/` and `ProjectSettings/`.

## spatialos.[worker_type].worker.json

This file tells SpatialOS how to build, launch, and interact with the Unity workers.

For details on the file format, see the [Configuring a worker (SpatialOS documentation)]
(https://docs.improbable.io/reference/13.0/shared/worker-configuration/worker-configuration) section.

* [Example `spatialos.UnityClient.worker.json`] 
    (https://github.com/spatialos/BlankProject/blob/master/workers/unity/spatialos.UnityClient.worker.json) (from Blank Project)
* [Example `spatialos.UnityWorker.worker.json`]
    (https://github.com/spatialos/BlankProject/blob/master/workers/unity/spatialos.UnityWorker.worker.json) (from Blank Project)

## Assets/EntityPrefabs

You should store the prefabs associated with SpatialOS entities in this directory.

## spatialos_worker_packages.json

`spatialos_worker_packages.json` contains the dependencies to download for the worker.

By default, it is auto-generated (ie, your `spatialos.[worker_type].worker.json` contains 
`"build" : { "generated_build_scripts_type": "unity" }`). If you are auto-generating this file, you should not add
it to version control. 

Auto-generating this file should be fine for most users, but you can turn off generated build scripts. If you do,
create this file to specify which dependencies the worker needs and where they 
should be downloaded to. See [this page (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/reference/file-formats/spatial-worker-packages) for file format details.
