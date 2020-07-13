using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public enum TransitionType
{
    None,
    Fade,
    WipeDown,
    WipeUp,
    WipeLeft,
    WipeRight
}

[Serializable]
public class TransitionTypeAnimator
{
    public TransitionType transitionType;
    public AnimatorOverrideController animator;
}

[CreateAssetMenu(fileName = "TransitionSettings", menuName = "ScriptableObjects/TransitionSettings")]
public class TransitionSettings: ScriptableObjectSingleton<TransitionSettings>
{
    public TransitionController transitionControllerPrefab;

    public TransitionTypeAnimator[] transitionTypeAnimators = {};

    [CanBeNull]
    public AnimatorOverrideController GetAnimatorOverride(TransitionType transitionType)
    {
        if (transitionType == TransitionType.None)
        {
            return null;
        }

        else
        {
            return transitionTypeAnimators
                .FirstOrDefault(data => data.transitionType == transitionType)?
                .animator;
        }
    }
}
