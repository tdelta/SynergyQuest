using UnityEditor;
using UnityEngine.Tilemaps;

namespace Editor.Validation.Issues
{
    public class MissingShaderControllerCheck : Check
    {
        public Issue PerformCheck()
        {
            var tilemap = TilemapExtensions.FindMainTilemap();

            if (!tilemap.TryGetComponent(out PuddleShaderController puddleShaderController))
            {
                return new MissingShaderControllerIssue(tilemap);
            }

            return null;
        }
        
        private class MissingShaderControllerIssue : Issue
        {
            public MissingShaderControllerIssue(Tilemap tilemap)
            {
                this.tilemap = tilemap;
            }
            private Tilemap tilemap;
                
            public string Description => $"The tilemap must have the {nameof(PuddleShaderController)} component to enable reflections.";
            public bool CanAutofix => true;
            public void Autofix()
            {
                Undo.AddComponent<PuddleShaderController>(tilemap.gameObject);
            }
        }
    }
}