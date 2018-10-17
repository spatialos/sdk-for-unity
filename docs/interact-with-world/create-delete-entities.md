# Creating and deleting entities

This page covers creating and deleting [entities (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#entity) in Unity.

## Prerequisites

* In order to create or delete an entity, a [worker (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#worker) must have
**permission** to do so. For more information, see
[Worker permissions (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/worker-configuration/permissions).
* If you send a creation or deletion command using `SpatialOS.Commands`, the MonoBehaviour you send it from needs
a [**component writer**](interact-components.md#Example-of-MonoBehaviours-component-readers-and-writers): a worker can only send a command when
it has [write access (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#read-and-write-access-authority) to some component.

    > The component writer helps prevent commands accidentally being sent twice when
    two workers have the same entity [checked out (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#checking-out).
    Because only [one worker can have write access to a component at a time (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/design/understanding-access),
    the MonoBehaviour from which the command is sent can only be enabled (and therefore executed) in one worker:
    so the command is only sent once.

    In rare circumstances, a worker won't have access to any component writers (for example, when a client worker
    is bootstrapping itself). In this case, use `SpatialOS.WorkerCommands` instead: it has an identical interface to 
    `SpatialOS.Commands`, but doesn't require a component writer.

    Using `SpatialOS.WorkerCommands`, commands are more likely to be accidentally sent twice, so **only use it
    when absolutely necessary**.

## Create an entity

To create an entity, you [create the entity's template](#1-create-an-entity-template), then 
[send the `CreateEntity` command](#2-create-an-entity).

> An [**entity template** (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#entity-template) is an object
that specifies all the components that the entity has.

To create an entity, you need:

* an entity template
* a component writer (for the reasons described [above](#))
* the statement `using SpatialOS.Commands;` and `using Improbable;`

> When you create an entity, the SpatialOS SDK for Unity will automatically handle creating all relevant
GameObjects. If you spawn a GameObject yourself, it won't be associated with an entity, and won't
be synchronized to any other Unity workers: **it'll only exist locally**.

### 1. Create an entity template

#### Required components

All entities must have at least these three components:

* `Position`, which specifies the location of your entity
* `Metadata`, which has the property `entity_type` that is used to specify the entity's prefab name (note that prefab names cannot have spaces)
* `EntityAcl`, which controls the read and write access that workers have to the entity and its components

Most entities also should have a `Persistence` component, which means that you want it to persist in
[snapshots (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#snapshot).
The EntityBuilder pattern requires you to specify whether or not you want your entity to be persistent.

For more information about the required components, see the
[standard schema library (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/schema/standard-schema-library)) page.
For more conceptual information about ACLs, see [Understanding write access (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/design/understanding-access).
For information about designing an entity, see the [Designing entities (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/design/design-entities) page.

#### Using EntityBuilder

To create an entity template, use `EntityBuilder` (in `Improbable.Unity.Entity`), which defines a pattern to create an entity template.
When you use `EntityBuilder`, you add the required components in order, then add your own components. This order is:

0. `Position`
0. `Metadata`
0. whether or not you want [persistence (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#persistence)
0. [read ACLs (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#acl)
0. your own components

As you add each component, you specify which workers can
[write (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#read-and-write-access-authority) to the component. For example:

``` 
var myEntityTemplate = EntityBuilder.Begin()
    .AddPositionComponent(Vector3.zero, CommonRequirementSets.PhysicsOnly)
    .AddMetadataComponent("MyPrefab")
    .SetPersistence(true)
    .SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
    .Build();
```

To add your own components, add them after `.SetReadAcl()` and before `.Build()`:

```
var myEntityTemplate = EntityBuilder.Begin()
    .AddPositionComponent(Vector3.zero, CommonRequirementSets.PhysicsOnly)
    .AddMetadataComponent("MyPrefab")
    .SetPersistence(true)
    .SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
    .AddComponent<ExampleComponent>(new ExampleComponent.Data(), CommonRequirementSets.PhysicsOnly)
    .Build();
```

(optional) If you need to set [write access (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#read-and-write-access-authority) 
on the [`EntityACL` component (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#acl) itself,
call `SetEntityAclComponentWriteAccess()` as shown below:

```
var myEntityTemplate = EntityBuilder.Begin()
    .AddPositionComponent(Vector3.zero, CommonRequirementSets.PhysicsOnly)
    .AddMetadataComponent("MyPrefab")
    .SetPersistence(true)
    .SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
    .SetEntityAclComponentWriteAccess(CommonRequirementSets.PhysicsOnly)
    .AddComponent<ExampleComponent>(new ExampleComponent.Data(), CommonRequirementSets.PhysicsOnly)
    .Build();
```

> To construct entity templates without using `EntityBuilder`, use the underlying
[C# API (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/csharpsdk/using#creating-and-deleting-entities).

#### Troubleshooting

If you try to add components out of order, or call `.Build()` before adding the read ACLs, you will see an error like this:

```
Type `Improbable.Unity.Entity.EntityBuilder.IPersistenceSetter` does not contain a definition for `SetReadAcl` and no extension method `SetReadAcl` of type `Improbable.Unity.Entity.EntityBuilder.IPersistenceSetter` could be found. Are you missing an assembly reference?
```

To fix this, make sure that you're calling all the methods in the correct order. For example, in the above case the solution is to
call `.SetPersistence()` after `.AddMetadataComponent()` and before `.SetReadAcl()`, so that they appear in the order as shown in the example above.

### 2. Create an entity

You can create an entity in two ways, both using methods on `SpatialOS.Commands`:

* Create an entity
* Reserve an entity ID, then create the entity using the reserved ID

The methods all have a `ICommandResponseHandler` return type, require an `IComponentWriter` object for access control, and an 
optional trailing `TimeSpan? timeout` argument.

You can call these methods from any `MonoBehaviour` that has a component writer and the using directive `using
SpatialOS.Commands;`.

#### 2a. Create an entity (the easy way)

This is the simplest way to create an entity. It first reserves an entity ID, and then
creates an entity with that ID. 

Method definition:

``` 
ICommandResponseHandler<CreateEntityResult> CreateEntity(IComponentWriter writer,
    Worker.Entity template, TimeSpan? timeout = null);
```

where:

* `writer` is any component writer
* `template` is the entity template that specifies which components the entity will have
* `timeout` is a length of time. If there's no response after this length of time, the command is considered to have
    failed, and any callback is called with a failure message. This argument is optional and if not provided a default
    value will be used.

For example:

``` 
SpatialOS.Commands.CreateEntity(exampleComponentWriter, myEntityTemplate)
    .OnSuccess(entityId => Debug.Log("Created entity with ID: " + entityId))
    .OnFailure(errorDetails => Debug.Log("Failed to create entity with error: " + errorDetails.ErrorMessage));
```

This example passes in a callback that checks whether the creation was successful or not, but you don't need
to have a callback if you don't want one.

#### 2b. Creating an entity (the complicated way)

Alternatively, you can manually reserve an entity ID, then create the entity with that ID. This method provides fewer options 
for error detection and recovery than the two-step approach.

`ReserveEntityId` reserves an entity ID that you can use later to create an entity with that ID.

Method definition:

```
ICommandResponseHandler<ReserveEntityIdResult> ReserveEntityId(IComponentWriter writer, TimeSpan? timeout = null);
```

where:

* `writer` is any component writer
* `timeout` is a length of time. If there's no response after this length of time, the command is considered to have
    failed, and any callback is called with a failure message. This argument is optional and if not provided a default
    value will be used.

This version of `CreateEntity` creates an entity with a previously-reserved entity ID. The ID **must** have been reserved 
by a previous call of `ReserveEntityId`. You **cannot** specify an arbitrary ID.

Method definition:

```   
ICommandResponseHandler<CreateEntityResult> CreateEntity(IComponentWriter writer, EntityId reservedEntityId,
    Worker.Entity template, TimeSpan? timeout = null);
```

where:

* `writer` is any component writer
* `reservedEntityId` is the ID issued by `ReserveEntityId`
* `template` is the entity template that specifies which components the entity will have
* `timeout` is a length of time. If there's no response after this length of time, the command is considered to have
    failed, and any callback is called with a failure message. This argument is optional and if not provided a default
    value will be used.

### Full example of creating an entity

This example uses the simpler method shown in step 2a above.

``` 
var myEntityTemplate = EntityBuilder.Begin()
     .AddPositionComponent(Vector3.zero, CommonRequirementSets.PhysicsOnly)
     .AddMetadataComponent("MyPrefab")
     .SetPersistence(true)
     .SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
     .AddComponent<ExampleComponent>(new ExampleComponent.Data(), CommonRequirementSets.PhysicsOnly)
     .Build();
 
// Create the entity based on myEntityTemplate
SpatialOS.Commands.CreateEntity(exampleComponentWriter, myEntityTemplate)
    .OnSuccess(entityId => Debug.Log("Created entity with ID: " + entityId))
    .OnFailure(errorDetails => Debug.Log("Failed to create entity with error: " + errorDetails.ErrorMessage));
```

## Creating multiple entities

There are two ways to create multiple entities. You can:

* create entities one by one similar to above,
* or bulk-reserve multiple entity IDs at once, then create them.

### Creating multiple entities through bulk reservation

This is very similar to [Creating an entity (the complicated way)](#2b-creating-an-entity-the-complicated-way) as described above.

Instead of using `ReserveEntityId`, you can use the plural form: `ReserveEntityIds`.

`ReserveEntityIds` reserves a range of entity IDs that you can use later to create entities with.

Method definition:

```csharp
ICommandResponseHandler<ReserveEntityIdsResult> ReserveEntityIds(IComponentWriter writer, uint numberOfEntityIds, TimeSpan? timeout)
```

where:

* `writer` is any component writer.
* `numberOfEntityIds` specifies how many entities you would like to reserve.
* `timeout` is a length of time. If there's no response after this length of time, the command is considered to have failed,
  and any callback is called with a failure message. This argument is optional.

The `ReserveEntityIdsResult` object within the response contains these fields:

* `ReadOnlyCollection<EntityId> ReservedEntityIds` a read-only collection that will have the same number of `EntityId`s as `numberOfEntityIds`.

## Deleting an entity

To delete an entity, you'll need: 

* a component writer 
* the using directive `using SpatialOS.Commands`
* the entity's `EntityId`

> When you delete an entity, the SpatialOS SDK for Unity will automatically handle deleting all relevant
GameObjects.

You can get the `EntityId` by:

* [querying the world](../interact-with-world/query-world.md)
* if it's the current entity/gameobject, using `gameObject.EntityId`
* using `public static Entity GetLocalEntity(EntityId entityId)` on the `SpatialOS` utility class
* to get all the entities on the worker, using `SpatialOS.Dispatcher` `public Map<EntityId, Entity> Entities`.

`DeleteEntity` deletes the entity with the given entity ID. Method definition:

```
ICommandResponseHandler<DeleteEntityResult> DeleteEntity(IComponentWriter writer, EntityId entityId, TimeSpan? timeout = null);
```

where:

* `writer` is any component writer
* `entityId` is the ID of the entity to be deleted
* `timeout` is a length of time. If there's no response after this length of time, the command is considered to have
    failed, and any callback is called with a failure message. This argument is optional and if not provided a default
    value will be used.

For example:

```
SpatialOS.Commands.DeleteEntity(exampleComponentWriter, gameObject.EntityId())
    .OnSuccess(entityId => Debug.Log("Deleted entity: " + entityId))
    .OnFailure(errorDetails => Debug.Log("Failed to delete entity with error: " + errorDetails.ErrorMessage));
```