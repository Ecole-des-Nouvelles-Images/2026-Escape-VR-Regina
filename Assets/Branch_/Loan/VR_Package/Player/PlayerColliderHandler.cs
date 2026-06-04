using System;
using UnityEngine;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Random = UnityEngine.Random;

public class PlayerColliderHandler : MonoBehaviour
{
   [Header("===== General Settings =====")]
   [SerializeField] private Transform _targetTransform;
   [SerializeField] private CharacterController _characterController;
   [FormerlySerializedAs("interactor")] [SerializeField] private XRBaseInteractor _rightBaseInteractor;
   [SerializeField] private XRBaseInteractor _leftBaseInteractor;
   
   [Header("===== Height collider Settings =====")]
   [SerializeField] private float _minHeight;
   [SerializeField] private float _maxHeight;
   
   [Header("===== Reglages RoomScale =====")]
   [Tooltip("Distance en mètres entre chaque pas (ex: 0.6 = 60cm)")]
   [SerializeField] private float _distanceParPas = 0.6f;
   [SerializeField] private LayerMask _carpetLayer;
   
   [Header("===== Audio Enigme Settings =====")]
   [SerializeField] private LayerMask _layerMask;
   [SerializeField] private AudioSource _audioSource;
   [SerializeField]  private AudioClip _audioClip;
   
   [Header("===== Audio Steps Settings =====")]
   [SerializeField] private AudioSource _audioStepsSource;
   [SerializeField] private AudioClip _audioStepWood;
   [SerializeField]   private AudioClip _audioStepCarpet;
   
   private bool _eg1;
   private Quaternion _initialRotation;
   private Transform _enemieTransform;
   public bool _islockGrab;
   private Vector3 _lastPosFloor;
   private LockManager _lockManager;

   private void Start()
   {
      _lastPosFloor = new Vector3(_targetTransform.position.x, 0, _targetTransform.position.z);
   }

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
      
      Vector3 positionActuelleSol = new Vector3(_targetTransform.position.x, 0, _targetTransform.position.z);
      
      float distanceParcourue = Vector3.Distance(_lastPosFloor, positionActuelleSol);
      
      if (distanceParcourue >= _distanceParPas)
      {
         CalculerEtJouerPas();
         _lastPosFloor = positionActuelleSol;
      }
   }
   
   private void CalculerEtJouerPas()
   {
      
      Ray ray = new Ray(_targetTransform.position, Vector3.down);
      RaycastHit hit;

      
      if (Physics.Raycast(ray, out hit, 3f))
      {
         
         if (((1 << hit.collider.gameObject.layer) & _carpetLayer.value) != 0)
         {
            PlayStepSound(_audioStepCarpet);
         }
         else
         {
            PlayStepSound(_audioStepWood);
         }
      }
   }

   private void PlayStepSound(AudioClip clip)
   {
      if (clip == null) return;
      
      _audioStepsSource.pitch = Random.Range(0.9f, 1.1f);
      _audioStepsSource.Stop();
      _audioStepsSource.PlayOneShot(clip);
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
