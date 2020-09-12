# How to Build ⚒️

## Preparing the build environment

Development of the game has been done on Windows and Linux devices.

1. Install Unity version 2019.4.2f1
   (We recommend installing via Unity Hub. You should also be able to retrieve it from here: https://unity3d.com/get-unity/download/archive)
2. Install [yarn](https://yarnpkg.com/)
3. If you also want to edit source code and don't want to use a plain text editor,
   you should also install an editor with support or plugins for C# ([Rider](https://www.jetbrains.com/rider/),
   [Visual Studio Code](https://code.visualstudio.com/), ...) and for
   Typescript ([WebStorm](https://www.jetbrains.com/webstorm/), ...).
   
   For further instructions regarding development, see the
   [development setup hints](DevelopmentSetup.md).

This guide assumes the reader knows how to execute commands from a command line like
`bash`. 

## Building

### Building the Controller Web App

Execute the following commands:
```sh
cd Controller/controller-client-lib
yarn install
yarn build

cd ../sensor-input-lib
yarn install
yarn build

cd ../controller-app
yarn install
yarn upgrade ../controller-client-lib
yarn upgrade ../sensor-input-lib
yarn build
```

### Building a required Helper Web App

Execute the following commands:
```sh
cd Controller/ssl-warning-info
yarn install
yarn build
```
 
### Building the Main Game

1. Build the Controller Web App first.
2. Open [/SynergyQuest](../SynergyQuest) in the Unity Editor and build the project for Linux and / or Windows.
   For the remainder of the guide we assume the build is saved in `SynergyQuest/Build/SynergyQuest`.
3. Create a folder `SynergyQuest/Build/SynergyQuest/controller-app` and copy all files in `Controller/controller-app/build/*` into there
4. Create a folder `SynergyQuest/Build/SynergyQuest/ssl-warning-info` and copy all files in `Controller/ssl-warning-info/build/*` into there
5. (Optional: Generate SSL certificate used by game.
    This is not strictly required since a pre-built certificate with a ridiculous expiration date (900 years) is already shipped with this repository.
    Still, on Linux systems with openssl installed, a certificate can be built by running `Certificates/generate-self-signed.sh`.)
6. Create a folder `SynergyQuest/Build/SynergyQuest/Certificates` and copy `Certficates/generated/server.pfx` into it.
7. Done, you can now run the executable produced by Unity
8. (If you want to distribute the game, put `SynergyQuest/UsageNotes/ReadMe.txt` also into the folder and ZIP `SynergyQuest/Build/SynergyQuest`.)
