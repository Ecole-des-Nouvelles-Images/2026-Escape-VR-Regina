using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    
    // Hardcoded scene names (adjust these to match your actual scene names)
    private const string ACT1 = "SC_Act1";
    private const string ACT2 = "SC_Act2";
    private const string ACT3 = "SC_Act3";
    
    private static string _currentSideScene = "";
    private static Vector3 _savedPosition;
    private static Quaternion _savedRotation;
    
    private void Start()
    {
        if (!_playerTransform)
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
    
    public void LoadAct1Scene()
    {
        LoadSideScene(ACT1);
    }
    
    public void LoadAct2Scene()
    {
        LoadSideScene(ACT2);
    }
    
    public void LoadAct3Scene()
    {
        LoadSideScene(ACT3);
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
}