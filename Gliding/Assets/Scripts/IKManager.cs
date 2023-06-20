using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class IKManager : MonoBehaviour
{
    [Header("Basic IK Info")]
    public int chainLen;
    public Transform target;
    public Transform pole;

    [Header("Solving Parameters")]
    public int iterations = 10;
    public float delta = 0.001f;

    private float[] bonesLen;
    public float completeLen;
    private Transform[] bones;
    private Vector3[] positions;
    private Vector3[] startDirSucc;
    private Quaternion[] startRotBone;
    private Quaternion startRotTarget;

    [Header("Gizmos")]
    public bool legIKWire;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        bones = new Transform[chainLen + 1];
        positions = new Vector3[chainLen + 1];
        bonesLen = new float[chainLen];
        startDirSucc = new Vector3[chainLen + 1];
        startRotBone = new Quaternion[chainLen + 1];

        startRotTarget = target.rotation;
        completeLen = 0;

        // set bones and bone lengths
        var current = transform;
        for (var i = bones.Length - 1; i >= 0; i--)
        {
            bones[i] = current;
            startRotBone[i] = current.rotation;

            // leaf bone
            if (i == bones.Length - 1)
            {
                startDirSucc[i] = target.position - current.position;
            }
            // mid bone
            else
            {
                startDirSucc[i] = bones[i + 1].position - current.position;
                bonesLen[i] = (bones[i + 1].position - current.position).magnitude;
                completeLen += bonesLen[i];
            }

            current = current.parent;
        }
    }

    private void LateUpdate()
    {
        ResolveIK();
    }

    private void ResolveIK()
    {
        if (target == null)
        {
            return;
        }

        if (bonesLen.Length != chainLen)
        {
            Init();
        }

        // get positions
        for (int i = 0; i < bones.Length; i++)
        {
            positions[i] = bones[i].position;
        }

        var RootRot = (bones[0].parent != null) ? bones[0].parent.rotation : Quaternion.identity;
        // is target out of reach
        if ((target.position - bones[0].position).sqrMagnitude >= completeLen * completeLen)
        {
            var dir = (target.position - positions[0]).normalized;
            for (int i = 1; i < positions.Length; i++)
            {
                positions[i] = positions[i - 1] + dir * bonesLen[i - 1];
            }
        }
        else
        {
            // backward iteration
            for (int i = positions.Length - 1; i > 0; i--)
            {
                if (i == positions.Length - 1)
                {
                    // set last point to target
                    positions[i] = target.position;
                }
                else
                {
                    // set point to correct vector away from next point
                    positions[i] = positions[i + 1] + (positions[i] - positions[i + 1]).normalized * bonesLen[i];
                }
            }

            // forward iteration
            for (int i = 1; i < positions.Length; i++)
            {
                // set point to correct vector away from last point
                positions[i] = positions[i - 1] + (positions[i] - positions[i - 1]).normalized * bonesLen[i - 1];
            }

            for (int i = 0; i < iterations; i++)
            {
                // is the position close enough
                if ((positions[positions.Length - 1] - target.position  ).sqrMagnitude < delta * delta)
                {
                    break;
                }

            }
        }

        // move towards pole
        if (pole != null)
        {
            for (int i = 1; i < positions.Length - 1; i++)
            {
                var plane = new Plane(positions[i + 1] - positions[i - 1], positions[i - 1]);
                var projectedPole = plane.ClosestPointOnPlane(pole.position);
                var projectedBone = plane.ClosestPointOnPlane(positions[i]);
                var angle = Vector3.SignedAngle(projectedBone - positions[i - 1], projectedPole - positions[i - 1], plane.normal);
                positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (positions[i] - positions[i - 1]) + positions[i - 1];
            }
        }

        // set positions and rotations
        for (int i = 0; i < positions.Length; i++)
        {
            if (i == positions.Length - 1)
            {
                bones[i].rotation = target.rotation * Quaternion.Inverse(startRotTarget) * startRotBone[i];
            }
            else
            {
                bones[i].rotation = Quaternion.FromToRotation(startDirSucc[i], positions[i + 1] - positions[i]) * startRotBone[i];
            }

            bones[i].position = positions[i];
        }
    }

    private void OnDrawGizmos()
    {
        if (legIKWire)
        {
            var current = this.transform;
            for (int i = 0; i < chainLen && current != null && current.parent != null; i++)
            {
                var scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
                //Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
                //Handles.color = Color.white;
                //Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
                current = current.parent;
            }
        }
    }
}
