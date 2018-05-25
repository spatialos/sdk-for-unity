# Alternative setup steps for Windows

If you don't want to use the [installer](../../get-started/setup/win.md) to set up your Windows machine for development with SpatialOS, you can either:

* use [Chocolatey](#set-up-your-machine-using-chocolatey)
* set up your machine [manually](#set-up-your-machine-manually)

> You need to [have access to SpatialOS](https://spatialos.improbable.io/get-spatialos) to download the SDK.

## Set up your machine using Chocolatey

### 1. System requirements

We support up-to-date versions of Windows 7 and 10.

Before following the setup guide check that your machine meets the
[hardware requirements](../../get-started/requirements.md#hardware).

> If you’re using a corporate network with a strict firewall, raise a [support request](https://improbableio.atlassian.net/servicedesk/customer/portal/5) (for customers with a service agreement) or ask on our [forums](https://forums.improbable.io) and we’ll take you through some custom setup steps.

### 2. Set up the `spatial` CLI

To set up the `spatial` CLI (`spatial`):

1. Install <a href="https://chocolatey.org/" data-track-link="chocolateyHomePageViewed|product=Docs|platform=Win" target="_blank">Chocolatey</a>, the package manager, following the instructions at
<a href="https://chocolatey.org/install" data-track-link="chocolateyHomePageViewed|product=Docs|platform=Win" target="_blank">chocolatey.org/install</a>.

1. Open a terminal (PowerShell or cmd) and run:

    ```
    choco install spatial --yes
    ```

    This will install the CLI and put it in your `PATH`. By installing it,
    you agree to the [SpatialOS EULA](https://auth.improbable.io/auth/v1/eula).

1. Close and re-open your console, then check the install succeeded by running `spatial update` in your terminal. You should get the output:

    `Current version of the 'spatial' command-line tool: <version number>`

    `Attempting to download the latest version...`
    

### 3. Set up Unity

To use the Unity SDK, **even if you've already got Unity installed**,
you **must** follow these steps, because SpatialOS only supports specific versions of Unity, and
requires specific build support.

> Unity versions **5.6.0** and **2017.3.0** have been tested with SpatialOS. Other versions may work fine, but have not been extensively tested.

1. Choose one of the options below:

    * **If you already have Unity 2017.3.0 installed**:

        In a terminal, run the following commands to install Unity build support for Linux and Mac, and MSVC
        Redistributable:

        ```
        spatial setup install-dependencies --sdk-version=13.0
        choco install --yes unity-linux --version 2017.3.0
        choco install --yes unity-mac --version 2017.3.0
        ```

    * **If you have another version of Unity**, or don't have it installed at all:

        In a terminal, run the following command to install Unity 2017.3.0,
        and MSVC Redistributable:

        ```
        spatial setup install-dependencies --sdk-version=13.0 --with_unity
        ```

    By running these commands you agree with the individual terms of each separate dependency.

    Run `spatial setup install-dependencies --sdk-version=13.0 --help`
    for more information.

1. Restart your computer to complete the install process.

1. If you didn't already have Unity installed, run it and register or activate your license.

### 4. (optional) Install the Launcher

If you want to run a game client to connect to a SpatialOS game running in the cloud, you must install the
[Launcher](https://docs.improbable.io/reference/13.0/shared/operate/launcher).

1. <a href="https://console.improbable.io/launcher/download/stable/latest/win" data-track-link="Launcher Download Clicked|platform=Win" target="_blank">Download the Launcher</a>.
1. Follow the instructions in the installation wizard.

## Set up your machine manually

### 1. System requirements

We support up-to-date versions of Windows 7 and 10. 

Before following the setup guide check that your machine meets the
[hardware requirements](../../get-started/requirements.md#hardware).

> If you’re using a corporate network with a strict firewall, raise a [support request](https://improbableio.atlassian.net/servicedesk/customer/portal/5) (for customers with a service agreement) or ask on our [forums](https://forums.improbable.io) and we’ll take you through some custom setup steps.

### 2. Set up the `spatial` CLI

To set up the `spatial` CLI (`spatial`):

1. Download `spatial` for Windows (64bit): <a href="https://console.improbable.io/toolbelt/download/latest/win" data-track-link="Spatial Downloaded|product=Docs|platform=Win" target="_blank">Download</a>.

    By downloading `spatial`, you agree to the [SpatialOS EULA](https://auth.improbable.io/auth/v1/eula).
1. Put `spatial.exe` in a directory where you have *administrator privileges*.
1. Add the directory containing `spatial.exe` to your `PATH` by following
[these instructions](https://www.java.com/en/download/help/path.xml).

1. Check this succeeded by running `spatial update` in your terminal. You should get the output:

    `Current version of the 'spatial' command-line tool: <version number>`

    `Attempting to download the latest version...`

### 3. Set up Unity

To use the Unity SDK, **even if you've already got Unity installed**,
you **must** follow these steps, because SpatialOS only supports specific versions of Unity, and
requires specific build support.

> Unity versions **5.6.0** and **2017.3.0** have been tested with SpatialOS. Other versions may work fine, but have not been extensively tested.

1. If you haven’t got it installed already, download and install [Visual C++ Redistributable for Visual Studio 2015](https://www.microsoft.com/en-gb/download/details.aspx?id=48145).
    > We recommend you download both the x64 and x86 versions of the the Visual C++ Redistributable. The Unity Editor requires x64, and the Wizards demo and Pirates tutorial produce 32-bit workers by default (which require x86).

1. Go to the [Unity Download Archive](https://unity3d.com/get-unity/download/archive).
1. Next to version 2017.3.0, from the "Downloads (Win)" dropdown, click "Unity Installer"
(which downloads the installer).
1. Run the installer. 
    
    * **If you already have Unity 2017.3.0 installed**:

        Select `Linux Build Support` and `Mac Build Support`. De-select all the other checkboxes:

        ![Unity screenshot](../../assets/setup/setup-unity-just-build-support-win.png)

    * **If you have another version of Unity**, or don't have it installed at all:

        **IMPORTANT**: In addition to the defaults, select `Linux Build Support` and `Mac Build Support`:

        ![Unity screenshot](../../assets/setup/setup-unity-build-support-win.png)

1. Install Unity into the default installation directory: `%PROGRAMFILES%\Unity`.

    Alternatively, you can set the environment variable `UNITY_HOME` to your customized Unity installation folder.
    For example, if you installed Unity to `C:\Unity 2017.3.0`, then set `UNITY_HOME`
    to `C:\Unity 2017.3.0`.

1. Launch Unity and complete the registration process.

### 4. (optional) Install the Launcher

If you want to run a game client to connect to a SpatialOS game running in the cloud, you must install the
[Launcher (SpatialOS documentation)](https://docs.improbable.io/reference/13.0/shared/glossary#launcher).

1. [Download the Launcher](https://console.improbable.io/launcher/download/stable/latest/win).
1. Follow the instructions in the installation wizard.