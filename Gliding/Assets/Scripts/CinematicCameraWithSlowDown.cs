using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicCameraWithSlowDown : MonoBehaviour
{
    public float speed;

    public AnimationCurve curve;

    public float multiplier;
    float time;

    Vector3 startRotation;

    public Transform models;
    int modelIndex = 0;

    private void Start()
    {
        startRotation = transform.rotation.eulerAngles;
    }

    void Update()
    {
        multiplier = curve.Evaluate(time);

        transform.Rotate(0, speed * multiplier * Time.deltaTime, 0);

        time += Time.deltaTime;

        if (time >= 2.8f)
        {
            models.GetChild(modelIndex).gameObject.SetActive(false);
            modelIndex++;
            if (modelIndex >= models.childCount)
            {
                modelIndex = 0;
            }
            models.GetChild(modelIndex).gameObject.SetActive(true);

            transform.rotation = Quaternion.Euler(startRotation);
            time = 0;
        }
    }
}