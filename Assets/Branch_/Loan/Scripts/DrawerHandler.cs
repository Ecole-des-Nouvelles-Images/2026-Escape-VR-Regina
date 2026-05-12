using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DrawerHandler : Puzzle
{
   [SerializeField] private GameObject _drawer01;
   [SerializeField] private GameObject _drawer02;
   [SerializeField] private XRGrabInteractable _grabDrawer;

   private bool _isEnabled;

   private void Awake()
   {
      _grabDrawer.enabled = false;
   }

   private void Update()
   {
      if (PuzzleSequenceManager.Instance.CurrentPuzzle != this)
         return;
      
      if (_isEnabled)
         return ;

      if (_drawer01.activeInHierarchy && _drawer02.activeInHierarchy)
      {
         Solve();
      }
         
   }

   public override void Solve()
   {
      base.Solve();
      _grabDrawer.enabled = true;
      _isEnabled = true;
   }
}