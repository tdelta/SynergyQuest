# controller-input-lib

This is a TypeScript library to send inputs from a remote controller to the
Unity game.

See the `ControllerInput` class in `./src/ControllerInputSender.ts` for
documentation.

The server part of the Game is implemented by the `ControllerInput` prefab /
the `ControllerInput.cs` script.

You can quickly test this library by starting the game in the `TestRoom` scene
with the `ControllerDebugUI` component and running `npm test`.
See also the `./__tests__/send.ts` file.

Furthermore, a react testing app which uses this app can be found at
`../input-lib-test-app`.

# Usage

To build the library

```bash
npm run build
```

To add the library to a node app:

```bash
npm install --save path/to/controller-input-lib
```

For a full usage example see `../input-lib-test-app`;

# Technical stuff

Setting up a typescript library that works in nodejs and the browser and can be
imported by other projects is hell. Fortunately I found this template which
has been used to setup the build environment for this lib:
[create-react-library](https://github.com/transitive-bullshit/create-react-library)
