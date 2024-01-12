using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBall : Projectile
{
    private float _progress;
    private Vector3 _initPos;

    public override void Initialize(Transform tStart, Transform tEnd, float speed)
    {
        base.Initialize(tStart, tEnd, speed);
        _initPos = tStart.position;
        _progress = 0;
    }

    public override void UpdatePosition()
    {
        if (!_tStart || !_tEnd)
        {
            DestroyProjectile();
            return;
        }

        if(_progress >= 1)
            return;
        
        _progress += Time.deltaTime * _speed;
        transform.position = Vector3.Lerp(_initPos, _tEnd.position, _progress);

        if (_progress >= 1)
        {
            onHitTarget?.Invoke();
            DestroyProjectile();
        }
    }
}
