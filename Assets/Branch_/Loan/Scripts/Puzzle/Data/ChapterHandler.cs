using UnityEngine;

public class ChapterHandler : MonoBehaviour
{
    [SerializeField] private ChapterData _sceneChapter;

    private void Start()
    {
        if (PuzzleSequenceManager.Instance != null && _sceneChapter != null)
        {
            PuzzleSequenceManager.Instance.InjectCurrentChapter(_sceneChapter);
        }else
        {
            Debug.LogWarning("Manager global introuvable ou ScriptableObject manquant !");
        }
    }
}