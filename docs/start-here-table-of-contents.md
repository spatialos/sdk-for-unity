![SpatialOS Unity SDK documentation](assets/unity-sdk-header.png)

### Welcome to the SpatialOS Unity SDK documentation

**New to the platform?**
You can:
* [take a tour of SpatialOS](get-started/tour.md)
* read about [what SpatialOS is (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/concepts/spatialos)
* learn how to make a game with the [Pirates tutorial](tutorials/pirates/overview.md)

**Need some help?** Join us at [forums.improbable.io](https://forums.improbable.io) for technical questions and issues.

 **Note**:
SpatialOS 13.0 is fine for local development and deployments using the `small` [deployment template (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/reference/file-formats/launch-config#templates), but deployments using larger templates may be unstable.

**License:**
See the [license](../LICENSE.md).

## Installing and upgrading
* If you are installing SpatialOS and the SpatialOS Unity SDK for the first time, see:
[Introduction to the Unity SDK](introduction.md).
* If you have a version of SpatialOS and the SpatialOS Unity SDK which is earlier than SpatialOS 13.0, you
have a combined SpatialOS and SpatialOS Unity SDK bundle. To get the separated version, see: [Upgrading to SpatialOS 13.0 and migrating to SpatialOS Unity SDK 1.0.0](migration.md).

## Documentation contents
### Get started
- [Introduction to the Unity SDK](introduction.md)
- [Take a tour of SpatialOS](get-started/tour.md)
- Setup guides
    - [Windows](get-started/setup/win.md)
    - [macOS](get-started/setup/mac.md)
- [System requirements](get-started/requirements.md)
### Learning
- [Learning resources](tutorials/learning-resources.md)
- **Pirates tutorial**
    - [Introduction](tutorials/pirates/overview.md)
    - [1 - Set up the game](tutorials/pirates/lesson1.md)
    - [2 - Add enemy pirates](tutorials/pirates/lesson2.md)
    - [3 - Update a component property](tutorials/pirates/lesson3.md)
    - [4 - Trigger and receive a component event](tutorials/pirates/lesson4.md)
    - [5 - Detect a collision](tutorials/pirates/lesson5.md)
    - [6 - Add a component](tutorials/pirates/lesson6.md)
    - [7 - Sink the ship](tutorials/pirates/lesson7.md)
    - [ext - Add a component command](tutorials/pirates/pirates-command.md)
    - [ext - Play in the cloud](tutorials/pirates/pirates-cloud.md)
- **C++ worker tutorial**
    + [Introduction](tutorials/cpp-worker-tutorial/introduction.md)
    + [1 - Add a C++ worker to your project](tutorials/cpp-worker-tutorial/add-cpp-worker.md)
    + [2 - Give access to a component](tutorials/cpp-worker-tutorial/give-component-access.md)
    + [3 - Update a component property](tutorials/cpp-worker-tutorial/move-pirate-ships.md)
    + [4 - Trigger a component event](tutorials/cpp-worker-tutorial/shooting-pirate-ships.md)
- **Recipes**
    - [Connection splash screen](tutorials/recipes/splash-screen.md)
    - [Create an entity](tutorials/recipes/entity-creation.md)
    - [Create an entity at runtime](tutorials/recipes/runtime-entity-creation.md)
    - [Client connection lifecycle](tutorials/recipes/client-lifecycle.md)
    - [Implement a command](tutorials/recipes/command.md)
    - [Player movement](tutorials/recipes/player-movement.md)
    - [Player camera visualization](tutorials/recipes/player-visualization.md)
    - [Voice over IP](tutorials/recipes/voip.md)
    - [Work with snapshots in the Unity Editor](tutorials/recipes/working-with-snapshots.md)
### Development process
- [Setting up a project](develop/set-up-unity-project.md)
- [What and when to build](develop/build.md)
- [Deploying](develop/deploy.md)
### Interacting with the world
- [Creating and deleting entities](interact-with-world/create-delete-entities.md)
- [Creating entity ACLs](interact-with-world/create-acls.md)
- [Querying the world](interact-with-world/query-world.md)
- [Getting information about local entities](interact-with-world/local-entities.md)
- [Interacting with entity components](interact-with-world/interact-components.md)
### Customising
- [Logging](customize/logging.md)
- [Configuring the build process](customize/configure-build.md)
- [Connecting to SpatialOS](customize/spatialos-connection.md)
- [Distribute with Steam](customize/steam.md)
- [Entity pipeline](customize/entity-pipeline.md)
- [Entity spawning](customize/spawn-rate-limiting.md)
- [Integrate with Playfab](customize/playfab.md)
- [Load from multiple scenes](customize/multiple-scenes.md)
- [Using the new build system](customize/minimal-build.md)
### Reference
- [Which components your Unity worker will check out](reference/component-interest.md)
- [Local Entities API](reference/local-entities-api.md)
- [Monobehaviour lifecycle](reference/monobehaviour-lifecycle.md)
- [SpatialOS window](reference/spatialos-window.md)
- [Unity worker project structure](reference/unity-worker-structure.md)
- [Unity worker project anatomy](reference/project-anatomy.md)
### iOS
- [Introduction](unity-ios/introduction.md)
- [Building for iOS targets](unity-ios/using.md)
### Releases
- [Release notes](releases/release-notes.md)
