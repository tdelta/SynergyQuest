using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

/**
 * Contains the settings for transition animations between scenes.
 * See `TransitionController` for further explanation.
 * 
 * To work correctly, an instance of this scriptable object must be saved as `Resources/TransitionSettings`.
 */
[CreateAssetMenu(fileName = "TransitionSettings", menuName = "ScriptableObjects/TransitionSettings")]
public class TransitionSettings: ScriptableObjectSingleton<TransitionSettings>
{
    /**
     * Prefab of the UI game object which is animated
     */
    public TransitionController transitionControllerPrefab;

    /**
     * Settings for every different kind of transition animation
     */
    public TransitionTypeAnimator[] transitionTypeAnimators = {};

    /**
     * Retrieves the animator override controller with the custom animation settings for a specific type of transition
     */
    [CanBeNull]
    public AnimatorOverrideController GetAnimatorOverride(TransitionType transitionType)
    {
        // If the transition type is None, no animation shall be played and we return null
        if (transitionType == TransitionType.None)
        {
            return null;
        }

        // Otherwise, we return the animator override controller stored in the settings for the given animation type
        else
        {
            return transitionTypeAnimators
                .FirstOrDefault(data => data.transitionType == transitionType)?
                .animatorOverride;
        }
    }
}

/**
 * The different animation types supported for transitioning between scenes
 */
public enum TransitionType
{
    None, // No animation
    Fade, // Fade to/from black
    // Black overlay wiping over scene from different directions. Nice for door transitions.
    WipeDown,
    WipeUp,
    WipeLeft,
    WipeRight
}

/**
 * Stores information, on how the animation shall be changed for every transition type
 */
[Serializable]
public class TransitionTypeAnimator
{
    public TransitionType transitionType;
    [FormerlySerializedAs("animator")] public AnimatorOverrideController animatorOverride;
}
