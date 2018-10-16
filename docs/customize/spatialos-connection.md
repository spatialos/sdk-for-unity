# Connecting to and disconnecting from SpatialOS

Connection and disconnection is already set up in all of our example projects, using the convention in `Bootstrap.cs`. See examples:

* [VR Starter Project](https://github.com/spatialos/VRStarterProject/blob/master/workers/unity/Assets/Gamelogic/Global/Bootstrap.cs)
* [Pirates Tutorial](https://github.com/spatialos/PiratesTutorial/blob/master/workers/unity/Assets/Gamelogic/Core/Bootstrap.cs)

This page is for those who want to customise or learn more about the connection process.

Customising the connection and disconnection process allows you to do things like:

* throw an error or try reconnecting if the first connection attempt fails
* switch scene when SpatialOS disconnects
* [set up command-line arguments](../customize/steam.md#3-set-up-command-line-arguments)
* [specify assets to precache](https://github.com/spatialos/PiratesTutorial/blob/master/workers/unity/Assets/Gamelogic/Core/Bootstrap.cs)

## Connect to SpatialOS

To connect to SpatialOS, you must configure your worker before calling `SpatialOS.Connect`.

```csharp
public class Bootstrap : MonoBehaviour
{
    private void Start()
    {
        SpatialOS.ApplyConfiguration(new WorkerConfigurationData());
        SpatialOS.Connect(gameObject);
    }
}
```

## Disconnect from SpatialOS

To disconnect from SpatialOS, call `SpatialOS.Disconnect()`.

```csharp
private void ConnectionTimeout()
{
    if (SpatialOS.IsConnected)
    {
        SpatialOS.Disconnect();
    }
}
```

## Optional callbacks

### Handle a successful attempt to connect

To register a callback in the event of a successful connection to SpatialOS, use `OnConnected`.

```csharp
public class Bootstrap : MonoBehaviour
{
    private void Start()
    {
        SpatialOS.ApplyConfiguration(new WorkerConfigurationData());
        SpatialOS.OnConnected += CreatePlayer;
        SpatialOS.Connect(gameObject);
    }

    private void CreatePlayer()
    {
        //CreatePlayer
    }
}
```

### Handle a failed attempt to connect

To register a callback in the event of an unsuccessful connection to SpatialOS, use `OnConnectionFailed`.

```csharp
public class Bootstrap : MonoBehaviour
{
    private void Start()
    {
        SpatialOS.ApplyConfiguration(new WorkerConfigurationData());
        SpatialOS.OnConnectionFailed += () => Debug.LogError("Connection Failed.");
        SpatialOS.Connect(gameObject);
    }
}
```

### Handle a disconnection

To register a callback in the event of a disconnection from SpatialOS, use `OnDisconnected`.

```csharp
public class Bootstrap : MonoBehaviour
{
    private void Start()
    {
        SpatialOS.ApplyConfiguration(new WorkerConfigurationData());
        SpatialOS.OnDisconnected += reason => UIHandler.ShowDisconnectionScreen(reason);
        SpatialOS.Connect(gameObject);
    }
}
```
