using UnityEngine;
using Utils;

namespace Effects
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BloodStain : MonoBehaviour
    {
        public Color color = new Color(1, 0.149f, 0, 1);
        [SerializeField] private Sprite[] bloodStainSprites = new Sprite[0];

        private SpriteRenderer _renderer = default;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            RandomExtensions
                .SelectUniform(bloodStainSprites)
                .Match(
                    some: sprite => _renderer.sprite = sprite,
                    none: () => Debug.LogError("No blood stain sprites available.")
                );

            _renderer.color = color;
        }
    }
}