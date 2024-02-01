using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIUnitButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private RaycastHit[] hitResult = new RaycastHit[1];
    private Ground _selectedGround;
    private GameController _controller;
    private Entity _spawnedEntity;
    private float _clickDuration = 0.2f;
    private float _timeCache;

    public UnitDataSO _unitSO;
    public TextMeshProUGUI _textCost;
    public Image _image;
    public CanvasGroup _canvasGroup;

    public Action<UIGameController.PlacedUnit> onUnitPlaced;
    public Action onSelected;
    public Action<UnitDataSO> onButtonClicked;

    public void Initialize(GameController gameController, Action<UIGameController.PlacedUnit> onUnitPlaced)
    {
        _controller = gameController;
        _textCost.text = _unitSO._cost.ToString();
        _image.sprite = _unitSO._sprite;
        this.onUnitPlaced = onUnitPlaced;
    }

    public void UpdateVisibility(int playerCurrency)
    {
        bool isVisible = playerCurrency >= _unitSO._cost;
        _canvasGroup.interactable = isVisible;
        _canvasGroup.blocksRaycasts = isVisible;
        _canvasGroup.alpha = isVisible ? 1f : 0.5f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _timeCache = Time.time;
        if (_controller.GameState == GameController.State.End)
            return;

        if (_spawnedEntity == null)
            _spawnedEntity = SpawnUnit();

        _spawnedEntity.gameObject.SetActive(false);
        _selectedGround = null;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (HandleClick())
            return;   
        
        if (_controller.GameState == GameController.State.End)
            return;

        if (!_selectedGround)
        {
            Destroy(_spawnedEntity.gameObject);
            return;   
        }

        _selectedGround.SetColor(Color.green);
        onUnitPlaced?.Invoke(new UIGameController.PlacedUnit() { _unitSO = _unitSO, _ground = _selectedGround , _spawnedEntity = _spawnedEntity, _uiButton = this});

        _selectedGround = null;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_controller.GameState == GameController.State.End)
            return;

        var cam = CameraController.Instance.Camera;

        var ray = cam.ScreenPointToRay(eventData.position);
        int hitCount = Physics.RaycastNonAlloc(ray, hitResult, Global.DRAGDROPRAYCASTLENGTH, Global.LayerMaskGround);

        for (int i = 0; i < hitCount; i++)
        {
            Ground ground = hitResult[i].collider.GetComponentInParent<Ground>();
            if (ground)
            {
                if (ground is GroundObjective || ground is GroundSpawnedEnemy)
                    return;

                var isValidPlacement = _controller.CheckIfThereIsAnEntityOnGround(ground, UnitDataSO.Team.Blue);
                if (!isValidPlacement)
                {
                    _spawnedEntity.transform.position = ground.Top;
                    _selectedGround = ground;
                    _spawnedEntity.gameObject.SetActive(true);
                }
            }
        }

        if (hitCount == 0)
            _selectedGround = null;
    }

    private bool HandleClick()
    {
        if (Time.time - _timeCache <= _clickDuration)
        {
            Debug.Log("Clicked!");
            this.onButtonClicked?.Invoke(_unitSO);
            return true;
        }

        return false;
    }

    private Entity SpawnUnit()
    {
        return Instantiate(_unitSO.prefab);
    }

    private void DestroyUnit()
    {
    }
}