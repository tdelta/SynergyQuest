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

using UnityEngine;

public class ButtonController: MonoBehaviour
{

  [SerializeField] private Sprite UnpressedButton = default;
  [SerializeField] private Sprite PressedButton = default;
  [SerializeField] private Pressable Effect = default;

  private bool _pressAnimation;
  private SpriteRenderer _renderer;
  private float _animationTime;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.sprite = UnpressedButton;
    }

    void Update()
    {
      if (_pressAnimation)
      {
        _animationTime -= Time.deltaTime;
        if (_animationTime <= 0)
        {
          _renderer.sprite = UnpressedButton;
          _pressAnimation = false;
          _animationTime = 1f;
        }
      }
    }

    public void OnPress()
    {
        _renderer.sprite = PressedButton;
        _pressAnimation = true;
        Effect.OnButtonPressed();
    }
}
