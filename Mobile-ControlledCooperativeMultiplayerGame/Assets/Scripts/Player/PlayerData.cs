using JetBrains.Annotations;
using System.Collections.Generic;

public class PlayerData
{
    public PlayerData(
        [NotNull] Input input
    )
    {
        _input = input;
    }
    
    private LinkedList<ItemDescription> _collectedItems = new LinkedList<ItemDescription>();
    public LinkedList<ItemDescription> CollectedItems => _collectedItems;

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
