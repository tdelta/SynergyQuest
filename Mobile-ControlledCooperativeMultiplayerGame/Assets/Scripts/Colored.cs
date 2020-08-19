using UnityEngine;

/**
 * <summary>
 * Players have an identifying color, <see cref="PlayerColor"/> which dictates whether or not they can interact with
 * certain objects or not.
 * This component assigns an identifying color to an object. If the color is changed in the editor, it makes sure other
 * color-controlled components like <see cref="ColorSwitch"/> or <see cref="ColorReplacer"/> use the same color.
 * </summary>
 */
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
