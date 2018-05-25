# Creating entity ACLs

This page explains how to construct an
[ACL (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#acl) (access control list)
component in Unity. They're used to control which workers have read and write access to an entity's
components.

For background on the concept of ACLs, see [Understanding write access (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/design/understanding-access).

## `EntityBuilder`

This page is about building an ACL on its own. For information about creating an ACL when you're using `EntityBuilder`
to create an entity template, see the [Using EntityBuilder](../interact-with-world/create-delete-entities.md#using-entitybuilder) section on the [Creating and deleting entities](../interact-with-world/create-delete-entities.md) page.

## Create an entity ACL

To create entity ACLs in Unity, use the entity ACL builder. Import this class using the statement
`using Improbable.Unity.Core.Acls;`.

To build an ACL:

1. [Define the worker attributes you want to reference](#worker-attributes).
2. [Define the worker requirements](#worker-requirements): which worker attributes you need for a particular use.
3. For each component, [give write access to a particular attribute](#setting-read-and-write-access).
4. For the whole entity, [give read access (on all components) to a particular attribute](#setting-read-and-write-access).
5. [Add the ACL to the entity template](#setting-the-acl).

To see how this works in practice, see the [example](#example) below.

### Reference

To build an ACL from scratch, start with `Acl.Build()`, for example:

    var myAcl = Acl.Build();

#### Worker attributes

You can find which attributes a worker has in the [`bridge` section of its worker configuration file (SpatialOS documentation)]
(https://docs.improbable.io/reference/13.0/shared/worker-configuration/bridge-config). Usually, a UnityWorker will have the attribute "physics", and a UnityClient
will have the attribute "visual".

To build a `WorkerAttributeSet`, use this static method on `Acl`:

* `public static WorkerAttributeSet MakeAttributeSet(string attribute1, params string[] attributeSet)`

    For example, `var unityWorkerAttributeSet = Acl.MakeAttributeSet("physics");`

The `CommonAttributeSets` class contains shortcuts for commonly used worker attribute sets:

* `public static WorkerAttributeSet Physics` is the same as `Acl.MakeAttributeSet("physics");`

    For example, `var unityWorkerOnlyRequirement = Acl.MakeRequirementSet(CommonAttributeSets.Physics);`
* `public static WorkerAttributeSet Visual` is the same as `Acl.MakeAttributeSet("visual");`

    For example, `var workerOrClientRequirement = Acl.MakeRequirementSet(CommonAttributeSets.Physics, CommonAttributeSets.Visual);`
* `public static WorkerAttributeSet SpecificClient(string clientId);`

    For example, `var specificClientRequirement = Acl.MakeRequirementSet(CommonAttributeSets.SpecificClient(clientWorkerId));`

#### Worker requirements

To build a `WorkerRequirementSet`, use this static method on `Acl`:

* `public static WorkerRequirementSet MakeRequirementSet(WorkerAttributeSet attribute1, params WorkerAttributeSet[] attributes)`

    For example, `var unityWorkerOnlyRequirement = Acl.MakeRequirementSet(unityWorkerAttributeSet);`

The `CommonRequirementSets` class contains shortcuts for commonly used worker requirements:

* `public static WorkerRequirementSet PhysicsOnly` is the same as `Acl.MakeRequirementSet(CommonAttributeSets.Physics);`

    For example,  `acl.SetWriteAccess<Position>(CommonRequirementSets.PhysicsOnly);`
* `public static WorkerRequirementSet VisualOnly` is the same as `Acl.MakeRequirementSet(CommonAttributeSets.Visual);`

    For example, `acl.SetReadAccess<Position>(CommonRequirementSets.VisualOnly);`
* `public static WorkerRequirementSet PhysicsOrVisual` is the same as `Acl.MakeRequirementSet(CommonAttributeSets.Physics, CommonAttributeSets.Visual);`

    For example, `acl.SetReadAccess(CommonRequirementSets.PhysicsOrVisual);`
* `public static WorkerRequirementSet SpecificClientOnly(string clientId);`

    For example, `acl.SetWriteAccess<Position>(CommonRequirementSets.SpecificClientOnly(clientWorkerId));`

#### Setting read and write access

To set read and write access, use these methods:

* `public Acl SetWriteAccess<TComponent>(WorkerRequirementSet requirement) where TComponent : IComponentMetaclass`

    For example, `acl.SetWriteAccess<Position>(unityWorkerOnlyRequirement);`
* `public Acl SetWriteAccess(uint componentId, WorkerRequirementSet requirement)`

    For example, `acl.SetWriteAccess(Position.ComponentId, unityWorkerOnlyRequirement);`
* `public Acl SetReadAccess(WorkerRequirementSet requirement)`

    For example, `acl.SetReadAccess(workerOrClientRequirement);`

#### Common ACLs

`Acl` provides the following static methods to generate completely some common types of ACL for an entity:

* `public static Acl GenerateServerAuthoritativeAcl(Entity entity);`

    This returns an entity ACL that has read access set to UnityClient or UnityWorker, and write access set to
    UnityWorker only, for all components that exist on the given entity.
* `public static Acl GenerateClientAuthoritativeAcl(Entity entity, string workerId);`

    This returns an entity ACL that has read access set to UnityClient or UnityWorker, and write access set to a
    UnityClient with the given worker ID, for all components that exist on the entity.

#### Setting the ACL

You can set the ACL on an `Entity` object or update it at runtime.

* `public EntityAcl.Data ToData()`

    For example, `exampleTemplate.Add(acl.ToData());`
* `public EntityAcl.Update ToUpdate()`

    For example, `exampleTemplate.Send(acl.ToUpdate());`

You can also set the ACL directly on the `Entity`. Remember that this will replace the entire ACL component on the
entity, so any components not specified will have their ACLs reset.

* `public void Entity.SetAcl(Acl)`

    For example, `exampleTemplate.SetAcl(acl);`

To merge an `Improbable.Unity.Core.Acls.Acl` into an entity's existing ACLs, you can use the `MergeAcl(Acl aclChanges)`
helper. Any components whose ACLs are specified in `aclChanges` will be updated. All other components will remain unchanged.

* `public void Entity.MergeAcl(Acl aclChanges)`

    For example,
```csharp
entityTemplate.SetAcl(baseAcl);       // Sets all ACLs for 'entityTemplate'
entityTemplate.MergeAcl(updatedAcl);  // Overwrites whichever ACLs are specified in 'updatedAcl'
```

### Example

In this example, we want to create an entity with `Position` and `Combat` components. `Position` is defined
in the [standard schema library (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/schema/standard-schema-library); `Combat is defined as follows:

```schemalang
package improbable.general;

component Combat {
  id = 1002;
  uint32 attack_power = 1;
}
```

Adding the components to the entity template looks like this:

```csharp
var exampleTemplate = new Entity();
exampleTemplate.Add(new Position.Data(new PositionData(new Coordinates(0, 0, 0))));
exampleTemplate.Add(new Combat.Data(new CombatData(5)));
```


Before you can [create an entity](../interact-with-world/create-delete-entities.md#create-an-entity) using this template,
you must add an entity ACL to it.

In this example:

* both UnityWorkers and UnityClients have read access on both components
* only UnityWorkers have write access, on both components


```csharp
var unityWorkerAttributeSet = Acl.MakeAttributeSet("physics");
var clientAttributeSet = Acl.MakeAttributeSet("visual");

var unityWorkerOnlyRequirementSet = Acl.MakeRequirementSet(unityWorkerAttributeSet);
var workerOrClientRequirementSet = Acl.MakeRequirementSet(unityWorkerAttributeSet, clientAttributeSet);

var acl = Acl.Build()
             .SetReadAccess(workerOrClientRequirementSet)
             .SetWriteAccess<Position>(unityWorkerOnlyRequirementSet)
             .SetWriteAccess<Combat>(unityWorkerOnlyRequirementSet);

exampleTemplate.SetAcl(acl);
```
