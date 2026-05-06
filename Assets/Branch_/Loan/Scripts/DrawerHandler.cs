using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DrawerHandler : MonoBehaviour
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
      if (_isEnabled)
         return ;

      if (_drawer01.activeInHierarchy && _drawer02.activeInHierarchy)
      {
         _grabDrawer.enabled = true;
         _isEnabled = true;
      }
         
   }
}