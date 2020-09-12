# ssl-warning-info

This is a small React web app which guides users through SSL certificate errors.
It is needed, since usually self-signed certificates must be used when serving the
main controller web app over SSL. (And the main controller web app must be served
over SSL to be able to use the IMU sensor web APIs).

This small web app has been implemented in Typescript using React.
For further information on how to include it in a build of the game, see
the general [build instructions](../../Notes/HowToBuild.md).

The app is employed by the game like this:

1. The game hosts 2 HTTP servers on two different ports: 
  * The server for the main controller web app and its websocket connections (default port 8000, SSL encrypted)
  * The server for this web app (default port 8080, not encrypted)
2. The game displays the address of the second server to users
3. The users visit the address and the SSL warnings they are about to encounter are explained by this web app.
4. This web app redirects the users to the first server where the main controller web
   app is served. They will likely encounter a certificate warning now, but should have
   now the knowledge to skip past the warning.

This sub-project was bootstrapped with [Create React App](https://github.com/facebook/create-react-app).

## Usage

To bundle this web app with the main game, orient yourself on the main build instructions:
[build instructions](../../Notes/HowToBuild.md).
However, if you want to test this web app in isolation from the other game components, you can
follow these instructions.

We use [yarn](https://yarnpkg.com/) for managing dependencies and building the
project.

1. Install dependencies: `yarn install`
2. Either start the web app for development with
   ```sh
   yarn start
   ```
   or produce a production build with
   ```sh
   yarn build
   ```
