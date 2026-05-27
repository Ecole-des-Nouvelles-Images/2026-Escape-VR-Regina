using System;
using UnityEngine;

public class TestUI : MonoBehaviour
{
    private bool _isGameStarted = false;
    private bool _isReturnMenu = false;
    private SceneTransitionManager _sceneManager;

    private void Start()
    {
        _sceneManager = FindFirstObjectByType<SceneTransitionManager>();
    }

    public void StartGame()
    {
        if (!_isGameStarted)
        {
            _isGameStarted = true;
            EventBus.OnGameStarted?.Invoke();
        }
        
    }

    public void ReturnMenu()
    {
        if (!_isReturnMenu)
        {
            _isReturnMenu = true;
            _sceneManager.LoadMenu();
        }
        
    }
}
