using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

namespace Checkers
{
    public class CellComponent : BaseClickComponent
    {
        private Dictionary<NeighborType, CellComponent> _neighbors;
        /// <summary>
        /// Возвращает соседа клетки по указанному направлению
        /// </summary>
        /// <param name="type">Перечисление направления</param>
        /// <returns>Клетка-сосед или null</returns>
        public CellComponent GetNeighbor(NeighborType type) => _neighbors[type];
        public Dictionary<NeighborType, CellComponent> GetNeighbors() => _neighbors;

        public override void OnPointerEnter(PointerEventData eventData)
        {
            CallBackEvent(this, true);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            CallBackEvent(this, false);
        }
        /// <summary>
        /// Конфигурирование связей клеток
        /// </summary>
		public void Configuration(Dictionary<NeighborType, CellComponent> neighbors)
		{
            if (_neighbors != null) return;
            _neighbors = neighbors;
		}

        /////////////////////////////
        /// <summary>
        /// Проверка на наличие пары
        /// </summary>
        /// <returns>есть ли пара</returns>
        public bool IsEmpty()
        {
            return this.Pair == null;
        }
    }    
}