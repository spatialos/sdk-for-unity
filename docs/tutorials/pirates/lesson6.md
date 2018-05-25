# Pirates 6 — Add a component

In the previous lesson, you detected a cannonball hit. But at the moment, nothing happens when a ship
gets hit by a cannonball. 

In this lesson you'll:

* **create a component** to store the ship's health
* **add this component** to players' ships and to pirate ships
* **reduce a ship's health** when it gets hit by a cannonball

## 1. Create the Health component

In [lesson 4](../../tutorials/pirates/lesson4.md#2-extend-the-shipcontrols-component-to-fire-cannonballs), you extended
an existing component: `ShipControls`. But now, you want to add a brand new component: one that stores the health of a ship.

> As a reminder: [Components (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#component) are defined in your
project’s [schema (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#schema), written in [schemalang (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#schemalang),
in the `/schema` directory of the project. SpatialOS uses the schema to
[generate code (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#code-generation) which workers use to read and write to components.

### 1.1. Define the new component

1. In the `schema/improbable/ship` directory, create a schema file called `Health`.
2. Add the following contents:

    ```schemalang
    package improbable.ship;

    component Health {
        // Component ID. Must be unique within the project.
        id = 1006;

        int32 current_health = 1;
    }
    ```

    This defines a component called `Health`. It has a single property, `current_health`. 

    `id = 1006` sets the ID for the `Health` component. Every component needs an ID that is unique within the project.

    `int32 current_health = 1` defines a property of type `int32`, called `current_health`, with the ID `1`.
    Every property needs an ID that is unique within its component.

> In the future, you could add other values associated with health to this component,
like maximum health, or health regeneration rate.

### 1.2. Generate the schema code

**Every time you change the schema**, you need to regenerate the generated code:

0. In Unity, in the SpatialOS window, click `Build`, then under `Generate from schema`, click `Build`.

    This **generates code that workers can use** to read and modify the `Health` component, and allows 
    SpatialOS to synchronize components across the system.

    If you don't do this, you won't be able to use a `Health.Writer` in the next step, because that code
    won't exist.

> **It's done when:** you see `'sdk codegen' succeeded` printed in your console output.

## 2. Extend the PlayerShip template 

The player's ship is put together using an **entity template**, which lists all the components that the entity has.
You created a new entity template in [lesson 2](../../tutorials/pirates/lesson2.md#1-2-create-a-pirate-entity-template).
This time, you're just going to extend templates that already exist, by:

* adding the `Health` component
* specifying which workers can write to `Health`

### 2.1. Add the Health component to the template

1. In the Unity project editor, navigate to the `Gamelogic/EntityTemplates` directory.
2. Open the script `EntityTemplateFactory.cs`.
   
    The `CreatePlayerShipTemplate()` method specifies which components the player entity has. These are currently
    `Rotation`, `ClientConnection`, `ShipControls` and `ClientAuthorityCheck`:

    ```csharp
    .AddComponent(new Rotation.Data(0), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
    .AddComponent(new ClientConnection.Data(SimulationSettings.TotalHeartbeatsBeforeTimeout), CommonRequirementSets.PhysicsOnly)
    .AddComponent(new ShipControls.Data(0, 0), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
    .AddComponent(new ClientAuthorityCheck.Data(), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
    ```

3. Extend the template by adding the `Health` component (with an initial value of `1000`) to the `playerEntityTemplate`, so 
`CreatePlayerShipTemplate` now has these lines:

    ```csharp
    .AddComponent(new Rotation.Data(0), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
    .AddComponent(new ClientConnection.Data(SimulationSettings.TotalHeartbeatsBeforeTimeout), CommonRequirementSets.PhysicsOnly)
    .AddComponent(new ShipControls.Data(0, 0), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
    .AddComponent(new ClientAuthorityCheck.Data(), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
    .AddComponent(new Health.Data(1000), CommonRequirementSets.PhysicsOnly)
    ```

> `Health.Data` was generated when you generated code. If you can't find `Health.Data`,
you may have missed the earlier step to regenerate the generated code after modifying your schema.

> You want only the UnityWorker, on the server side, to be able to modify the player's
health. We don't want to allow clients to increase their own health!

## 3. Extend the PirateShip template

You also want enemy pirate ships to have health (so you can damage them). This is very similar to what you just did
for the `PlayerShip`, so here's just a short overview:

1. Still in `EntityTemplateFactory.cs`, go to the `CreatePirateEntityTemplate()` method.

2. At the end of the section with `var pirateEntityTemplate = EntityBuilder.Begin()`, add a new line just before `.Build();`:

    ```csharp
    .AddComponent(new Health.Data(1000), CommonRequirementSets.PhysicsOnly)
    ```

This adds the `Health` component to the entity, and gives the UnityWorker (the "physics worker") write access.


## 4. Reduce the ship's health when a cannonball hits it

Now that ships have health, you can use this concept in `TakeDamage.cs`.

1. In the Unity Editor, navigate to the `Assets/Gamelogic/Pirates/Behaviours` directory.
2. Open the script `TakeDamage.cs`.
3. Add the following import, which gives this script access to the code generated for `Health`:

    ```csharp
    using Improbable.Ship;
    ```
4. You only want `TakeDamage.cs` enabled on a prefab **if the worker can 
write to the `Health` component**. Use the `[Require]` annotation to do this:

    ```csharp
    public class TakeDamage : MonoBehaviour
    {
        // Enable this MonoBehaviour only on the worker with write access for the entity's Health component
        [Require] private Health.Writer HealthWriter;
    ```
5. Unity's `OnTriggerEnter()` runs even if the MonoBehaviour is disabled, so non-authoritative UnityWorkers
must be protected against null writers.
To do this, add the following at the start of `OnTriggerEnter()`:

    ```csharp
    private void OnTriggerEnter(Collider other)
    {
        if (HealthWriter == null)
            return;
    ```
6. You don't want to do anything with a collision with a ship that's already dead. So below the previous check,
add another check. 

    Use `HealthWriter.Data` to check the value of `currentHealth`:

    ```csharp
    if (HealthWriter.Data.currentHealth <= 0)
        return;
    ```
7. After these checks, you can write the code to actually reduce the ship's health. You should do this inside
the existing check that asserts whether the collision was with a cannonball. Let's say reduce the `currentHealth` by 250
when a cannonball hits.

    In [lesson 3](../../tutorials/pirates/lesson3.md#1-2-create-a-monobehaviour-to-steer-the-ship), you used a `Writer` 
    to send an update of a property. This is very similar:

    ```csharp
    if (other != null && other.gameObject.tag == "Cannonball")
    {
        // Reduce health of this entity when hit
        int newHealth = HealthWriter.Data.currentHealth - 250;
        HealthWriter.Send(new Health.Update().SetCurrentHealth(newHealth));
    }
    ```

When you're done, `TakeDamage.cs` should look something like this:

```csharp
using Improbable.Ship;
using Improbable.Unity;
using Improbable.Unity.Visualizer;
using UnityEngine;

namespace Assets.Gamelogic.Pirates.Behaviours
{
    // Add this MonoBehaviour on UnityWorker (server-side) workers only
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class TakeDamage : MonoBehaviour
    {
        // Enable this MonoBehaviour only on the worker with write access for the entity's Health component
        [Require] private Health.Writer HealthWriter;
        
        private void OnTriggerEnter(Collider other)
        {
            /*
             * Unity's OnTriggerEnter runs even if the MonoBehaviour is disabled, so non-authoritative UnityWorkers
             * must be protected against null writers
             */
            if (HealthWriter == null)
                return;

            // Ignore collision if this ship is already dead
            if (HealthWriter.Data.currentHealth <= 0)
                return;

            if (other != null && other.gameObject.tag == "Cannonball")
            {
                // Reduce health of this entity when hit
                int newHealth = HealthWriter.Data.currentHealth - 250;
                HealthWriter.Send(new Health.Update().SetCurrentHealth(newHealth));
            }
        }
    }
}
```

## 5. Build the changes

1. Regenerate the default snapshot: Use the menu `Improbable > Snapshots > Generate Default Snapshot`.

    You need to do this because you added a new component to entities in the snapshot. If you don't,
    when you run the game, `PirateShip`s won't have the `Health` component.
2. Build worker code: In the SpatialOS window, click `Build`, then under `Workers`, click `Build`.

> You don't always have to build everything. For a handy reference, see
[What to build when](../../develop/build.md).

## 6. Check it worked

To test these changes, run the game locally:

1. In the SpatialOS window, under `Run SpatialOS locally`, click `Run`.
2. Run a client (open the scene `UnityClient.unity`, then click **Play ▶**).
3. Find another ship, and press `E` or `Q` to fire a cannon at it.

4. Open the [Inspector](http://localhost:21000/inspector).

5. Inspect the entity of the ship you hit by clicking on its icon in the main Inspector window.

6. The bottom right area of the Inspector shows you the entity's components.

    Find the property called `currentHealth` in the `improbable.ship.Health` component:

> **It's done when:** You can see ship's health is less than 1000.

To stop `spatial local launch` running, switch to the terminal window and use `Ctrl + C`.

### Deploying to the cloud

So far in the Pirates tutorial, you've always run your game locally. But there's an alternative: running
in the cloud. This has some advantages, including making it much easier to test multiplayer
functionality.

To try out deploying to the cloud, try the optional lesson 
[Play in the cloud](../../tutorials/pirates/pirates-cloud.md).

## Lesson summary

In this lesson, you:

* created the component `Health`
* when a ship is hit by a cannonball, reduced the ship's `currentHealth`.

### What's next?

At the moment, a ship's `currentHealth` can fall to `0` - but nothing happens visually. 

In the [next lesson](../../tutorials/pirates/lesson7.md), you'll trigger a sinking animation on the
Unity Client when a ship's `current_health` reaches zero.
