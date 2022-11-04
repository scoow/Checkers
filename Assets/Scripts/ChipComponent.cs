using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers
{

    public class ChipComponent : BaseClickComponent
    {
        public override void OnPointerEnter(PointerEventData eventData)
        {
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
        }

        public IEnumerator MoveFromTo(Vector3 startPosition, Vector3 endPosition, float time)
        {
            var currentTime = 0f;
            while (currentTime < time)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, currentTime / time);
                currentTime += Time.deltaTime;
                yield return null;
            }
            transform.position = endPosition;
        }
    }
}