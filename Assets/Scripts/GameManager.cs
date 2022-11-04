using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        [SerializeField] private Material _selectCellMaterial;
        [SerializeField] private Material _selectChipMaterial;

        private List<CellComponent> _cellComponents;
        private List<CellComponent> _blackWinPositionCellComponents;
        private List<CellComponent> _whiteWinPositionCellComponents;

        private List<ChipComponent> _whiteChipComponents;
        private List<ChipComponent> _blackChipComponents;
        private List<ChipComponent> _сhipComponents;

        private ColorType _currentPlayer = ColorType.White;

        private CameraManager _playerCamera;

        private CellComponent _selectedCell = null; //ссылка на нажатую клетку
        private List<CellComponent> _selectedCells = new(); //ссылка на выделенные клетки
        private ChipComponent _selectedChip = null;//ссылка на выделенную шашку

        private MainCam _mainCam;
        public PhysicsRaycaster _rayCast;

        private void Awake()
        {
            instance = this;
        }
        private void Start()
        {
            _mainCam = FindObjectOfType<MainCam>();
            _rayCast = _mainCam.GetComponent<PhysicsRaycaster>();//блокировка ввода отключением Raycaster

            _playerCamera = FindObjectOfType<CameraManager>();

            _сhipComponents = FindObjectsOfType<ChipComponent>().ToList();
            _cellComponents = FindObjectsOfType<CellComponent>().Where(t => t.GetColor == ColorType.Black).ToList();

            _whiteChipComponents = _сhipComponents.Where(t => t.GetColor == ColorType.White).ToList();
            _blackChipComponents = _сhipComponents.Where(t => t.GetColor == ColorType.Black).ToList();

            InitializeWinPosition();

            PairAllChips();//найти пару для всех шашек и связать
            FindNeighbors();//найти соседей для всех клеток
            SubscribeCells();//подписаться на события всех клеток
        }
        /// <summary>
        /// Обработка нажатия на клетку
        /// </summary>
        /// <param name="cell">клетка</param>
        private void CellOnClick(CellComponent cell)
        {
            if (!IsCellEmpty(cell))//Если на клетке стоит шашка
            {
                if (!SelectChipIsValid(cell.Pair as ChipComponent))//Если шашка не принадлежит игроку
                {
                    Debug.Log("Не ваш ход!");
                    return;
                }
                DeselectAllChips();
                DeselectAllCells();

                if (cell == _selectedCell)//Если повторно выбрана выделенная шашка
                {
                    _selectedCell = null;
                    return;
                }

                _selectedCell = cell;
                _selectedChip = _selectedCell.Pair as ChipComponent;
                
                ChipOnSelect(_selectedChip);//При выделении шашки
                return;
            }

            if (_selectedChip == null) return;//Ни одна шашка ещё не выделена

            if (IsValidMove(_selectedChip, cell as CellComponent))//Если возможен ход
            {
                MoveChip(_selectedChip, cell as CellComponent);//Ходим
                DeselectAllChips();//Убираем выделение с шашек
                DeselectAllCells();//Убираем выделение с клеток

                if (ChipReachedLastRow(_selectedChip))//Проверка условия победы 
                    Debug.Log("Победа " + _currentPlayer);

                _selectedChip = null;//запихнуть в метод
                ChangePlayer();
                return;
            }

            if (isValidEat(_selectedChip, cell as CellComponent))//Если возможно съедение
            {
                EatChip(_selectedChip, cell as CellComponent, DetermineDirection(_selectedChip, cell as CellComponent, out bool plug));//Едим
                DeselectAllChips();//Убираем выделение с шашек
                DeselectAllCells();//Убираем выделение с клеток

                if (ChipReachedLastRow(_selectedChip) || ChipsCountLessThanZero(_currentPlayer))//Проверка условия победы 
                    Debug.Log("Победа " + _currentPlayer);

                _selectedChip = null;
                ChangePlayer();
            }
        }
        /// <summary>
        /// Подписка на события всех клеток
        /// </summary>
        private void SubscribeCells()
        {
            foreach (var cell in _cellComponents)
            {
                cell.OnFocusEventHandler += CellFocus;
                cell.OnClickEventHandler += CellOnClick;
            }
        }
        /// <summary>
        /// Последние ряды клеток в отдельные списки
        /// </summary>
        private void InitializeWinPosition()
        {
            _blackWinPositionCellComponents = _cellComponents.Where(t => t.transform.position.x > 60).ToList();
            _whiteWinPositionCellComponents = _cellComponents.Where(t => t.transform.position.x < 0).ToList();
        }
        /// <summary>
        /// Проверка, не съедены ли все шашки стороны
        /// </summary>
        /// <param name="currentPlayer">сторона</param>
        /// <returns></returns>
        private bool ChipsCountLessThanZero(ColorType currentPlayer)
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
        private bool ChipReachedLastRow(ChipComponent chip)
        {
            if (_currentPlayer == ColorType.White)
            {
                foreach (var cell in _whiteWinPositionCellComponents)
                    if (cell.Pair == chip)
                        return true;
            }
            if (_currentPlayer == ColorType.Black)
            {
                foreach (var cell in _blackWinPositionCellComponents)
                    if (cell.Pair == chip)
                        return true;
            }
            return false;
        }

        /// <summary>
        /// Принадлежит ли выделенная шашка игроку
        /// </summary>
        /// <param name="chip">Шашка</param>
        /// <returns></returns>
        private bool SelectChipIsValid(ChipComponent chip)
        {
            return chip.GetColor == _currentPlayer;
        }
        /// <summary>
        /// Возвращает направление при поедании шашки
        /// </summary>
        /// <param name="chip">Шашка, которая ест</param>
        /// <param name="cell">Клетка, куда хотим пойти</param>
        /// <param name="isSuccess">Удалось ли найти направление</param>
        /// <returns></returns>
        private NeighborType DetermineDirection(ChipComponent chip, CellComponent cell, out bool isSuccess)
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
        /// <summary>
        /// При выделении шашки
        /// </summary>
        /// <param name="chip">Шашка</param>
        private void ChipOnSelect(ChipComponent chip)
        {
            chip.AddAdditionalMaterial(_selectChipMaterial);
            SelectPossibleMoves(chip.Pair as CellComponent, chip.GetColor);
        }
        /// <summary>
        /// Проверка, возможен ли ход
        /// </summary>
        /// <param name="chip">шашка</param>
        /// <param name="cell">клетка</param>
        /// <returns>ход возможен</returns>
        private bool IsValidMove(ChipComponent chip, CellComponent cell)
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
        private void MoveChip(ChipComponent chip, CellComponent cell)
        {
            chip.Pair.Pair = null;
            StartCoroutine(chip.MoveFromTo(chip.transform.position, cell.transform.position + new Vector3(0, 0.5f, 0), 1f));
            chip.Pair = cell;
            cell.Pair = chip;
            //сходили успешно
        }
        /// <summary>
        /// Смена стороны
        /// </summary>
        private void ChangePlayer()
        {
            _currentPlayer = (ColorType)((int)(_currentPlayer + 1) % 2);
            _playerCamera.RotateCam();
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
        /// <summary>
        /// Проверка, возможно ли поедание
        /// </summary>
        /// <param name="chip">шашка</param>
        /// <param name="cell">клетка</param>
        /// <param name="neighborType">тип соседа</param>
        /// <returns>поедание возможно</returns>
        private bool isValidEat(ChipComponent chip, CellComponent cell)
        {
            bool isValid = false;
            NeighborType neighborType = DetermineDirection(chip, cell, out isValid);
            if (!isValid) return false;
            /*
            * 1 клетка пустая и является соседом соседа
            * 2 между шашкой и клеткой есть шашка
            * 3 она вражеская
            */
            var c = chip.Pair as CellComponent;
            if (IsCellEmpty(cell) && IsNeighbor(cell.GetNeighbor(ReversedNeighborType(neighborType)), chip))
                if (!IsCellEmpty(c.GetNeighbor(neighborType)) && c.GetNeighbor(neighborType).Pair.GetColor != chip.GetColor)
                    return true;
            return false;
        }

        private void EatChip(ChipComponent chip, CellComponent cell, NeighborType neighborType)
        {
            var c = chip.Pair as CellComponent;
            {
                chip.Pair.Pair = null;
                StartCoroutine(chip.MoveFromTo(chip.transform.position, cell.transform.position + new Vector3(0, 0.5f, 0), 1f));

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
        /// Проверка на соседство
        /// </summary>
        /// <param name="chip">шашка</param>
        /// <param name="cell">клетка</param>
        /// <returns>является ли соседом</returns>
        private bool IsNeighbor(CellComponent cell, ChipComponent chip)
        {
            foreach (NeighborType type in Enum.GetValues(typeof(NeighborType)))
            {
                if (cell.GetNeighbor(type) == chip.Pair)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Проверка на наличие пары
        /// </summary>
        /// <param name="component">клетка</param>
        /// <returns>есть ли пара</returns>
        private bool IsCellEmpty(CellComponent component)
        {
            return component?.Pair == null;
        }
        /// <summary>
        /// Снимает выделение со всех шашек
        /// </summary>
        private void DeselectAllChips()
        {
            foreach (var chip in _сhipComponents)
            {
                chip.RemoveAdditionalMaterial();
            }
        }
        /// <summary>
        /// Поиск пар для все шашек
        /// </summary>
        private void PairAllChips()
        {
            foreach (var chip in _сhipComponents)
            {
                chip.Pair = _cellComponents.First(cell => (cell.transform.position.x == chip.transform.position.x) && (cell.transform.position.z == chip.transform.position.z));
                chip.Pair.Pair = chip;
            }
        }
        /// <summary>
        /// Поиск всех соседей для клеток
        /// </summary>
        /// <param name="cell">клетка</param>
        private void FindNeighbors()
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
        /// Выделяет или снимает выделение с клетки при наведении
        /// </summary>
        /// <param name="component"></param>
        /// <param name="isSelect"></param>
        private void CellFocus(CellComponent cell, bool isSelect)
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
        private void DeselectAllCells()
        {
           foreach (var cell in _cellComponents)
            {
                cell.RemoveAdditionalMaterial();
            }
            _selectedCells.Clear();
        }
        /// <summary>
        /// Выделить левого и правого соседа в зависимости от играющей стороны
        /// </summary>
        /// <param name="component"></param>
        private void SelectPossibleMoves(CellComponent cell, ColorType currentPlayer)
        {
            if (currentPlayer == ColorType.White)
            {
                if (IsCellEmpty(cell.GetNeighbor(NeighborType.TopLeft)))
                {
                    cell.GetNeighbor(NeighborType.TopLeft)?.AddAdditionalMaterial(_selectCellMaterial);
                    _selectedCells.Add(cell.GetNeighbor(NeighborType.TopLeft));
                }
                if (IsCellEmpty(cell.GetNeighbor(NeighborType.TopRight)))
                {
                    cell.GetNeighbor(NeighborType.TopRight)?.AddAdditionalMaterial(_selectCellMaterial);
                    _selectedCells.Add(cell.GetNeighbor(NeighborType.TopRight));
                } 
            }
            else
            {
                if (IsCellEmpty(cell.GetNeighbor(NeighborType.BottomLeft)))
                {
                    cell.GetNeighbor(NeighborType.BottomLeft)?.AddAdditionalMaterial(_selectCellMaterial);
                    _selectedCells.Add(cell.GetNeighbor(NeighborType.BottomLeft));
                }
                if (IsCellEmpty(cell.GetNeighbor(NeighborType.BottomRight)))
                {
                    cell.GetNeighbor(NeighborType.BottomRight)?.AddAdditionalMaterial(_selectCellMaterial);
                    _selectedCells.Add(cell.GetNeighbor(NeighborType.BottomRight));
                }
            }
            foreach (var c in _cellComponents)
                if (isValidEat(cell.Pair as ChipComponent, c))
                {
                    c.AddAdditionalMaterial(_selectCellMaterial);
                    _selectedCells.Add(c);
                }
        }
    }
}