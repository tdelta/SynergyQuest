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

using JetBrains.Annotations;
using System.Collections.Generic;
using DamageSystem;
using UnityEngine;

public class PlayerData : IHealthSaver
{
    public PlayerData(
        [NotNull] Input input
    )
    {
        _input = input;
        _playerInfo = new PlayerInfo();
        
        _input.UpdatePlayerInfo(_playerInfo);
    }
    
    private LinkedList<ItemDescription> _collectedItems = new LinkedList<ItemDescription>();
    public LinkedList<ItemDescription> CollectedItems => _collectedItems;

    private PlayerInfo _playerInfo;

    private Input _input;
    public Input input => _input;
    
    public delegate void GoldCounterChangedAction(int goldCounter);
    public event GoldCounterChangedAction OnGoldCounterChanged;

    public int GoldCounter {
      get => _playerInfo.Gold;
      set {
        _playerInfo.Gold = value;
        _input.UpdatePlayerInfo(_playerInfo);
        OnGoldCounterChanged?.Invoke(value);
      }
    }

    private bool _healthPointsInitialized = false;

    /**
     * <summary>
     * Set the health points to an initial value only if they have not been initialized in a previous scene yet.
     * </summary>
     */
    public void InitHealthPoints(int healthPoints)
    {
        if (!_healthPointsInitialized)
        {
            HealthPoints = healthPoints;
            _healthPointsInitialized = true;
        }
    }
    
    /*
     * FIXME: This value should only be readable and writable by the Health component.
     * If is is set here, OnHealthChanged will not be called.
     */
    public int HealthPoints
    {
        get => _playerInfo.HealthPoints;
        set {
        _playerInfo.HealthPoints = value;
        _input.UpdatePlayerInfo(_playerInfo);
      }
    }
    
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
