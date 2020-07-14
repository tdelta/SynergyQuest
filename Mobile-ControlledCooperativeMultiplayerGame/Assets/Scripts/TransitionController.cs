using System;
using UnityEngine;

/**
 * Singleton which can play animations for transitioning between scenes.
 * See also the `SceneController` singleton which makes use of this class.
 *
 * The game object prefab for instantiating this behavior and the used transition animations can be set in the
 * `TransitionSettings` scriptable object singleton, an instance of which can be found and edited in the Unity editor
 * at `Resources/TransitionSettings`.
 *
 * To provide a custom animation, perform the following steps:
 *
 * 1. Open the prefab set in the `TransitionSettings`
 * 2. Create two animations in it, one for transitioning out of a scene and one for transitioning into a new scene.
 *    You must call the `OnTransitionOutComplete` and `OnTransitionInComplete` methods using an AnimationEvent at the
 *    end of each animation respectively.
 * 3. Create a `AnimatorOverrideController` of the `Animations/Transitions/TransitionCanvas` animator.
 * 4. Set the `DefaultOutTransition` and `DefaultInTransition` to your custom animations in the override controller.
 * 5. Add a new value for your animation to the `TransitionType` enum
 * 6. Set the new enum value and your new animator override controller in the `TransitionSettings`
 *
 * The underlying concept on which all of this operates is based on this video:
 * https://www.youtube.com/watch?v=CE9VOZivb3I
 */
public class TransitionController: MonoBehaviour
{
    private Animator _animator;
    
    /**
     * We will override the controller of the animator to play different animations.
     * This field caches to original controller of the animator before we overwrote it.
     */
    private RuntimeAnimatorController _originalAnimatorController;
    
    /**
     * Instantiates this singleton lazily
     */
    private static readonly Lazy<TransitionController> _instance = new Lazy<TransitionController>(() =>
    {
        // Instantiate te prefab defined in the `TransitionSettings`
        var instance = Instantiate(TransitionSettings.Instance.transitionControllerPrefab);
        
        // make sure the game object can survive loading other scenes
        DontDestroyOnLoad(instance);

        return instance;
    });
    public static TransitionController Instance => _instance.Value;

    // Caches callback to call when animation completed
    public delegate void TransitionCompletionCallback();
    private TransitionCompletionCallback _onTransitionCompleted;
    
    private static readonly int TransitionOutOfSceneTrigger = Animator.StringToHash("TransitionOutOfScene");
    private static readonly int TransitionIntoSceneTrigger = Animator.StringToHash("TransitionIntoScene");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _originalAnimatorController = _animator.runtimeAnimatorController;
    }

    /**
     * Plays an animation to transition out of the current scene.
     * The game logic will be paused during the animation.
     * It does not actually change the current scene.
     * 
     * Usually you want to use `SceneController` instead of this singleton, since it can change the current scene and
     * does call this singleton internally to play animations.
     *
     * @param transitionType which transition animation to play
     * @param callback       function to call when the animation completed
     */
    public void TransitionOutOfScene(TransitionType transitionType, TransitionCompletionCallback callback)
    {
        // Pause game logic during animation
        PauseGameLogic.Instance.Pause();
        
        // Remember callback. It will be invoked once the animation completed in the `OnTransitionOutCompleted` method.
        _onTransitionCompleted = callback;

        // If the transition type is not none, play the transition animation
        if (transitionType != TransitionType.None)
        {
            // If there is a custom animation configured in the settings for the given transition type, use it.
            // Else, fall back to the default animation.
            _animator.runtimeAnimatorController = TransitionSettings.Instance.GetAnimatorOverride(transitionType) ?? _originalAnimatorController;
            
            // Play the animation
            _animator.SetTrigger(TransitionOutOfSceneTrigger);
        }

        // Otherwise, we do not play an animation and the transition is immediately complete
        else
        {
            OnTransitionOutCompleted();
        }
    }
    
    /**
     * Plays an animation to transition into the current scene.
     * The game logic will be paused during the animation.
     * It does not actually change the current scene.
     * 
     * Usually you want to use `SceneController` instead of this singleton, since it can change the current scene and
     * does call this singleton internally to play animations.
     *
     * @param transitionType which transition animation to play
     * @param callback       function to call when the animation completed
     */
    public void TransitionIntoScene(TransitionType transitionType, TransitionCompletionCallback callback)
    {
        // Pause game logic during animation
        PauseGameLogic.Instance.Pause();
        
        // Remember callback. It will be invoked once the animation completed in the `OnTransitionInCompleted` method.
        _onTransitionCompleted = callback;

        // If the transition type is not none, play the transition animation
        if (transitionType != TransitionType.None)
        {
            // If there is a custom animation configured in the settings for the given transition type, use it.
            // Else, fall back to the default animation.
            _animator.runtimeAnimatorController = TransitionSettings.Instance.GetAnimatorOverride(transitionType) ?? _originalAnimatorController;
            
            // Play the animation
            _animator.SetTrigger(TransitionIntoSceneTrigger);
        }

        // Otherwise, we do not play an animation and the transition is immediately complete
        else
        {
            OnTransitionInCompleted();
        }
    }

    /**
     * Called by the animator, once the animation completes for transitioning out of a scene
     * (AnimationEvent)
     */
    void OnTransitionOutCompleted()
    {
        // Resume paused game logic
        PauseGameLogic.Instance.Resume();
        
        // Invoke the callback for completing the transition animation
        _onTransitionCompleted?.Invoke();
    }

    /**
     * Called by the animator, once the animation completes for transitioning into a new scene
     * (AnimationEvent)
     */
    void OnTransitionInCompleted()
    {
        // Resume paused game logic
        PauseGameLogic.Instance.Resume();
        
        // Invoke the callback for completing the transition animation
        _onTransitionCompleted?.Invoke();
    }
}
