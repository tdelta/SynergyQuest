using UnityEngine;

public class TrapMazeController: Pressable
{
  [SerializeField] private TrapController[] Traps;
  [SerializeField] private int NumPlayers;

  void RotateColors()
  {
    foreach (var trap in Traps) {
      trap.SetColor(PlayerColorMethods.NextColor(trap.Color, NumPlayers));
    }
  }

  public override void OnButtonPressed()
  {
    RotateColors();
  }
}
