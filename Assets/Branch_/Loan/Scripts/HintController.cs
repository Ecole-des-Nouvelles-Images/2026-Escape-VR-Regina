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

   private Vector3 _startHintGrabPause = new Vector3();
   private Rigidbody _hintGrabRb;
   private XRGrabInteractable _grab;
   
   private bool _isRunning = true;

   private void Start()
   {
      EventBus.OnPuzzleChanged += PuzzleChanged;
      PuzzleChanged();
   }

   private void OnDisable()
   {
      EventBus.OnPuzzleChanged -= PuzzleChanged;
   }

   private void PuzzleChanged()
   {
      _currentTime = 0;
      _currentTimeToNextHint = _timeToPuzzleChange;
      Hide();
   }

   public void GivenHint()
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
      _hint.SetActive(false);
      _isRunning = true;
   }
}

