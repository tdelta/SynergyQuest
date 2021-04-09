# controller-app

This is the controller web app of the game implemented in Typescript using React.
For further information, see [README.md](../README.md) in the above folder and the general
[build instructions](../../Notes/HowToBuild.md).

The app was bootstrapped with [Create React App](https://github.com/facebook/create-react-app).

## Usage

You first should orient yourself on the [README.md](../README.md) in the above folder the general
[build instructions](../../Notes/HowToBuild.md).
However, if you want to test this web app in isolation from the other game components, you can
follow these instructions.

We use [yarn](https://yarnpkg.com/) for managing dependencies and building the
project.

1. Build the controller client library at [../controller-client-lib](../controller-client-lib).
2. Build the sensor input library at [../sensor-input-lib](../sensor-input-lib).
3. Install dependencies: `yarn install`
4. Update the dependency on the local libraries:
   ```sh
   yarn upgrade ../controller-client-lib
   yarn upgrade ../sensor-input-lib
   ```
5. Either start the web app for development with
   ```sh
   yarn start
   ```
   or produce a production build with
   ```sh
   yarn build
   ```
   
When resuming development at a later point in time, you can omit the first 4 steps after having
them executed at least once the first time.
However, if you change one of the local libs, you have to run the first 4 steps again.

## Debugging & Designing

In case you want to display the game controls without starting the actual game and connecting to it, we provide a dummy client implementation, that simply acts as if it was connected to a game server.

You can display the game controls using:
```sh
yarn start-dummy
```

The dummy allows setting values such as the current health points or the enabled buttons (see ``App.tsx``), which simplifies work on the visual design of the UI.
