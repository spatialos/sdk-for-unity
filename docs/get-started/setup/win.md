# Setup guide for Windows

This guide will prepare your Windows machine for local development with the SpatialOS SDK.

## 1. System requirements

SpatialOS works with up-to-date versions of Windows 7 and 10.

Before following the setup guide check that your machine meets the
[hardware requirements](../../get-started/requirements.md#hardware).

> If you’re using a corporate network with a strict firewall, raise a [support request](https://improbableio.atlassian.net/servicedesk/customer/portal/5) (for customers with a service agreement) or ask on our [forums](https://forums.improbable.io) and we’ll take you through some custom setup steps.

## 2. Install SpatialOS

Download the [SpatialOS installer](https://console.improbable.io/installer/download/stable/latest/win) and follow the steps.

This installs:

* the `spatial` CLI
* the SpatialOS Launcher

> If you don’t want to set up SpatialOS using the installer, see the [alternative setup steps for Windows](../../get-started/setup/win-alternative.md).

## 3. Set up Unity

To use the Unity SDK, **even if you've already got Unity installed**,
you **must** follow these steps, because SpatialOS only works with specific versions of Unity, and
requires specific build support.

> Unity versions **5.6.0** and **2017.3.0** have been tested with SpatialOS. Other versions may work fine, but have not been extensively tested.

1. If you haven’t got it installed already, download and install [Visual C++ Redistributable for Visual Studio 2015](https://www.microsoft.com/en-gb/download/details.aspx?id=48145).
	> We recommend you download both the x64 and x86 versions of the the Visual C++ Redistributable. The Unity Editor requires x64, and the Wizards demo
	 and Pirates tutorial produce 32-bit workers by default (which require x86).

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

## 5. Next steps

You've now set up your machine for development with SpatialOS!

To learn how to use SpatialOS and experiment with its main APIs, try the
[Pirates tutorial](../../tutorials/pirates/overview.md).
