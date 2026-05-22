using UnityEngine;

public class TestUI : MonoBehaviour
{
    private bool _isGameStarted = false;
    public void StartGame()
    {
        if (!_isGameStarted)
        {
            _isGameStarted = true;
            EventBus.OnGameStarted?.Invoke();
        }
        
    }
}
