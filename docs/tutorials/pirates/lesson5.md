# Pirates 5 — Detect a collision

In the previous lesson you made the ship fire cannonballs using an event which is synchronized by SpatialOS.
This resulted in the cannonballs being fired on all workers, not just the UnityClient of the firing player.

In this lesson you'll:

* **detect collisions** between cannonballs and ships using Unity functions
* **learn more about what happens on which worker**

This is a short lesson, because there isn't much to do here - it's purely Unity code. 

The reason there's not much to do is because, at the moment, there's no sensible consequence for a cannonball hit.
There's no concept of health, or dying. So this sets up for the next lesson, where you'll add the concept of health. 

This is what you're going to set up in the next few lessons:

![Diagram of events](../../assets/pirates/lesson5/events-diagram-2.png)

## 1. Understanding what happens on which worker

In the previous lesson, you were working on `CannonFirer.cs`, a script that is run on all workers that have
the firing ship checked out. When this script receives a `FireLeft` or `FireRight` event, it creates a cannonball
GameObject on the relevant workers.

### What does "checked out" mean?

To understand, it helps to think about a game world that's a little bigger than the world in this game.

Most of the things you have to take into consideration when designing a SpatialOS game come from one fact:
**workers don't know about the whole world**, only a subset of it that is allocated to them by SpatialOS. 
We call this its **checkout area**. 

A worker gets all updates (eg property changes, event triggers) for all entities in its checkout area - 
and none for any entity outside that area. It has **read access** on all components of all those entities,
and SpatialOS SDK for Unity instantiates GameObjects in the scene for all of these entities. 

**A worker's checkout area can overlap** with another worker. This could be a problem - what if two workers
both make a change to the same component? That's why **only one worker can have write access** on a component
at one time.

### Managing write access

As a developer, you specify which types of worker could have write access. You've seen an example in this tutorial,
with `ShipControls`, where you always want the player's specific UnityClient to have write access. You've also seen
the example of `Position`. It's important that only `UnityWorker`s and not `UnityClient`s have write access, but
*which* specific UnityWorker doesn't matter so much.

So for each entity, SpatialOS manages which worker has write access. If an entity moves from
area A to area B, the `UnityWorker` authoritative over area A will start off with write access. Then, as the entity moves
area B, SpatialOS transfers write access over that entity to the `UnityWorker` authoritative over area B.

As a developer, it doesn't matter to you whether you're writing code for A or B. You write them *in the same way*,
and make sure the right code is being run using the `[WorkerType]` and `[Require]` annotations.

> For a deeper discussion of this, see
[Understanding write access (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/design/understanding-access).

### Access control lists

As an example, let's take a look at the `ShipControls` component. What determines whether the UnityClient can write to this
component, and change the speed and steering?

In Unity, navigate to `Assets/Gamelogic/EntityTemplates/`, and open `EntityTemplateFactory.cs`.

In the `CreatePlayerShipTemplate` method, you'll see the following lines:

```csharp
.SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
.AddComponent<ClientConnection>(new ClientConnection.Data(SimulationSettings.TotalHeartbeatsBeforeTimeout), CommonRequirementSets.PhysicsOnly)
.AddComponent<ShipControls>(new ShipControls.Data(0, 0), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
.AddComponent<ClientAuthorityCheck>(new ClientAuthorityCheck.Data(), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
.AddComponent<Rotation>(new Rotation.Data(0), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
```

When you add a component, you also decide which workers have **access** to that component. This information gets built
into an **access control list**, or **ACL**, for this entity. You saw this briefly in
[lesson 2](../../tutorials/pirates/lesson2.md#1-2-create-a-pirate-entity-template). 

The ACL is a **component** itself, and it manages:

* which workers have **read access** to all of the components
* for each component, which workers have **write access**

In this context, we look at workers in terms of *requirements*: ie, a component *requires* a worker to have certain properties.
These might be:

* to be a 'specific client' - ie the Unity Client that owns a particular entity
* to be a 'physics' worker - ie a UnityWorker
* to be a 'visual' worker - ie a UnityClient.

When you build an entity, you specify which worker has write access when you add the component, like this
(as the second argument to the `Add()` method):

```
.AddComponent<ShipControls>(new ShipControls.Data(0, 0), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
```

So the ACL for the PlayerShip says:

* both the UnityWorker (aka “physics worker”) and UnityClient (aka “visual worker”) can read all components
(you don't specify a component with read access, it applies to all components equally)
* only the UnityClient that owns the entity (aka the “specific client”) can write to `Position`, `ShipControls`,
`ClientAuthorityCheck`, and `Rotation`
* only the UnityWorker can write to `ClientConnection`

Take a look at this line in particular:

```
.AddComponent<ShipControls>(new ShipControls.Data(0, 0), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
```

This line of code gives **only the player's** UnityClient access to write to `ShipControls`.
This allows the Unity Client to update the `ShipControls` component when the player presses keys.

## 2. Detect the collision on the UnityWorker

Enough background. Collisions are something that should be detected server-side, so on the UnityWorker,
because you don't want players deciding whether or not they've hit something.

Create a MonoBehaviour that runs on the UnityWorker, detecting collisions:

1. In Unity Editor's project panel, navigate to `Assets/Gamelogic/Pirates/Behaviours/`
2. Open the C# script called `TakeDamage`.
3. The script should look like the following:

    ```csharp
    using Improbable.Unity;
    using Improbable.Unity.Visualizer;
    using UnityEngine;

    namespace Assets.Gamelogic.Pirates.Behaviours
    {
        // Add this MonoBehaviour on UnityWorker (server-side) workers only
        [WorkerType(WorkerPlatform.UnityWorker)]
        public class TakeDamage : MonoBehaviour
        {
            private void OnTriggerEnter(Collider other)
            {
                if (other != null && other.gameObject.tag == SimulationSettings.CannonballTag)
                {

                }
            }
        }
    }
    ```

    This very simple script uses the Unity function 
    [`OnTriggerEnter()`](https://docs.unity3d.com/ScriptReference/Collider.OnTriggerEnter.html) to detect
    collisions.

5. Inside the `if` statement, add the following line:
    
    ```csharp
    // Reduce health of this entity when hit
    Debug.LogWarning("Collision detected with " + gameObject.EntityId());
    ```

    Now, you'll log a message when a cannonball hits the current GameObject.
4. Add `TakeDamage.cs` to the `PlayerShip` prefab.
5. Add `TakeDamage.cs` to the `PirateShip` prefab.

## 3. Build the changes

You added a new MonoBehaviour to the `PlayerShip` prefab: `TakeDamage`. For SpatialOS to make use of this
updated prefab:

0. Build entity prefabs: In the SpatialOS window, click `Build`, then under `Entity prefabs`, click `Build all`.
0. Build worker code: Under `Workers`, click `Build`.

> You don't always have to build everything. For a handy reference, see
[What to build when](../../develop/build.md).

## 4. Check it worked

To test the changes, run the game locally:

1. In the SpatialOS window, click `Build`, then under `Run SpatialOS locally`, click `Run`.
2. Run a client (open the scene `UnityClient.unity`, then click **Play ▶**).
3. Find another ship, and press `E` or `Q` to fire a cannon at it.

> **It's done when:** you see this message printed in the output of the console window that opened when you clicked `Run`:

> `WARN [improbable.bridge.logging.EngineLogMessageHandler] [Worker: UnityWorker0] Collision detected with <entity id>`

If you like, you can also connect another client (for a reminder on how to do this, see
[lesson 4](../../tutorials/pirates/lesson4.md#1-finding-the-problem-with-cannonballs)), and check that it's
working on the client too. 

To stop `spatial local launch` running, switch to the terminal window and use `Ctrl + C`.

## Lesson summary

In this lesson, you've written some server-side logic, detecting collisions between cannonballs and enemy pirate ships.

This script only runs on UnityWorkers: Unity Clients won't detect collisions. Which is as you want it, because things like
collisions and damage shouldn't be controlled by clients.

This is pretty limited, though - at the moment there are no consequences for a ship that's hit by a cannonball. That's because
there's nothing that *can* happen in this game yet. 

What *should* happen is that a cannonball should damage the ship it hits.

### What's next?

In the [next lesson](../../tutorials/pirates/lesson6.md), you'll create a new component: `Health`. You'll add this new
component to ships, and then use the script you wrote in this lesson to decrease a ship's health when it gets hit by a
cannonball. 
