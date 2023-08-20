using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ActivatedManager : MonoBehaviour
{
    public List<ActivatorManager> activators;
    public bool move;
    public bool rotate;
    public bool scale;

    public Vector3 newMove;
    public Vector3 newRotate;
    public Vector3 newScale;

    public float duration;
    public Ease easeType;
    public float delay;

    public bool appearAtActivation;
    public GameObject objToAppear;

    public bool allActivatorsActivated;

    public bool activated;
    public bool animationComplete;

    void Start()
    {
        if (appearAtActivation)
        {
            objToAppear.SetActive(false);
        }
    }

    void Update()
    {
        foreach (ActivatorManager activator in activators)
        {
            allActivatorsActivated = activator.activated;
            if (!activator.activated)
            {
                break;
            }
        }

        if (allActivatorsActivated && !activated)
        {
            Activated();
        }
    }

    private void Activated()
    {
        if (appearAtActivation)
        {
            objToAppear.SetActive(true);
        }

        if (move)
        {
            transform.DOLocalMove(newMove, duration).SetEase(easeType).SetDelay(delay).onComplete = AnimationComplete;
        }

        if (rotate)
        {
            transform.DOLocalRotate(newRotate, duration).SetEase(easeType).SetDelay(delay).onComplete = AnimationComplete; ;
        }

        if (scale)
        {
            transform.DOScale(newScale, duration).SetEase(easeType).SetDelay(delay).onComplete = AnimationComplete; ;
        }

        activated = true;
    }

    private void AnimationComplete()
    {
        animationComplete = true;
    }
}
