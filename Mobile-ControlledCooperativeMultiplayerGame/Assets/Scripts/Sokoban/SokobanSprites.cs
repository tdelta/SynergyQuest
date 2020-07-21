using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace Sokoban
{
    [CreateAssetMenu(fileName = "SokobanSprites", menuName = "ScriptableObjects/SokobanSprites")]
    public class SokobanSprites : ScriptableObject
    {
        [SerializeField] private Sprite redBox;
        [SerializeField] private Sprite blueBox;
        [SerializeField] private Sprite greenBox;
        [SerializeField] private Sprite yellowBox;
        [SerializeField] private Sprite anyBox;
        
        [SerializeField] private Sprite redSwitch;
        [SerializeField] private Sprite blueSwitch;
        [SerializeField] private Sprite greenSwitch;
        [SerializeField] private Sprite yellowSwitch;
        [SerializeField] private Sprite anySwitch;

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