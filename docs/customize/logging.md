# Logging

The SpatialOS SDK for Unity will send messages logged via Unity's `Debug.Log` family of functions to SpatialOS. You can view these logs
in your cloud deployment's web interface. See the documentation for [cloud deployments (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/operate/logs#cloud-deployments)
for more information.

## Global log levels
You can control which log messages are sent by setting flags on `WorkerConfigurationData`.
Log messages will be sent to SpatialOS when a flag is set to `true`.

| Unity logger          | WorkerConfigurationData flag | SpatialOS log level | Defaults to |
| --------------------- | ---------------------------- | ------------------- | -------------------- |
| `Debug.Log`           | `LogDebugToSpatialOs`        | `Info`              | `false` (Don't send) |
| `Debug.LogWarning`    | `LogWarningToSpatialOs`      | `Warn`              | `true`  (Send)       |
| `Debug.LogAssert`     | `LogAssertToSpatialOs`       | `Error`             | `false` (Don't send) |
| `Debug.LogError`      | `LogErrorToSpatialOs`        | `Error`             | `true`  (Send)       |
| `Debug.LogException`  | `LogExceptionToSpatialOs`    | `Error`             | `true`  (Send)       |

Configure your global log settings and then connect to SpatialOS.
In the example below, no warnings will be sent to SpatialOS.

```
// Don't send any warnings to SpatialOS.
configurationData.Debugging.LogWarningToSpatialOs = false;
// ...
SpatialOS.ApplyConfiguration(configurationData);
SpatialOS.Connect(gameObject);
```

## Log filtering

If you need more control than simply the level of log messages, you can assign a message filter to `SpatialOS.LogFilter`.
In the example below, all warnings will be sent to SpatialOS, except for a specific warning which comes from a badly-behaved plugin.

First, implement the interface:

``` 
private class ExcludeSpecificWarning : ILogFilterReceiver
{
    public LogAction FilterLogMessage(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Warning && logString.Contains("Trivial warning from a plugin"))
        {
            // Never send this message.
            return LogAction.DontSend;
        }
        // All other messages will be sent according to the filtering rules setup in configurationData.
        return LogAction.SendIfAllowed;
    }
}
```

Then install the handler:

```    
// Always send warnings to SpatialOS…
configurationData.Debugging.LogWarningToSpatialOs = true;
// …but ignore this specific warning from a badly-behaved plugin…
SpatialOS.LogFilter = new ExcludeSpecificWarning();
// …
SpatialOS.ApplyConfiguration(configurationData);
SpatialOS.Connect(gameObject);
```

You could also have the opposite problem: you generally don't want to send warnings, but one warning in particular is important.
In that case, you can globally disable warnings, and then look for a specific warning to always send.

As above, first implement the interface:

```
private class IncludeSpecificWarning : ILogFilterReceiver
{
    public LogAction FilterLogMessage(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Warning && logString.Contains("Very important warning from a plugin"))
        {
            // Always send this message.
            return LogAction.SendAlways;
        }
        // All other messages will be sent according to the filtering rules setup in configurationData.
        return LogAction.SendIfAllowed;
    }
}
```

Then install the handler:

``` 
// Never send warnings to SpatialOS…
configurationData.Debugging.LogWarningToSpatialOs = false;
// …but always send a warning from a specific plugin…
SpatialOS.LogFilter = new IncludeSpecificWarning();
// …
SpatialOS.ApplyConfiguration(configurationData);
SpatialOS.Connect(gameObject);
```
