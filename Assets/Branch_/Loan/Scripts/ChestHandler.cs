using System;
using System.Collections;
using UnityEngine;

public class ChestHandler : MonoBehaviour
{
    [SerializeField ] private GameObject _topChest;
    [SerializeField] private float _duration;
    [SerializeField] private Transform _posTransform;
    [SerializeField] private AnimationCurve _moveCurve = AnimationCurve.Linear(0, 0, 1, 1);
    private void Start()
    {
        EventBus.OnOpenChest += OpenChest;
        EventBus.OnChestMove += MoveChest;
        _posTransform = GameObject.FindWithTag("ChestPoint").GetComponent<Transform>();
    }

    private void OnDisable()
    {
        EventBus.OnOpenChest-=OpenChest ;
        EventBus.OnChestMove -= MoveChest;
    }
    
    [ContextMenu("Open Chest")]
    private void OpenChest()
    {
        StartCoroutine(OpenChestCoroutine());
    }

    private void MoveChest()
    {
        StartCoroutine(MoveChestCoroutine());
    }

    private IEnumerator MoveChestCoroutine()
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation; // Optionnel : si tu veux aussi aligner la rotation

        float elapsedTime = 0f;

        while (elapsedTime < _duration)
        {
            elapsedTime += Time.deltaTime;
        
            
            float normalizedTime = elapsedTime / _duration;
        
            
            float curveValue = _moveCurve.Evaluate(normalizedTime);
            
            transform.position = Vector3.Lerp(startPosition, _posTransform.position, curveValue);
        
            // Optionnel : Rotation fluide (décommente si tu en as besoin)
            transform.rotation = Quaternion.Slerp(startRotation, _posTransform.rotation, curveValue);

            // Attend le prochain frame avant de continuer la boucle
            yield return null;
        }

        // Sécurité : On force la position exacte à la fin pour éviter les micro-écarts de virgule flottante
        transform.position = _posTransform.position;
        transform.rotation = _posTransform.rotation;
    }
    
    private IEnumerator OpenChestCoroutine()
    {
        float startZ = _topChest.transform.localEulerAngles.z;
        float targetZ = -140f;

        _duration = 1.5f;
        float time = 0f;

        while (time < _duration)
        {
            time += Time.deltaTime;

            float t = time / _duration;

            float currentZ = Mathf.LerpAngle(startZ, targetZ, t);

            Vector3 rot = _topChest.transform.localEulerAngles;
            rot.z = currentZ;

            _topChest.transform.localEulerAngles = rot;

            yield return null;
        }

        Vector3 finalRot = _topChest.transform.localEulerAngles;
        finalRot.z = targetZ;

        _topChest.transform.localEulerAngles = finalRot;
    }
    
}
