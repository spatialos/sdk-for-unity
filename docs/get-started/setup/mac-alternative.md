# Alternative setup steps for macOS

If you don't want to use the [installer](../../get-started/setup/mac.md) to set up your Mac for development with SpatialOS, you can either:

* use [Homebrew](#set-up-your-machine-using-homebrew)
* set up your machine [manually](#set-up-your-machine-manually)

> You need to [have access to SpatialOS](https://spatialos.improbable.io/get-spatialos) to download the SDK.

## Set up your machine using Homebrew

### 1. System requirements

We support up-to-date versions of macOS Sierra, El Capitan and High Sierra.

Before following the setup guide check that your machine meets the
[hardware requirements](../../get-started/requirements.md#hardware).

> If you’re using a corporate network with a strict firewall, raise a [support request](https://improbableio.atlassian.net/servicedesk/customer/portal/5) (for customers with a service agreement) or ask on our [forums](https://forums.improbable.io) and we’ll take you through some custom setup steps.

### 2. Set up the `spatial` CLI

To set up the `spatial` CLI (`spatial`), and other prerequisites for the next step:

1. Install <a href="https://brew.sh/" data-track-link="homebrewHomePageViewed|product=Docs|platform=Mac" target="_blank">the Homebrew package manager</a>.

    ```
    /usr/bin/ruby -e "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/master/install)"
    ```

1. In a terminal, run:

    ```
    brew tap caskroom/cask
    brew tap improbable-io/spatialos
    brew update
    brew cask install spatial
    ```

    By installing `spatial`, you agree to the [SpatialOS EULA](https://auth.improbable.io/auth/v1/eula).

1. Check this succeeded by running `spatial update` in your terminal. You should get the output:

    `Current version of the 'spatial' command-line tool: <version number>`

    `Attempting to download the latest version...`

> Permission denied?
> 
> This is because the `spatial` command-line tool isn't executable. Try this:
> 
> 1. Run `which spatial` to get the path to `spatial`.
> 2. Run `chmod +x <path to spatial>`. This will make the tool executable.
> 
> For example, `chmod +x /usr/local/bin/spatial`

### 3. Set up Unity

To use the Unity SDK, **even if you've already got Unity installed**,
you **must** follow these steps, because SpatialOS only supports specific versions of Unity, and
requires specific build support.

> Unity versions **5.6.0** and **2017.3.0** have been tested with SpatialOS. Other versions may work fine, but have not been extensively tested.

1. Choose one of the options below:

    * **If you already have Unity 2017.3.0 installed**:

        In a terminal, run the following:

        ```
        brew cask install unity-ios-support-for-editor@2017.3.0
        brew cask install unity-linux-support-for-editor@2017.3.0
        brew cask install unity-windows-support-for-editor@2017.3.0
        ```

    * **If you have another version of Unity**, or don't have it installed at all:

        In a terminal, run the following:

        ```
        brew cask install unity@2017.3.0
        brew cask install unity-ios-support-for-editor@2017.3.0
        brew cask install unity-linux-support-for-editor@2017.3.0
        brew cask install unity-standard-assets@2017.3.0
        brew cask install unity-windows-support-for-editor@2017.3.0
        ```

        This installs Unity in the default installation directory, `/Applications/Unity`.

        If you want to use more than one version of Unity, you must:

        1. Rename this directory (for example, `mv /Applications/Unity /Applications/Unity2017.3.0`).
        This path cannot have spaces in it.
        1. Set the `UNITY_HOME` environment variable to point at your chosen Unity installation.

1. Launch Unity and complete the registration process.

### 4. (optional) Install the Launcher

If you want to run a game client to connect to a SpatialOS game running in the cloud, you must install the [Launcher](https://docs.improbable.io/reference/13.0/shared/operate/launcher).

1. <a href="https://console.improbable.io/launcher/download/stable/latest/mac" data-track-link="Launcher Download Clicked|platform=Mac" target="_blank">Download the Launcher</a>.
2. Follow the instructions in the installation wizard.

## Set up your machine manually

### 1. System requirements

We support up-to-date versions of macOS Sierra, El Capitan and High Sierra.

Before following the setup guide check that your machine meets the
[hardware requirements](../../get-started/requirements.md#hardware).

> If you’re using a corporate network with a strict firewall, raise a [support request](https://improbableio.atlassian.net/servicedesk/customer/portal/5) (for customers with a service agreement) or ask on our [forums](https://forums.improbable.io) and we’ll take you through some custom setup steps.

### 2. Set up the `spatial` CLI

To set up the `spatial` CLI (`spatial`):

1. Download `spatial` for macOS (64bit): <a href="https://console.improbable.io/toolbelt/download/latest/mac" data-track-link="Spatial Downloaded|product=Docs|platform=Mac" target="_blank">Download</a>.

    By downloading `spatial`, you agree to the [SpatialOS EULA](https://auth.improbable.io/auth/v1/eula).
1. Put `spatial` in a directory.

    For example, `bin` in your home directory: `mkdir -p $HOME/bin && mv $HOME/Downloads/spatial $HOME/bin`.
1. Add the directory to your `PATH`. To do so:

    1. Open a terminal window.
    1. Move into home directory: run `cd`.
    1. Determine which configuration file to use (you will need to know your configuration file again in later steps): run `ls -a ~`

        * If you have a `.bash_profile`, that's your configuration file.
        * Otherwise, if you have a `.bash_login`, that's your configuration file.
        * Otherwise, if you have a `.profile`, that's your configuration file.
        * If you don't have any of these files, you can use `.bash_profile` (which you'll create in the next step).
    1. Add the location of spatial to the `PATH` variable: run `echo 'export PATH=$PATH:/path_to_spatial_here' >> ~/your_config_file_here`

        For example: `echo 'export PATH=$PATH:$HOME/bin' >> ~/.bash_profile`
1. Make sure `spatial` is executable: run `chmod +x /path_to_spatial_here`.
1. Restart your terminal window to register the changes made to the path.
1. Check this succeeded by running `spatial update` in your terminal. You should get the output:

    `Current version of the 'spatial' command-line tool: <version number>`

    `Attempting to download the latest version...`

### 3. (optional) Set up Unity

To use the Unity SDK, **even if you've already got Unity installed**,
you **must** follow these steps, because SpatialOS only supports specific versions of Unity, and
requires specific build support.

> Unity versions **5.6.0** and **2017.3.0** have been tested with SpatialOS. Other versions may work fine, but have not been extensively tested.

Install Unity version **2017.3.0** and the optional extras that SpatialOS requires:

1. Go to the [Unity Download Archive](https://unity3d.com/get-unity/download/archive).
1. Next to version 2017.3.0, from the "Downloads (Mac)" dropdown, select "Unity Installer".
1. Run the installer. In addition to the defaults, select `Linux Build Support` and `Windows Build Support`:

    ![Unity screenshot](../../assets/setup/setup-unity-build-support-mac.png)

1. Install Unity into the default installation directory.

    Alternatively, you can set the environment variable `UNITY_HOME` to your customized Unity installation folder.
For example, if you installed Unity to `/Applications/Unity2017.3.0`,
then set `UNITY_HOME` to `/Applications/Unity2017.3.0`. __This path cannot have spaces in it.__

1. Launch Unity and complete the registration process.

> **Important**: Once you've set up a project, you'll need to set the path to `spatial` from the SpatialOS window in the Unity Editor.
See the [Unity SpatialOS window](../../reference/spatialos-window.md#settings) page for instructions.

### 4. (optional) Install the Launcher

If you want to run a game client to connect to a SpatialOS game running in the cloud, you must install the
[Launcher](https://docs.improbable.io/reference/13.0/shared/operate/launcher).

1. <a href="https://console.improbable.io/launcher/download/stable/latest/mac" data-track-link="Launcher Download Clicked|platform=Mac" target="_blank">Download the Launcher</a>.
1. Follow the instructions in the installation wizard.