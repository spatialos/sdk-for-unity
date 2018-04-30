# Querying the world

You can search the [world (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#spatialos-world) for [entities (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#entity),
using [entity queries (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#queries) that use a rich query language.

This is useful if you want to get information about entities, including entities that your worker doesn't have
[checked out (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#checking-out).

> You should keep entity queries as limited as possible. All queries hit the network and
cause a runtime lookup, which is expensive even in the best cases. This means you should:

> * always limit queries to a specific sphere of the world
> * only return the information you need from queries (eg the specific components you care about)
> * if you're looking for entities that are within your worker's checkout radius, search internally on the worker instead 
of using a query, for example using Unity's
[GameObject Find methods](https://docs.unity3d.com/ScriptReference/GameObject.html)

A query can return [entity IDs (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#entityid), so you can query for entities that you want to run a
[command (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#command) on.

## Prerequisites

In order to send an entity query, a worker must have **permission** to do so. For more information, see the
[Worker permissions (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/worker-configuration/permissions) page.

## Building a query

To build up a query, use the `Query` class (`using Improbable.Unity.Core.EntityQueries`). Each of its methods returns an `IConstraint`:
an object that defines the thing you're looking for. Some of the methods take combinations of `IConstraint`s, so you can build a 
query up out of several constraints. 

`Query` exposes the following methods:

```cs
public static IConstraint HasEntityId(EntityId entityId);

public static IConstraint HasComponent<TComponent>() where TComponent : IComponentMetaclass;

public static IConstraint HasComponent(uint componentId);

public static IConstraint InSphere(Coordinates center, double radius);

public static IConstraint InSphere(double x, double y, double z, double radius);

public static IConstraint And(IConstraint constraint1, IConstraint constraint2, params IConstraint[] constraints);

public static IConstraint Or(IConstraint constraint1, IConstraint constraint2, params IConstraint[] constraints);
```

Once you've built an `IConstraint`, call one of the following methods on it to convert it into an `EntityQuery` (the object type
taken by `SendQuery`). Which method you use depends on what information you want to get from the query:

* To get a count of the entities that match the query, use `public EntityQuery ReturnCount();`
* To only get the IDs of entities that match the query, use `public EntityQuery ReturnOnlyEntityIds();`
* To get specific components of entities that match the query, use `public EntityQuery ReturnComponents(uint componentType, 
  params uint[] componentTypes);`
* To get all components of entities that match the query, use `public EntityQuery ReturnAllComponents();`

## Sending a query

To send a query, use `SendQuery`:

    ICommandResponseHandler<EntityQueryResult> SendQuery(IComponentWriter writer, EntityQuery query)

## Example

This example finds all entities within 20 world units of a position that have a `Health` component. If the query succeeds, it logs
how many entities were found, picks the first one in the list, and runs a function on that entity.

```
var query = Query.And(
    Query.HasComponent<Health>(),
    Query.InSphere(transform.position.x, transform.position.y, transform.position.z, 20.0)
  ).ReturnOnlyEntityIds();

SpatialOS.Commands.SendQuery(healthWriter, query)
  .OnSuccess(result => {
    Debug.Log("Found " + result.EntityCount + " nearby entities with a health component");
    if (result.EntityCount < 1) {
      return;
    }
    Map<EntityId, Entity> resultMap = result.Entities;
    EntityId anEntityId = resultMap.First.Value.Key;
    DoSomething(anEntityId);
  })
  .OnFailure(errorDetails => Debug.Log("Query failed with error: " + errorDetails));
```
