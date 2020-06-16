using UnityEngine;
using System;
using System.Collections;

public class MonsterRoomDoorController: DoorController
{

    [SerializeField] protected int MonstersToKill = 1;

    private void OnEnable()
    {
        EnemyController.OnDeath += OnMonsterDied;
    }

    private void OnDisable()
    {
        EnemyController.OnDeath -= OnMonsterDied;
    }

    private void OnMonsterDied()
    {
        MonstersToKill -= 1;
        if (MonstersToKill <= 0){
            OpenDoor();
        }
    }
}
