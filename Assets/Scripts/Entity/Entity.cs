using System.Collections.Generic;
using Core;
using UnityEngine;
using UnityEngine.AI;

public class Entity : MonoBehaviour
{
    //Todo: reduce the usage of new state 
    public struct Data
    {
        public float health;
        public float _speed;
        public float _damage;
        public float _delay;
        public float _duration;
        public int team;
        public int teamTarget;

        public Projectile _projectile;
        public float _projectileSpeed;

        public Vector3 _endLocation;
        public Entity _target;
    }
    
    #region States

    public enum State
    {
        AttackAndMove,
        Attack,
        Idle,
        Move,
        Dead,
        DeadAndDamage
    }

    private StateAttack _stateAttack = new StateAttack();
    private StateIdle _stateIdle = new StateIdle();
    private StateMove _stateMove = new StateMove();
    private StateAttackandMove _stateAttackAndMove = new StateAttackandMove();
    private StateDeadDamage _stateDeadDamage = new StateDeadDamage();

    #endregion

    protected StateMachine<Entity> _stateMachine;
    protected bool isGameFinish;
    protected RaycastHit[] _hitResult;

    protected int _groundIndex1D;
    public int Index1D => _groundIndex1D;

    protected Vector2Int _groundIndex2D;
    public Vector2Int Index2D => _groundIndex2D;

    protected bool isInitialize;
    protected Ground _ground;
    [HideInInspector] public Data entityData;
    public bool IsDead => entityData.health <= 0;

    protected int _team;
    public int Team => _team;

    protected GameController _controller;
    public GameController GameController => _controller;

    [SerializeField] protected NavMeshAgent _agent;
    public NavMeshAgent Agent => _agent;

    [SerializeField] protected UnitDataSO _unitSO;
    public UnitDataSO UnitSO => _unitSO;

    [SerializeField] private AnimationController _animController;

    public event System.Action OnGroundChanged;
    public System.Action<Entity> onDead;
    public event System.Action OnDamaged;

    public System.Action<int> OnArrivedAtDestination;

    [TextArea(5, 10)] [SerializeField] private string _debugText;

    private void OnDestroy()
    {
        isInitialize = false;
        GameController.OnStateChanged -= OnStateChanged;
    }

    protected virtual void Awake()
    {
        SetupInternalData();
        _stateMachine = new StateMachine<Entity>(this);
        _hitResult = new RaycastHit[8];
        _animController.Play(AnimationController.AnimationType.Idle);
    }

    public virtual void Initialize(GameController controller)
    {
        _controller = controller;
        isInitialize = true;
        _animController.Initialize();
        _stateMachine.ChangeState(new StateIdle());
        GameController.OnStateChanged += OnStateChanged;
    }

    private void OnStateChanged()
    {
        isGameFinish = GameController.Instance.GameState == GameController.State.End;
        if (_agent != null)
        {
            _agent.isStopped = true;
            PlayAnimation(AnimationController.AnimationType.Idle);
        }
    }

    private void SetupInternalData()
    {
        if (!_unitSO)
            return;

        entityData = new Data()
        {
            _speed = _unitSO._speed,
            health = _unitSO._health,
            _damage = _unitSO._damage,
            _delay = _unitSO._delay,
            _duration = _unitSO._duration,
            _projectile = _unitSO._projectile,
            _projectileSpeed = _unitSO._projectileSpeed
        };

        if (_agent)
            _agent.speed = _unitSO._speed;
    }

    protected virtual void Update()
    {
        if (!isInitialize || isGameFinish)
            return;

        UpdateGroundIndex();
        _stateMachine.OnUpdate();

#if UNITY_EDITOR
        _debugText = $"Ground Index: {_groundIndex1D}, {_groundIndex2D}";
        if (_agent && _agent.isOnNavMesh)
        {
            _debugText += $"\n isStop: {_agent.isStopped}\n path Stale: {_agent.isPathStale}";
            _debugText += $"\n{_agent.pathStatus}";
        }

        _debugText += $"\nState: {_stateMachine.CurrentState.ToString()}";
#endif
    }

    public void MoveTo(Vector3 start, Vector3 end)
    {
        entityData._endLocation = end;
        var path = new NavMeshPath();
        bool res = Agent.CalculatePath(entityData._endLocation, path);
        Agent.SetPath(path);

        // ChangeState(new StateMove());
        ChangeState(State.Move);
    }

    public void ChangeState(State state)
    {
        switch (state)
        {
            case State.AttackAndMove:
                _stateMachine.ChangeState(_stateAttackAndMove);
                break;

            case State.Attack:
                _stateMachine.ChangeState(_stateAttack);
                break;

            case State.Idle:
                _stateMachine.ChangeState(_stateIdle);
                break;

            case State.Move:
                _stateMachine.ChangeState(_stateMove);
                break;

            case State.Dead:
                break;

            case State.DeadAndDamage:
                _stateMachine.ChangeState(_stateDeadDamage);
                break;
        }
    }

    public void StartAgent()
    {
        if (!_agent)
            return;

        _agent.speed = _unitSO._speed;
    }

    public void StopAgent()
    {
        if (!_agent)
            return;

        _agent.speed = 0;
    }

    private void UpdateGroundIndex()
    {
        Ground newGround = CheckGround();
        if (!newGround)
            return;

        _ground = newGround;
        _groundIndex1D = _ground.Index1D;
        _groundIndex2D = _ground.Index2D;

        OnGroundChanged?.Invoke();
    }

    private Ground CheckGround()
    {
        Ground res = null;
        var arr = System.Buffers.ArrayPool<RaycastHit>.Shared.Rent(4);
        int hitCount =
            Physics.RaycastNonAlloc(transform.position + Vector3.up * 0.25f, Vector3.down, arr, 1f,
                GameController.LayerMaskGround);

        for (int i = 0; i < hitCount; i++)
        {
            Ground ground = arr[i].collider.GetComponentInParent<Ground>();
            if (ground != _ground)
            {
                res = ground;
                break;
            }
        }

        System.Buffers.ArrayPool<RaycastHit>.Shared.Return(arr);
        return res;
    }

    public void Damage(float value)
    {
        entityData.health = Mathf.Max(entityData.health - value, 0);
        if (entityData.health <= 0)
        {
            onDead?.Invoke(this);
            GameController.Instance.UnRegisterEntity(this);
            return;
        }

        OnDamaged?.Invoke();
    }

    public Transform GetBone(HumanBodyBones bone)
    {
        return _animController.Animator.GetBoneTransform(bone);
    }

    public void PlayAnimation(AnimationController.AnimationType animationType, bool useBlend = true, float time = 0.15f)
    {
        if (useBlend)
            _animController.Play(animationType, time);
        else
            _animController.Play(animationType);
    }

    public void RegisterAnActionOnAnimation(AnimationController.AnimationType animationType, System.Action onTrigger)
    {
        _animController.RegisterAnAction(animationType, onTrigger);
    }

    public bool IsStandingOnTheSameGround(Ground ground)
    {
        return _ground == ground;
    }

    public Entity CheckIsEnemyOnTheSameGround(Entity t, int team)
    {
        Entity res = null;

        Ground ground = t.GameController.GetGround(t.Index2D);
        var list = t.GameController.GetEntitiesOnGround(ground, team);
        if (list.Count > 0)
            res = list[0];

        return res;
    }

    public Entity CheckEnemyInRange(Entity t, int team)
    {
        Entity res = null;
        foreach (var dir in t.UnitSO.attackDirection)
        {
            Vector2Int index2D = t.UnitSO.CalculateGroundIndex(dir, t.Index2D);
            Ground ground = t.GameController.GetGround(index2D);

            if (ground == null)
                continue;

            List<Entity> list = t.GameController.GetEntitiesOnGround(ground, team);

            if (list.Count > 0)
            {
                res = list[0];
                break;
            }
        }

        return res;
    }

    public bool CheckNearObjective()
    {
        if (Agent.remainingDistance <= 0.2f)
        {
            Ground ground = GameController.GetGround(Index2D);
            if (ground is GroundObjective)
            {
                return true;
            }
        }

        return false;
    }
}