using System.Buffers;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

[System.Serializable]
public class GroundSaveData
{
    public List<WrapperSaveData> saveData;
}

[System.Serializable]
public class WrapperSaveData
{
    public string prefabName;
    public string saveData;
}

[System.Serializable]
public class BaseSaveData
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
}

[SelectionBase]
public class Ground : MonoBehaviour
{
    public const int SIZE = 2;

    [System.Serializable]
    public struct Data
    {
        public int _index1D;
        public Vector2Int _index2D;
        [HideInInspector] public BaseSaveData _baseSaveData;
    }

    [SerializeField] protected BoxCollider _boxCollider;
    [SerializeField] protected Renderer _renderer;

    protected bool isInitialized;
    protected bool isNavmeshSetup;
    protected bool isGameFinish;

    [ReadOnly] 
    [SerializeField] private Vector3 _center;
    public Vector3 Center => _center;

    [ReadOnly] 
    [SerializeField] private Vector3 _top;
    public Vector3 Top => _top;

    private MaterialPropertyBlock _mbp;

    private int _index1D;
    public int Index1D => _index1D;
    private Vector2Int _index2D;
    public Vector2Int Index2D => _index2D;

    [TextArea(7, 10)] [SerializeField] protected string _debugText;

    protected virtual void OnValidate()
    {
        _boxCollider = GetComponentInChildren<BoxCollider>();
        _renderer = GetComponentInChildren<Renderer>();
    }

    private void Awake()
    {
        _mbp = new MaterialPropertyBlock();
    }

    protected virtual void Initialize(BaseSaveData baseSaveData, int index1D, Vector2Int index2D)
    {
        transform.position = baseSaveData.position;
        transform.localScale = baseSaveData.scale;
        transform.rotation = Quaternion.Euler(baseSaveData.rotation);

        Initialize(index1D, index2D);
    }

    public void Initialize(int index1D, Vector2Int index2D)
    {
        _center = transform.position + Vector3.one * SIZE * 0.5f;
        _top = _center + Vector3.up * SIZE * 0.5f;

        _index1D = index1D;
        _index2D = index2D;

        gameObject.SetActive(true);
        isInitialized = true;
    }

    public void GameOver()
    {
        isGameFinish = GameController.Instance.GameState == GameController.State.End;
    }

    public void RepairIndex(int index1D, Vector2Int index2D)
    {
        _index1D = index1D;
        _index2D = index2D;
    }

    protected virtual void Update()
    {
        if(isGameFinish)
            return;
        
#if UNITY_EDITOR
        _debugText = $"Center : {_center} \n Top: {_top}\nIndex1D: {_index1D} \nIndex2D: {_index2D}";
#endif
    }

    public virtual void SetupNavmeshLink()
    {
        var hitResult = ArrayPool<RaycastHit>.Shared.Rent(8);
        for (int i = 0; i < GameController.NavmeshLinkDirections.Length; i++)
        {
            var rayDirection = GameController.NavmeshLinkDirections[i];
            int hitCount =
                Physics.RaycastNonAlloc(_center, rayDirection, hitResult, Global.RAYCASTSETUPNAVMESH, Global.LayerMaskGround);

            if (hitCount <= 0)
                continue;

            GameObject goLink = new GameObject();
            goLink.name = $"Link {rayDirection}";
            goLink.transform.SetParent(transform);

            var _link = goLink.AddComponent<NavMeshLink>();
            _link.transform.position = _top;
            _link.startPoint = Vector3.zero;
            _link.width = 1f;
            _link.bidirectional = false;
            for (int j = 0; j < hitCount; j++)
            {
                var ground = hitResult[j].collider.GetComponentInParent<Ground>();
                var diff = ground.Top - _top;
                _link.endPoint = diff;
            }
        }

        isNavmeshSetup = true;
        ArrayPool<RaycastHit>.Shared.Return(hitResult);
    }

    public void SetColor(Color color)
    {
        _mbp.SetColor("_Color", color);
        _renderer.SetPropertyBlock(_mbp, 0);
    }

    public virtual string GetSaveData()
    {
        Data saveData = new Data();
        saveData._index1D = _index1D;
        saveData._index2D = _index2D;
        saveData._baseSaveData = new BaseSaveData()
        {
            position = transform.position,
            rotation = transform.rotation.eulerAngles,
            scale = transform.localScale,
        };
        return JsonUtility.ToJson(saveData);
    }

    public virtual void LoadData(string data)
    {
        Data parsed = JsonUtility.FromJson<Data>(data);
        Initialize(parsed._baseSaveData, parsed._index1D, parsed._index2D);
    }

    //Used only in editor
    public virtual void EditorCopyPaste(Ground ground)
    {
        _center = ground.Center;
        _top = ground.Top;
        _index1D = ground._index1D;
        _index2D = ground._index2D;

        transform.position = ground.transform.position;
        transform.rotation = ground.transform.rotation;
        transform.localScale = ground.transform.localScale;
        transform.SetParent(ground.transform.parent);
        transform.SetSiblingIndex(ground.transform.GetSiblingIndex());
#if UNITY_EDITOR
        _debugText = $"Center : {_center} \n Top: {_top}\nIndex1D: {_index1D} \nIndex2D: {_index2D}";
#endif
    }
}