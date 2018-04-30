# Setup guide for macOS

## 1. System requirements

SpatialOS works with up-to-date versions of macOS Sierra, El Capitan and High Sierra.

Before following the setup guide check that your machine meets the
[hardware requirements](../../get-started/requirements.md#hardware).

## 2. Set up the SpatialOS `spatial` CLI

To set up the `spatial` CLI (`spatial`):

0. Download the `spatial` CLI  for macOS (64bit): [Improbable GitHub Spatial CLI download for Mac](https://console.improbable.io/toolbelt/download/latest/mac).

    By downloading `spatial`, you agree to the [SpatialOS EULA](https://auth.improbable.io/auth/v1/eula).
0. Put `spatial` in a directory.

    For example, `bin` in your home directory: `mkdir -p $HOME/bin && mv $HOME/Downloads/spatial $HOME/bin`.
0. Add the directory to your `PATH`. To do so:
    0. Open a terminal window.
    0. Move into home directory: run `cd`.
    0. Determine which configuration file to use (you will need to know your configuration file again in later steps): run `ls -a ~`

        * If you have a `.bash_profile`, that's your configuration file.
        * Otherwise, if you have a `.bash_login`, that's your configuration file.
        * Otherwise, if you have a `.profile`, that's your configuration file.
        * If you don't have any of these files, you can use `.bash_profile` (which you'll create in the next step).
    0. Add the location of spatial to the `PATH` variable: run `echo 'export PATH=$PATH:/path_to_spatial_here' >> ~/your_config_file_here`

        For example: `echo 'export PATH=$PATH:$HOME/bin' >> ~/.bash_profile`
0. Make sure `spatial` is executable: run `chmod +x /path_to_spatial_here`.
0. Restart your terminal window to register the changes made to the path.
0. Check this succeeded by running `spatial update` in your terminal. You should get the output:

    `Current version of the 'spatial' command-line tool: <version number>`

    `Attempting to download the latest version...`

## 3. Set up Unity

To use the Unity SDK, **even if you've already got Unity installed**,
you **must** follow these steps, because SpatialOS only works with specific versions of Unity, and
requires specific build support.

> Unity versions **5.6.0** and **2017.3.0** have been tested with SpatialOS. Other versions may work fine, but have not been extensively tested.

Install Unity version **2017.3.0** and the optional extras that SpatialOS requires:

1. Go to the [Unity Download Archive](https://unity3d.com/get-unity/download/archive).
2. Next to version 2017.3.0, from the "Downloads (Mac)" dropdown, select "Unity Installer".
3. Run the installer. In addition to the defaults, select `Linux Build Support` and `Windows Build Support`:

    ![Unity screenshot](../../assets/setup/setup-unity-build-support-mac.png)

4. Install Unity into the default installation directory.

    Alternatively, you can set the environment variable `UNITY_HOME` to your customized Unity installation folder.
For example, if you installed Unity to `/Applications/Unity2017.3.0`,
then set `UNITY_HOME` to `/Applications/Unity2017.3.0`.

5. Launch Unity and complete the registration process.

> **Important**: Once you've set up a project, you'll need to set the path to `spatial` from the Unity SpatialOS editor window.
See the [Unity SpatialOS editor window](../../reference/spatialos-window.md#settings) page for instructions.

## 4. (optional) Install the Launcher

If you want to run a game client to connect to a SpatialOS game running in the cloud, you must install the
[Launcher (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#launcher).

1. <a href="https://console.improbable.io/launcher/download/stable/latest/mac" data-track-link="Launcher Download Clicked|platform=Mac" target="_blank">Download the Launcher</a>.
2. Follow the instructions in the installation wizard.

## 5. Next steps

You've now set up your machine for development with SpatialOS!
