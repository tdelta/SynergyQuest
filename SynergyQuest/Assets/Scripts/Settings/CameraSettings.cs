using UnityEngine;

/**
 * <summary>
 * This scriptable object singleton allows to store camera settings.
 * </summary>
 * <remarks>
 * An instance of this scriptable object should be stored as `Assets/Resources/CameraSettings` so that this resource
 * can be retrieved at runtime.
 * </remarks>
 */
[CreateAssetMenu(fileName = "CameraSettings", menuName = "ScriptableObjects/CameraSettings")]
public class CameraSettings : ScriptableObjectSingleton<CameraSettings>
{
    /**
     * <summary>
     * Radius around a player character which should always be included in the field of view.
     * </summary>
     * <seealso cref="GameObjectExtensions.SetFollowedByCamera"/>
     */
    public float PlayerInclusionRadius => playerInclusionRadius;
    [Tooltip("Which radius around a player character should always be included in the field of view?")]
    [SerializeField]
    private float playerInclusionRadius = 3;
}
