using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Windows;
public class UIInterface : MonoBehaviour
{
    [SerializeField] private Controller _controller;
    [SerializeField] private GameInput _input;

    [SerializeField] private TMP_Text _prefab;
    [SerializeField] private TMP_Text _scoreCount;
    [SerializeField] private TMP_Text _swapCount;

    [SerializeField] private Transform _plase;

    [SerializeField] private Image _windowPauseMenu;
    [SerializeField] private Image _windowEndMenu;
    [SerializeField] private Image _windowQwe;
    [SerializeField] private Button _restart;
    [SerializeField] private Button _stop;
    [SerializeField] private Button _pauseRestart;
    [SerializeField] private Button _pauseStop;
    [SerializeField] private Button _exe;
    [SerializeField] private Button _yes;
    [SerializeField] private Button _no;



    private int swap = 0;
    private int lastpos = 0;
    private int _poolSize = 15;

    private Queue<(int score, int plus)> _queue = new();
    private List<TMP_Text> _cards = new();

    private bool _isShowing;

    private void Awake()
    {
        GeneratePool(_poolSize);
    }

    private void Start()
    {
        _controller._scoreSystem.OnScoreRefresh += Refresh;
        _controller.RefreshSwapCount += RefreshSwap;
        _input.OnKey += OpenPauseMenu;
        _restart.onClick.AddListener(ClickRestart);
        _stop.onClick.AddListener(ClickStop);
        _exe.onClick.AddListener(OpenQwe);
        _yes.onClick.AddListener(Yes);
        _no.onClick.AddListener(No);
        _pauseRestart.onClick.AddListener(ClickRestart);
        _pauseStop.onClick.AddListener(ClickStop);


        _windowPauseMenu.gameObject.SetActive(false);
        _windowEndMenu.gameObject.SetActive(false);
        _windowQwe.gameObject.SetActive(false);

        SetScore(0);
        StartSwapCount();
    }

    private void SetScore(int score)
    {
        _scoreCount.text = score.ToString();
    }

    private void Yes()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }
    private void No() => CloseQwe();
    public void OpenQwe() => _windowQwe.gameObject.SetActive(true);
    public void CloseQwe() => _windowQwe.gameObject.SetActive(false);

    public void OpenPauseMenu()
    {
        if (_windowPauseMenu.gameObject.activeSelf)
        {
            _controller.Pause(false);
            _windowPauseMenu.gameObject.SetActive(false);
        }
        else
        {
            _controller.Pause(true);
            _windowPauseMenu.gameObject.SetActive(true);
        }
    }

    public void OpenEndMenu() => _windowEndMenu.gameObject.SetActive(true);

    public void StartSwapCount()
    {
        swap = _controller.GetStartSwapCount();
        _swapCount.text = swap.ToString();
    }

    public void RefreshSwap()
    {
        if (swap <= 1)
        {
            OpenEndMenu();
        }
        swap -= 1;
        _swapCount.text = swap.ToString();

    }

    public void Refresh(int score, int plus)
    {

        _queue.Enqueue((score, plus));

        if (!_isShowing)
        {
            ShowQueue().Forget();
        }
    }

    private async UniTaskVoid ShowQueue()
    {
        _isShowing = true;

        while (_queue.Count > 0)
        {
            var data = _queue.Dequeue();

            _scoreCount.text = data.score.ToString();

            TMP_Text text = GetPool();
            text.text = "+" + data.plus;

            text.rectTransform.anchoredPosition =
                new Vector2(GetCurrentPos(), Random.Range(-20, 20));

            await UniTask.Delay(400);

            SetPool(text);
        }

        _isShowing = false;
    }

    private float GetCurrentPos()
    {
        const int maxAttempts = 20;

        for (int i = 0; i < maxAttempts; i++)
        {
            int x = Random.Range(-200, 0);
            if (Mathf.Abs(x - lastpos) > 90)
                return x;
        }
        return Random.Range(-200, 0);
    }

    private IEnumerator HideAfter(TMP_Text text, float time)
    {
        yield return new WaitForSeconds(time);

        SetPool(text);
    }

    private void SetPool(TMP_Text card) => card.gameObject.SetActive(false);


    private TMP_Text GetPool()
    {
        foreach (var card in _cards)
        {
            if (!card.gameObject.activeSelf)
            {
                card.gameObject.SetActive(true);
                return card;
            }
        }
        GeneratePool(5);
        return GetPool();
    }

    private void GeneratePool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var card = Instantiate(_prefab, _plase);
            card.gameObject.SetActive(false);
            _cards.Add(card);
        }
    }

    private void ClickRestart() => SceneManager.LoadScene("Game");

    private void ClickStop() => SceneManager.LoadScene("MainMenu");

    private void OnDestroy()
    {
        _controller._scoreSystem.OnScoreRefresh -= Refresh;
        _controller.RefreshSwapCount -= RefreshSwap;

        _restart.onClick.RemoveListener(ClickRestart);
        _stop.onClick.RemoveListener(ClickStop);

        _exe.onClick.RemoveListener(OpenQwe);
        _yes.onClick.RemoveListener(Yes);
        _no.onClick.RemoveListener(No);

        _pauseRestart.onClick.RemoveListener(ClickRestart);
        _pauseStop.onClick.RemoveListener(ClickStop);
    }
}
