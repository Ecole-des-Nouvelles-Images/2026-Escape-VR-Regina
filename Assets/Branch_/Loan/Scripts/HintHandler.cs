using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HintHandler : MonoBehaviour
{
    private void Start()
    {
        GetComponent<XRSimpleInteractable>().selectEntered.AddListener(x => ToggleGiveHint());
    }

    private void ToggleGiveHint()
    {
        EventBus.OnHintGived?.Invoke();
    }
}
