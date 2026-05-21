using System;
using UnityEngine;

public class CubeTrans : MonoBehaviour
{
    [SerializeField] private SceneTransitionManager sceneTransitionManager;
    [Range(1,3)]
    [SerializeField] private int _act;

    private void Start()
    {
        if (!sceneTransitionManager) 
        sceneTransitionManager = FindFirstObjectByType<SceneTransitionManager>();
    }

    private void OnTriggerExit(Collider other)
    {
        switch (_act)
        {
            case 1:
                sceneTransitionManager.LoadAct1Scene();
                break;
            case 2:
                sceneTransitionManager.LoadAct2Scene();
                break;
            case 3:
                sceneTransitionManager.LoadAct3Scene();
                break;
        }
    }
}
