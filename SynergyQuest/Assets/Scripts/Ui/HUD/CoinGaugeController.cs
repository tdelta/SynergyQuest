// This file is part of the "Synergy Quest" game
// (github.com/tdelta/SynergyQuest).
// 
// Copyright (c) 2020
//   Marc Arnold     (m_o_arnold@gmx.de)
//   Martin Kerscher (martin_x@live.de)
//   Jonas Belouadi  (jonas.belouadi@posteo.net)
//   Anton W Haubner (anton.haubner@outlook.de)
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option) any
// later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
// FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for
// more details.
// 
// You should have received a copy of the GNU General Public License along with
// this program; if not, see <https://www.gnu.org/licenses>.
// 
// Additional permission under GNU GPL version 3 section 7 apply,
// see `LICENSE.md` at the root of this source code repository.

 using System.Collections;
 using Teleporting;
 using UnityEngine;

[RequireComponent(typeof(TeleportHandler))]
public class CoinGaugeController : MonoBehaviour
{
    // ToDo: Adjust height, so that lifeGauge and goldGauge can be displayed concurrently!

    [SerializeField] private Renderer[] renderers = default;

    private TextMesh _goldText;
    private PlayerController _player;
    private TeleportHandler _teleportHandler;

    private bool _subscribedToGoldCounterChange = false;

    private bool _visibility = true;

    private bool Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
            ApplyVisibility(true);
        }
    }

    private void ApplyVisibility(bool timeout)
    {
        foreach (var renderer in renderers)
        {
            renderer.enabled = _visibility;
        }
        
        if (_visibility && timeout)
        {
            StartCoroutine(DeactivateCoroutine());
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

        _teleportHandler = GetComponent<TeleportHandler>();
    }

    private void OnEnable()
    {
        Visibility = false;
        
        if (!_subscribedToGoldCounterChange && _player != null)
        {
            _player.Data.OnGoldCounterChanged += DrawGoldCounter;
            _subscribedToGoldCounterChange = true;
        }

        _teleportHandler.OnTeleportIn += OnTeleportIn;
        _teleportHandler.OnTeleportOut += OnTeleportOut;
    }

    private void OnDisable()
    {
        Visibility = false;
        
        if (_subscribedToGoldCounterChange && _player != null)
        {
            _player.Data.OnGoldCounterChanged -= DrawGoldCounter;
            _subscribedToGoldCounterChange = false;
        }
        
        _teleportHandler.OnTeleportIn -= OnTeleportIn;
        _teleportHandler.OnTeleportOut -= OnTeleportOut;
    }

    private void DrawGoldCounter(int gold)
    {
        Visibility = true;

        _goldText.text = gold.ToString();
    }

    IEnumerator DeactivateCoroutine() {
        yield return new WaitForSeconds(2f);
        Visibility = false;
    }

    private void OnTeleportIn()
    {
        // Invoke the visibility setter again, since teleporting messes with the renderers and we want to reset them to
        // a known state
        ApplyVisibility(false);
    }

    private void OnTeleportOut()
    {
        _visibility = false;
        ApplyVisibility(false);
    }
}



