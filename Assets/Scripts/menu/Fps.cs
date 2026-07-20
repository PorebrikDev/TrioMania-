using UnityEngine;

public class GameSettings : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;
    }
}