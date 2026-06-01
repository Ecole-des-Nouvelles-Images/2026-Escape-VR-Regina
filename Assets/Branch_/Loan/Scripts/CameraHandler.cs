using DG.Tweening;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private RectTransform _upPanel;
    [SerializeField] private RectTransform _downPanel;
    [SerializeField] private CanvasGroup _fadePanel;
    [SerializeField] private float _fadeDuration;
    
    [Header("Blink Settings")]
    [SerializeField] private float _closedOffset = 300f;
    
    [Header("Human Blink Randomization")]
    [SerializeField] private Vector2 _blinkIntervalRange = new Vector2(2f, 6f);
    [SerializeField] private Vector2 _blinkDurationRange = new Vector2(0.08f, 0.18f);
    [SerializeField] private float _microDelay = 0.03f;

    private Vector2 _upStartPos;
    private Vector2 _downStartPos;

    private Tween _upTween;
    private Tween _downTween;

    private int _collisionCount = 0;
    private Tween _fadeTween;

    
    private void Awake()
    {
        _upStartPos = _upPanel.anchoredPosition;
        _downStartPos = _downPanel.anchoredPosition;
        EventBus.OnCloseEyes += Blink;
        EventBus.OnOpenEyes += OpenEyes;
    }

    private void OnDestroy()
    {
        EventBus.OnCloseEyes -= Blink;
        EventBus.OnOpenEyes -= OpenEyes;
    }
    
    // CALL THIS FROM EVENT (scene change)
    public void PlaySceneBlink()
    {
        Blink();
    }

    #region BLINK CORE
//=========================================================================================================================================================================================================
    [ContextMenu("Blink")]
    public void Blink()
    {
        float duration = Random.Range(_blinkDurationRange.x, _blinkDurationRange.y);

        _upTween?.Kill();
        _downTween?.Kill();

        // fermeture
        _upTween = _upPanel
            .DOAnchorPosY(_upStartPos.y - _closedOffset, duration)
            .SetEase(Ease.InOutSine);

        _downTween = _downPanel
            .DOAnchorPosY(_downStartPos.y + _closedOffset, duration)
            .SetEase(Ease.InOutSine)
            .SetDelay(_microDelay);
    }

    [ContextMenu("Open Eyes")]
    public void OpenEyes()
    {
        float duration = Random.Range(_blinkDurationRange.x, _blinkDurationRange.y);

        _upTween?.Kill();
        _downTween?.Kill();

        _upTween = _upPanel
            .DOAnchorPos(_upStartPos, duration)
            .SetEase(Ease.InOutSine);

        _downTween = _downPanel
            .DOAnchorPos(_downStartPos, duration)
            .SetEase(Ease.InOutSine)
            .SetDelay(_microDelay);
    }
//=========================================================================================================================================================================================================
    #endregion
    private void SetFade(bool isFaded)
    {
        _fadeTween?.Kill();
        
        _fadeTween = _fadePanel.DOFade(isFaded ? 1f : 0, _fadeDuration).SetEase(Ease.InOutQuad);
    }

    #region Trigger 
//=========================================================================================================================================================================================================

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            return;
        
        if (other.CompareTag("Test"))
        {
            Blink();
            return;
        }
        
        if (!other.isTrigger)
        {
            _collisionCount++;
            SetFade(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            return;
        
        if (other.CompareTag("Test"))
        {
            OpenEyes();
            return;
        }
        
        if (!other.isTrigger)
        {
            _collisionCount = Mathf.Max(0, _collisionCount - 1);

            if (_collisionCount == 0)
                SetFade(false);
        }
    }

//=========================================================================================================================================================================================================
    #endregion
}
