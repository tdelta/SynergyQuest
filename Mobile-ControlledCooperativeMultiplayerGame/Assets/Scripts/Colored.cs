using UnityEngine;

public class Colored : MonoBehaviour
{
    [SerializeField] private PlayerColor color = PlayerColor.Any;
    public PlayerColor Color => color;

    private void OnValidate()
    {
        if (GetComponent<ColorSwitch>() is ColorSwitch colorSwitch)
        {
            colorSwitch.Color = color;
        }

        if (GetComponent<ColorReplacer>() is ColorReplacer colorReplacer)
        {
            colorReplacer.ReplacementColor = color.ToRGB();
        }
    }
}
