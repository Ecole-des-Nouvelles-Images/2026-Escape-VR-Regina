using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class RotaryDialTransformer : XRGeneralGrabTransformer
{
    [Tooltip("Maximum rotation angle in degrees")]
    public float maxRotationAngle = 360f;
    
    [Tooltip("Minimum rotation angle in degrees")]
    public float minRotationAngle = 0f;
    
    [Tooltip("Sensitivity of rotation")]
    public float sensitivity = 1f;
    
    [Tooltip("Enable smooth damping")]
    public bool useSmoothing = true;
    
    [Tooltip("Smoothing speed")]
    public float smoothingSpeed = 15f;
    
    Quaternion m_InitialLocalRotation;
    Vector3 m_LastControllerPosition;
    float m_CurrentAngle = 0f;
    float m_TargetAngle = 0f;
    bool m_IsFirstFrame = true;
    
    protected override RegistrationMode registrationMode => RegistrationMode.SingleAndMultiple;
    
    public override void OnLink(XRGrabInteractable grabInteractable)
    {
        base.OnLink(grabInteractable);
        m_InitialLocalRotation = grabInteractable.transform.localRotation;
        m_CurrentAngle = 0f;
        m_TargetAngle = 0f;
    }
    
    public override void Process(XRGrabInteractable grabInteractable, 
                                 XRInteractionUpdateOrder.UpdatePhase updatePhase, 
                                 ref Pose targetPose, 
                                 ref Vector3 localScale)
    {
        base.Process(grabInteractable, updatePhase, ref targetPose, ref localScale);
        
        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            // Get the interactor (controller) that's grabbing this object
            var interactor = grabInteractable.interactorsSelecting[0];
            Vector3 currentControllerPos = interactor.GetAttachTransform(grabInteractable).position;
            
            // Initialize last position on first frame
            if (m_IsFirstFrame)
            {
                m_LastControllerPosition = currentControllerPos;
                m_IsFirstFrame = false;
                return;
            }
            
            // Get controller position in object's local space
            Vector3 localCurrentPos = grabInteractable.transform.InverseTransformPoint(currentControllerPos);
            Vector3 localLastPos = grabInteractable.transform.InverseTransformPoint(m_LastControllerPosition);
            
            // Calculate circular motion around Z axis
            Vector2 currentDir = new Vector2(localCurrentPos.x, localCurrentPos.y);
            Vector2 lastDir = new Vector2(localLastPos.x, localLastPos.y);
            
            float currentAngle = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;
            float lastAngle = Mathf.Atan2(lastDir.y, lastDir.x) * Mathf.Rad2Deg;
            
            float deltaAngle = Mathf.DeltaAngle(lastAngle, currentAngle);
            deltaAngle *= sensitivity;
            
            // Apply angle limits
            m_TargetAngle = Mathf.Clamp(m_TargetAngle + deltaAngle, minRotationAngle, maxRotationAngle);
            
            // Apply smoothing
            if (useSmoothing)
            {
                m_CurrentAngle = Mathf.Lerp(m_CurrentAngle, m_TargetAngle, Time.deltaTime * smoothingSpeed);
            }
            else
            {
                m_CurrentAngle = m_TargetAngle;
            }
            
            // Apply ONLY Z-axis rotation (this locks X and Y axes)
            Quaternion targetLocalRotation = m_InitialLocalRotation * Quaternion.Euler(0, 0, -m_CurrentAngle);
            
            // Preserve the original X and Y rotations, only modify Z
            Vector3 finalEuler = targetLocalRotation.eulerAngles;
            Vector3 initialEuler = m_InitialLocalRotation.eulerAngles;
            
            // Lock X and Y axes to their initial values
            finalEuler.x = initialEuler.x;
            finalEuler.y = initialEuler.y;
            
            targetLocalRotation = Quaternion.Euler(finalEuler);
            
            targetPose.rotation = grabInteractable.transform.parent.rotation * targetLocalRotation;
            
            // Store for next frame
            m_LastControllerPosition = currentControllerPos;
        }
    }
    
    public override void OnUnlink(XRGrabInteractable grabInteractable)
    {
        base.OnUnlink(grabInteractable);
        m_IsFirstFrame = true;
    }
}