# sensor-input-lib

This is a TypeScript library which makes it easy for a web application to access the 3D orientation
of a device and interpret it as 2D directions using the Generic Sensor Web API:
https://developer.mozilla.org/en-US/docs/Web/API/Sensor_APIs

Be aware, that this library only works in a secure context:
https://developer.mozilla.org/en-US/docs/Web/Security/Secure_Contexts/features_restricted_to_secure_contexts

E.g. all connections must be SSL encrypted.

See [./src/OrientatinInput.ts](./src/OrientationInput.ts) for further information and documentation.

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
yarn add path/to/sensor-input-lib
```
