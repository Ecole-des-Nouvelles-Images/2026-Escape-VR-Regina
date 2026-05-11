using UnityEngine;

public class HintHandler : MonoBehaviour
{
    [SerializeField] private Collider _collider;
    
    void OnTriggerEnter(Collider other)
    {
        if (other == _collider)
        {
            string hint = PuzzleSequenceManager.Instance.GiveStringHint();
            Debug.Log(hint);
        }
    }
}
