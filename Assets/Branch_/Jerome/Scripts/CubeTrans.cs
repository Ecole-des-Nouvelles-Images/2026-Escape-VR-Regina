using UnityEngine;

public class CubeTrans : MonoBehaviour
{
    [SerializeField] private SceneTransitionManager sceneTransitionManager;

    private void OnTriggerEnter(Collider other)
    {
        sceneTransitionManager.TransitionToNextScene();
    }
}
