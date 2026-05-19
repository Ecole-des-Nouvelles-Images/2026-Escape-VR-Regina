using System;
using System.Collections;
using UnityEngine;

public class ChestHandler : MonoBehaviour
{
    [SerializeField ] private GameObject _topChest;
    [SerializeField] private float _duration;
    private void Start()
    {
        EventBus.OnOpenChest += OpenChest;
    }

    private void OnDisable()
    {
        EventBus.OnOpenChest-=OpenChest ;
    }
    
    [ContextMenu("Open Chest")]
    private void OpenChest()
    {
        StartCoroutine(OpenChestCoroutine());
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
