using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers
{
    [RequireComponent(typeof(PhysicsRaycaster))]
    public class RaycasterManager : MonoBehaviour
    {
        //private MainCam _mainCam;
        private PhysicsRaycaster _rayCaster;
        public PhysicsRaycaster RayCaster { get => _rayCaster; private set => _rayCaster = value; }

        public void RayCasterOn()
        {
            RayCaster.enabled = true;
        }
        public void RayCasterOff()
        {
            RayCaster.enabled = false;
        }
        private void Awake()
        {
            RayCaster = GetComponent<PhysicsRaycaster>();//блокировка ввода отключением Raycaster
        }
    }
}
