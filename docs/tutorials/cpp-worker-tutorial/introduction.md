# C++ worker tutorial

> **Learn how to work with SpatialOS by adding simple features implemented in
an engine-agnostic logic worker.**

This tutorial branches out of the Pirates tutorial to show you an alternative
way of architecting a SpatialOS game.

## What you'll do

You'll implement the behaviour of pirate ships. They'll move randomly just
like in [Lesson 3 of Pirates](../../tutorials/pirates/lesson3.md). You'll
also add a shooting behaviour to pirate ships similar to how player ships shoot
in [Lesson 4 of Pirates](../../tutorials/pirates/lesson4.md). This will
let you compare the SpatialOS SDK for Unity and workflow with its C++ counterpart.

At the end, there are suggestions for more complex behaviours you could tackle on your own
to reinforce what you've learned.

## What you'll learn

Through adding features to the game you'll learn:

- how to add a C++ managed worker to an existing SpatialOS project
- how to use the C++ SDK alongside a game running in a game engine
- features of SpatialOS and its C++ SDK

## Why is this useful

Sometimes you might want to take parts of the game logic out of the core game process.
Some of the reasons for doing this are outlined in [Designing workers](https://docs.improbable.io/reference/13.0/shared/design/design-workers).

Understanding how to use the C++ SDK through simple examples is the first step on the
way to building more complex logic workers. For example, if you want to integrate a
game engine with SpatialOS, the concepts covered in this tutorial will lay the foundation.

## Before you start

If you haven't already completed the Unity-only version of the
[Pirates tutorial](../../tutorials/pirates/overview.md), you are strongly encouraged
to do so. By doing this, you will find it much easier to follow this tutorial.
You will start with the completed
[Lesson 2 of Pirates](../../tutorials/pirates/lesson2.md) and add features to the game.

### Prerequisites

You will need:

- To set up your machine by following the guide for [Windows](../../get-started/setup/win.md) or
[Mac](../../get-started/setup/mac.md). This includes installing Unity so that you can run the Pirates game.

- [CMake](https://cmake.org/) which is used for building the C++ worker in
  this tutorial.

- A C++ compiler of your choice which supports C++11.

- [Git](https://git-scm.com/) - this is optional, but you might need to
  download some files manually if you don't have it.

> If you have any trouble, please ask for help on the [forums](https://forums.improbable.io/)!

**Begin the tutorial by starting the
[first lesson](../../tutorials/cpp-worker-tutorial/add-cpp-worker.md)**.
