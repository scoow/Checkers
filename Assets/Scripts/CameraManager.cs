using System.Collections;
using UnityEngine;

namespace Checkers
{
    public class CameraManager : MonoBehaviour
    {
        private bool _side = true; //в какую второну разворачивать камеру
        RaycasterManager _raycasterManager;

        private void Start()
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
            _raycasterManager.RayCasterOff();

            var currentTime = 0f;
            while (currentTime < time)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, angle, 0), currentTime / 20); // 0 - 1
                currentTime += Time.deltaTime;
                yield return null;
            }

            _raycasterManager.RayCasterOn();
        }
    }
}
