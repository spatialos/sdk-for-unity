# MonoBehaviour lifecycle

## Lifecycle of SpatialOS SDK for Unity-managed MonoBehaviours

[Prefabs](https://docs.unity3d.com/Manual/Prefabs.html) are used to instantiate the GameObjects associated with an entity. 

MonoBehaviours placed on these prefabs are subject to injection. **Any `MonoBehaviour` present on a prefab used to represent 
a SpatialOS entity will have its lifecycle managed by the SpatialOS SDK for Unity**. The SpatialOS SDK for Unity will make sure that any component readers and 
writers are available for use before enabling the MonoBehaviour.

This is to ensure that whenever the `MonoBehaviour` is activated, *all* of its readers and writers have their
requirements satisfied and are injected into the `MonoBehaviour`. In particular:

* The component for a particular reader or writer is present on the entity and visible to the worker.
* The worker has write access to  the component, if a Writer is required. Note that only one worker will have
  write access to a particular component on a particular entity at any given time.

The SpatialOS SDK for Unity will keep a `MonoBehaviour` disabled until *all* of the required fields can be injected. Once all the
requirements are satisfied, the script is enabled. If, at any point, one of the requirements is no longer true (for example
because the worker lost write access) the script will be disabled and the required fields set to `null`. Note that a
particular script can be disabled and re-enabled this way many times on a single `GameObject`.

It is recommended to keep readers and writers private to the `MonoBehaviour` into which they are injected to. A single
`MonoBehaviour` can have multiple readers and writers and a single reader or writer might be used in multiple
`MonoBehaviour`s enabled on the same GameObject.

## Best practices for Unity Event Functions

* `Awake` should be used for one-time Unity-specific logic such as getting
  Unity components. However, any components which are added or removed
  dynamically should be added or removed in `OnEnable` or `OnDisable`
  respectively.
* `OnEnable` should be used for accessing data or initialising logic related to
  SpatialOS component readers and writers. It will be called multiple times
  over the lifetime of a `GameObject` after authority changes for example.
* `OnEnable` and `OnDisable` should be mirrored. All event and command handlers
  registered in `OnEnable` should be removed or deregistered in `OnDisable`. In
  addition, you might consider reseting mutable fields to their default values
  in `OnDisable` based on your game logic.
* `Start` should be used rarely in very specific cases. Prefer `OnEnable` or
  `Awake` based on the details above.

## Enabling and disabling MonoBehaviours

In order to manage the lifecycle of `MonoBehaviour`s, the SpatialOS SDK for Unity uses Unity's `.enabled` flag. You should therefore
avoid using this flag to manage which scripts are run.

If you want to manually disable or enable a `MonoBehaviour`, you should use the SpatialOS SDK for Unity to do so:

```csharp
gameObject.GetEntityObject().DisableVisualizers(new []{myBehaviourToBeDisabled, myOtherBehaviourToBeDisabled});
gameObject.GetEntityObject().TryEnableVisualizers(new []{myVisualizer, myOtherVisualiser});
```

This former will mark a `MonoBehaviour` as manually disabled and the SpatialOS SDK for Unity will not enable it even if all requirements
for readers and writers have been satisfied. The latter will remove this mark. Note that this *won't* cause the
`MonoBehaviour` to be enabled if it doesn't have all of its component requirements met.

`MonoBehaviour`s are initially configured to become enabled as soon as their requirements become satisfied. If you
want a `MonoBehaviour` to be marked as disabled by default, you can annotate the class with `[DontAutoEnable]`.

```csharp
[DontAutoEnable]
public class MyMonoBehaviour : MonoBehaviour
{
    [Require]
    private ExampleComponentReader exampleComponentReader;

    // ...
}
```
