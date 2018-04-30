# Getting information about local entities

The `SpatialOS` utility class exposes the following methods for retrieving [entities (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#entity)
[checked out (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#checking-out) by a [worker (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#worker):

* `public static Worker.Entity GetLocalEntity(EntityId entityId);`

    Returns the entity object, if it is available on this worker, or null otherwise.

    For example, `SpatialOS.GetLocalEntity(entityId)`.
* `public static IComponentData<T> GetLocalEntityComponent<T>(EntityId entityId) where T : IComponentMetaclass`

    Returns a component of the specified type, or null if either the entity or the component is not available on this worker.

    For example, to get the value of the `exampleProperty` property on the `ExampleComponent` component,
    you'd use `SpatialOS.GetLocalEntityComponent<ExampleComponent>(entityId).Get().Value.exampleProperty`.

For getting information about entities that are located anywhere in the simulated world, use
[queries](../interact-with-world/query-world.md).