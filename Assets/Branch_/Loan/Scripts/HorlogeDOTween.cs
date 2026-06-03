using System;
using UnityEngine;
using DG.Tweening; // Requis pour DOTween

public class HorlogeDOTweenLocalZ : MonoBehaviour
{
    [Header("Aiguilles (Transforms)")]
    public Transform aiguilleHeures;
    public Transform aiguilleMinutes;
    public Transform aiguilleSecondes;

    [Header("Réglages Horloge")]
    [Tooltip("Cochez cette case pour que l'horloge tourne à l'envers")]
    public bool sensInverse = false;

    [Header("Réglages DOTween")]
    public Ease typeTransitionSecondes = Ease.OutBack;

    private int derniereSeconde = -1;

    // Variables pour stocker la rotation Z de départ (l'offset)
    private float offsetZHeures;
    private float offsetZMinutes;
    private float offsetZSecondes;

    void Start()
    {
        // 1. On sauvegarde la rotation locale d'origine (en Z) réglée dans l'éditeur
        if (aiguilleHeures != null) offsetZHeures = aiguilleHeures.localEulerAngles.z;
        if (aiguilleMinutes != null) offsetZMinutes = aiguilleMinutes.localEulerAngles.z;
        if (aiguilleSecondes != null) offsetZSecondes = aiguilleSecondes.localEulerAngles.z;

        // 2. Initialisation au démarrage (en instantané pour caler l'horloge direct)
        MettreAJourHorloge(instantane: true);
    }

    void Update()
    {
        // On ne déclenche l'animation que lorsque la seconde change
        if (DateTime.Now.Second != derniereSeconde)
        {
            derniereSeconde = DateTime.Now.Second;
            MettreAJourHorloge(instantane: false);
        }
    }

    void MettreAJourHorloge(bool instantane)
    {
        DateTime tempsActuel = DateTime.Now;

        // On détermine le multiplicateur de direction (1 si normal, -1 si inversé)
        float multiplicateurSens = sensInverse ? -1f : 1f;

        // On calcule l'angle de base selon le temps, on applique le sens, puis on ajoute l'offset d'origine
        float angleSecondes = (tempsActuel.Second * 6f) * multiplicateurSens + offsetZSecondes;
        float angleMinutes = ((tempsActuel.Minute * 6f) + (tempsActuel.Second * 0.1f)) * multiplicateurSens + offsetZMinutes;
        float angleHeures = ((tempsActuel.Hour % 12 * 30f) + (tempsActuel.Minute * 0.5f)) * multiplicateurSens + offsetZHeures;

        // On utilise Mathf.Repeat pour que l'angle reste strictement entre 0 et 360° (évite le tour complet de DOTween)
        float cibleSecondes = Mathf.Repeat(angleSecondes, 360f);
        float cibleMinutes = Mathf.Repeat(angleMinutes, 360f);
        float cibleHeures = Mathf.Repeat(angleHeures, 360f);

        if (instantane)
        {
            AppliquerRotationZLocal(aiguilleSecondes, cibleSecondes, 0, Ease.Linear);
            AppliquerRotationZLocal(aiguilleMinutes, cibleMinutes, 0, Ease.Linear);
            AppliquerRotationZLocal(aiguilleHeures, cibleHeures, 0, Ease.Linear);
        }
        else
        {
            AppliquerRotationZLocal(aiguilleSecondes, cibleSecondes, 0.3f, typeTransitionSecondes);
            AppliquerRotationZLocal(aiguilleMinutes, cibleMinutes, 0.5f, Ease.InOutQuad);
            AppliquerRotationZLocal(aiguilleHeures, cibleHeures, 0.5f, Ease.InOutQuad);
        }
    }

    void AppliquerRotationZLocal(Transform aiguille, float angleZ, float duree, Ease typeEase)
    {
        if (aiguille == null) return;

        // On stoppe l'animation en cours pour éviter les conflits
        aiguille.DOKill();

        // On prépare le Vector3 final
        Vector3 rotationCible = new Vector3(aiguille.localEulerAngles.x, aiguille.localEulerAngles.y, angleZ);

        if (duree <= 0)
        {
            aiguille.localEulerAngles = rotationCible;
        }
        else
        {
            // FastBeyond360 permet d'avoir une transition fluide quand on passe de 359° à 0° (ou de 0° à 359°)
            aiguille.DOLocalRotate(rotationCible, duree, RotateMode.FastBeyond360)
                    .SetEase(typeEase);
        }
    }
}