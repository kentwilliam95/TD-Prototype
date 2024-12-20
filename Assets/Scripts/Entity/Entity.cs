using System.Collections.Generic;
using Core;
using DG.Tweening;
using States;
using UnityEngine;
using UnityEngine.AI;

public class Entity : MonoBehaviour
{
    public struct Data
    {
        public float health;
        public float _duration;

        public UnitDataSO.AttackDirection[] _AttackDirections;
        public Projectile _projectile;
        public float _projectileSpeed;

        public List<Ground> listMovement;
        public Vector3 _endLocation;
        public Entity _target;
    }

    #region States

    public enum State
    {
        EnemyRangeAttack,
        EnemyAttack,
        AllyAttack,
        AllyRangeAttack,
        Idle,
        Move,
        Dead,
        DeadAndDamage
    }

    private StateAllyRangeAttack _stateAllyRangeAttack = new StateAllyRangeAttack();
    private StateAllyAttack _stateAllyAttack = new StateAllyAttack();
    private StateEnemyAttack _stateEnemyAttack = new StateEnemyAttack();
    private StateEnemyRangeAttack _stateEnemyRangeAttack = new StateEnemyRangeAttack();

    private StateIdle _stateIdle = new StateIdle();
    private StateMove _stateMove = new StateMove();
    private StateDeadDamage _stateDeadDamage = new StateDeadDamage();

    #endregion

    protected CustomAgent _customAgent;
    public CustomAgent CAgent => _customAgent;

    protected StateMachine<Entity> _stateMachine;
    protected bool isGameFinish;
    protected RaycastHit[] _hitResult;

    protected int _groundIndex1D;
    public int Index1D => _groundIndex1D;

    protected Vector2Int _groundIndex2D;
    public Vector2Int Index2D => _groundIndex2D;

    [field: SerializeField] public WeaponHandler WeaponHandler { get; private set; }
    [field: SerializeField] public Renderer Renderer { get; private set; }

    protected bool isInitialize;
    protected Ground _ground;

    public Ground Ground
    {
        get
        {
            if (_ground == null)
                UpdateGroundIndex();

            return _ground;
        }
    }

    [HideInInspector] public Data entityData;
    public bool IsDead => entityData.health <= 0;

    [SerializeField] private Transform _camPosition;
    public Transform CamPosition => _camPosition;

    protected GameController _controller;
    public GameController GameController => _controller;

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
        seq?.Kill();
        isInitialize = false;
    }

    protected virtual void Awake()
    {
        _stateMachine = new StateMachine<Entity>(this);
        _hitResult = new RaycastHit[8];
        _animController.Play(AnimationController.AnimationType.Idle);
        _customAgent = new CustomAgent();

        SetupInternalData();
    }

    public virtual void Initialize(GameController controller)
    {
        _controller = controller;
        isInitialize = true;
        _animController.Initialize();
        _stateMachine.ChangeState(new StateIdle());
    }

    public void SetLookDirection(Vector3 lookDirection)
    {
        transform.forward = lookDirection;
    }

    public void GameOver()
    {
        isGameFinish = GameController.Instance.GameState == GameController.State.End;
        _customAgent.Stop();
        PlayAnimation(AnimationController.AnimationType.Idle);
    }

    private void SetupInternalData()
    {
        if (!_unitSO)
            return;

        entityData = new Data()
        {
            health = _unitSO._health,
            _duration = _unitSO._duration,
            _projectile = _unitSO._projectile,
            _projectileSpeed = _unitSO._projectileSpeed,
            listMovement = new List<Ground>(),
            _AttackDirections = _unitSO.attackDirection
        };

        _customAgent.Speed = _unitSO._speed;
    }

    protected virtual void Update()
    {
        if (!isInitialize || isGameFinish)
            return;

        UpdateGroundIndex();
        _customAgent?.Update();
        _stateMachine.OnUpdate();

#if UNITY_EDITOR
        _debugText = $"Ground Index: {_groundIndex1D}, {_groundIndex2D}";
        if (_customAgent != null)
        {
            _debugText += $"\n isStop: {_customAgent.IsStopped}";
            _debugText += $"\n{_customAgent.PathStatus}";
        }

        _debugText += $"\nVelocity:{_customAgent.Velocity}";
        _debugText += $"\nforward:{transform.forward}";
        _debugText += $"\nState: {_stateMachine.CurrentState.ToString()}";
        _debugText += $"\nLookDirection : {ConvertLookDirection(transform.forward)}";
#endif
    }

    public void MoveTo(Vector3 start, Vector3 end)
    {
        _customAgent.Initialize(this, end);
        ChangeState(State.Move);
    }

    public void ChangeState(State state)
    {
        switch (state)
        {
            case State.EnemyRangeAttack:
                _stateMachine.ChangeState(_stateEnemyRangeAttack);
                break;

            case State.EnemyAttack:
                _stateMachine.ChangeState(_stateEnemyAttack);
                break;

            case State.AllyAttack:
                _stateMachine.ChangeState(_stateAllyAttack);
                break;

            case State.AllyRangeAttack:
                _stateMachine.ChangeState(_stateAllyRangeAttack);
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
        _customAgent.Start();
    }

    public void StopAgent()
    {
        _customAgent.Stop();
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
                Global.LayerMaskGround);

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

        DamageAnimation();
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

    public bool IsStandingOnTheSameGround(Ground ground)
    {
        return _ground == ground;
    }

    public Entity CheckIsEnemyOnTheSameGround()
    {
        Entity res = null;

        Ground ground = GameController.GetGround(Index2D);
        var list = GameController.GetEntitiesOnGround(ground, _unitSO._targetTeam);
        if (list.Count > 0)
            res = list[0];

        return res;
    }

    public Entity CheckEnemyInRange()
    {
        Entity res = null;
        foreach (var dir in UnitSO.attackDirection)
        {
            Vector2Int index2D = UnitSO.CalculateGroundIndex(dir, Index2D, ConvertLookDirection(transform.forward.normalized));
            Ground ground = GameController.GetGround(index2D);

            if (ground == null)
                continue;

            List<Entity> list = GameController.GetEntitiesOnGround(ground, _unitSO._targetTeam);

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
        if (_ground is GroundObjective)
        {
            var diff = _ground.Top - transform.position;
            var dist = diff.magnitude;
            if (dist <= 0.2f)
                return true;
        }

        return false;
    }

    private Sequence seq;
    private MaterialPropertyBlock mbp;

    public void DamageAnimation()
    {
        if (mbp == null)
            mbp = new MaterialPropertyBlock();

        Color colorOG = Renderer.sharedMaterial.color;
        mbp.SetColor("_Color", colorOG);

        if (seq == null)
        {
            seq = DOTween.Sequence();
            seq.SetAutoKill(false);
            for (int i = 0; i < 1; i++)
            {
                seq.Insert(i * 0.125f, DOVirtual.Color(colorOG, Color.red, 0.125f, (color) =>
                {
                    mbp.SetColor("_Color", color);
                    Renderer.SetPropertyBlock(mbp, 0);
                }).SetEase(Ease.OutBack, 2f));

                seq.Insert((i + 1) * 0.125f, DOVirtual.Color(Color.red, colorOG, 0.125f, (color) =>
                {
                    mbp.SetColor("_Color", color);
                    Renderer.SetPropertyBlock(mbp, 0);
                }));
            }
        }
        else
        {
            seq.Complete();
            seq.Restart();
        }
    }

    private Vector3 ConvertLookDirection(Vector3 forward)
    {
        float x = Mathf.Abs(forward.x);
        float z = Mathf.Abs(forward.z);

        if (x >= z)
            return new Vector3(1 * Mathf.Sign(forward.x), 0, 0);
        else
            return new Vector3(0, 1 * Mathf.Sign(forward.z), 0);
    }
}