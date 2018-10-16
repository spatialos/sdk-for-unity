# Setup guide for macOS

> Unity versions **2017.3.0** and **2018.1.3** have been tested with SpatialOS. Other versions may work fine, but have not been extensively tested.

## 1. Set up SpatialOS

Follow the [SpatialOS setup guide](https://docs.improbable.io/reference/13.0/shared/get-started/setup/mac).

## 2. Set up Unity

To use the SpatialOS SDK for Unity, **even if you've already got Unity installed**,
you **must** follow these steps, because SpatialOS only works with specific versions of Unity, and
requires specific build support.

1. Go to the [Unity Download Archive](https://unity3d.com/get-unity/download/archive).
1. Next to version 2018.1.3, from the "Downloads (Mac)" dropdown, select "Unity Installer".
1. Run the installer. In addition to the defaults, select
    * [x] `Linux Build Support`
    * [X] `Windows Mono Scripting Backend Support`

1. Install Unity into the default installation directory.

    Alternatively, you can set the environment variable `UNITY_HOME` to your customized Unity installation folder.
    For example, if you installed Unity to `/Applications/Unity2018.1.3`,
    then set `UNITY_HOME` to `/Applications/Unity2018.1.3`.

1. Launch Unity and complete the registration process.

> **Important**: Once you've set up a project, you'll need to set the path to `spatial` from the SpatialOS window in the Unity Editor.
See the [Unity SpatialOS window](../../reference/spatialos-window.md#settings) page for instructions.

## 3. Next steps

You've now set up your machine for development with SpatialOS!

To learn how to use SpatialOS and experiment with its main APIs, try the
[Pirates tutorial](../../tutorials/pirates/overview.md).
