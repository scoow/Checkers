using System.Collections.Generic;
using System;
using System.Linq;

namespace Checkers
{
    public class GameInitializator
    {
        private readonly List<CellComponent> _cellComponents;
        private readonly List<ChipComponent> _сhipComponents;

        private List<CellComponent> _blackWinPositionCellComponents = new();
        private List<CellComponent> _whiteWinPositionCellComponents = new();

        public List<CellComponent> BlackWinPositionCellComponents { get { return _blackWinPositionCellComponents; } }
        public List<CellComponent> WhiteWinPositionCellComponents { get { return _whiteWinPositionCellComponents; } }
        /// <summary>
        /// Конструктор принимает ссылки на клетки и шашки
        /// </summary>
        /// <param name="cellComponents">Клетки</param>
        /// <param name="chipComponents">Шашки</param>
        public GameInitializator(List<CellComponent> cellComponents, List<ChipComponent> chipComponents)
        {
            _cellComponents = cellComponents;
            _сhipComponents = chipComponents;
        }

        /// <summary>
        /// Последние ряды клеток в отдельные списки
        /// </summary>
        public void InitializeWinPosition()
        {
            _blackWinPositionCellComponents = _cellComponents.Where(t => t.transform.position.x > 60).ToList();
            _whiteWinPositionCellComponents = _cellComponents.Where(t => t.transform.position.x < 0).ToList();
        }

        /// <summary>
        /// Поиск всех соседей для клеток
        /// </summary>
        /// <param name="cell">клетка</param>
        public void FindNeighbors()
        {
            foreach (var cell in _cellComponents)
            {
                Dictionary<NeighborType, CellComponent> neighbors = new();

                foreach (NeighborType type in Enum.GetValues(typeof(NeighborType)))
                {
                    neighbors.Add(type, FindNeighbor(cell, type));
                }
                cell.Configuration(neighbors);
            }
        }
        /// <summary>
        /// Поиск соседа типа type для клетки cell
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private CellComponent FindNeighbor(CellComponent cell, NeighborType type)
        {
            return type switch
            {
                NeighborType.TopLeft => _cellComponents.FirstOrDefault(c => (c.transform.position.x == cell.transform.position.x - 10) && (c.transform.position.z == cell.transform.position.z - 10)),
                NeighborType.TopRight => _cellComponents.FirstOrDefault(c => (c.transform.position.x == cell.transform.position.x - 10) && (c.transform.position.z == cell.transform.position.z + 10)),
                NeighborType.BottomLeft => _cellComponents.FirstOrDefault(c => (c.transform.position.x == cell.transform.position.x + 10) && (c.transform.position.z == cell.transform.position.z - 10)),
                NeighborType.BottomRight => _cellComponents.FirstOrDefault(c => (c.transform.position.x == cell.transform.position.x + 10) && (c.transform.position.z == cell.transform.position.z + 10)),
                _ => null,
            };
        }
        /// <summary>
        /// Поиск пар для все шашек
        /// </summary>
        public void PairAllChips()
        {
            foreach (var chip in _сhipComponents)
            {
                chip.Pair = _cellComponents.First(cell => (cell.transform.position.x == chip.transform.position.x) && (cell.transform.position.z == chip.transform.position.z));
                chip.Pair.Pair = chip;
            }
        }
    }
}
