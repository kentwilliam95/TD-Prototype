using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameController))]
public class EditorGameController : Editor
{
    private GameController _controller;

    private void OnEnable()
    {
        _controller = target as GameController;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(Application.isPlaying)
            return;
        
        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        _controller._size.x = EditorGUILayout.IntField("X", _controller._size.x);
        _controller._size.y = EditorGUILayout.IntField("Y", _controller._size.y);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create"))
        {
            _controller._mapDataSo = null;
            DestroyChilds(_controller.TrEditorContainer);
            GenerateGrounds();
        }

        if (GUILayout.Button("Clear"))
        {
            _controller._mapDataSo = null;
            DestroyChilds(_controller.TrEditorContainer);
        }

        EditorGUILayout.EndHorizontal();
        
        if (GUILayout.Button("Save"))
        {
            if (_controller._mapDataSo != null)
            {
                var so = _controller._mapDataSo;
                so.mapData = GetSaveString();
                so._sizeX = _controller._size.x;
                so._sizeY = _controller._size.y;
                EditorUtility.SetDirty(so);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return;
            }

            var path = EditorUtility.SaveFilePanel("Save Map", "Assets/Resources/Map/", "", "");
            var name = Path.GetFileNameWithoutExtension(path);
            if(string.IsNullOrEmpty(name))
                return;
            
            var str = GetSaveString();
            var mapSO = ScriptableObject.CreateInstance<MapDataSO>();
            mapSO.mapData = str;
            mapSO._sizeX = _controller._size.x;
            mapSO._sizeY = _controller._size.y;
            
            var filename = Path.GetFileName(path);

            var uniquePath = AssetDatabase.GenerateUniqueAssetPath($"Assets/Resources/Map/{filename}.asset");
            filename = Path.GetFileName(uniquePath);

            mapSO.name = filename;
            AssetDatabase.CreateAsset(mapSO, uniquePath);
            _controller._mapDataSo = mapSO;
        }

        if (GUILayout.Button("Load"))
        {
            var path = EditorUtility.OpenFilePanel("Open a Map", "Assets/Resources/Map", ".asset");
            string filename = Path.GetFileNameWithoutExtension(path);
            
            if (string.IsNullOrEmpty(filename))
                return;

            var filePath = Path.GetRelativePath("Assets/Resources/", path);
            filePath = filePath.Substring(0, filePath.IndexOf('.'));

            DestroyChilds(_controller.TrEditorContainer);
            
            var map = Resources.Load<MapDataSO>(filePath);
            _controller.EditorLoadFromMapDataSO(map);
            _controller._size.x = map._sizeX;
            _controller._size.y = map._sizeY;
            _controller._mapDataSo = map;
        }
    }

    private string GetSaveString()
    {
        Debug.Log("Saved!");
        GroundSaveData data = new GroundSaveData();
        data.saveData = new List<WrapperSaveData>();

        var transform = _controller.TrEditorContainer;
        var total = transform.childCount;
        for (int i = 0; i < total; i++)
        {
            var ground = transform.GetChild(i).gameObject.GetComponent<Ground>();

            WrapperSaveData wrapper = new WrapperSaveData();
            wrapper.prefabName = ground.name;
            wrapper.saveData = ground.GetSaveData();
            data.saveData.Add(wrapper);
        }

        var str = JsonUtility.ToJson(data);
        return str;
    }

    public void GenerateGrounds()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<Ground>("Assets/Resources/Ground/Ground.prefab");
        var container = _controller.TrEditorContainer;
        
        for (int z = 0; z < _controller._size.y; z++)
        {
            for (int x = 0; x < _controller._size.x; x++)
            {
                var instance = Instantiate(prefab, container);
                instance.name = prefab.name;

                int index1D = z * _controller._size.x + x;
                Vector2Int index2D = new Vector2Int(x, z);

                instance.transform.localPosition = new Vector3(x * Ground.SIZE, 0, z * Ground.SIZE);
                instance.Initialize(index1D, index2D);
            }
        }
    }

    private void DestroyChilds(Transform transform)
    {
        var total = transform.childCount;
        for (int i = 0; i < total; i++)
            DestroyImmediate(transform.GetChild(0).gameObject);
    }
}