using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : EnemyController {

    protected override Vector2 computeNewOffset() {
        var offset = Time.deltaTime * directionSpeed * direction;
        return offset;
    }

}
