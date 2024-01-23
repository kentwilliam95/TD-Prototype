using DG.Tweening;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    struct CameraData
    {
        public Vector3 _position;
    }

    private CameraData _data;

    private static CameraController _instance;
    public static CameraController Instance => _instance;
    
    [SerializeField] private Camera _camera;
    public Camera Camera => _camera;

    private void Awake()
    {
        _instance = this;
    }

    public void Focus(Transform target)
    {
        _data = new CameraData()
        {
            _position = _camera.transform.position,
        };
        _camera.transform.DOMove(target.position, 0.25f);
    }

    public void ResetCamera()
    {
        _camera.transform.DOMove(_data._position, 0.25f);
    }
}