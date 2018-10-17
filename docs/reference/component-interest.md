# Which components your Unity worker will check out

The SpatialOS SDK for Unity manages component delivery to enhance the default behaviour.
For more information on the default behaviour, see [Component delivery (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/worker-configuration/bridge-config#component-delivery).

Your SpatialOS SDK for Unity workers will automatically be interested in only the components that you have referenced in your scripts and prefabs.
See [below](#automatic-component-interest) for how this works.
This automatic interest is calculated globally across all entities that your worker has checked out.

This is convenient, but sometimes you need a greater amount of control over the automatic calculations.
For example, you may reference a component that is expensive to send, and you want to control the interest per-entity.

> Note that workers will *always* have interest in components that they have write access on.

## Automatic component interest

How does the automatic calculation work?
For each `GameObject` associated with a SpatialOS entity, the SpatialOS SDK for Unity figures out its component interest by:

0. Finding each `Reader` and `Writer` field marked with the `[Require]` [attribute](../interact-with-world/interact-components.md).
0. Finding each experimental `MonoBehaviour` component.

This tracking overrides the interest configured in the worker bridge settings mentioned above.

> The SpatialOS SDK for Unity requires interest in `improbable.Position` and `improbable.Metadata` in order to
spawn a `GameObject` that represents a SpatialOS entity. Make sure that you have set up your worker to be delivered these components.

## Customizing component interest

If you want to have more control over when you receive updates for components, use `SpatialOS.WorkerComponentInterestModifier`
to modify the interest calculations.

> Note that this API will have no effect on components that your worker has write access on.

### Example use case

Imagine you have an `Inventory` component on hundreds of treasure chests scattered around the world.
`Inventory` contains a lot of data, and you don't want it sent until the player has walked up to the chest and opened it.

You have a visualizer defined like this:

```
public class InventoryVisualizer : MonoBehaviour
{
    [Require] public Inventory.Reader Inventory;
    // …
}
```

Because of the `[Require]` attribute, the SpatialOS SDK for Unity will calculate interest in all `Inventory`
components. SpatialOS will send unwanted data, even if you specify that you're not interested in your [bridge configuration (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/worker-configuration/bridge-config#component-delivery).

To implement the desired player selection behaviour:

1. Ensure that your `Inventory` component has `checkout_initially` set to `false` in your
[bridge configuration (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/worker-configuration/bridge-config#components-to-check-out-initially).

2. Globally disable interest in the `Inventory` component so that the SpatialOS SDK for Unity won't calculate interest,
even though it's marked with `[Require]`.

    ```
    public class Bootstrap : MonoBehaviour
    {
        public WorkerConfigurationData Configuration = new WorkerConfigurationData();

        public void Start()
        {
            // We don't ever want to be interested in Inventory components,
            // so we can set this globally without having to set it for each entity.
            SpatialOS.WorkerComponentInterestModifier.SetComponentInterest<Inventory>(WorkerComponentInterest.Never);

            // ...Additional setup...

            // Connect.
            SpatialOS.ApplyConfiguration(Configuration);
            SpatialOS.Connect(gameObject);
        }
    }
    ```

3. Add a `MonoBehaviour` that requests interest in the component when the player selects the treasure chest entity:

    ```
    /// <summary>
    /// A behaviour that ensures that when an entity is selected, this worker becomes interested in its inventory component.
    /// </summary>
    public class InventorySelector : MonoBehaviour
    {
        private bool selected;

        /// <summary>
        /// Gets or sets a value indicating that this object is selected by the user.
        /// </summary>
        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                // When the users selects this object, become interested in the contents of the inventory for just this entity.
                SpatialOS.WorkerComponentInterestModifier.SetComponentInterest<Inventory>(
                    gameObject.EntityId(),
                    selected ? WorkerComponentInterest.Always : WorkerComponentInterest.Default
                    );
            }
        }
    }
    ```

When the player selects a specific treasure chest your worker will become interested in it and enable `InventoryVisualizer`.
When the player deselects the treasure chest, the SpatialOS SDK for Unity will stop having interest in it and your worker will no
longer receive `Inventory` data for that treasure chest.

## API reference

### WorkerComponentInterest

```
/// <summary>
/// Enumeration of modifications that can be made to the calculated component interest.
/// </summary>
/// <remarks>See <seealso cref="IWorkerComponentInterestOverrider"/> for more information about component interest.</remarks>
public enum WorkerComponentInterest
{
    /// <summary>
    /// No override - use the calculated interest.
    /// </summary>
    Default,
    /// <summary>
    /// Always be interested in a component, even if it hasn't been calculated.
    /// </summary>
    Always,
    /// <summary>
    /// Never be interested in a component, even if it's been calculated.
    /// </summary>
    Never
}
```

### IWorkerComponentInterestOverrider

```
/// <summary>
/// Provides the ability to override calculated component interests for the current worker.
/// These interests are calculated for a GameObject associated with a SpatialOS Entity based on:
///     a) The presence of the [Require] attribute on a field referencing a SpatialOS component on a MonoBehaviour.
///     b) The presence of a generated SpatialOS MonoBehaviour component.
/// </summary>
public interface IWorkerComponentInterestOverrider
{
    /// <summary>
    /// Sets an interest override for a component on ALL entities on the current worker.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <param name="interest">The interest override to apply.</param>
    void SetComponentInterest<T>(WorkerComponentInterest interest) where T : IComponentMetaclass;

    /// <summary>
    /// Sets an interest override for a component on a specific entity on the current worker.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <param name="entityId">The entity that the override applies to.</param>
    /// <param name="interest">The interest override to apply.</param>
    void SetComponentInterest<T>(EntityId entityId, WorkerComponentInterest interest) where T : IComponentMetaclass;

    /// <summary>
    /// Returns the set of component interest overrides active on the current worker for a specific entity, including any global
    /// overrides that have been set. Global overrides are themselves overridden by entity-specific overrides.
    /// </summary>
    /// <param name="entityId">The entity to retrieve.</param>
    /// <returns>A mapping of componentId to its override status.</returns>
    IEnumerator<KeyValuePair<uint, WorkerComponentInterest>> GetComponentInterest(EntityId entityId);
}
```
