using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HintController : MonoBehaviour
{
   [Header("===== Update Settings =====")]
   [SerializeField] private float _currentTime;
   [SerializeField] private float _currentTimeToNextHint;
   [Header("===== Time to Next Hint Settings =====")]
   [SerializeField] private float _timeToNextHintGive;
   [SerializeField]  private float _timeToPuzzleChange;
   [Header("===== Hint Object =====")]
   [SerializeField] private GameObject _hint;
   [SerializeField] private GameObject _hintGrab;
   [SerializeField] private Transform _hintGrabParent;

   private Vector3 _startHintGrabPause = new Vector3();
   private Rigidbody _hintGrabRb;
   private XRGrabInteractable _grab;
   
   private bool _isRunning = true;

   private void Start()
   {
      _grab = GetComponentInChildren<XRGrabInteractable>();
      _startHintGrabPause = _hintGrab.transform.position;
      _hintGrabRb = _hintGrab.GetComponent<Rigidbody>();
      
      EventBus.OnPuzzleChanged += PuzzleChanged;
      EventBus.OnHintGived += GivenHint;
      _currentTimeToNextHint = _timeToPuzzleChange;
   }

   private void OnDisable()
   {
      EventBus.OnPuzzleChanged -= PuzzleChanged;
      EventBus.OnHintGived -= GivenHint;
   }

   private void PuzzleChanged()
   {
      _currentTime = 0;
      _currentTimeToNextHint = _timeToPuzzleChange;
      Hide();
   }

   private void GivenHint()
   {
      _currentTime = 0;
      _currentTimeToNextHint = _timeToNextHintGive;
      
      string hint = PuzzleSequenceManager.Instance.GiveStringHint();
      Debug.Log(hint);
      
      Hide();
   }

   private void Update()
   {
      if (!_isRunning)
         return;
      
      _currentTime += Time.deltaTime;

      if (_currentTime >= _currentTimeToNextHint)
      {
         _hint.SetActive(true);
         _isRunning = false;
      }
   }
   
   public void Hide()
   {
      ForceRelease();

      _hintGrab.transform.position = _startHintGrabPause;
      _hintGrabRb.angularVelocity = Vector3.zero;
      _hintGrabRb.linearVelocity = Vector3.zero;
      _hint.SetActive(false);
      _isRunning = true;
   }

   private void ForceRelease()
   {
      if (!_grab.isSelected)
         return;

      var interactor = _grab.firstInteractorSelecting;

      _grab.interactionManager.SelectExit(interactor, _grab);
      _hintGrab.transform.SetParent(_hintGrabParent);
   }
}

