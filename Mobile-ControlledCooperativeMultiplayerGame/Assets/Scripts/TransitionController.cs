using System;
using UnityEngine;

public class TransitionController: MonoBehaviour
{
    private Animator _animator;
    private RuntimeAnimatorController _originalAnimatorController;
    
    private static readonly Lazy<TransitionController> _instance = new Lazy<TransitionController>(() =>
    {
        var instance = Instantiate(TransitionSettings.Instance.transitionControllerPrefab);
        
        // make sure the game object can survive loading other scenes
        DontDestroyOnLoad(instance);

        return instance;
    });
    public static TransitionController Instance => _instance.Value;

    public delegate void TransitionCompletionCallback();

    private TransitionCompletionCallback _onTransitionCompleted;
    private static readonly int TransitionOutOfSceneTrigger = Animator.StringToHash("TransitionOutOfScene");
    private static readonly int TransitionIntoSceneTrigger = Animator.StringToHash("TransitionIntoScene");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _originalAnimatorController = _animator.runtimeAnimatorController;
    }

    public void TransitionOutOfScene(TransitionType transitionType, TransitionCompletionCallback callback)
    {
        PauseGameLogic.Instance.Pause();
        
        _onTransitionCompleted = callback;

        if (transitionType != TransitionType.None)
        {
            _animator.runtimeAnimatorController = TransitionSettings.Instance.GetAnimatorOverride(transitionType) ?? _originalAnimatorController;
            _animator.SetTrigger(TransitionOutOfSceneTrigger);
        }

        else
        {
            OnTransitionOutCompleted();
        }
    }
    
    public void TransitionIntoScene(TransitionType transitionType, TransitionCompletionCallback callback)
    {
        PauseGameLogic.Instance.Pause();
        _onTransitionCompleted = callback;

        if (transitionType != TransitionType.None)
        {
            _animator.runtimeAnimatorController = TransitionSettings.Instance.GetAnimatorOverride(transitionType) ?? _originalAnimatorController;
            _animator.SetTrigger(TransitionIntoSceneTrigger);
        }

        else
        {
            OnTransitionInCompleted();
        }
    }

    void OnTransitionOutCompleted()
    {
        PauseGameLogic.Instance.Resume();
        _onTransitionCompleted.Invoke();
    }

    void OnTransitionInCompleted()
    {
        PauseGameLogic.Instance.Resume();
        _onTransitionCompleted.Invoke();
    }
}
