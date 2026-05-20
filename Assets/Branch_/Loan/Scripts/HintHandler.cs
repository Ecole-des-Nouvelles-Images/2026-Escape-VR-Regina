using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HintHandler : MonoBehaviour
{
    private HintController _hintController;
    private void Start()
    {
        _hintController = GetComponentInParent<HintController>();
        GetComponent<XRSimpleInteractable>().selectExited.AddListener(x => ToggleGiveHint());
    }

    private void ToggleGiveHint()
    {
        _hintController.GivenHint();
    }
}
