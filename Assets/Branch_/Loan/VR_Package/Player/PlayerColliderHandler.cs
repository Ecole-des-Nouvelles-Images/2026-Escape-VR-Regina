using UnityEngine;

public class PlayerColliderHandler : MonoBehaviour
{
   [Header("General Settings")]
   [SerializeField] private Transform _targetTransform;
   [SerializeField] private CharacterController _characterController;
   
   [Header("Height collider Settings")]
   [SerializeField] private float _minHeight;
   [SerializeField] private float _maxHeight;
   
   private Quaternion _initialRotation;
   private Transform _enemieTransform;

   private void Update()
   {
      float currentHeight = (_targetTransform.position.y - transform.position.y) + 0.1f;

      float newHeight = Mathf.Clamp(currentHeight, _minHeight, _maxHeight);

      _characterController.height = newHeight;

      _characterController.center =
         new Vector3(_characterController.center.x, newHeight / 2f, _characterController.center.z);
   }
   
}
