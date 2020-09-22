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

public class TrapController: MonoBehaviour
{

  [SerializeField] public PlayerColor Color;
  
  private float _contdown;
  private float _colorIntensity;
  private bool _disappearAnimation;

  private Material _material;

  void Start()
  {
    SetColor(this.Color);
  }

  public void SetColor(PlayerColor color)
  {
    this.Color = color;

    _material = GetComponent<Renderer>().material;
    _material.SetColor("_Color", this.Color.ToRGB());

    ShowColor();
  }

  public void ShowColor()
  {
    _contdown = 3f;
    _disappearAnimation = false;
    _colorIntensity = 1f;
  }

  void Update()
  {
    _material.SetFloat("_ColorIntensity", _colorIntensity);
    if (_disappearAnimation)
    {
      if (_colorIntensity > 0)
      {
        _colorIntensity -= Time.deltaTime;
      }
      else
      {
        _colorIntensity = 0;
        _disappearAnimation = false;
      }
    }
    else if (_contdown > 0)
    {
      _contdown -= Time.deltaTime;
      if (_contdown <= 0)
      {
        _disappearAnimation = true;
      }
    }
  }


  void OnTriggerStay2D(Collider2D other) {
    if (other.gameObject.tag == "Player") 
    {
      PlayerController player = other.gameObject.GetComponent<PlayerController>();
      if (player.Color == this.Color){
        player.InitiateFall();
      }
    }
  }
}
