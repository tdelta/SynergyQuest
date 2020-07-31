using JetBrains.Annotations;
using System.Collections.Generic;

public class PlayerData
{
    public PlayerData(
        [NotNull] Input input
    )
    {
        _input = input;
        _collectedItems = new LinkedList<Item>();
    }
    
    private LinkedList<Item> _collectedItems;
    public LinkedList<Item> CollectedItems => _collectedItems;

    private Input _input;
    public Input input => _input;
    public Item item { get; set; }

    public int goldCounter = 0;

    public string name
    {
        get
        {
            if (input is ControllerInput controllerInput)
            {
                return controllerInput.PlayerName;
            }

            else
            {
                return "Debug Player";
            }
        }
    }
}
