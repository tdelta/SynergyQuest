using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawn : MonoBehaviour
{
    [SerializeField] EnemyController boss = default;
    [SerializeField] AudioClip bossInactive = default;
    [SerializeField] AudioClip bossActive = default;

    Switchable _switchable;
    Switch _switch;
    AudioSource _audioSource;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _switchable = GetComponent<Switchable>();
        _switch = GetComponent<Switch>();
    }

    void Start()
    {
        if (!_switch.Value)
            _switchable.OnActivationChanged += OnSwitchActivate;
    }

    void OnSwitchActivate(bool activated)
    {
        if (activated)
        {
            _switchable.OnActivationChanged -= OnSwitchActivate;
            var instance = Instantiate(boss, transform.position, Quaternion.identity);
            instance.OnDeath += OnBossDead;
            instance.ShowParticles();
            _audioSource.clip = bossActive;
            _audioSource.Play();
        }
    }

    void OnBossDead()
    {
        _switch.Value = true;
        _audioSource.clip = bossInactive;
        _audioSource.Play();
    }
}
