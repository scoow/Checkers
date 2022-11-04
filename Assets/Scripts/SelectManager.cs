using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    public interface ISelectManager
    {
        void SelectPossibleMoves(CellComponent cell, ColorType currentPlayer);
    }

    public class SelectManager : ISelectManager
    {
        private readonly List<CellComponent> _cellComponents;
        private readonly List<ChipComponent> _сhipComponents;

        private CellComponent _selectedCell = null; //ссылка на нажатую клетку
        public CellComponent SelectedCell { get { return _selectedCell; } set { _selectedCell = value;  } }

        public SelectManager(List<CellComponent> cellComponents, List<ChipComponent> chipComponents)
        {
            _cellComponents = cellComponents;
            _сhipComponents = chipComponents;

            _selectCellMaterial = Resources.Load("Materials/SelectedCell", typeof(Material)) as Material;
            _selectChipMaterial = Resources.Load("Materials/SelectedChip", typeof(Material)) as Material;
        }

        [SerializeField] private Material _selectCellMaterial;
        [SerializeField] private Material _selectChipMaterial;
        private List<CellComponent> _selectedCells = new(); //ссылка на выделенные клетки

        public void SelectPossibleMoves(CellComponent cell, ColorType currentPlayer)
        {
            CellComponent _leftNeighbor;
            CellComponent _rightNeighbor;
            if (currentPlayer == ColorType.White)
            {
                _leftNeighbor = cell.GetNeighbor(NeighborType.TopLeft);
                _rightNeighbor = cell.GetNeighbor(NeighborType.TopRight);
            }
            else
            {
                _leftNeighbor = cell.GetNeighbor(NeighborType.BottomLeft);
                _rightNeighbor = cell.GetNeighbor(NeighborType.BottomRight);
            }

            if (_leftNeighbor != null && _leftNeighbor.IsEmpty())
            {
                _leftNeighbor.AddAdditionalMaterial(_selectCellMaterial);
                _selectedCells.Add(_leftNeighbor);
            }
            if (_rightNeighbor != null && _rightNeighbor.IsEmpty())
            {
                _rightNeighbor.AddAdditionalMaterial(_selectCellMaterial);
                _selectedCells.Add(_rightNeighbor);
            }

/*            foreach (var c in _cellComponents)
                if (isValidEat(cell.Pair as ChipComponent, c))
                {
                    c.AddAdditionalMaterial(_selectCellMaterial);
                    _selectedCells.Add(c);
                }*/
        }
        /// <summary>
        /// Выделяет или снимает выделение с клетки при наведении
        /// </summary>
        /// <param name="component"></param>
        /// <param name="isSelect"></param>
        public void CellFocus(CellComponent cell, bool isSelect)
        {
            if (isSelect)
            {
                cell.AddAdditionalMaterial(_selectCellMaterial);
                cell.Pair?.AddAdditionalMaterial(_selectChipMaterial);
            }
            else
            {
                if (cell != _selectedCell && !_selectedCells.Contains(cell))//исправить
                {
                    cell.RemoveAdditionalMaterial();
                    cell.Pair?.RemoveAdditionalMaterial();
                }
            }
        }
        /// <summary>
        /// Снять выделение со всех соседей клетки
        /// </summary>
        /// <param name="component"></param>
        public void DeselectAllCells()
        {
            foreach (var cell in _cellComponents)
            {
                cell.RemoveAdditionalMaterial();
            }
            _selectedCells.Clear();
        }
        /// <summary>
        /// При выделении шашки
        /// </summary>
        /// <param name="chip">Шашка</param>
        public void ChipOnSelect(ChipComponent chip)
        {
            chip.AddAdditionalMaterial(_selectChipMaterial);
            SelectPossibleMoves(chip.Pair as CellComponent, chip.GetColor);
        }
        /// <summary>
        /// Снимает выделение со всех шашек
        /// </summary>
        public void DeselectAllChips()
        {
            foreach (var chip in _сhipComponents)
            {
                chip.RemoveAdditionalMaterial();
            }
        }
    }
}
