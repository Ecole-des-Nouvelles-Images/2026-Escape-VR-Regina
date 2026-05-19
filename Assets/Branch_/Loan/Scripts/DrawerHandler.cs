using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit;

public class DrawerHandler : Puzzle
{
   [SerializeField] private GameObject _drawer01;
   [SerializeField] private GameObject _drawer02;
   [SerializeField] private XRGrabInteractable _grabDrawer; 
   [SerializeField]private XRSocketInteractor  _grabSocket;
   [SerializeField]private XRSocketInteractor  _grabSocket1;

   private bool _isEnabled;
   private bool _isOpen;

   private void Awake()
   {
      _grabDrawer.enabled = false;
      _grabSocket.selectEntered.AddListener(OnSocket);
      _grabSocket1.selectEntered.AddListener(OnSocket1);
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

   private void OnSocket(SelectEnterEventArgs args)
   {
      _drawer01.gameObject.SetActive(true);
      GameObject insertedObject = args.interactableObject.transform.gameObject;
      Destroy(insertedObject);
      _grabSocket.gameObject.SetActive(false);
   }
   
   private void OnSocket1(SelectEnterEventArgs args)
   {
      _drawer02.gameObject.SetActive(true);
      GameObject insertedObject = args.interactableObject.transform.gameObject;
      Destroy(insertedObject);
      _grabSocket1.gameObject.SetActive(false);
   }

   public override void Solve()
   {
      base.Solve();
      
      _grabDrawer.enabled = true;
      _isEnabled = true;
   }
}