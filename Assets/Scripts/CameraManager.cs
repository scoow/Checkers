using Checkers;
using System;
using System.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    bool side = true;

    public void RotateCam()
    {
        if (side)
            StartCoroutine(RotateCamera(2f, 180));
        else
            StartCoroutine(RotateCamera(2f, 0));

        side = !side;
    }

    private IEnumerator RotateCamera(float time, float angle)
    {
        var currentTime = 0f;
        GameManager.instance._rayCast.enabled = false;
        while (currentTime < time)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, angle, 0), currentTime / 20); // 0 - 1
            currentTime += Time.deltaTime;
            yield return null;
        }
        GameManager.instance._rayCast.enabled = true; 
    }
}
