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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * Allows a player to collect and use items.
 */
[RequireComponent(typeof(PlayerController))]
public class ItemController: MonoBehaviour
{
  private PlayerController _player; 
  
  /**
   * Stores whether a specific item can currently not be used since it has a cooldown which hast not run out
   */
  private Dictionary<ItemDescription, bool> _cooldownFlags = new Dictionary<ItemDescription, bool>();

  private void Awake()
  {
      _player = GetComponent<PlayerController>();
  }

  /**
   * Whether the player has already collected the given item
   */
  public bool HasItem(ItemDescription itemDescription)
  {
      return _player
          .CollectedItems
          .Any(otherItemDescription => otherItemDescription.name == itemDescription.name);
  }
  
  /**
   * Allows the player to use the given item.
   * This also means, that the item's usage action will be enabled on controllers
   */
  public bool Collect(ItemDescription itemDescription)
  {
      if (!HasItem(itemDescription))
      {
          _player.CollectedItems.AddLast(itemDescription);
          _player.Input.EnableButtons((itemDescription.UseButton, true));
          return true;
      }
      return false;
  }

  private void Update()
  {
      var direction = _player.CarryPosition - _player.Center;
      // For every collected item
      foreach (var itemDescription in _player.CollectedItems)
      {
          if (
              _player.Input.GetButtonDown(itemDescription.UseButton) &&
              !_cooldownFlags.GetOrDefault(itemDescription, false) && // ...and whether it is not currently cooling down
              !_player.throwable.IsBeingCarried // a player can not use items when being carried
          )
          {
              // Instantiate the item
              var itemInstance = Instantiate(
                  itemDescription.ItemInstancePrefab,
                  VectorExtensions.Assign2D(itemDescription.ItemInstancePrefab.transform.position, this.transform.position),
                  Quaternion.identity
              );
              // Tell it, that this player is using it
              itemInstance.Activate(_player, itemDescription);
              
              // if the item has a cooldown, start it
              if (itemDescription.Cooldown > 0)
              {
                  _cooldownFlags[itemDescription] = true;
                  _player.Input.SetCooldownButtons((itemDescription.UseButton, true));
                  StartCoroutine(
                      CoroutineUtils.Wait(itemDescription.Cooldown, () =>
                      {
                          _cooldownFlags[itemDescription] = false;
                          _player.Input.SetCooldownButtons((itemDescription.UseButton, false));
                      })
                  );
              }
          }
      }
  }

  /**
   * Reset cooldowns when the scene changes or a player dies
   */
  private void OnDisable()
  {
      foreach (var itemDescription in _player.CollectedItems) {
          _cooldownFlags[itemDescription] = false;
          _player.Input.SetCooldownButtons((itemDescription.UseButton, false));
      }
  }
}
