# Pirates - Add a component command

In this extension to the Pirates tutorial, you'll add a scoring system that gives players points when they sink other players' ships.

In this lesson you'll:

* **create a component to track how many kills** a player has made
* **use component commands to inform a ship** that it sunk another player
* **create a MonoBehaviour to respond to that command**
* **update the Unity Client's UI** to track how many kills a player has made

## 1. Track the number of points a player has scored

PlayerShip entities need the concept of a 'score'. This information is persistent, so it should exist as a new component.

You also need a way to tell a ship that it has scored a kill. You can do this using [commands (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#command)
so a sinking ship can command the ship which fired the fatal cannonball to award itself points. You'll
then create a MonoBehaviour to implement what the firing ship does when it is told to award itself points,
which will include incrementing its points counter and updating the score on the client's UI.

### Create the `Score` component

In the `schema/improbable/ship` directory, create a new file, `Score.schema`.
Define a component called `Score` as follows:

```schemalang
package improbable.ship;

type AwardPoints {
    uint32 amount = 1;
}

type AwardResponse {
    uint32 amount = 1;
}

component Score {
    // Component ID, unique within the project
    id = 1007;

    int32 number_of_points = 1;

    // Used by other entities to give points to entity with the Score component
    command AwardResponse award_points(AwardPoints);
}
```
It has an `int32` property `number_of_points` to store the player's current score.

Generate the C# classes associated with this new component: In Unity, in the SpatialOS window,
click `Build`, then under `Generate from schema`, click `Build`.

### Add the `Score` component to the PlayerShip entity template

Now the `Score` component exists, you need to:

* add it to the `PlayerShip` entity template so `PlayerShip` entities have the associated properties and commands
* set its permissions to `PhysicsOnly` so the correct worker type has write-access to the new component

In the Unity Editor, navigate to the `Assets/Gamelogic/EntityTemplates` directory, open the script `EntityTemplateFactory.cs`,
and locate the `CreatePlayerShipTemplate` method.

Add the `Score` component to the `Entity` object with the same syntax you used earlier to add the `Health` component:

```csharp
.AddComponent(new Score.Data(0), CommonRequirementSets.PhysicsOnly)
```

This also updates the [access control list (ACL) (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#acl) for this new component.
Remember that the ACL controls which workers can write to the component's properties? It also controls which
workers can provide implementations for the component's commands.

Control of `Score` needs to be server-side: you don't want clients to be able to falsely report how many points they have.
Using `CommonRequirementSets.PhysicsOnly` makes sure that only physics workers (the UnityWorkers) have write access.

Note that other worker types (such as the client) will still have read access to the `Score` component as `SetReadAcl`
is called with `CommonRequirementSets.PhysicsOrVisual`. This `CommonRequirementSets` attribute set includes both
`UnityClient` and `UnityWorker`.

Once you've finished, the `CreatePlayerShipTemplate` function
will look like this:

```csharp
var playerEntityTemplate = EntityBuilder.Begin()
  // Add components to the entity, then set the access permissions for the component on the entity relative to the client or server worker ids.
  .AddPositionComponent(initialPosition, CommonRequirementSets.SpecificClientOnly(clientWorkerId))
  .AddMetadataComponent(SimulationSettings.PlayerShipPrefabName)
  .SetPersistence(false)
  .SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
  .AddComponent(new Rotation.Data(0), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
  .AddComponent(new ClientConnection.Data(SimulationSettings.TotalHeartbeatsBeforeTimeout), CommonRequirementSets.PhysicsOnly)
  .AddComponent(new ShipControls.Data(0, 0), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
  .AddComponent(new ClientAuthorityCheck.Data(), CommonRequirementSets.SpecificClientOnly(clientWorkerId))
  .AddComponent(new Health.Data(1000), CommonRequirementSets.PhysicsOnly)
  .AddComponent(new Score.Data(0), CommonRequirementSets.PhysicsOnly)
  .Build();

return playerEntityTemplate;
```

## 2. Send a message to the firing ship

When a ship sinks, it needs to notify the ship that fired the fatal cannonball
that it should award itself some points. When you created the schema for the `Score`
component, you included the definition of a command:

```
command AwardResponse award_points(AwardPoints);
```

Commands are requests to trigger some action on the recipient entity or component. The
`AwardPoints` and `AwardResponse` types were also specified so that any information
that should be transmitted as part of the command can be specified. In this case,
the outgoing command request includes a `uint32` specifying the `amount` of points
which should be awarded. The response is empty.

In the Unity Editor, navigate to the `Assets/Gamelogic/Pirates/Behaviours`
directory and open the script `TakeDamage`.

You previously altered this script to reduce the entity's health if the entity
is hit by a `Cannonball` GameObject. When those GameObjects are spawned, the entity
that spawned them tags them with its own `EntityId`. You can use this information
to determine who gets points when a ship is sunk.

Add a function to the `TakeDamage` class to give points to the entity
with the given `EntityId`. The `SendCommand` function requires *any* writer (you can
use `HealthWriter` as it's already been injected into this class with the `[Require]`
syntax).

Sending the command could look like this:

```csharp

// ... CODE ... //

// Use Commands API to issue an AwardPoints request to the entity who fired the cannonball
SpatialOS.Commands.SendCommand(HealthWriter, Score.Commands.AwardPoints.Descriptor, new AwardPoints(pointsToAward), firerEntityId)
    .OnSuccess(OnAwardPointsSuccess)
    .OnFailure(OnAwardPointsFailure);

// ... CODE ... //
```

with the success and failure callback functions defined as follows:

```csharp
private void OnAwardPointsSuccess(AwardResponse response)
{
    Debug.LogWarning("AwardPoints command succeeded. Points awarded: " + response.amount);
}

private void OnAwardPointsFailure(ICommandErrorDetails response)
{
    Debug.LogError("Failed to send AwardPoints command with error: " + response.ErrorMessage);
}
```
> `SpatialOS.Commands.SendCommand()` returns a so-called `ICommandResponseHandler`
object which you can use to define the optional callbacks `OnSuccess()` and `OnFailure()`. You may leave these
callbacks undefined, in which case no reaction will be triggered in response to either a successful or failed
command request.

In the `OnTriggerEnter` function, add the logic that calls the command-sending function if the ship's health has
fallen to zero.

### The finished script

The finished `TakeDamage` script will look something like this:

```csharp
using Improbable;
using Improbable.Ship;
using Improbable.Unity;
using Improbable.Unity.Core;
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

                // Notify firer to increment score if this entity was killed
                if (newHealth <= 0)
                {
                    AwardPointsForKill(new EntityId(other.GetComponent<Cannons.DestroyCannonball>().firerEntityId.Value.Id));
                }
            }
        }

        private void AwardPointsForKill(EntityId firerEntityId)
        {
            uint pointsToAward = 1;
            // Use Commands API to issue an AwardPoints request to the entity who fired the cannonball
            SpatialOS.Commands.SendCommand(HealthWriter, Score.Commands.AwardPoints.Descriptor, new AwardPoints(pointsToAward), firerEntityId)
                .OnSuccess(OnAwardPointsSuccess)
                .OnFailure(OnAwardPointsFailure);
        }

        private void OnAwardPointsSuccess(AwardResponse response)
        {
            Debug.Log("AwardPoints command succeeded. Points awarded: " + response.amount);
        }

        private void OnAwardPointsFailure(ICommandErrorDetails response)
        {
            Debug.LogError("Failed to send AwardPoints command with error: " + response.ErrorMessage);
        }
    }
}
```

## 3. Implement the AwardPoints command

An entity with the `Score` component needs an implementation for the `AwardPoints`
command, so that the worker with write access over the command's component knows
what to do when an `AwardPoints` command arrives.

In Unity's Editor's project panel, navigate to `Assets/Gamelogic/Pirates/Behaviours/` and create
a new C# script called `TrackScore`. Replace its contents with the following code:

```csharp
using Improbable.Entity.Component;
using Improbable.Ship;
using Improbable.Unity;
using Improbable.Unity.Visualizer;
using UnityEngine;

namespace Assets.Gamelogic.Pirates.Behaviours
{
    // Add this MonoBehaviour on UnityWorker (server-side) workers only
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class TrackScore : MonoBehaviour
    {
        /*
         * An entity with this MonoBehaviour will only be enabled for the single UnityWorker
         * which has write access for its Score component.
         */
        [Require] private Score.Writer ScoreWriter;

        void OnEnable()
        {
            // Register command callback
            ScoreWriter.CommandReceiver.OnAwardPoints.RegisterResponse(OnAwardPoints);
        }

        private void OnDisable()
        {
            // Deregister command callbacks
            ScoreWriter.CommandReceiver.OnAwardPoints.DeregisterResponse();
        }

        // Command callback for handling points awarded by other entities when they sink
        private AwardResponse OnAwardPoints(AwardPoints request, ICommandCallerInfo callerInfo)
        {
            int newScore = ScoreWriter.Data.numberOfPoints + (int)request.amount;
            ScoreWriter.Send(new Score.Update().SetNumberOfPoints(newScore));
            // Acknowledge command receipt
            return new AwardResponse(request.amount);
        }
    }
}
```

When you added the `Score` component to the `PlayerShip` template, you granted write access
to UnityWorkers (by specifying `CommonRequirementSets.PhysicsOnly`). In the new `TrackScore`
MonoBehaviour, you inject a `Score.Writer` which means this MonoBehaviour will only ever be enabled
on UnityWorkers. To make this clear, the platform constraint
`[WorkerType(WorkerPlatform.UnityWorker)]` is also specified just above the `TrackScore` class.

The script uses the injected `ScoreWriter` to access Unity events generated when
the entity receives a command, to which you can attach a callback function:

```csharp
ScoreWriter.CommandReceiver.OnAwardPoints.RegisterResponse(OnAwardPoints);
```

You can name the callback anything. Here, it's `OnAwardPoints`, just like the CommandReceiver
event, to make it clear which it matches up with. This callback function contains logic
to update the score with the number of points awarded in the payload of the message.

All commands must be responded to, which you can do with the `type` defined in the `Score` schema file:

```csharp
return new AwardResponse(request.amount);
```

A UnityWorker will update the score and send a response in acknowledgement if:

* it has the entity checked out
* it has write access to the entity's `Score` component

Now the `TrackScore` MonoBehaviour is complete, add it to the `PlayerShip` prefab.

## 4. Show the score

You now have a score for every player, which is increased every time the player
sinks another ship. All you need now is to display it on the client!

In the Unity Editor, navigate to the `Assets/Gamelogic/Pirates/Behaviours`
directory and open the script `ScoreGUI`.

To be able to read the `Score` component, add the following injection:

```csharp
[Require] private Score.Reader ScoreReader;
```

In the `OnEnable` function, register a callback for whenever `NumberOfPoints` property in the `Score`
component is updated:

```csharp
ScoreReader.NumberOfPointsUpdated.Add(OnNumberOfPointsUpdated);
```

Don't forget to deregister the callback in `OnDisable`!

In the callback function, check if the associated `Score.Update` contains a change to the component's
`number_of_points` field, and call the UI update function if it has.

The finished `ScoreGUI` script should look like this:

```csharp
using Improbable.Ship;
using Improbable.Unity;
using Improbable.Unity.Visualizer;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Gamelogic.Pirates.Behaviours
{
    // Add this MonoBehaviour on client workers only
    [WorkerType(WorkerPlatform.UnityClient)]
    public class ScoreGUI : MonoBehaviour
    {
        /*
         * Client will only have write access for their own designated PlayerShip entity's ShipControls component,
         * so this MonoBehaviour will be enabled on the client's designated PlayerShip GameObject only and not on
         * the GameObject of other players' ships.
         */
        [Require] private ShipControls.Writer ShipControlsWriter;
        [Require] private Score.Reader ScoreReader;

        private Canvas scoreCanvasUI;
        private Text totalPointsGUI;

        private void Awake()
        {
            if (scoreCanvasUI != null) {
                totalPointsGUI = scoreCanvasUI.GetComponentInChildren<Text>();
                scoreCanvasUI.enabled = false;
                updateGUI(0);
            }
        }

        private void OnEnable()
        {
            // Register callback for when components change
            ScoreReader.NumberOfPointsUpdated.Add(OnNumberOfPointsUpdated);
        }

        private void OnDisable()
        {
            // Deregister callback for when components change
            ScoreReader.NumberOfPointsUpdated.Remove(OnNumberOfPointsUpdated);
        }

        // Callback for whenever one or more property of the Score component is updated
        private void OnNumberOfPointsUpdated(int numberOfPoints)
        {
            updateGUI(numberOfPoints);
        }

        void updateGUI(int score)
        {
            if (scoreCanvasUI != null) {
                if (score > 0)
                {
                    scoreCanvasUI.enabled = true;
                    totalPointsGUI.text = score.ToString();
                }
                else
                {
                    scoreCanvasUI.enabled = false;
                }
            }
        }
    }
}
```

The `ScoreGUI` class requires write access to the entity's `ShipControls` component, so that
**only client workers** will get write access. The MonoBehaviour has also been tagged
with the `[WorkerType(WorkerPlatform.UnityClient)]` to make this very
obvious when reading the code.

Now the `ScoreGUI` MonoBehaviour is complete, add it to the `PlayerShip` prefab.

## 5. Build changes

You've now added two more MonoBehaviours to the `PlayerShip` prefab: `TrackScore`
and `ScoreGUI`.

For SpatialOS to make use of this updated prefab you must build it:

0. Build entity prefabs: In the SpatialOS window (open it using the menu `Window > SpatialOS`), under `Entity prefabs`, 
click `Build all`.
0. Rebuild the worker code: In the SpatialOS window, under `Workers`, click `Build`.

## 6. Check it worked

To test these changes, run the game locally:

0. Run the game locally from the SpatialOS window.
0. Run one client from Unity (open the scene `UnityClient.unity`, then click **Play ▶**).
0. Run another client using
`spatial local worker launch UnityClient default`.
0. Fire cannonballs from one ship at another.

> **It's done when:** The ship you hit sinks after some cannonballs hit it, and the firing ship's score
> increases by 1.

## Lesson summary

In this lesson you made players track their number of kills in a component, and updated the display in the
UI using this information.

