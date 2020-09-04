using System.Collections;
using UnityEngine;

public class CoinGaugeController : MonoBehaviour
{
    // ToDo: Adjust height, so that lifeGauge and goldGauge can be displayed concurrently!
    
    private TextMesh _goldText;
    private PlayerController _player;

    public void Init(PlayerController player)
    {
        _player = player;
        _player.Data.OnGoldCounterChanged += DrawGoldCounter;
    }

    private void Awake()
    {
        _goldText = this.gameObject.transform.GetChild(0).gameObject.GetComponent<TextMesh>();
    }

    private void OnDestroy()
    {
        if (_player != null)
        {
            _player.Data.OnGoldCounterChanged -= DrawGoldCounter;
        }
    }

    public void DrawGoldCounter(int gold) {
        this.gameObject.SetActive(true);

        _goldText.text = gold.ToString();
        StartCoroutine(DeactiveCoroutine());
    }

    IEnumerator DeactiveCoroutine() {
        yield return new WaitForSeconds(2f);
        this.gameObject.SetActive(false);
    }
}



