> Building Unity iOS workers is experimental.

> iOS support has not been tested with Unity 2018.x.

# Unity iOS

Building Unity client workers for iOS platforms is experimental.
You can run your SpatialOS Unity client workers on both iOS devices and simulators.

## Requirements

* At least MacOS 10.12.6 (Sierra) and with Xcode 9 installed.
* Unity 2018.1.3 installed.
* "iOS Build Support" installed as an additional Unity component (specified when you install Unity).
* Active enrolment in the [Apple Developer Program for iOS](https://developer.apple.com/programs/),
either as a team or as an individual.
  + An Apple Developer Account is not strictly required for developing only with the iOS Simulator.
    But it is still required for code-signing any external libraries, which is a necessary step in this guide.
  + Apple requires an active enrolment for developing and deploying on **iOS devices**.

> Details about Apple Developer Accounts and the general distribution pipeline for Xcode and
iOS is beyond the scope of this guide.

> For more details, see
[Apple's developer documentation](https://developer.apple.com/library/content/documentation/IDEs/Conceptual/AppStoreDistributionTutorial/Introduction/Introduction.html#//apple_ref/doc/uid/TP40013839-CH1-SW1).

## Known issues

* Currently, developing for iOS devices only works with remote deployments: you can perform **iOS Device** testing against a remote deployment using `spatial cloud launch`.
Developing using the **iOS Simulator** works with both local and remote deployments.
* When building out iOS workers with Unity, the generated XCode project contains compilation errors due to a bug with Unity IL2CPP code generation.
See how to resolve these issues in our
[iOS Guide](../unity-ios/using.md#11-build-and-run-xcode-project).

## Getting started

The next section describes the steps required to getting with
[using SpatialOS with Unity iOS builds](../unity-ios/using.md).
