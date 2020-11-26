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

using DamageSystem;
using UnityEngine;
using Random = UnityEngine.Random;
using Single = System.Single;

[RequireComponent(typeof(Health))]
public abstract class EnemyController : EntityController
{
    [SerializeField] protected float directionSpeed = 1;
    [SerializeField] protected float directionChangeTime = 1;
    [SerializeField] private ParticleSystem smokeEffect = default;
    
    /**
     * Instance of Absorbables ScriptableObject which holds all Absorbables
     * a monster could potentially drop when it dies.
     */
    [SerializeField] private Absorbables monsterDrops = default;

    protected float directionTimer;
    protected Vector2 direction;
    
    private RaycastHit2D[] _hit = new RaycastHit2D[3];
    private readonly int deadTrigger = Animator.StringToHash("Dead");
    private readonly System.Random rand = new System.Random();

    private Health _health;

    protected override void Awake()
    {
        base.Awake();
        _health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        _health.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        _health.OnDeath -= OnDeath;
    }

    protected override void Start()
    {
        base.Start();
        directionTimer = directionChangeTime;
        direction = Random.insideUnitCircle.normalized;
    }

    protected virtual void Update()
    {
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

        foreach (var player in FindObjectsOfType<PlayerController>())
        {
            Vector2 target = player.Center - Rigidbody2D.position;
            var distance = target.magnitude;

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

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("PlayerHit"))
            direction = -direction;
    }

    /*
     * Different enemy types only differ in their movements. New enemies should implement this method,
     * to define their behaviour.
     */
    protected abstract Vector2 ComputeOffset();
    
    void FixedUpdate()
    {
        if (!_health.IsDead)
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

    private void DropAbsorbables()
    {
        int amountCoins = Random.Range(0,5);
        for(int i = 0; i < amountCoins; i++) {
            Instantiate(monsterDrops[rand.Next(0, monsterDrops.Length)], transform.position, Quaternion.identity);    
        }
    }

    /**
     * <summary>
     * Invoked, if the <see cref="Health"/> component signals death (0 health points)
     * Will...
     * ...disable colliders
     * ...trigger death animation
     * ...drop items (absorbables)
     * ...destroy this object
     * </summary>
     */
    private void OnDeath()
    {
        this.GetComponent<Collider2D>().enabled = false;
        Animator.SetTrigger(deadTrigger);
        DropAbsorbables();
        // FIXME: Use event in death animation to trigger destroy instead of fixed timer.
        Destroy(gameObject, 1);
    }
}
