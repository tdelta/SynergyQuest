using TMPro;
using UnityEngine;

public class Ranking
{
    public int Rank;
    public string Name;
    public int Gold;
}

public class PlayerScoreScroll : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankingText = default;
    [SerializeField] private TextMeshProUGUI playerName = default;
    [SerializeField] private TextMeshProUGUI score = default;
    
    public void Init(Ranking ranking)
    {
        var rankingSuffix = "th";
        var rankingColor = new Color32(66, 66, 66, 255);
        switch (ranking.Rank)
        {
            case 1:
                rankingSuffix = "st";
                rankingColor = new Color32(180, 151, 0, 255);
                break;
            case 2:
                rankingSuffix = "nd";
                rankingColor = new Color32(158, 151, 147, 255);
                break;
            case 3:
                rankingSuffix = "rd";
                rankingColor = new Color32(192, 103, 77, 255);
                break;
        }
        
        rankingText.SetText($"{ranking.Rank}{rankingSuffix}");
        rankingText.color = rankingColor;
        playerName.SetText(ranking.Name);
        score.SetText(ranking.Gold.ToString());
        
        playerName.ForceMeshUpdate();
        playerName.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, playerName.textBounds.size.x);
    }
}
