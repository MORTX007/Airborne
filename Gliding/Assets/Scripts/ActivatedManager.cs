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

    private bool allActivated;

    void Start()
    {
        
    }

    void Update()
    {
        foreach (ActivatorManager activator in activators)
        {
            allActivated = activator.activated;
            if (!activator.activated)
            {
                break;
            }
        }

        if (allActivated)
        {
            Activated();
        }
    }

    private void Activated()
    {
        if (move)
        {
            transform.DOLocalMove(newMove, 1.5f).SetEase(Ease.OutBounce).SetDelay(1f);
        }

        if (rotate)
        {
            transform.DOLocalRotate(newRotate, 1.5f).SetEase(Ease.OutBounce).SetDelay(1f);
        }

        if (scale)
        {
            transform.DOScale(newScale, 1.5f).SetEase(Ease.OutBounce).SetDelay(1f);
        }
    }
}
