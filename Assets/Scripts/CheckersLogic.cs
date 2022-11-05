using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Checkers
{
    public class CheckersLogic
    {
        //private readonly SelectManager _selectManager;
        private readonly GameInitializator _gameInitializator;
        private readonly CameraManager _cameraManager;

        private ColorType _currentPlayer = ColorType.White;
        public ColorType CurrentPlayer { get { return _currentPlayer; } }

        //private readonly List<CellComponent> _cellComponents;
        private readonly List<ChipComponent> _сhipComponents;

        private readonly List<ChipComponent> _whiteChipComponents;
        private readonly List<ChipComponent> _blackChipComponents;


        public CheckersLogic(GameInitializator gameInitializator, CameraManager cameraManager, List<ChipComponent> chipComponents)
        {
            //_cellComponents = cellComponents;
            _сhipComponents = chipComponents;
            _gameInitializator = gameInitializator;
            //_selectManager = selectManager;
            _cameraManager = cameraManager;

            //
            _whiteChipComponents = _сhipComponents.Where(t => t.GetColor == ColorType.White).ToList();
            _blackChipComponents = _сhipComponents.Where(t => t.GetColor == ColorType.Black).ToList();
        }



        /// <summary>
        /// Проверка, возможно ли поедание
        /// </summary>
        /// <param name="chip">шашка</param>
        /// <param name="cell">клетка</param>
        /// <param name="neighborType">тип соседа</param>
        /// <returns>поедание возможно</returns>
        public bool IsValidEat(ChipComponent chip, CellComponent cell)
        {
            NeighborType neighborType = DetermineDirection(chip, cell, out bool isValid);
            if (!isValid) return false;
            /*
            * 1 клетка пустая и является соседом соседа
            * 2 между шашкой и клеткой есть шашка
            * 3 она вражеская
            */
            var c = chip.Pair as CellComponent;
            if (cell.IsEmpty() && IsNeighbor(cell.GetNeighbor(ReversedNeighborType(neighborType)), chip))
                if (!c.GetNeighbor(neighborType).IsEmpty() && c.GetNeighbor(neighborType).Pair.GetColor != chip.GetColor)
                    return true;
            return false;
        }
        /// <summary>
        /// Проверка на соседство
        /// </summary>
        /// <param name="chip">шашка</param>
        /// <param name="cell">клетка</param>
        /// <returns>является ли соседом</returns>
        public bool IsNeighbor(CellComponent cell, ChipComponent chip)
        {
            foreach (NeighborType type in Enum.GetValues(typeof(NeighborType)))
            {
                if (cell.GetNeighbor(type) == chip.Pair)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Проверка, не съедены ли все шашки стороны
        /// </summary>
        /// <param name="currentPlayer">сторона</param>
        /// <returns></returns>
        public bool ChipsCountLessThanZero(ColorType currentPlayer)
        {
            if (currentPlayer == ColorType.Black)
                return _whiteChipComponents.Count() <= 0;
            else
                return _blackChipComponents.Count() <= 0;
        }

        /// <summary>
        /// Проверка достижения последнего ряда
        /// </summary>
        /// <param name="chip"></param>
        /// <returns></returns>
        public bool ChipReachedLastRow(ChipComponent chip)
        {
            if (_currentPlayer == ColorType.White)
            {
                foreach (var cell in _gameInitializator.WhiteWinPositionCellComponents)
                    if (cell.Pair == chip)
                        return true;
            }
            if (_currentPlayer == ColorType.Black)
            {
                foreach (var cell in _gameInitializator.BlackWinPositionCellComponents)
                    if (cell.Pair == chip)
                        return true;
            }
            return false;
        }
        /// <summary>
        /// Проверка, возможен ли ход
        /// </summary>
        /// <param name="chip">шашка</param>
        /// <param name="cell">клетка</param>
        /// <returns>ход возможен</returns>
        public bool IsValidMove(ChipComponent chip, CellComponent cell)
        {
            var c = chip.Pair as CellComponent;
            if (_currentPlayer == ColorType.White)
                return (c.GetNeighbor(NeighborType.TopLeft) == cell || c.GetNeighbor(NeighborType.TopRight) == cell);
            return (c.GetNeighbor(NeighborType.BottomLeft) == cell || c.GetNeighbor(NeighborType.BottomRight) == cell);
        }

        /// <summary>
        /// Двигает шашку на клетку
        /// </summary>
        /// <param name="chip">шашка</param>
        /// <param name="cell">клетка</param>
        public void MoveChip(ChipComponent chip, CellComponent cell)
        {
            chip.Pair.Pair = null;
            chip.Move(chip.transform.position, cell.transform.position + new Vector3(0, 0.5f, 0), 1f);
            chip.Pair = cell;
            cell.Pair = chip;
            //сходили успешно
        }
        /// <summary>
        /// Смена стороны
        /// </summary>
        public void ChangePlayer()
        {
            _currentPlayer = (ColorType)((int)(_currentPlayer + 1) % 2);
            _cameraManager.RotateCam();
        }
        /// <summary>
        /// Определяет обратное направление при поедании
        /// </summary>
        /// <param name="neighborType"></param>
        /// <returns></returns>
        private NeighborType ReversedNeighborType(NeighborType neighborType)
        {
            return (NeighborType)((int)(neighborType + 2) % 4);
        }

        public void EatChip(ChipComponent chip, CellComponent cell, NeighborType neighborType)
        {
            var c = chip.Pair as CellComponent;
            {
                chip.Pair.Pair = null;
                chip.Move(chip.transform.position, cell.transform.position + new Vector3(0, 0.5f, 0), 1f);

                //???
                c.GetNeighbor(neighborType).Pair.gameObject.SetActive(false);

                if (c.GetNeighbor(neighborType).Pair.GetColor == ColorType.White)
                    _whiteChipComponents.Remove(c.GetNeighbor(neighborType).Pair as ChipComponent);
                else
                    _blackChipComponents.Remove(c.GetNeighbor(neighborType).Pair as ChipComponent);

                _сhipComponents.Remove(c.GetNeighbor(neighborType).Pair as ChipComponent);
                c.GetNeighbor(neighborType).Pair = null;

                chip.Pair = cell;
                cell.Pair = chip;
            }
        }
        /// <summary>
        /// Возвращает направление при поедании шашки
        /// </summary>
        /// <param name="chip">Шашка, которая ест</param>
        /// <param name="cell">Клетка, куда хотим пойти</param>
        /// <param name="isSuccess">Удалось ли найти направление</param>
        /// <returns></returns>
        public NeighborType DetermineDirection(ChipComponent chip, CellComponent cell, out bool isSuccess)
        {
            Vector3 direction = cell.transform.position - chip.transform.position;
            //-20, 20 TopRight 
            //-20, -20 TopLeft
            //20, 20 BottomRight
            //20, -20 BottomLeft
            isSuccess = true;
            if (direction.x < 0 && direction.z < 0)
                return NeighborType.TopLeft;
            else if (direction.x > 0 && direction.z > 0)
                return NeighborType.BottomRight;
            else if (direction.x < 0 && direction.z > 0)
                return NeighborType.TopRight;
            else if (direction.x > 0 && direction.z < 0)
                return NeighborType.BottomLeft;

            isSuccess = false;
            return NeighborType.TopLeft;
        }

    }
}
