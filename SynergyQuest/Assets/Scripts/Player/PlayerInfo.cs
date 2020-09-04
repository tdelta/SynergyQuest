using System;

/**
 * Information about the player that is synchronized with the controller web app.
 *
 * To synchronize an additional field, add it to the PlayerInfo interface in ControllerClient.ts
 * and make sure ControllerInput.UpdatePlayerInfo is called (maybe from the PlayerData class).
 */
[Serializable]
public class PlayerInfo
{
    public static int MAX_HEALTH_POINTS = 5;

    /**
     * Initialize to default values in order to spawn a new player.
     */
    public PlayerInfo()
    {
        HealthPoints = MAX_HEALTH_POINTS;
        Gold = 0;
    }
    
    public int HealthPoints;
    public int Gold;
}