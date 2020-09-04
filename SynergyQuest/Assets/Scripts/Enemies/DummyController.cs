using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyController : EnemyController {
    protected override Vector2 ComputeOffset() {
        var offset = Time.deltaTime * directionSpeed * direction;
        return offset;
    }
}
