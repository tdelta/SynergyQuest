using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace Sokoban
{
    [CreateAssetMenu(fileName = "SokobanSprites", menuName = "ScriptableObjects/SokobanSprites")]
    public class SokobanSprites : ScriptableObject
    {
        [SerializeField] private Sprite redBox = default;
        [SerializeField] private Sprite blueBox = default;
        [SerializeField] private Sprite greenBox = default;
        [SerializeField] private Sprite yellowBox = default;
        [SerializeField] private Sprite anyBox = default;
        
        [SerializeField] private Sprite redSwitch = default;
        [SerializeField] private Sprite blueSwitch = default;
        [SerializeField] private Sprite greenSwitch = default;
        [SerializeField] private Sprite yellowSwitch = default;
        [SerializeField] private Sprite anySwitch = default;

        public Sprite GetBoxSprite(PlayerColor color)
        {
            switch (color)
            {
                case PlayerColor.Red:
                    return redBox;
                case PlayerColor.Blue:
                    return blueBox;
                case PlayerColor.Green:
                    return greenBox;
                case PlayerColor.Yellow:
                    return yellowBox;
                case PlayerColor.Any:
                    return anyBox;
            }

            return anyBox;
        }

        public Sprite GetSwitchSprite(PlayerColor color)
        {
            switch (color)
            {
                case PlayerColor.Red:
                    return redSwitch;
                case PlayerColor.Blue:
                    return blueSwitch;
                case PlayerColor.Green:
                    return greenSwitch;
                case PlayerColor.Yellow:
                    return yellowSwitch;
                case PlayerColor.Any:
                    return anySwitch;
            }
            
            return anySwitch;
        }
    }
}
