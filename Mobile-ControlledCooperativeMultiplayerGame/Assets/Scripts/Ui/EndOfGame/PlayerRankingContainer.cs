using System.Linq;
using UnityEngine;

[RequireComponent(typeof(FlexibleGridLayout))]
public class PlayerRankingContainer : MonoBehaviour
{
    [SerializeField] private PlayerScoreScroll playerScoreScrollPrefab;

    private FlexibleGridLayout _flexibleGridLayout;

    private void Awake()
    {
        _flexibleGridLayout = GetComponent<FlexibleGridLayout>();
    }
    
    void Start()
    {
        var playerRankings = PlayerDataKeeper.Instance
            .PlayerDatas
            .OrderByDescending(data => data.GoldCounter)
            .GroupBy(data => data.GoldCounter)
            .SelectMany((group, index) =>
                group.Select(data => new Ranking()
                {
                    Rank = index + 1,
                    Name = data.name,
                    Gold = data.GoldCounter
                })
            )
            .Take(4)
            .ToArray();

        _flexibleGridLayout.Columns = playerRankings.Length;
        foreach (var playerRanking in playerRankings)
        {
            var playerScroll = Instantiate(playerScoreScrollPrefab, _flexibleGridLayout.transform);
            playerScroll.Init(playerRanking);
        }
        Debug.Log(playerRankings.Length);
        _flexibleGridLayout.enabled = false;
        _flexibleGridLayout.enabled = true;
    }
}
