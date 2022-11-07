using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    public class SelectManager
    {
        private readonly List<CellComponent> _cellComponents;
        private readonly List<ChipComponent> _сhipComponents;
        private readonly CheckersLogic _checkersLogic;

        private CellComponent _selectedCell = null; 
        public CellComponent SelectedCell { get { return _selectedCell; } set { _selectedCell = value; } }
        /// <summary>
        /// Конструктор принимает ссылки на класс логики шашек, список клеток и шашек
        /// </summary>
        /// <param name="checkersLogic"></param>
        /// <param name="cellComponents"></param>
        /// <param name="chipComponents"></param>
        public SelectManager(CheckersLogic checkersLogic, List<CellComponent> cellComponents, List<ChipComponent> chipComponents)
        {
            _checkersLogic = checkersLogic;
            _cellComponents = cellComponents;
            _сhipComponents = chipComponents;

            _selectCellMaterial = Resources.Load("Materials/SelectedCell", typeof(Material)) as Material;
            _selectChipMaterial = Resources.Load("Materials/SelectedChip", typeof(Material)) as Material;
        }
        [SerializeField] private Material _selectCellMaterial;
        [SerializeField] private Material _selectChipMaterial;
        private readonly List<CellComponent> _selectedCells = new(); //ссылка на выделенные клетки
        private ChipComponent _selectedChip = null;//ссылка на выделенную шашку
        /// <summary>
        /// Выделить все возможные ходя для стороны
        /// </summary>
        /// <param name="cell">Выделенная клетка</param>
        /// <param name="currentPlayer">Текцщая сторона</param>
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

            foreach (var c in _cellComponents)
                if (_checkersLogic.IsValidEat(cell.Pair as ChipComponent, c))
                {
                    c.AddAdditionalMaterial(_selectCellMaterial);
                    _selectedCells.Add(c);
                }
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
        /// Снять выделение со всех клеткок
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
        /// <summary>
        /// Принадлежит ли выделенная шашка игроку
        /// </summary>
        /// <param name="chip">Шашка</param>
        /// <returns></returns>
        private bool SelectChipIsValid(ChipComponent chip)
        {
            return chip.GetColor == _checkersLogic.CurrentPlayer;
        }
        /// <summary>
        /// Обработка нажатия на клетку
        /// </summary>
        /// <param name="cell">клетка</param>
        public void CellOnClick(CellComponent cell)
        {
            if (!cell.IsEmpty())//Если на клетке стоит шашка
            {
                if (!SelectChipIsValid(cell.Pair as ChipComponent))//Если шашка не принадлежит игроку
                {
                    Debug.Log("Не ваш ход!");
                    return;
                }
                DeselectAllChips();
                DeselectAllCells();

                if (cell == SelectedCell)//Если повторно выбрана выделенная шашка
                {
                    SelectedCell = null;
                    return;
                }

                SelectedCell = cell;
                _selectedChip = SelectedCell.Pair as ChipComponent;

                ChipOnSelect(_selectedChip);//При выделении шашки
                return;
            }

            if (_selectedChip == null) return;//Ни одна шашка ещё не выделена

            if (_checkersLogic.IsValidMove(_selectedChip, cell as CellComponent))//Если возможен ход
            {
                _checkersLogic.MoveChip(_selectedChip, cell as CellComponent);//Ходим
                DeselectAllChips();//Убираем выделение с шашек
                DeselectAllCells();//Убираем выделение с клеток

                if (_checkersLogic.ChipReachedLastRow(_selectedChip))//Проверка условия победы 
                    Debug.Log("Победа " + _checkersLogic.CurrentPlayer);

                _selectedChip = null;//запихнуть в метод
                _checkersLogic.ChangePlayer();
                return;
            }

            if (_checkersLogic.IsValidEat(_selectedChip, cell as CellComponent))//Если возможно съедение
            {
                _checkersLogic.EatChip(_selectedChip, cell as CellComponent, _checkersLogic.DetermineDirection(_selectedChip, cell as CellComponent, out _));//Едим
                DeselectAllChips();//Убираем выделение с шашек
                DeselectAllCells();//Убираем выделение с клеток

                if (_checkersLogic.ChipReachedLastRow(_selectedChip) || _checkersLogic.ChipsCountLessThanZero(_checkersLogic.CurrentPlayer))//Проверка условия победы 
                    Debug.Log("Победа " + _checkersLogic.CurrentPlayer);

                _selectedChip = null;
                _checkersLogic.ChangePlayer();
            }
        }
        /// <summary>
        /// Подписка на события всех клеток
        /// </summary>
        public void SubscribeCells()
        {
            foreach (var cell in _cellComponents)
            {
                cell.OnFocusEventHandler += CellFocus;
                cell.OnClickEventHandler += CellOnClick;
            }
        }
    }
}
