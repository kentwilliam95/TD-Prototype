using UnityEngine;

public class Projectile : MonoBehaviour
{
    private bool _isInitialize;
    public System.Action onHitTarget;
    protected Transform _tStart, _tEnd;
    protected float _speed;
    
    public virtual void Initialize(Transform tStart, Transform tEnd, float speed)
    {
        _tStart = tStart;
        _tEnd = tEnd;
        _isInitialize = true;
        _speed = speed;
    }

    private void Update()
    {
        if(!_isInitialize)
            return;
        
        UpdatePosition();
    }

    public virtual void UpdatePosition()
    {
        
    }

    protected void DestroyProjectile()
    {
        _isInitialize = false;
        _tStart = _tEnd = null;
        onHitTarget = null;
        
        Destroy(gameObject);
    }
}
