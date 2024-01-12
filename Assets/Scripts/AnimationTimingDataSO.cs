using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Timing data", fileName = "Timing data")]
public class AnimationTimingDataSO : ScriptableObject
{
    private float _counter;
    private bool _isAlreadyTrigger;
    public bool IsAlreadyTrigger => _isAlreadyTrigger;
    public AnimationClip clip;
    public float triggerTime;
    
    public System.Action OnTrigger;
    
    public void Update()
    {
        if(_isAlreadyTrigger)
            return;
        
        _counter += Time.deltaTime;
        if (_counter >= triggerTime)
        {
            OnTrigger?.Invoke();
            _counter = 0;
            _isAlreadyTrigger = true;
        }
    }

    public void Initialize()
    {
        _counter = 0;
        _isAlreadyTrigger = false;
    }
}
