using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class VRHandMenuController : MonoBehaviour
{
    [Header("Menu Parent (L'objet Empty)")]
    [SerializeField] private Transform _menuParentEmpty; 

    [Header("Menu Canvas (L'enfant)")]
    [SerializeField] private RectTransform _menuCanvas; 

    [Header("VR Configuration")]
    [SerializeField] private Transform _vrCamera; 
    [SerializeField] private float _spawnDistance = 1.2f; 

    [Header("Animation Settings")]
    [SerializeField] private float _duration = 0.5f;
    [SerializeField] private float _startMoveOffset = -0.15f; // Un poil plus court pour l'effet holo
    
    [Header("State")]
    private bool _isMenuOpen = false;
    private Vector3 _canvasOriginalLocalPos;
    private Vector3 _canvasOriginalScale;

    void Awake()
    {
        if (_vrCamera == null && Camera.main != null)
        {
            _vrCamera = Camera.main.transform;
        }

        _canvasOriginalLocalPos = _menuCanvas.localPosition;
        _canvasOriginalScale = _menuCanvas.localScale;
        
        ResetMenuInstant();
    }
    
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
        if (_vrCamera == null) return;

        _isMenuOpen = true;
        _menuCanvas.DOKill(true); 

        // 1. Placement du point d'ancrage de l'Hologramme
        Vector3 forwardLook = _vrCamera.forward;
        forwardLook.y = 0;
        forwardLook.Normalize();

        Vector3 targetWorldPos = _vrCamera.position + (forwardLook * _spawnDistance);
        targetWorldPos.y = 1.25f; 

        _menuParentEmpty.position = targetWorldPos;
        _menuParentEmpty.rotation = Quaternion.LookRotation(forwardLook);

        // 2. Configuration de départ de l'Hologramme (Ligne horizontale écrasée)
        _menuCanvas.localPosition = _canvasOriginalLocalPos + new Vector3(0, _startMoveOffset, 0);
        // On commence par une ligne quasi-invisible (X très fin, Y et Z à 0)
        _menuCanvas.localScale = new Vector3(_canvasOriginalScale.x * 0.05f, 0f, _canvasOriginalScale.z);

        _menuParentEmpty.gameObject.SetActive(true);

        // 3. SEQUENCE D'ANIMATION HOLOGRAPHIQUE
        // A) Mouvement de montée avec un effet élastique / magnétique
        _menuCanvas.DOLocalMove(_canvasOriginalLocalPos, _duration)
            .SetEase(Ease.OutElastic, 0.7f, 0.6f)
            .SetUpdate(true); 

        // B) Étape 1 du scale : Déploiement de la ligne horizontale d'abord (Flash d'activation)
        _menuCanvas.DOScaleX(_canvasOriginalScale.x, _duration * 0.4f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                // Étape 2 du scale : Le panneau se déploie verticalement d'un coup (comme un vieil écran qui s'allume)
                _menuCanvas.DOScaleY(_canvasOriginalScale.y, _duration * 0.6f)
                    .SetEase(Ease.OutBounce) // L'effet Bounce simule le "glitch" de stabilisation de la mémoire
                    .SetUpdate(true);
            });

        // C) Effet de scintillement (Glitch de mémoire)
        // On fait vibrer très légèrement la position locale pour simuler l'instabilité d'un hologramme mémoriel
        _menuCanvas.DOShakePosition(_duration, new Vector3(0.02f, 0.02f, 0f), 15, 90f, false, false)
            .SetUpdate(true);
    }

    public void CloseMenu()
    {
        _isMenuOpen = false;
        _menuCanvas.DOKill();
    
        // 1. EFFET DE SÉQUENCE : Crash de l'hologramme
        // On commence par un gros glitch (Secousse violente) pour simuler la perte de signal mémoriel
        _menuCanvas.DOShakePosition(_duration * 0.3f, new Vector3(0.05f, 0.03f, 0f), 30, 90f, false, false)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                // 2. IMPLOSION : Une fois le signal perdu, le panneau s'écrase instantanément sur l'axe Y (il devient une ligne plate)
                _menuCanvas.DOScaleY(0.01f, _duration * 0.3f)
                    .SetEase(Ease.InExpo) // Chute ultra rapide
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        // 3. PIXEL MORT : La ligne s'effondre sur elle-même horizontalement à toute vitesse
                        _menuCanvas.DOScaleX(0f, _duration * 0.4f)
                            .SetEase(Ease.InCirc)
                            .SetUpdate(true)
                            .OnComplete(() => 
                            {
                                // Tout est éteint, on désactive l'Empty
                                _menuParentEmpty.gameObject.SetActive(false);
                            });
                    });
            });

        // 4. MOUVEMENT DE RECUL : Pendant qu'il glitch et s'effondre, l'hologramme est aspiré vers l'arrière / s'enfonce
        // Ça donne un effet de profondeur (implosion 3D) au lieu d'un simple affaissement vers le bas
        Vector3 targetBackPos = _canvasOriginalLocalPos + new Vector3(0, _startMoveOffset * 0.3f, 0.2f); // Recule de 0.2 sur l'axe Z
        _menuCanvas.DOLocalMove(targetBackPos, _duration)
            .SetEase(Ease.InCubic)
            .SetUpdate(true);
    }

    private void ResetMenuInstant()
    {
        _menuCanvas.localScale = Vector3.zero;
        _menuCanvas.localPosition = _canvasOriginalLocalPos + new Vector3(0, _startMoveOffset, 0);
        _menuParentEmpty.gameObject.SetActive(false);
    }
}