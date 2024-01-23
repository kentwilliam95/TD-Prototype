using System;
using System.Collections.Generic;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using States.UIStates;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Sequence = DG.Tweening.Sequence;

public class UIGameController : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public struct PlacedUnit
    {
        public UnitDataSO _unitSO;
        public Ground _ground;
        public Vector3 _unitDirection;
        public Entity _spawnedEntity;
        public UIUnitButton _uiButton;
    }

    private StateMachine<UIGameController> _stateMachine;

    public List<UIUnitButton> _buttons;
    public Image _inputImage;
    public CanvasGroup _cgVignette;

    [Header("Win")] public RectTransform _rtWin;
    public Button _buttonWin;

    [Header("Game Over")] public RectTransform _rtGameOver;
    public Button _buttonGameOver;

    [Header("Lifes")] public TextMeshProUGUI _textLifesLeft;
    public TextMeshProUGUI _textLifesRight;

    [Header("Unit Currency")] public TextMeshProUGUI _textCurrency;
    public Image _progressCurrency;

    private Action<PointerEventData> _onPointerDrag;
    private Action<PointerEventData> _onPointerUp;
    private System.Action<PlacedUnit> _onUnitPlacedOnGround;

    private void Start()
    {
        _rtGameOver.gameObject.SetActive(false);
        _rtWin.gameObject.SetActive(false);

        _stateMachine = new StateMachine<UIGameController>(this);
        _stateMachine.ChangeState(new UIStateIdle());
    }

    private void Update()
    {
        _stateMachine?.OnUpdate();
    }

    public void UpdateUnitCurrency(float progress, int value)
    {
        _textCurrency.text = value.ToString();
        _progressCurrency.fillAmount = progress;
    }

    public void UpdateLifeTexts(int left, int right)
    {
        UpdateLifeLeft(left);
        UpdateLifeRight(right);
    }

    public void UpdateLifeLeft(int value, bool animate = false)
    {
        _textLifesLeft.text = value.ToString();
        if (animate)
        {
            Sequence seq = DOTween.Sequence();
            seq.SetAutoKill(true);
            var ogColor = _textLifesLeft.color;
            seq.Insert(0f, _textLifesLeft.DOColor(Color.red, 0.2f));
            seq.Insert(0.2f, _textLifesLeft.DOColor(ogColor, 0.2f));
        }
    }

    public void UpdateLifeRight(int value)
    {
        _textLifesRight.text = value.ToString();
    }

    public void ShowGameOver()
    {
        _rtGameOver.gameObject.SetActive(true);
        _buttonGameOver.onClick.AddListener(() => { SceneManager.LoadScene(Global.SCENEGAME); });
    }

    public void ShowWin()
    {
        _rtWin.gameObject.SetActive(true);
        _buttonWin.onClick.AddListener(() => { SceneManager.LoadScene(Global.SCENEGAME); });
    }

    public void Initialize(GameController controller, Action<PlacedUnit> onUnitPlacedOnGround)
    {
        for (int i = 0; i < _buttons.Count; i++)
            _buttons[i].Initialize(controller, ButtonUI_OnUnitPlaced);
        _onUnitPlacedOnGround = onUnitPlacedOnGround;
    }

    private void ButtonUI_OnUnitPlaced(PlacedUnit placedUnit)
    {
        Debug.Log("Unit Placed On a Ground");
        _stateMachine.ChangeState(new UIStateSelectUnit(placedUnit));
    }

    public void EnableInput(Action<PointerEventData> onPointerDrag, Action<PointerEventData> onPointerUp)
    {
        _inputImage.enabled = true;
        _onPointerDrag = onPointerDrag;
        _onPointerUp = onPointerUp;
    }

    public void DisableInput()
    {
        _onPointerDrag = null;
        _onPointerUp = null;
    }

    public void ShowVignette(bool isShow)
    {
        _cgVignette.DOFade(isShow ? 1 : 0, 0.25f);
        _cgVignette.interactable = isShow;
        _cgVignette.blocksRaycasts = isShow;
    }

    public void UpdateButtonVisibility(int playerCurrency)
    {
        for (int i = 0; i < _buttons.Count; i++)
            _buttons[i].UpdateVisibility(playerCurrency);
    }

    public void RecycleUIButton(UIUnitButton button)
    {
        _buttons.Remove(button);
        Destroy(button.gameObject);
    }

    public void OnDrag(PointerEventData eventData)
    {
        _onPointerDrag?.Invoke(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _onPointerUp?.Invoke(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void InvokeOnUnitPlaceOnGround(PlacedUnit placedUnit)
    {
        _onUnitPlacedOnGround.Invoke(placedUnit);
    }

    public void ChangeState(IState<UIGameController> state)
    {
        _stateMachine.ChangeState(state);
    }
}