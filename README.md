![development build](https://github.com/tdelta/SynergyQuest/workflows/Synergy%20Quest%20CI/badge.svg?branch=develop)

# Synergy Quest - A Mobile-Controlled Cooperative Multiplayer Game

<p align="center">
  <img width="460" src="./Graphics/Logo/logo_web.png">
</p>

SynergyQuest is a 2D top-down cooperative multiplayer action-adventure video
game.
It entertains 2-4 players by letting them delve into dungeon levels
filled with a variety of puzzle challenges, mini-games, and battle sequences.

<p align="center">
  <a href="https://youtu.be/S4HkH0xsG0k">
    <img width="230" src="./Graphics/Logo/trailer.png">
  </a>
</p>

What is special about this game is that players use their smartphones as
controllers. The game runs locally on a PC and serves a web-page to which the
players can connect with their phones. Using touch controls and sensor input
(accelerometer etc.) they can control their game character.

Download links for the latest release:

* [Download for Linux (x86_64)](https://github.com/tdelta/SynergyQuest/releases/download/release/SynergyQuest_Linux.zip)
* [Download for Windows](https://github.com/tdelta/SynergyQuest/releases/download/release/SynergyQuest_Windows.zip)

For detailed information on the game and the development process, see the
technical report (distributed on the `Releases` page of this repository).
Information on how to **build** the project is included in the readme files of
this repository:

* [Components & Repository Structure](Notes/RepositoryStructure.md)
* [How to Build ⚒](Notes/HowToBuild.md)
* [Setting up Development](Notes/DevelopmentSetup.md)

## Nightlies :milky_way:

You can download the latest builds of the development version here.
They might be unstable though:

* [Nightly for Linux (x86_64)](https://nightly.link/tdelta/SynergyQuest/workflows/main/develop/Linux%20x86_64%20Artifact.zip)
* [Nightly for Windows](https://nightly.link/tdelta/SynergyQuest/workflows/main/develop/Windows%20Artifact.zip)

## System Requirements

* A Linux 🐧 or Windows 💰 PC
* A smartphone for every player with a modern web browser installed
  ("officially" supported is the newest version of Google Chrome on Android
   Not supported: Firefox)
  * iOS devices are not supported
  
## Credits

The game code (excluding engine) and some assets have been originally developed by

* Jonas Belouadi - [@potamides](https://github.com/potamides)
* Marc Arnold - [@m-arnold](https://github.com/m-arnold)
* Martin Kerscher - [@maruker](https://github.com/maruker)
* Anton Haubner - [@ahbnr](https://github.com/ahbnr)
* David Heck - [@heckstrahler](https://github.com/heckstrahler)

The main dungeon graphics stem from the mobile roguelike game
[Shattered Pixel Dungeon](https://github.com/00-Evan/shattered-pixel-dungeon)
by [Evan Debenham a.k.a. 00-Evan](https://github.com/00-Evan) which is based on
[Pixel Dungeon](https://github.com/watabou/pixel-dungeon)
by [Oleg Dolya a.k.a. Watabou](http://www.watabou.ru/) who have generously allowed us to use their art
(see also the [License](#license) section).

Furthermore, additional art, music and sounds have been sourced from many external artists
through [OpenGameArt.Org](https://opengameart.org) etc.
All external artists are credited in the game menu and in
[SynergyQuest/Assets/Resources/ExternalArtCredits.yaml](SynergyQuest/Assets/Resources/ExternalArtCredits.yaml).

### Used Libraries

Besides the game engine [Unity](https://unity.com/) the game makes use of the following C#
libraries:

* [websocket-sharp](https://github.com/sta/websocket-sharp) ([MIT License](https://github.com/sta/websocket-sharp/blob/master/LICENSE.txt))
* [Clipper](http://www.angusj.com/delphi/clipper.php) ([Boost Software License](https://www.boost.org/LICENSE_1_0.txt))
* [Json.NET by Newtonsoft](https://www.newtonsoft.com/json) ([MIT License](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md))
* [YamlDotNet](https://github.com/aaubry/YamlDotNet) ([MIT License](https://github.com/aaubry/YamlDotNet/blob/master/LICENSE.txt))
* [ZXing.Net](https://github.com/micjahn/ZXing.Net) ([Apache License 2.0](https://github.com/micjahn/ZXing.Net/blob/master/COPYING))
* [Harmony2](https://github.com/pardeike/Harmony) ([MIT License](https://github.com/pardeike/Harmony/blob/master/LICENSE))

(We ship pre-built versions of these libraries with this repository. Binaries and license texts see [SynergyQuest/Assets/Plugins](./SynergyQuest/Assets/Plugins).)

Furthermore, various Javascript/Typescript libraries are in use in the game controller web app.
See
[1](./Controller/controller-client-lib/package.json)
[2](./Controller/sensor-input-lib/package.json)
[3](./Controller/controller-app/package.json)
[4](./Controller/ssl-warning-info/package.json).

## License

Excluding of course Unity, the libraries and external art assets,
this work is licensed under **GPLv3**, but additional permissions under
section 7 of GPLv3 apply.
Short Explanation (informal, please see `LICENSE.md` for the formal terms):

* Although Unity's license is not compatible with the standard GPLv3,
  we grant permission to convey (a modified version of) this work
  combined/linked with the Unity engine
* This work contains art assets by [00-Evan](https://github.com/00-Evan) and
  [Watabou](http://www.watabou.ru/) from the
  [Shattered Pixel Dungeon](https://github.com/00-Evan/shattered-pixel-dungeon)
  and [Pixel Dungeon](https://github.com/watabou/pixel-dungeon) games
  which are licensed under GPLv3. Similar permission as the above has been
  granted by 00-Evan and Watabou to convey these assets with the Unity engine
  when combined with this work but under certain conditions.
  Please see the complete note regarding their permission in [LICENSE.md](./LICENSE.md).

Of course, for all external assets and especially the Unity components, their respective licenses apply and their terms must be upheld.

Details see [LICENSE.md](./LICENSE.md).

