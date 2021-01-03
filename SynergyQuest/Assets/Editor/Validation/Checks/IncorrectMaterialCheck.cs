using JetBrains.Annotations;
using Settings;
using UnityEditor;
using UnityEngine.Tilemaps;

namespace Editor.Validation.Issues
{
    public class IncorrectMaterialCheck : Check
    {
        [CanBeNull]
        public Issue PerformCheck()
        {
            var tilemap = TilemapExtensions.FindMainTilemap();

            if (tilemap.TryGetComponent(out TilemapRenderer renderer))
            {
                if (renderer.sharedMaterial.name.Replace(" (Instance)", "") != MaterialSettings.Instance.TilemapMaterial.name)
                {
                    return new IncorrectMaterialIssue(renderer);
                }
            }

            return null;
        }
        
        private class IncorrectMaterialIssue : Issue
        {
            public IncorrectMaterialIssue(TilemapRenderer renderer)
            {
                this.renderer = renderer;
            }
                
            private TilemapRenderer renderer;
                
            public string Description => "The tilemap must use the correct tilemap material from the MaterialSettings";
            public bool CanAutofix => true;
            public void Autofix()
            {
                renderer.material = MaterialSettings.Instance.TilemapMaterial;
                EditorUtility.SetDirty(renderer);
            }
        }
    }
}