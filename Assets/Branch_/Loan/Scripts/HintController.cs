using System;
using UnityEngine;
using UnityEngine.Serialization;

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

   private bool _isRunning = true;

   private void Start()
   {
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
      _hint.SetActive(false);
      _isRunning = true;
   }

   private void GivenHint()
   {
      _currentTime = 0;
      _currentTimeToNextHint = _timeToNextHintGive;
      
      string hint = PuzzleSequenceManager.Instance.GiveStringHint();
      Debug.Log(hint);
      
      _hint.SetActive(false);
      _isRunning = true;
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
}
