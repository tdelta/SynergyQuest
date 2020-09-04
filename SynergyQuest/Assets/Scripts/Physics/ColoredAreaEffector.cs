using System.Collections.Generic;
using UnityEngine;

/**
 * <summary>
 * Applies a force on any object with a <see cref="PlayerController"/> component while it is within the bounds of this
 * object.
 * Furthermore this behavior must be assigned a color using the <see cref="Colored"/> component.
 * The force is not applied to a player, if the player either does not have a <see cref="Colored"/> component or
 * if it has a <see cref="Colored"/> component and the component has been assigned the same color as this object.
 * If this behaviour is assigned the color <see cref="PlayerColor.Any"/> or no <see cref="Colored"/> component at all,
 * all players are affected by the force.
 * </summary>
 */
[RequireComponent(typeof(PlayerController))]
public class ColoredAreaEffector : MonoBehaviour
{
    /**
     * Force to be applied
     */
    [SerializeField] private Vector2 force = default;
    
    private PlayerColor _unaffectedColor = PlayerColor.Any;
    
    /**
     * Caches handles to all forces applied to players so far, so that the forces can be removed as soon as the players
     * leave the area of effect.
     */
    private Dictionary<PlayerController, ForceEffect> _activeForces = new Dictionary<PlayerController, ForceEffect>();

    private void Awake()
    {
        if (TryGetComponent<Colored>(out var colored))
        {
            _unaffectedColor = colored.Color;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (
            other.TryGetComponent(out PlayerController player) &&
            (this._unaffectedColor == PlayerColor.Any ||
            !player.Color.IsCompatibleWith(this._unaffectedColor))
        )
        {
            var forceEffect = player.PhysicsEffects.ApplyForce(force);
            
            _activeForces.Add(player, forceEffect);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (
            other.TryGetComponent(out PlayerController player) &&
            _activeForces.TryGetValue(player, out var forceEffect)
        )
        {
            player.PhysicsEffects.RemoveForce(forceEffect);
            _activeForces.Remove(player);
        }
    }
}
