using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralWalkManager : MonoBehaviour
{
    public Transform obj;

    [Header("Legs Info")]
    public Transform[] legTargets;
    public Transform[] legPosPredictors;
    public float[] legStepDistances;
    public float stepDistance;
    public float stepSpeed;
    public float stepHeight;
    public float footElevation;
    public LayerMask walkLayerMask;
    private Vector3[] legTargetPositions;
    private Vector3[] legTargetNewPositions;

    [Header("Body Info")]
    public Transform body;
    public float bodyRotSmooth;

    [Header("Root Info")]
    public float rootLiftSmooth;

    private void Start()
    {
        // initialize private lists
        legTargetPositions = new Vector3[legTargets.Length];
        legTargetNewPositions = new Vector3[legTargets.Length];

        for (int i = 0; i < legTargets.Length; i++)
        {
            legTargetPositions[i] = legTargets[i].position + footElevation * Vector3.up;
            legTargetNewPositions[i] = legTargetPositions[i];
        }
    }

    private void Update()
    {
        // move and interpolate legs when predictor is far enough
        CheckAndRepositionTarget();

        // fix feet to the target
        for (int i = 0; i < legTargets.Length; i++)
        {
            legTargets[i].position = legTargetPositions[i];
        }
    }

    private void CheckAndRepositionTarget()
    {
        // run for every leg
        for (int i = 0; i < legTargets.Length; i++)
        {
            var ray = new Ray(legPosPredictors[i].position, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 20f, walkLayerMask))
            {
                // if legs are far enough from check
                if (Vector3.Distance(legTargetPositions[i], hit.point) > legStepDistances[i])
                {
                    legTargetNewPositions[i] = hit.point + footElevation * Vector3.up;
                }
            }

            // if current leg target position is not new position then interpolate the leg
            if (legTargetPositions[i] != legTargetNewPositions[i])
            {
                InterpolateLeg(i, legTargetNewPositions[i]);
            }

            RepositionRoot();
            RotateBody();
        }
    }

    private void InterpolateLeg(int i, Vector3 newPos)
    {
        // lerp leg target towards new target position
        legTargetPositions[i] = Vector3.MoveTowards(legTargetPositions[i], newPos, stepSpeed * Time.deltaTime);

        // lift leg when moving
        legTargetPositions[i].y += -Mathf.Cos((legTargetPositions[i].magnitude / legTargetNewPositions[i].magnitude) * Mathf.PI) * stepHeight;
    }

    private void RepositionRoot()
    {
        // apply leg height to root
        var avgNewLegsPos = Vector3.zero;
        foreach (Vector3 pos in legTargetNewPositions)
        {
            avgNewLegsPos += pos;
        }
        avgNewLegsPos /= legTargetNewPositions.Length;
        transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, avgNewLegsPos.y + 0.7f, transform.position.z), rootLiftSmooth);
    }

    private void RotateBody()
    {
        if (legTargets.Length == 4)
        {
            // make 2 planes and find normal of average of planes
            var plane1 = new Plane(legTargetPositions[0], legTargetPositions[2], legTargetPositions[3]);
            var plane2 = new Plane(legTargetPositions[1], legTargetPositions[2], legTargetPositions[3]);
            var avgNormal = (plane1.normal + plane2.normal) / 2;

            // apply the normal to body to change its rotation
            body.up = Vector3.Lerp(body.up, avgNormal, bodyRotSmooth);
            body.eulerAngles = new Vector3(-body.eulerAngles.x, obj.eulerAngles.y, body.eulerAngles.z);
        }
    }
}
