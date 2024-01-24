using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public enum AnimationType
    {
        Idle,
        MeleeAttack,
        RangeAttack,
        Run,
        Damaged,
        Dead
    }
    [System.Serializable]
    public struct Data
    {
        public AnimationType _animationType;
        public AnimationTimingDataSO _timing;
    }

    private Dictionary<AnimationType, AnimationTimingDataSO> _dictTiming;
    private AnimationTimingDataSO _processedTimingData;
    [SerializeField] private Animator _animator;
    public Animator Animator => _animator;

    public Data[] _timingData;

    public void Initialize()
    {
        // Debug.Log("Initialize", gameObject);
        _dictTiming = new Dictionary<AnimationType, AnimationTimingDataSO>();
        for (int i = 0; i < _timingData.Length; i++)
        {
            if (!_dictTiming.ContainsKey(_timingData[i]._animationType))
                _dictTiming.Add(_timingData[i]._animationType, _timingData[i]._timing);
        }
    }

    public AnimatorStateInfo GetAnimatorState()
    {
        return _animator.GetCurrentAnimatorStateInfo(0);
    }

    //force play animation
    public void Play(AnimationType animationType)
    {
        _animator.Play(animationType.ToString());
    }

    //use cross fade to blend two animations
    public void Play(AnimationType animationType, float time)
    {
        // Debug.Log("Play", gameObject);
        _animator.CrossFade(animationType.ToString(), time);
        _dictTiming.TryGetValue(animationType, out _processedTimingData);
        
        if(_processedTimingData)
            _processedTimingData.Initialize();
    }

    private void Update()
    {
        if(_processedTimingData == null)
            return;

        if (_processedTimingData.IsAlreadyTrigger)
            _processedTimingData = null;
        else
            _processedTimingData.Update();
        
    }

    public void RegisterAnAction(AnimationType animationType, System.Action onTrigger = null)
    {
        if (!_dictTiming.ContainsKey(animationType))
            return;

        _dictTiming[animationType].OnTrigger = onTrigger;
    }
}