# Mobile-Controlled Cooperative Multiplayer Game

Serious Games Lab P09

# How to Build

1. Go to `./Controller/controller-client-lib` and run `npm install` and `npm build`
2. Go to `./Controller/controller-app` and run `npm install` and `npm build`
3. Open `./Mobile-ControlledCooperativeMultiplayerGame` in Unity and Build the project for Linux and or Windows
4. Copy the build files from step 3 into a folder `Build`
5. Create a folder `Build/controller-app` and copy `./Controller/controller-app/build/*` into there
6. Done, you can now run the executable produced by Unity

# Important Hints for Development

## How to setup Git LFS when cloning this project

```sh
sudo pacman -S git-lfs
git lfs install
```

## How to setup git to merge Unity Prefabs and Scenes

Unity Prefabs and Scenes are textfiles, but very complex and impossible to merge
by hand. However, Unity comes with a merging tool for them which must be
integrated with git:

1. Find out the path to the merge tool.
   For me, its at `/home/$USER/Unity/Hub/Editor/2019.3.14f1/Editor/Data/Tools/UnityYAMLMerge`.

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
arises. If the `.meta` file seems not to contain any important information, you
might get away with deleting the file and starting Unity instead of merging,
since Unity will try to regenerate them.

## Folder Structure

* Unity Game: `./Mobile-ControlledCooperativeMultiplayerGame`
* Mockups: `./Documentation/Mockups`
* Component Diagram: `./Documentation/ComponentDiagram`
* Controller
  * TypeScript Client library: `./Controller/controller-client-lib`
  * React App which shows how to use the library: `./Controller/controller-lib-test-app`

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
