using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class WheelHandler : MonoBehaviour
{
    [Header("===== References =====")]
    [SerializeField] private int _index;
    [SerializeField] private Transform _visual;
    
    [Header("===== Settings =====")]
    [SerializeField] private float _sensitivity = 5f;

    private float _currentAngle;
    private int _lastValue = -1;
    private int _currentValue;
    private Vector3 _lastFingerPosition;
    private Transform _activeFinger;
    private bool _isInteracting;
    private bool _isGrabbing;
    [SerializeField]private Transform _inspectTransform;

    // VARIABLE PRO : On stocke l'axe de déplacement pour toute la durée de cette interaction
    [SerializeField] private Vector3 _interactionAxis;

    // SÉCURITÉ DOTWEEN : On stocke le scale de départ pour éviter les déformations en boucle
    private Vector3 _startScale;
    
    #region Trigger
    private void OnTriggerEnter(Collider other)
    {
        if (!_isGrabbing)
            return;

        if (other.CompareTag("FingerTip"))
        {
            // Sécurité : On remet le scale normal avant de tuer le tween en cours
            _visual.localScale = _startScale;
            _visual.DOKill();
            
            _activeFinger = other.transform;
            _lastFingerPosition = _activeFinger.position;
            
            // LOGIQUE PRO : On fige l'axe X local actuel (axe rouge) de la molette en coordonées World.
            // Comme on le stocke ici, même si la molette tourne au frame suivant, notre axe de référence reste fixe !
            _interactionAxis = _inspectTransform.right; 

            _isInteracting = true;
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("FingerTip"))
        {
            _isInteracting = false;
            _activeFinger = null;
            SnapToValue(); // Envoie le code uniquement ici au moment du lâcher
        }
    }
    #endregion

    private void Start()
    {
        // On sauvegarde le vrai scale d'origine défini dans l'inspecteur
        _startScale = _visual.localScale;
        _inspectTransform = GameObject.Find("InspectPoints").transform;
        _visual.DOKill();
    }

    private void Update()
    {
        if (!_isInteracting || _activeFinger == null) return;

        Vector3 currentFingerPos = _activeFinger.position;

        // Au lieu de faire un calcul World, on traduit la position du doigt dans l'espace LOCAL de la molette
        Vector3 localCurrentPos = transform.InverseTransformPoint(currentFingerPos);
        Vector3 localLastPos = transform.InverseTransformPoint(_lastFingerPosition);

        // Maintenant, on fait une simple soustraction sur l'axe X local. 
        // C'est 100% fiable, peu importe l'orientation de l'objet dans l'espace !
        float localDeltaX = localCurrentPos.x - localLastPos.x;

        // Indépendance du framerate (basé sur ~90 FPS de référence pour garder ta sensibilité d'origine)
        float frameIndependentDelta = localDeltaX * (Time.deltaTime * 90f);

        // Rotation
        _currentAngle += frameIndependentDelta * _sensitivity * 360f;
        _visual.localRotation = Quaternion.Euler(0f, -_currentAngle, 0f);

        // Calcul de la valeur en continu (0 à 9)
        _currentValue = (10 - (Mathf.RoundToInt(Mathf.Repeat(_currentAngle, 360f) / 36f) % 10)) % 10;

        if (_currentValue != _lastValue)
        { 
            _lastValue = _currentValue;
            
            // ==========================================
            // EFFET 1 : LE CLIQUETIS (PUNCH ROTATION)
            // ==========================================
            // Au lieu de toucher à la POSITION (qui décalait ta roulette en Z), 
            // on fait un punch de ROTATION sur l'axe Y. La roulette va "sauter" d'un coup sec.
            _visual.DOPunchRotation(new Vector3(0f, 15f, 0f), 0.08f, 5, 1f);
        }
        
        _lastFingerPosition = currentFingerPos;
    }

    private void SnapToValue()
    {
        // Sécurité pour s'assurer que _lastValue ne vaut pas -1 au premier clic
        if (_lastValue == -1) _lastValue = _currentValue;
        float snappedAngle = (10 - _lastValue) % 10 * 36f;
        _currentAngle = snappedAngle;
        
        // Sécurité : On remet le scale normal avant de relancer l'enchaînement de Tweens
        _visual.localScale = _startScale;
        _visual.DOKill();
        
        _visual.DOLocalRotate(new Vector3(0f, -snappedAngle, 0f), 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => 
            {
                // ==========================================
                // EFFET 3 : L'ENCLENCHEMENT (PUNCH SCALE SÉCURISÉ)
                // ==========================================
                // On fait un punch d'échelle UNIQUEMENT sur X et Z pour simuler le "clac".
                // Si ton pivot bouge encore, c'est que le centre (Center/Pivot) de ton modèle 3D 
                // dans Unity n'est pas bien aligné au milieu de la roulette.
                _visual.DOPunchScale(new Vector3(0.08f, 0f, 0.08f), 0.12f, 8, 1f);
            });
        // _visual.localRotation = Quaternion.Euler(0f, -snappedAngle, 0f);
        
        // C'est bien ici et seulement ici que le code est envoyé
        EventBus.OnResendCode?.Invoke(_lastValue, _index);
    }

    public void IsGrabLock() => _isGrabbing = true;
    public void IsReleaseGrab() 
    {
        _isGrabbing = false;
        if (_isInteracting)
        {
            _isInteracting = false;
            _activeFinger = null;
            SnapToValue();
        }
    }
}