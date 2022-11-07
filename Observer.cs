using UnityEngine;

namespace Checkers
{
   
    public class Observer : MonoBehaviour, ICheckersObserver
    {
        public void RecieveTurn()
        {
            Debug.Log("Событие получено обсервером");
        }
    }
}
