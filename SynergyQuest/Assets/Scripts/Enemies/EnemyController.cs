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
using Single = System.Single;

abstract public class EnemyController : EntityController
{
    [SerializeField] protected int   healthPoints = 1;
    [SerializeField] protected float directionSpeed = 1;
    [SerializeField] protected float directionChangeTime = 1;
    [SerializeField] protected int damageFactor = 1;
    [SerializeField] ParticleSystem smokeEffect = default;
    [SerializeField] private MultiSound hitSounds = default;
    [SerializeField] Absorbables monsterDrops;

    protected float directionTimer;
    protected Vector2 direction;
    
    bool isDead;
    RaycastHit2D[] _hit = new RaycastHit2D[3];
    readonly int deadTrigger = Animator.StringToHash("Dead");
    readonly System.Random rand = new System.Random();

    /**
     * Used to briefly flash an enemy in a certain color. For example red when it is hit.
     */
    private TintFlashController _tintFlashController;

    /**
     * used by BossSpawn and EnemySpawn to find out when all enemies are dead to trigger switch
     */
    public delegate void EnemyDied();
    public event EnemyDied OnDeath;

    protected override void Start()
    {
        base.Start();
        directionTimer = directionChangeTime;
        direction = Random.insideUnitCircle.normalized;
        
        _tintFlashController = GetComponent<TintFlashController>();
    }

    protected override void Update()
    {
        base.Update();
        directionTimer -= Time.deltaTime;

        if (directionTimer < 0)
        {
            direction = Random.insideUnitCircle.normalized;
            directionTimer = directionChangeTime;
        }
    }

    protected (bool, Vector2) FindNearestPlayer(Vector2 direction, float viewCone)
    {
        Vector2 playerVector = new Vector2(0, 0);
        float minPlayerDistance = Single.PositiveInfinity;

        foreach (var player in GameObject.FindObjectsOfType(typeof(PlayerController)) as PlayerController[])
        {
            Vector2 target = player.Center - Rigidbody2D.position;
            float distance = target.magnitude;

            if (Vector2.Angle(target, direction) <= viewCone / 2 && distance < minPlayerDistance &&
                // if no gameObject blocks line of sight to player
                Physics2D.LinecastNonAlloc(Rigidbody2D.position, player.Center, _hit) == 2)
            {
                minPlayerDistance = distance;
                playerVector = target;
            }
        }

        return (!Single.IsInfinity(minPlayerDistance), playerVector.normalized);
    }


    void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var player = other.gameObject.GetComponent<EntityController>();
            player.PutDamage(damageFactor, (other.transform.position - transform.position).normalized); 
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("PlayerHit"))
            direction = -direction;
    }

    public override bool ChangeHealth(int amount, bool playSounds = true)
    {
        healthPoints += amount;

        if (amount <= 0)
        {
            PlayDamageEffects();
        }

        if (healthPoints <= 0)
        {
            isDead = true;
            this.GetComponent<Collider2D>().enabled = false;
            Animator.SetTrigger(deadTrigger);
            dropCoins();
            Destroy(gameObject, 1);
            OnDeath?.Invoke();
        }
        return true;
    }

    /*
     * Different enemy types only differ in their movements. New enemies should implement this method,
     * to define their behaviour.
     */
    protected abstract Vector2 ComputeOffset();
    
    void FixedUpdate()
    {
        if (!isDead)
        {
            Vector2 position = Rigidbody2D.position;
            position += ComputeOffset();
            
            PhysicsEffects.MoveBody(position);
        }
    }

    public void ShowParticles()
    {
        smokeEffect.Play();
    }

    /**
     * Plays some effects to give feedback that the enemy has taken damage.
     *
     * Currently, it flashes the enemy red for the duration of its invincibility after being hit.
     * It also plays a hit sound, when present
     */
    private void PlayDamageEffects()
    {
        // Flash enemy red for the duration of its temporary invincibility
        _tintFlashController.FlashTint(
            Color.red, TimeInvincible
        );
        
        // Play sound
        if (!ReferenceEquals(hitSounds, null))
        {
            hitSounds.PlayOneShot();
        }
    }

    private void dropCoins()
    {
        int amountCoins = Random.Range(0,5);
        for(int i = 0; i < amountCoins; i++) {
            Instantiate(monsterDrops[rand.Next(0, monsterDrops.Length)], transform.position, Quaternion.identity);    
        }
    }

}
