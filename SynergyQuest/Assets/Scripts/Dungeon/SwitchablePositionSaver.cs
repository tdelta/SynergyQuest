using System.Linq;
using UnityEngine;

/**
 * <summary>
 * Saves the positions of objects persistently across scenes, depending on the state of switches.
 * For example, with this, one can save the positions of Sokoban boxes only, iff the Sokoban was solved correctly.
 *
 * It requires the saved objects to have the <see cref="Guid"/> component.
 * This component only triggers the save/removal of positions. The actual lookup of saved positions is performed by the
 * <see cref="Guid"/> component.
 * </summary>
 */
[RequireComponent(typeof(Switchable))]
public class SwitchablePositionSaver : MonoBehaviour
{
    /**
     * Objects whose position shall be saved, when this switchable is activated
     */
    [SerializeField] private Guid[] objectsToSave = new Guid[0];
    
    /**
     * Automatically find all objects in the scene who have a tag of the given name and save their positions, iff
     * the switchable is activated
     */
    [SerializeField] private string[] autoDiscoveredTags = new string[0];
    
    private Switchable _switchable;

    private void Awake()
    {
        _switchable = GetComponent<Switchable>();

        objectsToSave = autoDiscoveredTags
            .SelectMany(GameObject.FindGameObjectsWithTag)
            .Select(gameObject => gameObject.GetComponent<Guid>())
            .Where(maybeGuid =>
            {
                if (maybeGuid is null)
                {
                    Debug.LogError(
                        "All objects for which a position shall be saved must have a Guid component. However, while searching for objects to save by the given tags, there was one included without a Guid component");
                    return false;
                }

                return true;
            })
            .Concat(objectsToSave)
            .ToArray();
    }

    private void Start()
    {
        // The Switchable component does not trigger this callback by itself for the initial activation when loading the
        // scene. Hence, we look the initial value up ourselves
        OnActivationChanged(this._switchable.Activation);
    }

    private void OnEnable()
    {
        _switchable.OnActivationChanged += OnActivationChanged;
    }
    
    private void OnDisable()
    {
        _switchable.OnActivationChanged -= OnActivationChanged;
    }

    private void OnActivationChanged(bool activation)
    {
        foreach (var guid in objectsToSave)
        {
            if (activation)
            {
                DungeonDataKeeper.Instance.SavePosition(guid);
            }

            else
            {
                DungeonDataKeeper.Instance.RemovePosition(guid);
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        // Make spawner visible in the editor by displaying an icon
        Gizmos.DrawIcon(transform.position, "floppy.png", true);
    }
}
