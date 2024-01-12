using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Ground), true), CanEditMultipleObjects]
public class EditorGround : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Convert To Ground"))
        {
            var asset = AssetDatabase.LoadAssetAtPath<Ground>("Assets/Resources/Ground/Ground.prefab");
            SetupGround(asset);
        }

        if (GUILayout.Button("Convert to Enemy Ground"))
        {
            var asset = AssetDatabase.LoadAssetAtPath<GroundSpawnedEnemy>("Assets/Resources/Ground/GroundEnemySpawnPoint.prefab");
            SetupGround(asset);
        }

        if (GUILayout.Button("Convert to Ground Objective"))
        {
            var asset = AssetDatabase.LoadAssetAtPath<GroundObjective>("Assets/Resources/Ground/Ground Objective.prefab");
            SetupGround(asset);
        }
        
        if (GUILayout.Button("Convert to Ground Empty"))
        {
            var asset = AssetDatabase.LoadAssetAtPath<GroundEmpty>("Assets/Resources/Ground/Ground Empty.prefab");
            Debug.Log(asset);
            SetupGround(asset);
        }
    }

    private void SetupGround(Ground asset)
    {
        var enemyGround = GameObject.Instantiate(asset);
        enemyGround.gameObject.SetActive(true);
        enemyGround.name = asset.name;
        
        var groundRef = target as Ground;
        enemyGround.EditorCopyPaste(groundRef);
        
        Selection.activeGameObject = enemyGround.gameObject;
        DestroyImmediate(groundRef.gameObject);
    }
}