using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HintHandler : MonoBehaviour
{
    private void Start()
    {
        GetComponent<XRSimpleInteractable>().selectExited.AddListener(x => ToggleGiveHint());
    }

    private void ToggleGiveHint()
    {
        EventBus.OnHintGived?.Invoke();
    }
}
