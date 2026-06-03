using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization; // N'oublie pas d'importer DOTween !

public class VRHandMenuController : MonoBehaviour
{
    [FormerlySerializedAs("menuTransform")]
    [Header("Menu Canvas / Mesh")]
    [SerializeField] private RectTransform _menuTransform; // Ou Transform classique si ce n'est pas un Canvas

    [FormerlySerializedAs("duration")]
    [Header("Animation Settings")]
    [SerializeField] private float _duration = 0.4f;
    [FormerlySerializedAs("startMoveOffset")] [SerializeField] private float _startMoveOffset = -0.3f; 
    
    [Header("State")]
    private bool _isMenuOpen = false;
    private Vector3 _originalPosition;
    private Vector3 _originalScale;

    void Awake()
    {
       
        _originalPosition = _menuTransform.localPosition;
        _originalScale = _menuTransform.localScale;
        
        ResetMenuInstant();
    }
    
    /// <summary>
    /// Fonction principale à appeler (via input VR ou bouton)
    /// </summary>
    [ContextMenu("MenuToggle")]
    public void ToggleMenu()
    {
        if (_isMenuOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    public void OpenMenu()
    {
        _isMenuOpen = true;
        _menuTransform.gameObject.SetActive(true);

       
        _menuTransform.localPosition = _originalPosition + new Vector3(0, _startMoveOffset, 0);
        _menuTransform.localScale = Vector3.zero;

       
        _menuTransform.DOKill();

       
        _menuTransform.DOLocalMove(_originalPosition, _duration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true); 

        
        _menuTransform.DOScale(_originalScale, _duration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }

    public void CloseMenu()
    {
        _isMenuOpen = false;

        _menuTransform.DOKill();
        
        _menuTransform.DOLocalMove(_originalPosition + new Vector3(0, _startMoveOffset, 0), _duration * 0.75f)
            .SetEase(Ease.InCubic)
            .SetUpdate(true);

        _menuTransform.DOScale(Vector3.zero, _duration * 0.75f)
            .SetEase(Ease.InCubic)
            .SetUpdate(true)
            .OnComplete(() => 
            {
                _menuTransform.gameObject.SetActive(false);
            });
    }

    private void ResetMenuInstant()
    {
        _menuTransform.localScale = Vector3.zero;
        _menuTransform.localPosition = _originalPosition + new Vector3(0, _startMoveOffset, 0);
        _menuTransform.gameObject.SetActive(false);
    }
}