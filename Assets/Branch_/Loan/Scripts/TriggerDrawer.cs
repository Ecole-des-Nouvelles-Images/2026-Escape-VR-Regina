using System.Collections;
using UnityEngine;

public class TriggerDrawer : MonoBehaviour
{
   [SerializeField] private LayerMask _layerMask;
   [SerializeField] private GameObject _drawer;
   private void OnTriggerEnter(Collider other)
   {
      if (((1 << other.gameObject.layer) & _layerMask.value) != 0)
      {
         Rigidbody rb = other.GetComponent<Rigidbody>();
         if (rb != null)
         {
            StartCoroutine(RotateToTarget(rb));
         }
         StartCoroutine(MoveToCenter(other.transform));
      }
   }

   #region Coroutines
//===================================================================================================================================================================================================
   
   IEnumerator RotateToTarget(Rigidbody rb)
   {
      float t = 0;

      Quaternion start = rb.rotation;
      Quaternion end = transform.rotation;

      while (t < 1)
      {
         t += Time.deltaTime;

         Quaternion rot = Quaternion.Lerp(start, end, t);
         rb.MoveRotation(rot);

         yield return null;
      }
   }
   IEnumerator MoveToCenter(Transform target)
   {
      float t = 0;
      Vector3 start = target.position;
      Vector3 end = transform.position;

      while (t < 1)
      {
         t += Time.deltaTime;
         target.position = Vector3.Lerp(start, end, t);
         yield return null;
      }
      
      _drawer.SetActive(true);
      Destroy(target.gameObject);
      Destroy(this.gameObject);
   }

//===================================================================================================================================================================================================
      #endregion
}