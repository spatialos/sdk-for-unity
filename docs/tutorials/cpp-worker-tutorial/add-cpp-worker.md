# Lesson 1 - Add a C++ worker to your project

The starting point of this tutorial is the completed
[Lesson 2 of the Pirates tutorial](../../tutorials/pirates/lesson2.md).
If you already have a project at this point or want to follow the steps yourself in
the other tutorial, you're free to do so.

It's best to clone the starting point from the [C++ worker tutorial
repository](https://github.com/spatialos/CppWorkerPiratesTutorial/tree/master), build it by running
`spatial worker build UnityClient UnityWorker`, and [check it
works](../../tutorials/pirates/lesson2.md#3-check-it-worked).

## Quick start

Here are some basic shell commands to set up everything described in the
sections below. This is just provided for your convenience so that you don't
have to move files around and change names. **Before you run them, read through
the rest of this page and make sure you know what you use the commands for.**
Based on your environment, you might need to make some tweaks to the commands.

This assumes you are in root directory of the tutorial project, have
[Git](https://git-scm.com/), and building this on Windows. To build for a
different platform just change the `spatial worker build` command at the end:

```sh
DEST_PROJECT=`pwd`

# Go to the parent directory of Pirates
cd ..

# Clone C++ blank project
git clone git@github.com:spatialos/CppBlankProject.git

cd CppBlankProject
chmod +x worker_create.sh
./worker_create.sh Managed PirateShipMovement $DEST_PROJECT

# Build the new worker
cd $DEST_PROJECT
spatial worker build PirateShipMovement --target=windows

# Go back to project root
cd ../..
```

## 1. Add a worker project

1. Clone the C++ blank project.
2. Copy the `workers/Managed` directory to the `workers/PirateShipMovement`
   directory of Pirates.
3. Rename the worker configuration file `spatialos.Managed.worker.json` to
   `spatialos.PirateShipMovement.worker.json`.
4. Find and replace all occurrences of "Managed" (case-sensitive) with "PirateShipMovement" in
   the directory you copied:
   - In `spatialos.PirateShipMovement.worker.json` - multiple occurrences.
   - In `CMakeLists.txt`
   - In `build.json`
   - In `src/startup.cc`

## 2. Configure build of generated C++ components

1. Copy the `CppBlankProject/schema/CMakeLists.txt` to `PiratesTutorial/schema/CMakeLists.txt`.
2. Remove the `blank.schema` and generated sources for the `blank` component.

The file should look like:

```
# This script is included by worker and library builds
# It is not meant to be built as a standalone library

set(GENERATED_CODE_DIR "${APPLICATION_ROOT}/generated_code/cpp")

# Schema generated code in library
set(SCHEMA_FILES
  )

set(SCHEMA_SOURCE_FILES
  "${GENERATED_CODE_DIR}/improbable/standard_library.cc"
  "${GENERATED_CODE_DIR}/improbable/standard_library.h"
  )

source_group(schema "${CMAKE_CURRENT_SOURCE_DIR}/[^/]*")
source_group(generated_code\\schema "${GENERATED_CODE_DIR}/[^/]*")
source_group(generated_code\\improbable "${GENERATED_CODE_DIR}/improbable/[^/]*")

add_library(Schema STATIC ${SCHEMA_FILES} ${SCHEMA_SOURCE_FILES})
target_include_directories(Schema SYSTEM PUBLIC "${GENERATED_CODE_DIR}")

target_link_libraries(Schema PRIVATE WorkerSdk)
```

In the next part of the tutorial you will add the sources for the
`ShipControls` component.

## 3. Configure the C++ SDK

The worker project you copied earlier already contains a
`spatialos_worker_packages.json` with the required packages for the C++ SDK. You need
to make the SDK available to the worker executable when compiling.

1. Create a new directory `PiratesTutorial/dependencies`.
2. Copy the `CppBlankProject/dependencies/CMakeLists.txt` to
   `PiratesTutorial/dependencies/CMakeLists.txt`.

## 4. Ignore generated files from source control

If your project is under source control, add the following directories to your ignored files:

```
# Worker SDK
dependencies/worker_sdk/

# Generated code from schema
generated_code/

# Cache from generated code and other metadata
.spatialos/
```

## 5. Build the worker

To make sure everything was added correctly, build the worker you added. The
target value will be one of `windows`, `linux`, or `macos` based on your
platform:

```sh
# On Windows
spatial worker build PirateShipMovement --target=windows

# On Linux
spatial worker build PirateShipMovement --target=linux

# On MacOS
spatial worker build PirateShipMovement --target=macos
```

The first time you build there will be many warnings about missing `pdb` files
from some of the libraries used in the C++ SDK - **you shouldn't worry about
this**. If the command succeeds, you are ready to continue with the
[next lesson](../../tutorials/cpp-worker-tutorial/move-pirate-ships.md).

> Note that you won't be able to simply use `spatial worker build` to build all
> your workers anymore. Because the targets of the C++ worker build depend on
> the platform, you normally won't be able to build for all platforms at the
> same time.
>
> If you really want to, you can achieve this by setting up [cross-compiling
> in CMake](https://cmake.org/Wiki/CMake_Cross_Compiling). As an
> aside, it is highly unlikely you will often need to build all workers of
> different types. Usually, you will only have changes that require rebuilding
> a certain type of worker. For this tutorial use: 
> ```
> spatial worker build UnityWorker UnityClient
> ```
> or
> ```
> spatial worker build PirateShipMovement -t=<platform>
> ```
> to build your workers. See [`spatial worker build` (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/spatial-cli/spatial-worker-build) for more details.
