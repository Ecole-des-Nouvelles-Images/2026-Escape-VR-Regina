using System;
using UnityEngine;

public class TestUI : MonoBehaviour
{
    [SerializeField] private GameObject _panelOption;    
    [SerializeField] private GameObject _panelMenu;    
    [SerializeField] private GameObject _panelCredits;       
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

    public void ReturnMenuPanel()
    {
        _panelMenu.SetActive(true);
        _panelOption.SetActive(false);
        _panelCredits.SetActive(false);
    }
    
    public void OpenCredits()
    {
        _panelCredits.SetActive(true);
        _panelMenu.SetActive(false);
    }

    public void OpenOption()
    {
        _panelOption.SetActive(true);
        _panelMenu.SetActive(false);
    }
}
