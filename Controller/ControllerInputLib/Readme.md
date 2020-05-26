This is a TypeScript library to send inputs from a remote controller to the
Unity game.

See the `ControllerInput` class in `./src/ControllerInputSender.ts` for
documentation.

The server part of the Game is implemented by the `ControllerInput` prefab /
the `ControllerInput.cs` script.

You can quickly test this library by starting the game in the `TestRoom` scene
with the `ControllerDebugUI` component and running `npm test`.
See also the `./__tests__/send.ts` file.
