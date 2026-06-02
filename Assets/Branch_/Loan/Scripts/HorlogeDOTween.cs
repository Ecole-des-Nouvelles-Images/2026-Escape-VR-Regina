using System;
using UnityEngine;
using DG.Tweening; // Requis pour DOTween

public class HorlogeDOTweenLocalZ : MonoBehaviour
{
    [Header("Aiguilles (Transforms)")]
    public Transform aiguilleHeures;
    public Transform aiguilleMinutes;
    public Transform aiguilleSecondes;

    [Header("Réglages DOTween")]
    public Ease typeTransitionSecondes = Ease.OutBack;

    private int derniereSeconde = -1;

    void Start()
    {
        // Initialisation instantanée au démarrage
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

        // Angles cibles (360° / unité)
        float cibleSecondes = tempsActuel.Second * 6f;
        float cibleMinutes = (tempsActuel.Minute * 6f) + (tempsActuel.Second * 0.1f);
        float cibleHeures = (tempsActuel.Hour % 12 * 30f) + (tempsActuel.Minute * 0.5f);

        if (instantane)
        {
            // Application immédiate sur le Z local (en gardant X et Y locaux d'origine)
            AppliquerRotationZLocal(aiguilleSecondes, cibleSecondes, 0, Ease.Linear);
            AppliquerRotationZLocal(aiguilleMinutes, cibleMinutes, 0, Ease.Linear);
            AppliquerRotationZLocal(aiguilleHeures, cibleHeures, 0, Ease.Linear);
        }
        else
        {
            // Animations fluides DOTween sur le Z local
            AppliquerRotationZLocal(aiguilleSecondes, cibleSecondes, 0.3f, typeTransitionSecondes);
            AppliquerRotationZLocal(aiguilleMinutes, cibleMinutes, 0.5f, Ease.InOutQuad);
            AppliquerRotationZLocal(aiguilleHeures, cibleHeures, 0.5f, Ease.InOutQuad);
        }
    }

    void AppliquerRotationZLocal(Transform aiguille, float angleZ, float duree, Ease typeEase)
    {
        if (aiguille == null) return;

        // On stoppe le tween précédent pour éviter les conflits
        aiguille.DOKill();

        // On prépare le Vector3 en modifiant UNIQUEMENT le Z local
        Vector3 rotationCible = new Vector3(aiguille.localEulerAngles.x, aiguille.localEulerAngles.y, angleZ);

        if (duree <= 0)
        {
            aiguille.localEulerAngles = rotationCible;
        }
        else
        {
            // DOLocalRotate applique l'animation dans le repère de l'objet (Local)
            // RotateMode.FastBeyond360 gère proprement le passage de 359° à 0°
            aiguille.DOLocalRotate(rotationCible, duree, RotateMode.FastBeyond360)
                    .SetEase(typeEase);
        }
    }
}