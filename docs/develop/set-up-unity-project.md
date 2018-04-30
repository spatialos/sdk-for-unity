# Setting up a project

## Prerequisites

* **You've gone through the SDK setup guide** for [Windows](../get-started/setup/win.md) or
[Mac](../get-started/setup/mac.md) and have
    everything installed.
* **You have a [project (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#project).** You can use one of the examples at the
[SpatialOS GitHub repository](https://github.com/spatialos).

## 1. Configure project name

Make sure your [project name (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#project-name) is set correctly in your
[project (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#project)'s `spatialos.json`:

1. In the project's root directory, open `spatialos.json`. This file holds your project's global configuration.
2. In the `name` field, make sure the value is your assigned SpatialOS project name.

    You can find your project name in the [Console](https://console.improbable.io). It'll be something
    like `beta_someword_anotherword_000`.

> In our [example projects](https://github.com/spatialos), the
project name is set to `your_project_name_here`.

> You **must** change this. If you don't:

> * `spatial upload` will fail with the error `code = PermissionDenied desc = No permission to create assembly...`
> * [deployments (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#deploying) will fail with the error
`Encountered an error during command execution. no permissions to access Project...`

## 2. Initialize the project

Before you can do any development on your project, you need to initialize it:

To set up a project on your local machine:

0. Make sure the Unity Editor is closed. If it's not, it'll cause build errors.
0. Open a terminal and navigate (`cd`) to the root directory of the project.
0. Build the project by running the
[command (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/glossary#the-spatial-command-line-tool)
[`spatial worker build` (SpatialOS documentation)](https://docs.improbable.io/reference/12.2/shared/spatial-cli/spatial-worker-build).

Even if you're going to use Unity or an IDE and not the command line to build, you must build your project first, since
this will properly initialize it (e.g. obtain relevant dependencies).
