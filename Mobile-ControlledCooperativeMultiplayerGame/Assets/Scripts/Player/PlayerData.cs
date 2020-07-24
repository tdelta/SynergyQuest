using JetBrains.Annotations;

public class PlayerData
{
    public PlayerData(
        [NotNull] Input input,
        [CanBeNull] Item item
    )
    {
        _input = input;
        this.item = item;
    }
    
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
