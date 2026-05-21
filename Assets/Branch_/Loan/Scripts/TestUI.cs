using UnityEngine;

public class TestUI : MonoBehaviour
{
    public void StartGame()
    {
        EventBus.OnGameStarted?.Invoke();
    }
}
