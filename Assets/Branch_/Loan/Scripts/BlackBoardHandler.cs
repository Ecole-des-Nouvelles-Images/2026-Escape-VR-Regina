using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class BlackBoardHandler : MonoBehaviour
{
   [Header("===== Settings =====")] [SerializeField]
   private GameObject _objectPrefab;
   public bool IsOccuped;

   private BlackBoardManager _blackBoardManager;
   private GameObject _currentObject;
   private XRSocketInteractor _socket;
   
   private void Start()
   {
      _blackBoardManager = GetComponentInParent<BlackBoardManager>();
      _socket = GetComponent<XRSocketInteractor>();
      
      _socket.selectEntered.AddListener(OnSnapped);
      _socket.selectExited.AddListener(OnUnsnapped);
   }

   private void OnSnapped(SelectEnterEventArgs args)
   {
      _currentObject = args.interactableObject.transform.gameObject;
      IsOccuped = true;
      _blackBoardManager.SocketIsOccuped();
   }

   private void OnUnsnapped(SelectExitEventArgs args)
   {
      _currentObject = null;
      IsOccuped = false;
   }

   public bool IsObject()
   {
      if (!IsOccuped)
         return false;
      
      if (_currentObject == _objectPrefab)
      {
         return true;
      }

      return false;
   }
}