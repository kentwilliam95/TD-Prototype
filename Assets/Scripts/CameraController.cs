using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private static CameraController _instance;
    public static CameraController Instance => _instance;
    
    [SerializeField] private Camera _camera;
    public Camera Camera => _camera;

    private void Awake()
    {
        _instance = this;
    }
}