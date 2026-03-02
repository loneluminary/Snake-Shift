using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.Extensions;
using Random = UnityEngine.Random;

public class UIManager : Singleton<UIManager>
{
    [FoldoutGroup("Animations")][SerializeField] private Transform coinIcon;

    [FoldoutGroup("Screens")][SerializeField] GameObject lossScreen;
    [FoldoutGroup("Screens")][SerializeField] GameObject winScreen;
    [FoldoutGroup("Screens")][SerializeField] GameObject pauseScreen;

    [FoldoutGroup("UI Texts")][SerializeField] TextMeshProUGUI levelText;
    [FoldoutGroup("UI Texts")][SerializeField] TextMeshProUGUI coinText;

    [FoldoutGroup("Objective Popup")][SerializeField] private CanvasGroup objectivePopup;
    [FoldoutGroup("Objective Popup")][SerializeField] private TMP_Text objectiveText;

    [FoldoutGroup("Toast Popup")][SerializeField] private RectTransform toastPopupContainer;
    [FoldoutGroup("Toast Popup")][SerializeField] private CanvasGroup toastPopupTemplate;
    [FoldoutGroup("Toast Popup")] private List<string> _currentToasts = new();

    private void Start()
    {
        if (coinText) coinText.text = $"{CoinsManager.Instance.CurrentCoins:N0}";

        if (toastPopupTemplate)
        {
            toastPopupTemplate.alpha = 0f;
            toastPopupTemplate.gameObject.SetActive(false);
        }

        if (!GameManager.HasInstance) return;

        if (levelText)
        {
            int levelno = PlayerPrefs.GetInt(GameManager.CURRENT_LEVEL_PREFS);
            levelText.text = $"Level: {levelno}";
        }

        GameManager.Instance.OnGameLose.AddListener(LoseScreen);
        GameManager.Instance.OnGameWin.AddListener(() => this.DelayedExecution(2f, WinScreen));
    }

    public void TogglePauseScreen(bool toggle)
    {
        Time.timeScale = toggle ? 0f : 1f;
        pauseScreen.SetActive(toggle);
    }

    public void LoseScreen(string reason)
    {
        if (lossScreen)
        {
            if (lossScreen.activeSelf) return;

            Time.timeScale = 0f;
            lossScreen.SetActive(true);
        }
    }

    public void WinScreen()
    {
        if (winScreen)
        {
            if (winScreen.activeSelf) return;

            Time.timeScale = 0f;
            GameManager.Instance.UnlockNextLevel();
            winScreen.SetActive(true);
        }
    }

    public void ShowToastMessage(string text)
    {
        if (!toastPopupContainer || _currentToasts.Contains(text)) return;

        var popup = Instantiate(toastPopupTemplate, toastPopupContainer);
        popup.GetComponentInChildren<TMP_Text>().text = text;
        _currentToasts.Add(text);

        popup.gameObject.SetActive(true);
        popup.DOFade(1f, 0.3f).OnComplete(() =>
        {
            popup.DOFade(0f, 0.3f).SetDelay(3f).OnComplete(() =>
            {
                _currentToasts.Remove(text);
                Destroy(popup.gameObject);
            });
        });
    }

    public void UpdateCoinsText(bool adding)
    {
        if (!coinText) return;

        coinText.text = CoinsManager.Instance.CurrentCoins.ToString();

        coinText.DOComplete();
        coinText.transform.DOScale(1.1f, 0.2f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
        if (adding) coinText.DOColor(Color.green, 0.2f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
        else coinText.DOColor(Color.red, 0.2f).SetLoops(2, LoopType.Yoyo).SetUpdate(true);
    }

    public void CoinsAddingAnimation(Vector3 startPos)
    {
        for (int i = 0; i < 7; i++)
        {
            var coin = Instantiate(coinIcon, startPos, Quaternion.LookRotation(Camera.main.transform.forward), coinIcon.root);
            coin.gameObject.SetActive(true);
            coin.localScale = Vector3.zero;

            var random = new Vector3(Random.Range(-1f, 1f), Random.Range(-2f, 2f));

            var seq = DOTween.Sequence().OnComplete(() => Destroy(coin.gameObject)).SetUpdate(true);
            seq.Append(coin.DOScale(1f, 0.5f).SetEase(Ease.OutQuad));
            seq.Join(coin.DOMove(startPos + random, 0.5f).SetEase(Ease.InSine));
            seq.Append(coin.DOMove(coinText.transform.position, 0.5f).SetEase(Ease.InQuad));
        }
    }

    public void ShowObjective(string text)
    {
        if (!objectivePopup) return;

        objectivePopup.gameObject.SetActive(true);
        objectivePopup.DOFade(1f, 1f).SetUpdate(true);
        if (objectiveText) objectiveText.text = text;
        Time.timeScale = 0f;
    }

    public void CloseObjective()
    {
        if (!objectivePopup) return;

        Time.timeScale = 1f;
        objectivePopup.DOFade(0f, 1f).OnComplete(() => objectivePopup.gameObject.SetActive(false)).SetUpdate(true);
    }

    public void PlayGame() => SceneManager.LoadScene(1);

    public void QuitGame()
    {
        Debug.Log("Quit Game");
#if UNITY_EDITOR
        // If running inside the Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // If running as a built player
        Application.Quit();
#endif
    }
}