using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class UIGameController : MonoBehaviour
{
    public UIUnitButton[] _buttons;

    [Header("Win")] 
    public RectTransform _rtWin;
    public Button _buttonWin;
    
    [Header("Game Over")] 
    public RectTransform _rtGameOver;
    public Button _buttonGameOver;
    
    [Header("Lifes")] 
    public TextMeshProUGUI _textLifesLeft;
    public TextMeshProUGUI _textLifesRight;
    
    [Header("Unit Currency")] 
    public TextMeshProUGUI _textCurrency;
    public Image _progressCurrency;

    private void Start()
    {
        _rtGameOver.gameObject.SetActive(false);
        _rtWin.gameObject.SetActive(false);
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
        _buttonGameOver.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(GameController.SCENEGAME);
        });
    }

    public void ShowWin()
    {
        _rtWin.gameObject.SetActive(true);
        _buttonWin.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(GameController.SCENEGAME);
        });
    }

    public void Initialize(GameController controller,Action<UnitDataSO, Ground> onUnitPlaced)
    {
        for (int i = 0; i < _buttons.Length; i++)
            _buttons[i].Initialize(controller, onUnitPlaced);
    }

    public void UpdateButtonVisibility(int playerCurrency)
    {
        for (int i = 0; i < _buttons.Length; i++)        
            _buttons[i].UpdateVisibility(playerCurrency);
    }
}