# Setting up Development and General Hints

These are some general hints on how to work on the game. However, most technical
information is available in the technical report (see `Releases` page of this
repository) and in the documentation of the code (docstrings).

However, you should at least install all required programs mentioned in
[How To Build](HowToBuild.md) and follow the instructions in the
[first](#setup-git-lfs-when-cloning-this-project),
[second](#setup-git-to-merge-unity-prefabs-and-scenes) and
[last](#working-on-the-controller-web-app)
hint on this page.

## Table Of Contents

* [Setup Git LFS when cloning this project](#setup-git-lfs-when-cloning-this-project)
* [Setup git to merge Unity Prefabs and Scenes](#setup-git-to-merge-unity-prefabs-and-scenes)
* [How to deal with .meta files](#how-to-deal-with-meta-files)
* [How to keep Graphics Pixel Perfect](#how-to-keep-graphics-pixel-perfect)
* [The Debug Settings](#the-debug-settings)
* [Scene Checklist](#scene-checklist)
* [Working on the Controller Web App](#working-on-the-controller-web-app)

## Setup Git LFS when cloning this project

You should use [Git LFS](https://git-lfs.github.com/) on the repository server and
in your git client. This makes it easier to deal with (large) binary files and keeps
the repository size manageable.

On an Arch Linux system, you can setup Git LFS for the client like this:
```sh
sudo pacman -S git-lfs
git lfs install
```

## Setup git to merge Unity Prefabs and Scenes

Unity Prefabs and Scenes are text files, but very complex and impossible to merge
by hand. However, Unity comes with an automatic merging tool for them which can be
integrated with git:

1. Find out the path to the merge tool.
   On a linux system, it can possibly be found at `/home/$USER/Unity/Hub/Editor/2019.4.2f1/Editor/Data/Tools/UnityYAMLMerge`
   when using Unity Hub.

   If its a system-wide installation, you might find it at `/opt/Unity/Editor/Data/Tools/UnityYAMLMerge`.
   If you are on windows, you can find it around here: `C:\Program Files\Unity\Editor\Data\Tools\UnityYAMLMerge.exe`
2. Edit your `~/.gitconfig` and add the following entries:
   ```
   [mergetool "unityyamlmerge"]
     binary = true
     trustExitCode = false
     cmd = '<path to UnityYamlMerge>' merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"
     keepTemporaries = true
     keepBackup = false
   [merge "unityyamlmerge"]
     name = Unity SmartMerge (UnityYamlMerge)
     driver = '<path to UnityYamlMerge>' merge -h -p --force %O %B %A %A
     recursive = binary
     keepTemporaries = true
     keepBackup = false
   ```

   (I have not tested this on Windows! You might need to mask some parts of the
   `cmd` strings, for example replace `\` in the path with `\\`.)

3. Set the git attributes of `.unity` and `.prefab` files to
   `merge=unityyamlmerge`. This has already been configured in `.gitattributes`
   for this project, so you should not need to do anything here.

4. Now, `git merge` should automatically merge Scene and Prefab files with
   the `UnityYamlMerge` tool.
   However, for a cleaner history, use of `git rebase` instead of `git merge` is
   recommended. For some reason, git does not respect the merge driver settings
   when using `rebase` automatically. However, when getting a conflict in a
   Scene/Prefab file you can still apply the Unity auto merge tool to the
   conflicted files like this:

   ```sh
   git mergetool --tool=unityyamlmerge path/to/scene.unity
   ```

   The configuration should make sure to leave the `REMOTE`, `LOCAL` and `BASE`
   versions of Scene and Prefab files in the file system. This is useful should
   the tool fail and you need to combine the different versions of a scene
   manually.
   Otherwise, you can just delete them.

For more information on the Unity merge tool, see
https://docs.unity3d.com/Manual/SmartMerge.html.

### How to deal with .meta files

It seems those files sometimes store important settings, e.g. links between
Scenes and Prefabs, etc. So you should try to merge them manually if a conflict
arises. If a `.meta` file seems not to contain any important information, you
might get away with deleting the file and starting Unity instead of merging,
since Unity will try to regenerate them.

`.meta` files should always be added to git.

## How to keep Graphics Pixel Perfect

* we do not want any filtering, anti-aliasing etc. to happen to our sprites
  since the game shall emphasize a pixel-look
* Hence, always set the following settings for new sprites:
  * Filter Mode: Point (no filter)
  * Compression: None
  * Sprite Editor -> Pivot Unit Mode: Pixels
* All sorts of anti-aliasing should be disabled in the render pipeline
* Furthermore, if you get tiny gaps when rendering a tilemap, make sure,
  "Pixel Snap" is enabled for the material of the tilemap.
  You can also use the TileMap Material in the Materials folder
* See also https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@7.1/manual/2d-pixelperfect.html
* We also use a Pixel Perfect Camera for the game
  (see https://blogs.unity3d.com/2019/03/13/2d-pixel-perfect-how-to-set-up-your-unity-project-for-retro-8-bits-games/
   and https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@7.1/manual/pixel-cinemachine.html).

## The Debug Settings

Always connecting to the game with a real phone or browser as game controller is tedious.
Instead you can spawn debug player instances which are controlled by the keyboard in any scene
with a spawner.

This and some other useful settings can be set in the `DebugSettings` of the game.
Just open [SynergyQuest/Assets/Resources/DebugSettings.asset](../SynergyQuest/Assets/Resources/DebugSettings.asset)
in the Unity Editor.
All settings are explained in the C# script implementing them
([SynergyQuest/Assets/Scripts/Settings/DebugSettings.cs](../SynergyQuest/Assets/Scripts/Settings/DebugSettings.cs)).

Also check out the other settings files in the same folder.

# Scene Checklist

When adding a new scene, make sure the following criteria are met, since these are frequent errors.
(For the future, it would be nice to implement an automatic validator for this.)

* There is MainCamera, CameraTargetGroup and MultiTargetCamera from Prefabs/Camera
  * CameraTargetGroup is assigned to MultiTargetCamera.Follow
* Grid Object:
  * Scale: (1, 1, 1)
  * Cell-Size: (1, 1, 1)
* Tilemap
  * Layer: LevelStatic
  * Tile Anchor: y=0
  * Position z=-2
  * Tilemap Renderer.Mode = Individual
  * Composite Collider 2D - Geometry Type: Polygon
  * Ground should be drawn with z=0
  * Walls should be drawn with z=2
  * Use prefab brushes whenever possible!
* There should be at least a RoomDefaultPlayerSpawner
* If it is a Dungeon level:
  * there must be a correctly configured DungeonLoader object
  * make sure the doors match the neighboring rooms. E.g. if your room has a gate one one side, the next room should also have a gate, not a door
  * (To learn more about how scenes are connected into a dungeon, read the documentation of
     [SynergyQuest/Assets/Scripts/Dungeon/DungeonLayout.cs](../SynergyQuest/Assets/Scripts/Dungeon/DungeonLayout.cs))
* If Sokoban boxes are present:
  * There should usually be a Prefabs/Sokoban/SokobanPositionSaver instance, so that sokoban positions are saved when a riddle is solved

# Working on the Controller Web App

See [Controller/README.md](../Controller/README.md).
