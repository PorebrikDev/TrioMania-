using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MeinMenu : MonoBehaviour
{
    [SerializeField] private Button _start;
    [SerializeField] private Button _exid;

    private void Awake()
    {

    }

    private void Start()
    {
        _start.onClick.AddListener(StartClick);
        _exid.onClick.AddListener(ExidClick);
    }

    private void OnDestroy()
    {
        _start.onClick.RemoveListener(StartClick);
        _exid.onClick.RemoveListener(ExidClick);
    }

    private void StartClick()
    {
        SceneManager.LoadSceneAsync("Game");
    }

    private void ExidClick()
    {
        Application.Quit();
    }

}
