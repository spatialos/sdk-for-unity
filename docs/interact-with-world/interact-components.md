# Interacting with entity components

This page details the ways that a Unity [worker (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#worker) can find out about and
interact with the [components (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#component) on
SpatialOS [entities (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#entity).

The code used to do this is generated from [schema (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#schema)
when you run `spatial worker codegen`. When you change your schema, you need to run codegen to get the
up-to-date code to use.

For information about creating components, see the
[schema documentation (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/schema/introduction).

## Prerequisites

MonoBehaviours must be added to the prefab associated with the entity. The methods will
be run on the current entity/GameObject. To do things to another entity/GameObject, you can
either run a query, or expose the methods using Unity and the GameObject.

>  The Unity SDK changes the
[usual MonoBehaviour lifecycle of Unity](https://docs.unity3d.com/Manual/ExecutionOrder.html).
Component readers and writers will be available when the `OnEnable` event function is called.

> For more information, have a look at
[MonoBehaviour lifecycle](../reference/monobehaviour-lifecycle.md).

### Example of MonoBehaviours component readers and writers
In order to read from or write to a component, you need to include a component reader or writer - classes created by [code generation  (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#code-generation).

All the examples on this page use a component reader (an `IComponentReader`) or writer 
(an `IComponentWriter`), imported like this:

```csharp
[Require]
private Improbable.ExampleComponent.Reader exampleComponentReader;

[Require]
private Improbable.ExampleComponent.Writer exampleComponentWriter;
```

The `Reader` and `Writer` interfaces are nested inside the classes of their corresponding components.

## Get the value of a property

Prerequisites:

* the MonoBehaviour needs a [component reader *or* writer](interact-components.md#Example-of-MonoBehaviours-component-readers-and-writers) for the relevant component
* the worker must have [read access (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#read-and-write-access-authority) on the component

To get the current value of a property, use `exampleComponentReader.Data.ExampleProperty`. `exampleComponentReader.Data` contains the full
persistent properties of the component.

## Set the value of a property

Prerequisites:

* the MonoBehaviour needs a [component writer](interact-components.md#Example-of-MonoBehaviours-component-readers-and-writers) for the relevant component
* the worker must have [write access (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#read-and-write-access-authority) on the component

To set the value of a property, use `exampleComponentWriter.Send()`, which takes an `Update` object.

To create an update object that changes a property:

```csharp
// Create the update object
var update = new ExampleComponent.Update();

// Add a property change to the update object
update.SetExampleProperty(1);

// Send the update to SpatialOS
exampleComponentWriter.Send(update);
```

You can update multiple properties in the same update, like this:

```csharp
// Create the update object
var update = new ExampleComponent.Update();

// Add the property changes to the update object
update.SetExampleProperty(1);
update.SetAnotherExampleProperty(2);

// Send the update to SpatialOS
exampleComponentWriter.Send(update);
```

You can send an update in one line like this:

```csharp
exampleComponentWriter.Send(new ExampleComponent.Update().SetExampleProperty(1));
```

Note that:

* Anything set in the `Update` will be sent to SpatialOS, even if the value hasn't changed, and empty updates won't be dropped.
* Don't try to modify an `Update` object once it's sent. If you want to modify it later and perhaps re-use it, send a copy of it
  instead, for example by doing `exampleComponentWriter.Send(update.DeepCopy())`.
* If you change a property's value multiple times, for example by doing `update.SetExampleProperty(1).SetExampleProperty(2);`,
  only the last value will be sent.
* The update will be observed (the update applied to `.Data` and callbacks invoked) by the worker once it's
  processed, typically on the next frame.

> You can trigger events in the same way, and mix property and event changes in one update.
See [below](#triggering-an-event).

## Responding to a change of a property

Prerequisites:

* the MonoBehaviour needs a [component reader *or* writer](interact-components.md#Example-of-MonoBehaviours-component-readers-and-writers) for the relevant component
* the worker must have [read access (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#read-and-write-access-authority) on the component

Register callbacks on change of a specific property, or for *any* property change on the component, using
`exampleComponentReader.ExamplePropertyUpdated`, or `exampleComponentReader.ComponentUpdated` respectively.

Register all callbacks in `OnEnable` or later, and deregister them in `OnDisable`, in order to prevent unexpected and 
hard-to-debug errors.

These callbacks:

* Are invoked whenever an update to the property is received. The property won't necessarily have changed.
* Do not get called immediately on registration. If you want to invoke the callback when it's registered,
use `exampleComponentReader.ComponentUpdated.AddAndInvoke` instead.

Example:

```csharp
private void OnEnable()
{
    // Register callbacks for component and property change
    exampleComponentReader.ComponentUpdated.Add(OnExampleComponentUpdated);
    exampleComponentReader.ExamplePropertyUpdated.Add(OnExamplePropertyUpdated);
}

private void OnDisable()
{
    // Deregister callbacks
    exampleComponentReader.ComponentUpdated.Remove(OnExampleComponentUpdated);
    exampleComponentReader.ExamplePropertyUpdated.Remove(OnExamplePropertyUpdated);
}

private void OnExampleComponentUpdated(Component.Update update)
{
    // Respond to the component change here
}

void OnExamplePropertyUpdated(int examplePropertyValue)
{
    // Respond to the property change here
}
```

## Triggering an event

Prerequisites:

* the MonoBehaviour needs a [component writer](interact-components.md#Example-of-MonoBehaviours-component-readers-and-writers) for the relevant component
* the worker must have [write access (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#read-and-write-access-authority) on the component

To trigger an event, use `exampleComponentWriter.Send()`, which takes an `Update` object.

To create an update object that triggers an event:

```csharp
// Create the update object
var update = new Improbable.ExampleComponent.Update();

// Create an instance of the event, and give it data
var exampleEvent = new Improbable.ExampleComponent.ExampleEvent();
exampleEvent.eventValue = 2;

// Add the event to the update object
update.AddExampleEvent(exampleEvent);

// Send the update to SpatialOS
exampleComponentWriter.Send(update);
```
Note that:

* Anything set in the `Update` will be sent to SpatialOS, even if the value hasn't changed, and empty updates won't be dropped.
* Don't try to modify an `Update` object once it's sent. If you want to modify it later and perhaps re-use it, send a copy of it
instead, for example by doing `exampleComponentWriter.Send(update.DeepCopy())`.
* The update will be observed (the update applied to `.Data` and callbacks invoked) by the worker once it's
processed, typically on the next frame.

> You can set properties in the same way, and mix property and event changes in one update. 
See [above](#set-the-value-of-a-property).

## Responding to an event

Prerequisites:

* the MonoBehaviour needs a [component reader *or* writer](interact-components.md#Example-of-MonoBehaviours-component-readers-and-writers) for the relevant component
* the worker must have [read access (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#read-and-write-access-authority) on the component

Register callbacks for events using `exampleComponentReader.ExampleEventTriggered`.

Register all callbacks in `OnEnable()` or later, and deregister them in `OnDisable`, in order to prevent unexpected and 
hard-to-debug errors.

```csharp
private void OnEnable()
{
    // Register callback for the event
    exampleComponentReader.ExampleEventTriggered.Add(OnExampleEvent);
}

private void OnDisable()
{
    // Deregister callback
    exampleComponentReader.ExampleEventTriggered.Remove(OnExampleEvent);
}

private void OnExampleEvent()
{
    // Respond to the event here
}
```

## Sending a command

You define commands in the schema, and the definition includes an input type 
and a return type.

When you send a command, register a callback to receive a response, which includes:

* information about the command's success or failure
* a response object of the return type

It is guaranteed that SpatialOS will only report a 
command's success if it actually succeeded, but [no other guarantees are made (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/design/commands#caveats). 

Prerequisites:

* the MonoBehaviour needs a [component writer](interact-components.md#Example-of-MonoBehaviours-component-readers-and-writers) - this **doesn't** need to be on the component the command is on 
(it's just used to avoid commands being sent repeatedly)
* the worker must have [write access (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#read-and-write-access-authority) on the component

To send a command, use `SpatialOS.SendCommand()` (with `using Improbable.Unity.Core`). For example:

```csharp
void RunACommand(EntityId exampleEntityId) 
{
    SpatialOS.Commands
        .SendCommand(someComponentWriter, ExampleComponent.Commands.ExampleCommand.Descriptor, new ExampleCommandRequest(), exampleEntityId)
        .OnSuccess(OnExampleCommandRequestSuccess)
        .OnFailure(OnExampleCommandRequestFailure);
}

void OnExampleCommandRequestSuccess(ExampleResponse response)
{
    Debug.Log("Command succeeded.");
}

void OnExampleCommandRequestFailure(ICommandErrorDetails response)
{
    Debug.LogError("Failed to send command, with error: " + response.ErrorMessage);
}
```

* `writer` is the component writer used to avoid commands being sent repeatedly.
* `commandDescriptor` is a special object identifying a command. Command descriptors are generated for each command 
defined in your schema, and are named as `{ComponentName}.Commands.{CommandName}.Descriptor`. 

    To access the available list of commands you can invoke on a component, type `{ComponentName}.Commands.`
    and look at your code completion results. (This will only work once you've run `spatial worker codegen`).
* `request` is the request argument to the command, of the type specified in the schema.
* `entityId` is the ID of the entity you are sending the command to.
* `timeout` (optional, default is `null`) is the timeout period for a command response. It's a
[`TimeSpan`](https://msdn.microsoft.com/en-us/library/system.timespan(v=vs.110).aspx).
* `commandDelivery` (optional) denotes whether the worker will attempt to short-circuit entity commands if
possible. `CommandDelivery` is an enum: the available values are
`CommandDelivery.RoundTrip` and `CommandDelivery.ShortCircuit`. 

    If you don't specify, the default value (`CommandDelivery.RoundTrip`) will be used: no short-circuiting.

`SendCommand()` returns an `ICommandResponseHandler` object which exposes the following methods:

* `OnSuccess(CommandSuccessCallback<TResponse> successCallback)` is the callback triggered if the command succeeds.
    * `TResponse Response` is the response object, of the type defined in the schema.
* `OnFailure(CommandFailureCallback failureCallback)` is the callback triggered if the `StatusCode != StatusCode.Success`.

You don't have to specify either of these callbacks if you don't want to.

For a full example, see the [command recipe](../tutorials/recipes/command.md).

## Responding to a command request

To decide which worker should respond to a command, look at which workers have access to the component containing the command.
Whichever worker has [*write access* (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#read-and-write-access-authority) to
the component will be sent the command request, and should respond. For example, if the UnityWorker has write
access to a player's Health component, the UnityWorker should implement the command.

You can respond to a command:

* synchronously, using `exampleComponentWriter.CommandReceiver.OnTakeDamage.RegisterResponse`
* asynchronously, using 

Respond to a command by registering a callback using `exampleComponentWriter.CommandReceiver.OnExampleCommand`. 
The callback is passed a `ResponseHandle` object which has:

* the incoming request (`.Request`) member
* a method to send back the response (`.Respond(CommandReturnType response)`)
* information data about the command caller - its `WorkerId` and [attributes](../interact-with-world/create-acls.md) (.`CallerDetails`)

Register all callbacks in `OnEnable()` or later, and deregister them in `OnDisable`, in order to prevent unexpected and 
hard-to-debug errors.

For example:

```
public void OnEnable() 
{
    // Register callback for the command being received
    exampleComponentWriter.CommandReceiver.OnExampleCommand.RegisterResponse(RespondToExampleCommand);
}
public void OnDisable()
{
    // Deregister callback
    exampleComponentWriter.CommandReceiver.OnExampleCommand.DeregisterResponse();
}

private ExampleResponse RespondToExampleCommand(ExampleRequest request, ICommandCallerInfo callerInfo)
{
    // Do what is requested in the command here

    // Send a response to the requester of the command
    return new ExampleResponse();
}

```

For a full example, see the [command recipe](../tutorials/recipes/command.md) page.
