using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class FixedRotationAxisLockGrabTransformer : XRGeneralGrabTransformer
{
    [Tooltip("Defines which rotation axes are allowed when grabbing. Unchecked axes maintain initial rotation.")]
    public ManipulationAxes PermittedRotationAxes = ManipulationAxes.Y;  // Set Y for doorknob style

    Vector3 m_InitialEulerRotation;

    protected override RegistrationMode registrationMode => RegistrationMode.SingleAndMultiple;

    public override void OnLink(XRGrabInteractable grabInteractable)
    {
        base.OnLink(grabInteractable);
        m_InitialEulerRotation = grabInteractable.transform.rotation.eulerAngles;
    }

    public override void Process(XRGrabInteractable grabInteractable, 
        XRInteractionUpdateOrder.UpdatePhase updatePhase, 
        ref Pose targetPose, 
        ref Vector3 localScale)
    {
        base.Process(grabInteractable, updatePhase, ref targetPose, ref localScale);

        var newRotationEuler = targetPose.rotation.eulerAngles;

        if ((PermittedRotationAxes & ManipulationAxes.X) == 0)
            newRotationEuler.x = m_InitialEulerRotation.x;

        if ((PermittedRotationAxes & ManipulationAxes.Y) == 0)
            newRotationEuler.y = m_InitialEulerRotation.y;

        if ((PermittedRotationAxes & ManipulationAxes.Z) == 0)
            newRotationEuler.z = m_InitialEulerRotation.z;

        targetPose.rotation = Quaternion.Euler(newRotationEuler);
    }
}