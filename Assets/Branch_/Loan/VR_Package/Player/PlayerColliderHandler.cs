using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerColliderHandler : MonoBehaviour
{
   [Header("General Settings")]
   [SerializeField] private Transform _targetTransform;
   [SerializeField] private CharacterController _characterController;

   [Header("Grab By Enemie Settings")]
   [SerializeField] private float _moveSpeed;
   [SerializeField] private float _rotateSpeed;
   
   [Header("Height collider Settings")]
   [SerializeField] private float _minHeight;
   [SerializeField] private float _maxHeight;

   [Header("Shake Camera Settings")] 
   [SerializeField] private float _duration;
   [SerializeField] private float _amplitudeZ;
   [SerializeField] private float _amplitudeY;

   private bool _isDeath;
   private Quaternion _initialRotation;
   private Transform _enemieTransform;

   public Transform EnemieTransform;

   // [ContextMenu("Test Event")]
   // void testEnemie()
   // {
   //    EventBus.OnEnemyAttack.Invoke(EnemieTransform);
   // }
   // private void OnEnable()
   // {
   //    EventBus.OnEnemyAttack += Death;
   // }
   //
   // private void OnDisable()
   // {
   //    EventBus.OnEnemyAttack -= Death;
   // }

   private void Death(Transform enemy)
   {
      _enemieTransform = enemy;
      _isDeath = true;
   }

   private void Update()
   {
      if (_isDeath)
      {
         MoveToEnemie();
         RotateToEnemie();

         if (transform.position == _enemieTransform.position)
         {
            StartCoroutine(ShakeCoroutine(_duration, _amplitudeZ,_amplitudeY));
         }
         return;
      }
      
      float currentHeight = (_targetTransform.position.y - transform.position.y) + 0.1f;

      float newHeight = Mathf.Clamp(currentHeight, _minHeight, _maxHeight);

      _characterController.height = newHeight;

      _characterController.center =
         new Vector3(_characterController.center.x, newHeight / 2f, _characterController.center.z);
   }

   private void MoveToEnemie()
   {
      transform.position = Vector3.MoveTowards(
         transform.position,
         _enemieTransform.position,
         _moveSpeed * Time.deltaTime);
   }

   private void RotateToEnemie()
   {
      Vector3 direction = (_enemieTransform.position - transform.position).normalized;

      if (direction == Vector3.zero) return;

      Quaternion targetRotation = Quaternion.LookRotation(direction);

      transform.rotation = Quaternion.Slerp(
         transform.rotation,
         targetRotation,
         _rotateSpeed * Time.deltaTime
      );
   }
   
   private IEnumerator ShakeCoroutine(float duration, float amplitudeZ, float amplitudeY)
   {
      _initialRotation = transform.localRotation;

      float elapsed = 0f;

      // Seeds fixes pour éviter les dérives synchronisées
      float seedZ = Random.Range(0f, 100f);
      float seedY = Random.Range(0f, 100f);

      while (elapsed < duration)
      {
         float t = elapsed / duration;
         float damper = 1f - t;

         // Perlin centré (-1 → 1)
         float noiseZ = Mathf.PerlinNoise(seedZ, Time.time * 25f);
         float centeredZ = (noiseZ - 0.5f) * 2f;

         float noiseY = Mathf.PerlinNoise(seedY, Time.time * 25f);
         float centeredY = (noiseY - 0.5f) * 2f;

         float angleZ = centeredZ * amplitudeZ * damper;
         float angleY = centeredY * amplitudeY * damper;

         transform.localRotation =
            _initialRotation *
            Quaternion.Euler(0f, angleY, angleZ);

         elapsed += Time.deltaTime;
         yield return null;
      }

      transform.localRotation = _initialRotation;
   }
   
}
