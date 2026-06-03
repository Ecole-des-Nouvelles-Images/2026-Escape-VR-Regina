using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PlayerColliderHandler : MonoBehaviour
{
   [Header("General Settings")]
   [SerializeField] private Transform _targetTransform;
   [SerializeField] private CharacterController _characterController;
   [FormerlySerializedAs("interactor")] [SerializeField] private XRBaseInteractor _rightBaseInteractor;
   [SerializeField] private XRBaseInteractor _leftBaseInteractor;
   
   [Header("Height collider Settings")]
   [SerializeField] private float _minHeight;
   [SerializeField] private float _maxHeight;
   
   [SerializeField] private LayerMask _layerMask;
   [SerializeField] private AudioSource _audioSource;
   [SerializeField]  private AudioClip _audioClip;
   private bool _eg1;
   private Quaternion _initialRotation;
   private Transform _enemieTransform;
   public bool _islockGrab;
   [SerializeField]private LockManager _lockManager;

   private void OnEnable()
   {
      if (_rightBaseInteractor != null)
      {
         _rightBaseInteractor.selectEntered.AddListener(OnObjectGrabbed);
         _leftBaseInteractor.selectEntered.AddListener(OnObjectGrabbed);
      }
   }

   private void OnDisable()
   {
      if (_rightBaseInteractor != null)
      {
         _rightBaseInteractor.selectEntered.RemoveListener(OnObjectGrabbed);
         _leftBaseInteractor.selectEntered.RemoveListener(OnObjectGrabbed);
      }
   }

   private void Update()
   {
      float currentHeight = (_targetTransform.position.y - transform.position.y) + 0.1f;

      float newHeight = Mathf.Clamp(currentHeight, _minHeight, _maxHeight);

      _characterController.height = newHeight;

      _characterController.center =
         new Vector3(_characterController.center.x, newHeight / 2f, _characterController.center.z);
   }

   private void OnObjectGrabbed(SelectEnterEventArgs args)
   {
      if (!_islockGrab)
      {
         GameObject grabbedObject = args.interactableObject.transform.gameObject;
         if (grabbedObject.CompareTag("Lock"))
         {
            _lockManager = grabbedObject.GetComponent<LockManager>();
            _islockGrab = true;
         }
      }
      else
      {
         GameObject grabbedObject = args.interactableObject.transform.gameObject;
         if (grabbedObject.CompareTag("Lock"))
         {
            _lockManager = null;
            _islockGrab = false;
         }
         
         if (_lockManager != null)
         {
            _lockManager.OnGrab(args);
            _islockGrab = false;
            _lockManager = null;
         }
      }
      
      GameObject obj = args.interactableObject.transform.gameObject;

      if (((1 << obj.layer) & _layerMask.value) != 0 && !_eg1)
      {
         _audioSource.Stop();
         _audioSource.PlayOneShot(_audioClip);
         _eg1 = true;
      }
   }
}
