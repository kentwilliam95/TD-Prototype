using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static Vector3Int[] NavmeshLinkDirections = new Vector3Int[]
        { Vector3Int.right, Vector3Int.forward, Vector3Int.left, Vector3Int.back };

    private static GameController _instance;
    public static GameController Instance => _instance;

    public enum State
    {
        Starting,
        Ready,
        End,
    }

    private Ground _spawnedObjective;

    private int _playerUnitCurrency = 10;
    private float _unitCurrencyCounter;
    private int _playerLifes;

    private int _totalKill;
    private int _maxKill;

    private List<Ground> _spawnedGround;
    private List<Entity> _spawnedEntity;

    private State _gameState;
    public State GameState => _gameState;

    [field: SerializeField] public Transform TrGroundContainer { get; private set; }
    [field: SerializeField] public Transform TrEditorContainer { get; private set; }

    public MapDataSO _mapDataSo;
    public Ground GroundObjective => _spawnedObjective;
    public UIGameController _ui;

    [HideInInspector] public Vector2Int _size;

    [TextArea(5, 10)] [SerializeField] private string _debugText;

    private void Awake()
    {
        _instance = this;
        _spawnedEntity = new List<Entity>();
        _spawnedGround = new List<Ground>();
        _playerLifes = Global.TOTALLIVES;
        Global.Init();
        System.Buffers.ArrayPool<RaycastHit>.Create(100, 10);
    }

    private void OnDestroy()
    {
        
    }

    private void Start()
    {
        TrEditorContainer.gameObject.SetActive(false);
        _gameState = State.Starting;
        
        LoadFromMapDataSO(_mapDataSo);
    }

    private void Update()
    {
#if UNITY_EDITOR
        _debugText = String.Empty;

        _debugText += $"\nState : {_gameState}";
        _debugText += $"\nUnitCurrency: {_playerUnitCurrency}";
        _debugText += $"\nUnitCurrencyCounter: {_playerUnitCurrency}";
        _debugText += $"\nMax Kill: {_maxKill}";
        _debugText += $"\ntotal Kill: {_totalKill}";
#endif
        if (GameState == State.End || _gameState == State.Starting)
            return;

        UpdateWinCondition();

        _unitCurrencyCounter += Time.deltaTime * 5;
        if (_unitCurrencyCounter >= 1)
        {
            _playerUnitCurrency = Mathf.Min(_playerUnitCurrency + 1, 100);
            _unitCurrencyCounter = 0;
        }

        _ui.UpdateButtonVisibility(_playerUnitCurrency);
        _ui.UpdateUnitCurrency(_unitCurrencyCounter, _playerUnitCurrency);
    }

    private void UpdateWinCondition()
    {
        if (_totalKill < _maxKill)
            return;

        _gameState = State.End;
        _ui.ShowWin();
    }

    private void OnUnitPlacedOnGroundAndFinishSetup(UIGameController.PlacedUnit placedUnit)
    {
        var currency = _playerUnitCurrency - placedUnit._unitSO._cost;
        if (currency < 0)
            return;

        _playerUnitCurrency = currency;
        Entity unit = SpawnUnit(placedUnit._unitSO, placedUnit._ground.Top);
        unit.SetLookDirection(placedUnit._unitDirection);
    }

    public void RegisterEntity(Entity ent)
    {
        if (_spawnedEntity.Contains(ent))
            return;

        ent.Initialize(this);
        _spawnedEntity.Add(ent);
    }

    public void UnRegisterEntity(Entity ent)
    {
        if (!_spawnedEntity.Contains(ent))
        {
            Debug.Log($"[GameController] Entity {ent} already removed", ent);
            return;
        }

        _spawnedEntity.Remove(ent);
        Destroy(ent.gameObject);
    }

    private void LoadFromMapDataSO(MapDataSO so)
    {
        StartCoroutine(LoadMapProgress(so, () =>
        {
            //temporary
            _ui.Initialize(this, OnUnitPlacedOnGroundAndFinishSetup);
            _ui.UpdateLifeTexts(_playerLifes, _playerLifes);
            _gameState = State.Ready;
        }));
    }

    private IEnumerator LoadMapProgress(MapDataSO so, Action onComplete)
    {
        var save = JsonUtility.FromJson<GroundSaveData>(so.mapData);
        for (int i = 0; i < save.saveData.Count; i++)
        {
            var detail = save.saveData[i];
            Ground ground = Resources.Load<Ground>($"Ground/{detail.prefabName}");
            Ground instance = Instantiate(ground, TrGroundContainer);
            instance.LoadData(detail.saveData);

            //Temporary fix indexing for ground
            int x = i % so._sizeX;
            int y = i / so._sizeX;
            instance.RepairIndex(i, new Vector2Int(x, y));

            _spawnedGround.Add(instance);
            var objective = instance.GetComponent<GroundObjective>();
            if (objective)
                _spawnedObjective = objective;

            var grEnemy = instance.GetComponent<GroundSpawnedEnemy>();
            if (grEnemy)
                _maxKill += grEnemy._totalEnemy;
        }

        yield return null;
        yield return null;
        yield return null;

        for (int i = 0; i < _spawnedGround.Count; i++)
            _spawnedGround[i].SetupNavmeshLink();

        onComplete?.Invoke();
    }

    public void EditorLoadFromMapDataSO(MapDataSO so)
    {
        var save = JsonUtility.FromJson<GroundSaveData>(so.mapData);
        for (int i = 0; i < save.saveData.Count; i++)
        {
            var detail = save.saveData[i];
            Ground ground = Resources.Load<Ground>($"Ground/{detail.prefabName}");
            Ground instance = Instantiate(ground, TrEditorContainer);
            instance.name = ground.name;
            instance.LoadData(detail.saveData);
        }
    }

    public Ground GetGround(Vector2Int index2D)
    {
        if (index2D.x < 0 || index2D.y < 0)
            return null;

        var index = index2D.y * _mapDataSo._sizeX + index2D.x;
        if (index >= _spawnedGround.Count || index < 0)
            return null;

        return _spawnedGround[index];
    }

    public List<Entity> GetEntitiesOnGround(Ground ground, int team)
    {
        List<Entity> res = new List<Entity>();
        for (int i = 0; i < _spawnedEntity.Count; i++)
        {
            Entity entity = _spawnedEntity[i];
            if (!entity.IsStandingOnTheSameGround(ground) || entity.Team != team)
                continue;
            res.Add(entity);
        }

        return res;
    }

    public bool CheckIfThereIsAnEntityOnGround(Ground ground, int team)
    {
        for (int i = 0; i < _spawnedEntity.Count; i++)
        {
            Entity entity = _spawnedEntity[i];
            if (entity.IsStandingOnTheSameGround(ground) && entity.Team == team)
                return true;
        }

        return false;
    }

    //TODO: use object pooling rather that instantiate
    public Enemy SpawnEnemy(Enemy prefab, Vector3 pos, Action onComplete)
    {
        Enemy enemy = Instantiate(prefab);
        enemy.transform.position = pos;
        enemy.OnArrivedAtDestination = OnEnemyArrivedAtDestination;
        enemy.onDead = EnemyOnKilled;
        RegisterEntity(enemy);

        return enemy;
    }

    private void EnemyOnKilled(Entity ent)
    {
        ent.onDead = null;
        _totalKill += 1;
    }

    private void OnEnemyArrivedAtDestination(int dmg)
    {
        _totalKill += 1;
        _playerLifes -= 1;
        _ui.UpdateLifeLeft(_playerLifes, true);
        if (_playerLifes <= 0)
        {
            _gameState = State.End;
            _ui.ShowGameOver();
            
            for (int i = _spawnedEntity.Count - 1; i >= 0; i--)
                _spawnedEntity[i].GameOver();

            for (int i = _spawnedGround.Count - 1; i >= 0; i--)
                _spawnedGround[i].GameOver();
        }
    }

    //TODO: use object pooling rather that instantiate
    public Entity SpawnUnit(UnitDataSO so, Vector3 position)
    {
        var entity = Instantiate(so.prefab);
        entity.transform.position = position;
        
        RegisterEntity(entity);
        return entity;
    }
}