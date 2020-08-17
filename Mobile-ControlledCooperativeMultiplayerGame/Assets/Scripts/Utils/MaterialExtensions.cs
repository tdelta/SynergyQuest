using UnityEngine;

public static class MaterialExtensions
{
    public static Material Instantiate(this Material materialPrefab, Renderer renderer)
    {
        var oldMaterials = renderer.materials;

        renderer.materials = oldMaterials.Plus(materialPrefab);
        var instance = renderer.materials[oldMaterials.Length];

        return instance;
    }

    public static void Destroy(this Material materialInstance, Renderer renderer)
    {
        renderer.materials = renderer.materials.RemoveFirst(materialInstance);
        Object.Destroy(materialInstance);
    }
}
