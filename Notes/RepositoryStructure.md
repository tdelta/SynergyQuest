# Components & Repository Structure

The game is split into a controller web app to be run on smartphones and
the main game, which also serves the controller web app.
More information on the implementation of the game can be found in the technical
report (see `Releases` page of this repository) and in the code documentation
(docstrings).

The repository is split into the following folders:

| Folder                           | Description                                                           |
|----------------------------------|-----------------------------------------------------------------------|
| [/SynergyQuest](../SynergyQuest) | The main game. Implemented in Unity                                   |
| [/Controller](../Controller)     | Contains projects to build the controller web-app (React+Typescript). |
| [/Certificates](../Certificates) | Contains scripts to generate self-signed certificates for SSL encryption. Used to encrypt all connections between game and controllers |
| [/Graphics](../Graphics)         | Source files for art specifically made for this game.*                |

---
*This does not include pixel art, as no special source files are required for that.
All finalized art used in the game (including sound files etc.) can be found in
[/SynergyQuest/Assets/Art](../SynergyQuest/Assets/Art).
Most art however was not specifically made for this game and stems from external
sources like [OpenGameArt](https://opengameart.org). All art from external sources is
credited to the original authors in the game. Furthermore, such art files are always
accompanied by a `Credits.md` file which lists the original source and author.
