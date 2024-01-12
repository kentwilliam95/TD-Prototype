using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundEmpty : Ground
{
    protected override void OnValidate()
    {
        base.OnValidate();
        _boxCollider.enabled = false;
        _renderer.enabled = false;
    }

    public override void SetupNavmeshLink()
    {
        
    }
}
