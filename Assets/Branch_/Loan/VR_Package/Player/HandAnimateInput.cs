using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimateInput : MonoBehaviour
{
    [SerializeField] private Animator _handAnimator;
    [SerializeField] private InputActionProperty _triggerValue;
    [SerializeField] private InputActionProperty _gripValue;

    private void Update()
    {
        float trigger = _triggerValue.action.ReadValue<float>();
        float grip = _gripValue.action.ReadValue<float>();
        
        _handAnimator.SetFloat("Trigger", trigger);
        _handAnimator.SetFloat("Grip", grip);
    }
}
