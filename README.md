# Mobile-Controlled Cooperative Multiplayer Game

Serious Games Lab P09

## How to setup Git LFS when cloning this project

```sh
sudo pacman -S git-lfs
git lfs install
```

## Folder Structure

* Unity Game: `./Mobile-ControlledCooperativeMultiplayerGame`
* Mockups: `./Documentation/Mockups`
* Component Diagram: `./Documentation/ComponentDiagram`
* Controller
  * TypeScript Client library: `./Controller/controller-input-lib`
  * React App which shows how to use the library: `./Controller/input-lib-test-app`

## How to keep Graphics Pixel Perfect

* we do not want any filtering, anti-aliasing etc. to happen to our sprites
  since the game shall emphasize a pixel-look
* Hence, always set the following settings for new sprites:
  * Filter Mode: Point (no filter)
  * Compression: None
  * Sprite Editor -> Pivot Unit Mode: Pixels
* Furthermore, if you get tiny gaps when rendering a tilemap, make sure,
  "Pixel Snap" is enabled for the material of the tilemap.
  You can also use the TileMap Material in the Materials folder
* See also https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@7.1/manual/2d-pixelperfect.html
