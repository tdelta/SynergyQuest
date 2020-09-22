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

﻿using UnityEngine;
using Random = UnityEngine.Random;

public class CoinController : MonoBehaviour
{
    [SerializeField] private float thrust = default;
    [SerializeField] private int deactivationTime = default;

    public void Init(int deactivationTimeOverride)
    {
        deactivationTime = deactivationTimeOverride;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Rigidbody2D>().AddForce(getRandomDirection() * thrust);
        
        if (deactivationTime > 0)
        {
            StartCoroutine(
                CoroutineUtils.Wait(
                    deactivationTime,
                    () => Destroy(this.gameObject)
                )
            );
        }
    }

    private Vector3 getRandomDirection() 
    {
        var xDirection = Random.Range(-1.0f, 1.0f);
        var yDirection = Random.Range(-1.0f, 1.0f);
        
        return new Vector3(xDirection, yDirection);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var other = collision.collider;
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().IncreaseGoldCounter();
            
            this.gameObject.PlaySoundAndDestroy();
        } 
    }
}
