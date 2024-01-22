using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CustomAgent
{
    private NavMeshPath _path;
    private Vector3[] v3Cache = new Vector3[64];
    private Vector3 _forward;

    private List<Ground> _listGround;
    private Entity _ent;
    private bool _isStart;
    public bool IsStopped => !_isStart;

    public NavMeshPathStatus PathStatus
    {
        get
        {
            if (_path == null)
                return default;
            return _path.status;
        }
    }

    private Vector3 _velocityMod;

    private Vector3 _velocity;
    public Vector3 Velocity => _velocity;

    public float Speed { get; set; }

    public CustomAgent()
    {
        _listGround = new List<Ground>();
    }

    public CustomAgent(Entity entity, Vector3 endPos) : base()
    {
        Initialize(entity, endPos);
    }

    public void Update()
    {
        if (!_isStart)
            return;

        Vector3 dir = GetDirection();
        _velocity = (dir + _velocityMod) * Speed;

        RotateTo(dir);
        UpdateVelocityModifier();
        _ent.transform.position += _velocity * Time.deltaTime;
    }

    private void RotateTo(Vector3 dir, bool isInstant = false)
    {
        if (isInstant)
        {
            _ent.transform.forward = dir;
            return;
        }

        var angle = Vector3.Angle(_ent.transform.forward, dir);
        float modSpeed = angle > 90 ? 1.5f : 1;
        float rotateSpeed = (5 * modSpeed) * Time.deltaTime;

        _ent.transform.forward = Vector3.RotateTowards(_ent.transform.forward, dir, rotateSpeed, rotateSpeed);
    }

    private void UpdateVelocityModifier()
    {
        _velocityMod = Vector3.MoveTowards(_velocityMod, Vector3.zero, Time.deltaTime * 2);
    }

    private Vector3 GetDirection()
    {
        Ground nextGround = GetNextGround(_ent.Ground);
        if (nextGround == null)
            return default;

        var diff = nextGround.Top - _ent.transform.position;
        if (diff.magnitude <= 0.1f)
            return Vector3.zero;

        return diff.normalized;
    }

    private Ground GetNextGround(Ground ground)
    {
        for (int i = 0; i < _listGround.Count; i++)
        {
            if (_listGround[i] == ground)
            {
                if (i + 1 >= _listGround.Count)
                    return _listGround[i];
                return _listGround[i + 1];
            }
        }

        return null;
    }

    public void Initialize(Entity target, Vector3 end)
    {
        _ent = target;
        _isStart = true;
        CalculatePath(_ent.transform.position, end);
    }

    private void CalculatePath(Vector3 start, Vector3 end)
    {
        var path = new NavMeshPath();
        bool isValid = NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path);
        CalculatePath(path);

        var dir = GetDirection();
        RotateTo(dir, true);
    }

    private void CalculatePath(NavMeshPath path)
    {
        _listGround.Clear();
        int count = path.GetCornersNonAlloc(v3Cache);

        var hitResult = System.Buffers.ArrayPool<RaycastHit>.Shared.Rent(16);

        for (int i = 0; i < count; i++)
        {
            int hitCount = Physics.RaycastNonAlloc(v3Cache[i] + Vector3.up * 0.1f, Vector3.down, hitResult);

            for (int j = 0; j < hitCount; j++)
            {
                var ground = hitResult[j].collider.GetComponentInParent<Ground>();
                if (_listGround.Contains(ground))
                    continue;
                _listGround.Add(ground);
            }
        }

        System.Buffers.ArrayPool<RaycastHit>.Shared.Return(hitResult);
        for (int i = 0; i < _listGround.Count; i++)
        {
            if (i + 1 >= _listGround.Count)
                Debug.DrawLine(_listGround[i].Top, _listGround[i].Top, Color.magenta, 100f);
            else
                Debug.DrawLine(_listGround[i].Top, _listGround[i + 1].Top, Color.magenta, 100f);
        }
    }

    public void Start()
    {
        _isStart = true;
    }

    public void Stop()
    {
        _isStart = false;
    }

    public void Push(Vector3 force)
    {
        _velocityMod = force;
    }

    public void Push(Vector3 dir, float power)
    {
        Push(dir * power);
    }
}