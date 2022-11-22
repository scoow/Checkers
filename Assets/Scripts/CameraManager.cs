using System.Collections;
using UnityEngine;

namespace Checkers
{
    public class CameraManager : MonoBehaviour
    {
        private bool _side = true; //в какую сторону разворачивать камеру
        RaycasterManager _raycasterManager;

        public void DisableRaycaster()
        {
            _raycasterManager.RayCasterOff();
        }
        public void EnableRaycaster()
        {
            _raycasterManager.RayCasterOn();
        }
        private void Awake()
        {
            _raycasterManager = GetComponentInChildren<RaycasterManager>();
        }
        public void RotateCamera()
        {
            if (_side)
                StartCoroutine(Rotate(2f, 180));
            else
                StartCoroutine(Rotate(2f, 0));

            _side = !_side;
        }

        private IEnumerator Rotate(float time, float angle)
        {
            DisableRaycaster();

            var currentTime = 0f;
            while (currentTime < time)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, angle, 0), currentTime / 20); // 0 - 1
                currentTime += Time.deltaTime;
                yield return null;
            }

            EnableRaycaster();
        }
    }
}
