using System;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    public class SelectManager
    {
        public event MoveEventHandler OnMoveEventHandler;//
        public delegate void MoveEventHandler(string move);//

        private readonly List<CellComponent> _cellComponents;
        private readonly List<ChipComponent> _сhipComponents;
        private readonly CheckersLogic _checkersLogic;
        private readonly IObserver _observer;

        private CellComponent _selectedCell = null;
        /// <summary>
        /// Воспроизведение хода
        /// </summary>
        public void ReplayMove()
        {
            string move = _observer.SendTurn();
           
            StringToMove(move, out ColorType currentPlayer, out ActionType actionType, out BaseClickComponent chip, out BaseClickComponent cell);
            switch (actionType)
            {
                case ActionType.selects:
                    ChipOnSelect(chip as ChipComponent);
                    break;
                case ActionType.moves:
                    _checkersLogic.MoveChip(chip as ChipComponent, cell as CellComponent);
                    DeselectAllChips();//Убираем выделение с шашек
                    DeselectAllCells();//Убираем выделение с клеток

                    if (_checkersLogic.ChipReachedLastRow(_selectedChip))//Проверка условия победы 
                        Debug.Log("Победа " + _checkersLogic.CurrentPlayer);

                    _selectedChip = null;//запихнуть в метод
                    _checkersLogic.ChangePlayer();
                    break;
                case ActionType.takes:
                    _checkersLogic.EatChip(chip as ChipComponent, cell as CellComponent, _checkersLogic.DetermineDirection(chip as ChipComponent, cell as CellComponent, out _));
                    DeselectAllChips();//Убираем выделение с шашек
                    DeselectAllCells();//Убираем выделение с клеток

                    if (_checkersLogic.ChipReachedLastRow(_selectedChip) || _checkersLogic.ChipsCountLessThanZero(_checkersLogic.CurrentPlayer))//Проверка условия победы 
                        Debug.Log("Победа " + _checkersLogic.CurrentPlayer);

                    _selectedChip = null;
                    _checkersLogic.ChangePlayer();
                    break;
                default:
                    break;
            }
        }

        public CellComponent SelectedCell { get { return _selectedCell; } set { _selectedCell = value; } }
        /// <summary>
        /// Конструктор принимает ссылки на класс логики шашек, список клеток и шашек
        /// </summary>
        /// <param name="checkersLogic"></param>
        /// <param name="cellComponents"></param>
        /// <param name="chipComponents"></param>
        public SelectManager(CheckersLogic checkersLogic, IObserver observer, List<CellComponent> cellComponents, List<ChipComponent> chipComponents)
        {
            _checkersLogic = checkersLogic;
            _cellComponents = cellComponents;
            _сhipComponents = chipComponents;
            _observer = observer;
            

            _selectCellMaterial = Resources.Load("Materials/SelectedCell", typeof(Material)) as Material;
            _selectChipMaterial = Resources.Load("Materials/SelectedChip", typeof(Material)) as Material;

            OnMoveEventHandler += _observer.RecieveTurn;
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
        public void CellFocus (CellComponent cell, bool isSelect)
        {
            if (isSelect)
            {
                cell.AddAdditionalMaterial(_selectCellMaterial);
                cell.Pair?.AddAdditionalMaterial(_selectChipMaterial);
            }
            else
            {
                if (cell != _selectedCell && !_selectedCells.Contains(cell))
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


            OnMoveEventHandler.Invoke(MoveToString(_checkersLogic.CurrentPlayer, ActionType.selects, chip, null));
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
                OnMoveEventHandler.Invoke(MoveToString(_checkersLogic.CurrentPlayer, ActionType.moves, _selectedChip, cell));
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
                OnMoveEventHandler.Invoke(MoveToString(_checkersLogic.CurrentPlayer, ActionType.takes, _selectedChip, cell));
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
        /// <summary>
        /// Преобразование хода в строку
        /// </summary>
        /// <param name="currentPlayer"></param>
        /// <param name="actionType"></param>
        /// <param name="chip"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        public string MoveToString(ColorType currentPlayer, ActionType actionType, ChipComponent chip, CellComponent cell)
        {
            string result;
            result = currentPlayer.ToString() + " " + actionType.ToString() + " " + chip.Pair.gameObject.name.ToString();
            if (actionType != ActionType.selects)
                result += " " + cell.gameObject.name.ToString();
            return result;
        }
        /// <summary>
        /// Преобразование строки в ход
        /// </summary>
        /// <param name="move"></param>
        /// <param name="currentPlayer"></param>
        /// <param name="actionType"></param>
        /// <param name="chip"></param>
        /// <param name="cell"></param>
        public void StringToMove(string move, out ColorType currentPlayer, out ActionType actionType, out BaseClickComponent chip, out BaseClickComponent cell)
        {
            string[] result = move.Split(' ');
            var _currentPlayer = result[0] switch
            {
                "White" => ColorType.White,
                _ => ColorType.Black,
            };

            var _actionType = result[1] switch
            {
                "selects" => ActionType.selects,
                "moves" => ActionType.moves,
                _ => ActionType.takes,
            };

            BaseClickComponent.FindByName(result[2], out BaseClickComponent _chip);

            BaseClickComponent _cell = null;
            if (_actionType != ActionType.selects)
                BaseClickComponent.FindByName(result[3], out _cell);

            currentPlayer = _currentPlayer;
            actionType = _actionType;

            chip = _chip.Pair as ChipComponent;
            cell = _cell as CellComponent;
        }
    }
}
