using System.Collections;
using UnityEngine;

public class CoinGaugeController : MonoBehaviour
{
    // ToDo: Adjust height, so that lifeGauge and goldGauge can be displayed concurrently!

    [SerializeField] private Renderer[] renderers = default;

    private TextMesh _goldText;
    private PlayerController _player;

    private bool _subscribedToGoldCounterChange = false;

    private void SetVisiblility(bool visible)
    {
        foreach (var renderer in renderers)
        {
            renderer.enabled = visible;
        }
    }

    public void Init(PlayerController player)
    {
        _player = player;
        if (!_subscribedToGoldCounterChange)
        {
            _player.Data.OnGoldCounterChanged += DrawGoldCounter;
            _subscribedToGoldCounterChange = true;
        }
    }

    private void Awake()
    {
        _goldText = this.gameObject.transform.GetChild(0).gameObject.GetComponent<TextMesh>();
    }

    private void Start()
    {
        SetVisiblility(false);
    }

    private void OnEnable()
    {
        if (!_subscribedToGoldCounterChange && _player != null)
        {
            _player.Data.OnGoldCounterChanged += DrawGoldCounter;
            _subscribedToGoldCounterChange = true;
        }
    }

    private void OnDisable()
    {
        if (_subscribedToGoldCounterChange && _player != null)
        {
            _player.Data.OnGoldCounterChanged -= DrawGoldCounter;
            _subscribedToGoldCounterChange = false;
        }
    }

    private void DrawGoldCounter(int gold) {
        SetVisiblility(true);

        _goldText.text = gold.ToString();
        StartCoroutine(DeactiveCoroutine());
    }

    IEnumerator DeactiveCoroutine() {
        yield return new WaitForSeconds(2f);
        SetVisiblility(false);
    }
}



