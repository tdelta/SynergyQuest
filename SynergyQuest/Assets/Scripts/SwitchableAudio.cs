using UnityEngine;

/**
 * Allows to change the clip played by a audio source using one or more switches.
 *
 * For example, this behaviour can be used to change the background music of a monster room when all monsters have been
 * defeated.
 */
[RequireComponent(typeof(AudioSource), typeof(Switchable))]
public class SwitchableAudio : MonoBehaviour
{
    [SerializeField] private AudioClip initialAudio = default;
    [SerializeField] private AudioClip switchedAudio = default;
    
    private AudioSource _audioSource;
    private Switchable _switchable;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _switchable = GetComponent<Switchable>();
    }

    private void OnEnable()
    {
        _switchable.OnActivationChanged += OnActivationChanged;
    }

    private void OnDisable()
    {
        _switchable.OnActivationChanged -= OnActivationChanged;
    }

    private void Start()
    {
        OnActivationChanged(_switchable.Activation);
    }

    private void OnActivationChanged(bool activation)
    {
        _audioSource.clip = activation ? switchedAudio : initialAudio;
        _audioSource.Play();
    }
}
