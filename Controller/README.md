# Controller Web App Components

This folder contains the projects to build the controller web app.

* [controller-client-lib](./controller-client-lib) contains the code to communicate with the main game
* [sensor-input-lib](./sensor-input-lib) abstracts over the 3D orientation of a phone and converts it into 2D directions which can be used as input to the game
* [controller-app](./controller-app) is the actual web app built in React and depends on the two libraries above

Furthermore, though the game contains a http server to serve the web app, it is rather
inconvenient to use during development. Hence, by default, the http server is disabled when
running the game in the Unity Editor, so that the web server included with React can be used
instead which is more suited for development.
Still, the game needs to serve websockets so that the phone controllers can establish a connection to it.

Thus, a proxy program at [./proxy](./proxy) is included in this repository.
When being run, it serves a web server at the normal game port
([https://localhost:8000](https://localhost:8000)) and redirects all "normal" HTTP requests to
the React server, while redirecting websocket requests to the game.

The typical web app development workflow is hence as follows:

1. Run [controller-client-lib](./controller-client-lib)
2. Run the main game in the Unity editor
3. Run the [proxy](./proxy)
4. Start developing

---

If you still want to serve the web app directly through the main game without the proxy and React
server, you have to adjust the server settings of the game in
[SynergyQuest/Assets/Resource/ServerSettings.asset](../SynergyQuest/Assets/Resources/ServerSettings.asset)
in the Unity Editor.
They are explained in [SynergyQuest/Assets/Scripts/Settings/ServerSettings.cs](../SynergyQuest/Assets/Scripts/Settings/ServerSettings.cs).

You also have to produce a production build of the web app (see [build instructions](../Notes/HowToBuild.md))
and place it in a folder `controller-app` in the working directory of the Unity Editor.
This is usually the folder [SynergyQuest](../SynergyQuest).
