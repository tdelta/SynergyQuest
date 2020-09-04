# controller-client-lib

This is a TypeScript library to send inputs from a remote controller (phone) to the
main Unity game and also receive data from the game over websockets.
It works in the browser and in nodejs (thanks to [isomorphic-ws](https://github.com/heineiuo/isomorphic-ws)).

See the `ControllerClient` class in [./src/ControllerClient.ts](./src/ControllerClient.ts) for
documentation.

The server part of the game is implemented by
[ControllerServer.cs](../../SynergyQuest/Assets/Scripts/ControllerInput/ControllerServer.cs),
[ControllerInput.cs](../../SynergyQuest/Assets/Scripts/ControllerInput/ControllerInput.cs)
and the other scripts in
[../../SynergyQuest/Assets/Scripts/ControllerInput](../../SynergyQuest/Assets/Scripts/ControllerInput).
Take a look, these files are also quite well documented.

The full controller web app which makes use of this library can be found at
[../controller-app](../controller-app).

## Usage

You first should orient yourself on the [README.md](../README.md) in the above folder the general
[build instructions](../../Notes/HowToBuild.md).
However, if you want to test this library in isolation from the other game components, you can
follow these instructions.

We use [yarn](https://yarnpkg.com/) for managing dependencies and building the
project.

1. Install dependencies: `yarn install`
2. Produce a production build with
   ```sh
   yarn build
   ```

To add this library to a node app:

```sh
yarn add path/to/controller-client-lib
```

## Remarks

Setting up a typescript library that works in nodejs and the browser and can be
imported by other projects is hell. Fortunately I found this template which
has been used to setup the build environment for this lib:
[create-react-library](https://github.com/transitive-bullshit/create-react-library)
