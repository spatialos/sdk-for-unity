# Release notes

## SpatialOS SDK for Unity 3.1.0
_Released: 2018-11-01_

### New features
* We have upgraded to use SpatialOS [13.3.0](https://docs.improbable.io/reference/13.3/releases/release-notes#13-3-0).

## SpatialOS SDK for Unity 3.0.0
_Released: 2018-10-17_

### Fixes
* The UnityClient worker no longer tries to login with a previously-used worker ID. You will no longer see this error message:
```
[improbable.receptionist.ReceptionistServiceImpl] Worker trying to login with previously used worker ID: UnityClient[...]. This is not allowed, please use a unique worker ID for each login attempt
```
* Improved performance for EntityPrefab loading in editor playmode. You will now see a short progress bar when entering play mode while EntityPrefabs are being prepared for loading.

### Breaking changes
* We have removed the `useExternalIp` command line flag. It is now be automatically set to `true` for UnityClient and to `false` for UnityWorker. To set `UseExternalIp` directly, call `SpatialOS.Configuration.UseExternalIp` before connecting to SpatialOS.

## SpatialOS SDK for Unity 2.0.1
_Released: 2018-07-05_

### New features
* We have upgraded to use SpatialOS [13.1.0](https://docs.improbable.io/reference/13.1/releases/release-notes#13-1-0).

### Fixes
* The timeout argument for sending commands (both schema defined commands and worker commands such as `CreateEntity`, `DeleteEntity`, ...) is now properly respected.

## SpatialOS SDK for Unity 2.0.0
_Released: 2018-06-06_

### New features
* We have added support for Unity 2018.1.3.

### Breaking changes
* We have removed support for Unity 5.6 and below. The minimum version supported is now 2017.3.0.
* We have removed the `Improbable.Unity.MinimalBuildSystem.Configuration.FallbackIndentLevelScope` class,
which provided backwards compatibility with Unity 5.6 and below.
In the unlikely event that you've used this class, please use `UnityEditor.EditorGUI.IndentLevelScope` instead. It should drop into place with no other changes necessary.
* We have removed the Unity 5.6 build target `StandaloneOSXIntel64` in `player-build-config.json`.
You should change this to `StandaloneOSX`.

## SpatialOS SDK for Unity 1.0.1
_Released: 2018-05-24_

### Fixes
* Deregistering a command response twice will no longer throw an exception.
* When a user exits a game while trying to connect, the connection failure callback reason now says "An application quit signal was received." Previously the reason was blank.
* The worker now sends component updates correctly including when its authority loss is imminent.
