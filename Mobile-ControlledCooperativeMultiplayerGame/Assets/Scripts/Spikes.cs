using System;
using UnityEngine;

/**
 * <summary>
 * Spikes are a zone which can hurt the player, but which can be enabled and disabled by switches.
 * It relies on the <see cref="Switchable"/> component to receive signals from switches, <see cref="Switch"/>, which can
 * enable or disable it.
 * </summary>
 * <seealso cref="Switch"/>
 * <seealso cref="Switchable"/>
 */
[RequireComponent(typeof(Switchable))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(AudioSource))]
public class Spikes : MonoBehaviour
{
    /**
     * The sprite which is shown when the spikes are active
     */
    [SerializeField] private Sprite spikesOnSprite;
    /**
     * The sprite which is shown when the spikes are retracted
     */
    [SerializeField] private Sprite spikesOffSprite;
    /**
     * Sound which is played when the spikes are activated
     */
    [SerializeField] private AudioClip spikesUpSound;
    /**
     * Sound which is played when the spikes are retracted
     */
    [SerializeField] private AudioClip spikesDownSound;
    /**
     * How much damage the spikes shall deal on contact
     */
    [SerializeField] private int damage = 2;
    
    private SpriteRenderer _renderer;
    private Switchable _switchable;
    private BoxCollider2D _collider;
    private AudioSource _audioSource;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _switchable = GetComponent<Switchable>();
        _collider = GetComponent<BoxCollider2D>();
        _audioSource = GetComponent<AudioSource>();
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
        // The Switchable component does not trigger this callback by itself for the initial activation when loading the
        // scene. Hence, we look the initial value up ourselves
        OnActivationChanged(_switchable.Activation);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            var player = other.GetComponent<EntityController>();
            
            player.PutDamage(damage, Vector2.zero);
        }
    }

    /**
     * This callback is invoked, when all required switches to enable/disable the spikes change their state,
     * see also the `Switchable` component.
     */
    void OnActivationChanged(bool activation)
    {
        // The spikes are enabled, when the switch is not active
        // (Currently we only use this component together with DeadManSwitches)
        var spikesAreOn = !activation;
        
        if (spikesAreOn)
        {
            _audioSource.PlayOneShot(spikesUpSound);
            _renderer.sprite = spikesOnSprite;
            _collider.enabled = true;
        }
        
        else
        {
            _audioSource.PlayOneShot(spikesDownSound);
            _renderer.sprite = spikesOffSprite;
            _collider.enabled = false;
        }
    }
}
