using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LockManager : Puzzle
{
    [Header("===== Puzzle Settings =====")]

    [SerializeField] private List<int> _values = new();
    [SerializeField] private List<int> _codes = new();

    [Header("===== Inspect Settings =====")]

    [SerializeField] private Transform _inspectPoint;
    [SerializeField] private float _inspectScaleMultiplier = 2f;
    [SerializeField] private bool _useStartPos;
    
    [Header("===== Solve Settings =====")]
    [SerializeField] private GameObject _upLock;
    [SerializeField] private float _duration = 0.5f;

    private XRGrabInteractable _grabInteractable;

    private Rigidbody _rb;
    [Header("===== Debugs Settings =====")]
    [SerializeField]private Vector3 _startPosition;
    [SerializeField]private Quaternion _startRotation;
    [SerializeField]private Vector3 _startScale;
    [SerializeField]private Transform _startParent;

    private bool _isInspecting;
    private bool _isUnlocked;

    private void Start()
    {
        EventBus.OnResendCode += GetValue;
        while (_values.Count < _codes.Count)
        {
            _values.Add(0);
        }

        while (_values.Count > _codes.Count)
        {
            _values.RemoveAt(_values.Count - 1);
        }

        _grabInteractable = GetComponent<XRGrabInteractable>();
        _rb = GetComponent<Rigidbody>();
        
        _grabInteractable.selectEntered.AddListener(OnGrab);
        _grabInteractable.selectExited.AddListener(OnRelease);

        _startScale = transform.localScale;
        
        if (_useStartPos)
        {
            _startPosition = transform.localPosition;
            _startRotation = transform.localRotation;
            _startParent = transform.parent;
        }
        _inspectPoint = GameObject.FindWithTag("InspectPoints").GetComponent<Transform>();
    }

    #region Puzzle
//===================================================================================================================================================================================================
    public void GetValue(int value, int index)
    {
        if (index < 0 || index >= _values.Count)
            return;

        _values[index] = value;

        CheckCode();
    }

    private void CheckCode()
    {
        Debug.Log(PuzzleSequenceManager.Instance.CurrentPuzzle);
        if (PuzzleSequenceManager.Instance.CurrentPuzzle.PuzzleID != PuzzleID)
            return;
        
        for (int i = 0; i < _codes.Count; i++)
        {
            if (_values[i] != _codes[i])
                return;
        }

        Solve();
    }

    public override void Solve()
    {
        base.Solve();
        _isUnlocked = true;
        StartCoroutine(AnimateUpLock());
    }
    
    private IEnumerator AnimateUpLock()
    {
        if (_upLock == null) yield break;

        // 1. On crée une Séquence DOTween
        Sequence lockSequence = DOTween.Sequence();

        // 2. On calcule nos cibles locales
        Vector3 targetPosition = _upLock.transform.localPosition + new Vector3(0, 0.02f, 0);
        // Avec DOTween, on passe directement les angles en Vector3 pour la rotation, c'est plus simple !
        Vector3 targetRotation = _upLock.transform.localEulerAngles + new Vector3(0, 50f, 0);

        // 3. On ajoute toutes les animations en MÊME TEMPS dans la séquence (via .Join)
        // .SetEase(Ease.InOutQuad) ajoute automatiquement le lissage parfait pour la VR
        lockSequence.Join(_upLock.transform.DOLocalMove(targetPosition, _duration).SetEase(Ease.InOutQuad));
        lockSequence.Join(_upLock.transform.DOLocalRotate(targetRotation, _duration).SetEase(Ease.InOutQuad));
        
        // On ajoute le petit tremblement mécanique (shake) pendant la même durée
        lockSequence.Join(gameObject.transform.DOShakeRotation(_duration, new Vector3(5f, 0f, 5f), 10));
        gameObject.transform.DOPunchPosition(new Vector3(0, 0.01f, 0), 0.3f, 5, 1f);

        // 4. On attend que la séquence entière se termine
        // ( lockSequence.WaitForCompletion() est un utilitaire DOTween magique pour les coroutines )
        yield return lockSequence.WaitForCompletion();


        // ==========================================
        // ANIMATION DE FIN : DISPARITION
        // ==========================================
        
        // Le cadenas rétrécit proprement avant de mourir
        yield return gameObject.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).WaitForCompletion();

        // Fin de l'action
        EventBus.OnOpenChest?.Invoke();

        Destroy(gameObject);
    }
//===================================================================================================================================================================================================
    #endregion

    #region XR Interaction
//===================================================================================================================================================================================================
    private void OnGrab(SelectEnterEventArgs args)
    {
        if (_isInspecting) return;
        _isInspecting = true;
        
        _rb.isKinematic = true;
        _rb.useGravity = false;
        
        transform.SetParent(_inspectPoint);
        
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        _grabInteractable.trackPosition = false;
        _grabInteractable.trackRotation = false;
        _inspectPoint.localScale = Vector3.one * _inspectScaleMultiplier;

        EventBus.OnGrabLock?.Invoke();
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (!_isInspecting || _isUnlocked)
            return;

        _isInspecting = false;

        // 1. On remet le parent d'origine d'abord
        transform.SetParent(_startParent);
        transform.localScale = _startScale; // Assure-toi de l'avoir setup dans le Start() !

        if (_useStartPos)
        {
            // 2. On replace le transform
            transform.localPosition = _startPosition;
            transform.localRotation = _startRotation;

            // 3. SECURITÉ PHYSIQUE : On stoppe net toutes les forces accumulées pendant le Grab
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        
            // On force le moteur physique à s'aligner sur la position initiale
            _rb.position = transform.position;
            _rb.rotation = transform.rotation;
        }

        // 4. On réactive la physique
        _rb.isKinematic = false;
        _rb.useGravity = true;
    
        // 5. On rend le contrôle au XR Toolkit
        _grabInteractable.trackPosition = true;
        _grabInteractable.trackRotation = true;
    
        _inspectPoint.localScale = Vector3.one;
        EventBus.OnReleaseLock?.Invoke();
    }
//===================================================================================================================================================================================================
    #endregion
}