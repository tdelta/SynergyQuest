using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : EnemyController {

    protected override Vector2 computeForce() {
        var offset = directionSpeed * direction;
        return offset;
    }

}
