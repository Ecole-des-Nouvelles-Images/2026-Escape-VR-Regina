using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    
    // Hardcoded scene names (adjust these to match your actual scene names)
    // SC_Jerome_Test_Act
    [SerializeField] private string _hub = "SC_Loan_TestHub";
    [SerializeField] private string _act1 = "SC_Act1";
    [SerializeField] private string _act2 = "SC_Act2";
    [SerializeField] private string _act3 = "SC_Act3";
    [SerializeField] private string _win = "SC_Win";
    [SerializeField] private string _loose = "SC_Loose";
    
    private static string _currentSideScene = "";
    private static Vector3 _savedPosition;
    private static Quaternion _savedRotation;
    
    private void Start()
    {
        if (!_playerTransform)
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        EventBus.OnGameLoose += LoadLooseScene;
        EventBus.OnGameWin += LoadWinScene;
        EventBus.OnGameStarted += LoadAct1Scene;
        EventBus.OnCloseEyes?.Invoke();
        StartCoroutine(LoadSceneCoroutine(_hub));
    }

    private void OnDisable()
    {
        EventBus.OnGameLoose -= LoadLooseScene;
        EventBus.OnGameWin -= LoadWinScene;
        EventBus.OnGameStarted -= LoadAct1Scene;
    }

    public void LoadActScene(int sceneIndex)
    {
        switch (sceneIndex)
        {
            case 1: 
                {
                    StartCoroutine(LoadSceneCoroutine(_act1));
                } break;
            
            case 2:
                {
                    StartCoroutine(LoadSceneCoroutine(_act2));
                } break;
            
            case 3:
                {
                    StartCoroutine(LoadSceneCoroutine(_act3));
                } break;
            
            default:
                {
                    Debug.LogError("Invalid scene index");
                } break;
        }
    }

    public void LoadHubScene()
    {
        StartCoroutine(LoadSceneCoroutine(_hub));
    }
    public void LoadAct1Scene()
    {
        StartCoroutine(LoadSceneCoroutine(_act1));
    }
    
    public void LoadAct2Scene()
    {
        StartCoroutine(LoadSceneCoroutine(_act2));
    }
    
    public void LoadAct3Scene()
    {
        StartCoroutine(LoadSceneCoroutine(_act3));
    }

    private void LoadWinScene()
    {
        StartCoroutine(LoadSceneCoroutine(_win));
    }
    
    private void LoadLooseScene()
    {
        StartCoroutine(LoadSceneCoroutine(_loose));
    }
    
    private void LoadSideScene(string sceneName)
    {
        // Unload current side scene if exists
        if (!string.IsNullOrEmpty(_currentSideScene))
        {
            Scene sceneToUnload = SceneManager.GetSceneByName(_currentSideScene);
            if (sceneToUnload.isLoaded)
            {
                SceneManager.UnloadSceneAsync(sceneToUnload);
            }
        }
        
        // Save player position
        _savedPosition = _playerTransform.position;
        _savedRotation = _playerTransform.rotation;
        
        // Load new side scene
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        _currentSideScene = sceneName;
    }
    
    public void ReturnToMain()
    {
        if (!string.IsNullOrEmpty(_currentSideScene))
        {
            Scene sceneToUnload = SceneManager.GetSceneByName(_currentSideScene);
            if (sceneToUnload.isLoaded)
            {
                SceneManager.UnloadSceneAsync(sceneToUnload);
                _currentSideScene = "";
                
                // Restore player position
                _playerTransform.position = _savedPosition;
                _playerTransform.rotation = _savedRotation;
            }
        }
    }
    
    private IEnumerator LoadSceneCoroutine(string scene)
    {
        EventBus.OnCloseEyes?.Invoke();
        yield return new WaitForSeconds(1f);
        LoadSideScene(scene);
        yield return new WaitForSeconds(1f);
        EventBus.OnOpenEyes?.Invoke();
    }
}