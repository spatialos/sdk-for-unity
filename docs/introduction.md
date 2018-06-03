![SpatialOS Unity SDK documentation](assets/unity-sdk-header.png)

# Introduction to the SpatialOS Unity SDK

> You need to set up SpatialOS before you can work on a project using the SpatialOS Unity SDK. To set up SpatialOS, download and set up the `spatial` CLI. For more information, see the setup guides:
[Windows](get-started/setup/win.md),
[macOS](get-started/setup/mac.md),
[Linux](get-started/setup/linux.md).

You can use the game engine [Unity](https://unity3d.com/) as a [worker (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#worker)
in a SpatialOS project, to add physics, game logic, and visualization to a SpatialOS simulated world. We provide a
Unity SDK to make it easier to use Unity as a worker.

> **Compatible Unity versions**: Unity versions **5.6.0** and **2017.3.0** have been tested with SpatialOS. Other versions may work fine, but have not been extensively tested.

**License**
* See the [license](../LICENSE.md).

**Documentation**
* The SpatialOS documentation is on the [SpatialOS documentation website](https://docs.improbable.io).
* The Unity SDK documentation is on [GitHub](start-here-table-of-contents.md). 

## The relationship between SpatialOS and Unity

When you're using Unity on its own, a Unity scene is the canonical source of truth about the game world. What's in the
Unity scene is in the game world.

When you use Unity as a SpatialOS worker, this isn't true any more: the canonical source of truth is the world of
the SpatialOS simulation, and the entities in that world. Each Unity worker has a view onto part of that world. It
represents the entities from SpatialOS as GameObjects in a scene.

A Unity worker can do whatever it likes to its own representation of the
world and run whatever logic it likes but if the worker doesn't send these
changes to SpatialOS in the form of an update to a SpatialOS entity, those
changes will only ever be local: they can't be seen by any other worker.

Sometimes this is fine. For example, if on a client worker, you are making a purely visual change to a scene, no other
worker needs to know about it, so it doesn't need to be represented in SpatialOS.

But for anything else that another worker would need to be aware of, those changes must be made
to a SpatialOS entity.

### How can Unity workers change the SpatialOS world?

They can:

* create entities
* delete entities
* set properties of an entity
* trigger an event on an entity
* send a command to an entity

### How do Unity workers get information about the SpatialOS world?

Within the worker's area of interest, SpatialOS will send the worker updates about changes to
components/entities it can read. So a worker can:

* get the value of a property
* watch for events being triggered
* watch for commands being called

Outside its area of interest, a worker can find out about the world by querying for entities.

### What to use Unity for

Typical usages of Unity with SpatialOS include:

* A player's game client reads the position of each visible SpatialOS entity, and updates the position of
   the corresponding Unity GameObjects.
* A player's game client writes the user's input to a component on the player's entity, so it gets synchronized
    to other workers in the world.
* A physics worker applies forces to GameObjects, based on properties or events in the world. It then updates
    the position of each of the GameObject's corresponding entities.

## Useful software for developing with Unity

These aren't required, but when you're developing with Unity, you might find the following tools useful:

* [Visual Studio for Unity](https://www.visualstudio.com/en-us/features/unitytools-vs.aspx)
* [Resharper for Unity](https://github.com/JetBrains/resharper-unity)

## Internal features

The SpatialOS Unity SDK is provided as source files to aid in debugging.
This means that `internal` classes, structs and methods are all visible to your code.
You should take care to avoid using these items, as they can change without notice outside of major versions.
