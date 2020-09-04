# Proxy for Development With a React Server

Though the game contains a http server to serve the controller web app, it is rather
inconvenient to use during development.
To instead use the normal React development server to serve the controller web app
while still being able to connect to the game via websockets, use this proxy application.

Further instructions and explanation in the [README.md](../README.md) in the above folder.

## Usage

You should orient yourself on the [README.md](../README.md) in the above folder.

We use [yarn](https://yarnpkg.com/) for managing dependencies and building this proxy.
Run the following commands to retrieve dependencies and start the proxy:

```sh
yarn install
yarn start
```
