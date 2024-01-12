using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIUnitButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private RaycastHit[] hitResult = new RaycastHit[1];
    private Ground _selectedGround;
    private GameController _controller;
    public UnitDataSO _unitSO;
    public TextMeshProUGUI _textCost;
    public CanvasGroup _canvasGroup;
    
    public Action<UnitDataSO, Ground> onUnitPlaced;
    
    public void Initialize(GameController gameController, Action<UnitDataSO, Ground> onUnitPlaced)
    {
        _controller = gameController;
        _textCost.text = _unitSO._cost.ToString();
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
        if(_controller.GameState == GameController.State.End)
            return;
        
        _selectedGround = null;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(_controller.GameState == GameController.State.End)
            return;
        
        if(!_selectedGround)
            return;
        
        _selectedGround.SetColor(Color.green);
        onUnitPlaced?.Invoke(_unitSO, _selectedGround);
        
        _selectedGround = null;
        gameObject.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(_controller.GameState == GameController.State.End)
            return;
        
        var cam = CameraController.Instance.Camera;
        
        var ray = cam.ScreenPointToRay(eventData.position);
        int hitCount = Physics.RaycastNonAlloc(ray, hitResult, 25f, GameController.LayerMaskGround);

        for (int i = 0; i < hitCount; i++)
        {
            Ground ground = hitResult[i].collider.GetComponentInParent<Ground>();
            if (ground)
            {
                if (ground is GroundObjective || ground is GroundSpawnedEnemy)
                    return;
                
                var isValidPlacement = _controller.CheckIfThereIsAnEntityOnGround(ground, GameController.TEAMALLY);
                if (!isValidPlacement)
                    _selectedGround = ground;
            }
        }
        
        if (hitCount == 0)
            _selectedGround = null;
    }
}