using System;
using System.Data;
using Core;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace States.UIStates
{
    public class UIStateSelectUnit : IState<UIGameController>
    {
        private bool isPickingADirection;
        private UIGameController _controller;
        private UIGameController.PlacedUnit _placedUnit;

        public UIStateSelectUnit(UIGameController.PlacedUnit placedUnit)
        {
            _placedUnit = placedUnit;
        }

        public void OnStateEnter(UIGameController t)
        {
            t.EnableInput(OnPointerDrag, OnPointerUp);
            t.ShowVignette(true);
            _controller = t;
            
            Global.SlowDown();
            CameraController.Instance.Focus(_placedUnit._spawnedEntity.CamPosition);
        }

        public void OnStateUpdate(UIGameController t)
        {
        }

        public void OnStateExit(UIGameController t)
        {
            Global.Play();
            t.DisableInput();
        }

        private void OnPointerDrag(PointerEventData data)
        {
            var diff = data.position - data.pressPosition;
            var norm = diff.normalized;

            float x = Mathf.Abs(norm.x);
            float y = Mathf.Abs(norm.y);
            
            if (x >= y)
                _placedUnit._unitDirection = new Vector3(1 * Mathf.Sign(norm.x), 0, 0);
            else
                _placedUnit._unitDirection = new Vector3(0,0,  1 * Mathf.Sign(norm.y));
            
            _placedUnit._spawnedEntity.transform.forward = _placedUnit._unitDirection;
        }

        private void OnPointerUp(PointerEventData data)
        {
            var dist = Vector3.Distance(data.pressPosition, data.position);
            if (dist < 250)
                return;
            
            _controller.ShowVignette(false);
            CameraController.Instance.ResetCamera();
            
            GameObject.Destroy(_placedUnit._spawnedEntity.GameObject());
            _controller.RecycleUIButton(_placedUnit._uiButton);
            
            _controller.InvokeOnUnitPlaceOnGround(_placedUnit);
            _controller.ChangeState(new UIStateIdle());
        }
    }
}