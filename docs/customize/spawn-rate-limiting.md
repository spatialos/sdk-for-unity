# Controlling how many entities are spawned in a frame

There are times when a Unity worker receives many `AddEntity` ops in a single frame, such as when it starts up,
or when it moves into a new area with many new entities.
The Unity SDK may spend so much time spawning new entities that the player's screen appears to hang.

To help you avoid this, the Unity SDK provides a few ways to manage spawning of new entities.
Configure this before you connect to SpatialOS.
If you're using one of our starter projects, customize your spawn rate limiting in `workers/unity/Assets/Gamelogic/Core/Bootstrap.cs`.

## Count based (default)

The simplest strategy is to only spawn a limited number of entities per frame.
*This is the default if you don't explicitly set a spawn limiter.*

The default is a maximum of 100 entities per frame. You will probably want to tune this for your specific project.

``` 
// Allow editing from the Unity inspector.
[Range(1,10000)]
public int MaxEntitiesToSpawnPerFrame = 100;
public void Start()
{
    SpatialOS.EntitySpawnLimiter = new CountBasedSpawnLimiter(MaxEntitiesToSpawnPerFrame);
    // ...
    SpatialOS.ApplyConfiguration(workerConfigurationData);
    SpatialOS.Connect(gameObject);
}
```

## Time based

A more flexible strategy is to limit the amount of time the Unity SDK spends spawning new entities during a single frame.

```
// Allow editing from the Unity inspector.
[Range(1, 1000)]
public int MaxMillisToSpendSpawningEntities = 4;    // 4 milliseconds is 25% of a frame at 60fps.
public void Start()
{
    SpatialOS.EntitySpawnLimiter = new TimeBasedSpawnLimiter(TimeSpan.FromMilliseconds(MaxMillisToSpendSpawningEntities));
    // ...
    SpatialOS.ApplyConfiguration(workerConfigurationData);
    SpatialOS.Connect(gameObject);
```

> Note that the time limit is not an exact budget.
Spawning an expensive entity may go over the time budget for a frame.
This approach will smooth out large stutters, but can't fully remove the chance of shorter ones.

## Always spawn

If you don't care about limiting spawn rates, use the greediest strategy. The greedy strategy will spawn all of the
available entities every frame that they're available.

```
public void Start()
{
    SpatialOS.EntitySpawnLimiter = new GreedySpawnLimiter();
    // ...
    SpatialOS.ApplyConfiguration(workerConfigurationData);
    SpatialOS.Connect(gameObject);
}
```