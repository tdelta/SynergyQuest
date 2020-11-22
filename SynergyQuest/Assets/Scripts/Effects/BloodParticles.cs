using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Effects
{
    [RequireComponent(typeof(BloodParticles))]
    public class BloodParticles : MonoBehaviour
    {
        public Color lightColor = new Color(1, 0.149f, 0, 1);
        public Color darkColor = new Color(0.733f, 0.0784f, 00, 1);
        
        public delegate void OnDoneAction();
        [CanBeNull, NonSerialized] public OnDoneAction onDoneCallback = default;
            
        private ParticleSystem particleSystem;

        private void Awake()
        {
            particleSystem = GetComponent<ParticleSystem>();
        }

        private void Start()
        {
            var main = particleSystem.main;
            var startColor = main.startColor;
            startColor.colorMin = lightColor;
            startColor.colorMax = darkColor;

            main.startColor = startColor;

            main.stopAction = ParticleSystemStopAction.Callback;
        }

        public void Trigger(Vector2 direction)
        {
            var rotation = Quaternion.FromToRotation(
                Vector2.up,
                direction
            );
            particleSystem.transform.rotation = rotation;
            
            particleSystem.Play();
        }

        private void OnParticleSystemStopped()
        {
            onDoneCallback?.Invoke();
        }
    }
}