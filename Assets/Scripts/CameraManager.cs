using System.Collections;
using UnityEngine;

namespace Checkers
{
    public class CameraManager : MonoBehaviour
    {
        private bool _side = true;

        public void RotateCam()
        {
            if (_side)
                StartCoroutine(RotateCamera(2f, 180));
            else
                StartCoroutine(RotateCamera(2f, 0));

            _side = !_side;
        }

        private IEnumerator RotateCamera(float time, float angle)
        {
            var currentTime = 0f;
            //GameManager.instance.RayCaster.enabled = false;//////////////////////////

            while (currentTime < time)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, angle, 0), currentTime / 20); // 0 - 1
                currentTime += Time.deltaTime;
                yield return null;
            }
            //GameManager.instance.RayCaster.enabled = true;
        }
    }
}
