using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private string nextSceneName;
    [SerializeField] private Transform _playerTransform;
    private static Vector3 savedPosition;
    private static Quaternion savedRotation;

    [ContextMenu("Load Scene")]
    private void Start()
    {
        if (!_playerTransform)
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    public void TransitionToNextScene()
    {
        Debug.Log("Load Scene");
        // Save current position before leaving
        savedPosition = _playerTransform.position;
        savedRotation = _playerTransform.rotation;
        
        SceneManager.LoadScene(nextSceneName);
    }
    
    // Call this in your player script after scene loads
    public void RestorePosition()
    {
        _playerTransform.position = savedPosition;
        _playerTransform.rotation = savedRotation;
    }
}