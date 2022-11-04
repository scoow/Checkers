using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers
{
    internal class RaycasterManager : MonoBehaviour
    {
        private MainCam _mainCam;
        private PhysicsRaycaster _rayCaster;
        public PhysicsRaycaster RayCaster { get => _rayCaster; private set => _rayCaster = value; }

        public RaycasterManager()
        {
            _mainCam = FindObjectOfType<MainCam>();
            _rayCaster = _mainCam.GetComponent<PhysicsRaycaster>();//блокировка ввода отключением Raycaster
        }
    }
}
