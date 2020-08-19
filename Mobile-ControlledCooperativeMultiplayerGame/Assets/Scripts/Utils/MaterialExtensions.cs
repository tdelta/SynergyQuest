using UnityEngine;

public static class MaterialExtensions
{
    /**
     * Adds a new instance of the given material to the given renderer and return a reference to it.
     * You must use <see cref="MaterialExtensions.Destroy"/> to destroy the material again.
     */
    public static Material Instantiate(this Material materialPrefab, Renderer renderer)
    {
        var oldMaterials = renderer.materials;

        renderer.materials = oldMaterials.Plus(materialPrefab);
        var instance = renderer.materials[oldMaterials.Length];

        return instance;
    }

    /**
     * <summary>
     * Removes the given material from the given renderer and destroys it.
     * </summary>
     * <seealso cref="Instantiate"/>
     */
    public static void Destroy(this Material materialInstance, Renderer renderer)
    {
        renderer.materials = renderer.materials.RemoveFirst(materialInstance);
        Object.Destroy(materialInstance);
    }
}
