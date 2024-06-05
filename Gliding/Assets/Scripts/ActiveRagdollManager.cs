using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveRagdollManager : MonoBehaviour
{
    [Header("Matching Animation")]
    public ConfigurableJoint[] physicalJoints;
    public Transform[] animatedJoints;
    private Quaternion[] startJointRots;

    void Start()
    {
        startJointRots = new Quaternion[physicalJoints.Length];
        for (int i = 0; i < physicalJoints.Length; i++)
        {
            startJointRots[i] = physicalJoints[i].transform.localRotation;
        }
    }

    void Update()
    {
        // matching Animation
        for (int i = 0; i < physicalJoints.Length; i++)
        {
            ConfigurableJointExtensions.SetTargetRotationLocal(physicalJoints[i], animatedJoints[i].localRotation, startJointRots[i]);
        }
    }
}
