using UnityEngine;

namespace Settings
{
    [CreateAssetMenu(fileName = nameof(MaterialSettings), menuName = "ScriptableObjects/"+nameof(MaterialSettings))]
    public class MaterialSettings: ScriptableObjectSingleton<MaterialSettings>
    {
        public Material TilemapMaterial => tilemapMaterial;
        [SerializeField] private Material tilemapMaterial = default;
    }
}