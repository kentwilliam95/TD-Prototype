using System;
using Unity.VisualScripting;
using UnityEngine;

[SelectionBase]
public class GroundSpawnedEnemy : Ground
{
    [System.Serializable]
    public struct Data
    {
        public int _index1D;
        public Vector2Int _index2D;
        public int totalEnemy;
        public float duration;
        public BaseSaveData baseSaveData;
    }

    private float _counter;
    private int _totalSpawned;
    
    public float spawnDuration;
    public int _totalEnemy;
    public Enemy enemyPrefab;
    
    public void Update()
    {
        if (!isInitialized || !isNavmeshSetup || isGameFinish)
            return;
        
        if(_totalSpawned >= _totalEnemy)
            return;
        
        _counter -= Time.deltaTime;
        if (_counter <= 0)
        {
            var enemy = GameController.Instance.SpawnEnemy(enemyPrefab, Top + Vector3.up,null);
            enemy.MoveTo(Top, GameController.Instance.GroundObjective.Top);
            _counter = spawnDuration;
            _totalSpawned += 1;
        }
    }

    public override string GetSaveData()
    {
        Data data = new Data();
        data.duration = spawnDuration;
        data.totalEnemy = _totalEnemy;
        data.baseSaveData = new BaseSaveData()
        {
            position = transform.position,
            rotation = transform.rotation.eulerAngles,
            scale = transform.localScale
        };
        data._index1D = Index1D;
        data._index2D = Index2D;
        
        return JsonUtility.ToJson(data);
    }

    public override void LoadData(string data)
    {
        Data parsed = JsonUtility.FromJson<Data>(data);
        base.Initialize(parsed.baseSaveData, parsed._index1D, parsed._index2D);
        spawnDuration = parsed.duration;
        _totalEnemy = parsed.totalEnemy;
        
        Debug.Log($"total enemy {_totalEnemy}");
    }
}