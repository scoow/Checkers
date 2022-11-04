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
        private List<ChipComponent> _�hipComponents;

        private ColorType _currentPlayer = ColorType.White;

        private CameraManager _playerCamera;

        private CellComponent _selectedCell = null; //������ �� ������� ������
        private List<CellComponent> _selectedCells = new(); //������ �� ���������� ������
        private ChipComponent _selectedChip = null;//������ �� ���������� �����

        private MainCam _mainCam;
        public PhysicsRaycaster _rayCast;

        private void Awake()
        {
            instance = this;
        }
        private void Start()
        {
            _mainCam = FindObjectOfType<MainCam>();
            _rayCast = _mainCam.GetComponent<PhysicsRaycaster>();//���������� ����� ����������� Raycaster

            _playerCamera = FindObjectOfType<CameraManager>();

            _�hipComponents = FindObjectsOfType<ChipComponent>().ToList();
            _cellComponents = FindObjectsOfType<CellComponent>().Where(t => t.GetColor == ColorType.Black).ToList();

            _whiteChipComponents = _�hipComponents.Where(t => t.GetColor == ColorType.White).ToList();
            _blackChipComponents = _�hipComponents.Where(t => t.GetColor == ColorType.Black).ToList();

            InitializeWinPosition();

            PairAllChips();//����� ���� ��� ���� ����� � �������
            FindNeighbors();//����� ������� ��� ���� ������
            SubscribeCells();//����������� �� ������� ���� ������
        }
        /// <summary>
        /// ��������� ������� �� ������
        /// </summary>
        /// <param name="cell">������</param>
        private void CellOnClick(CellComponent cell)
        {
            if (!IsCellEmpty(cell))//���� �� ������ ����� �����
            {
                if (!SelectChipIsValid(cell.Pair as ChipComponent))//���� ����� �� ����������� ������
                {
                    Debug.Log("�� ��� ���!");
                    return;
                }
                DeselectAllChips();
                DeselectAllCells();

                if (cell == _selectedCell)//���� �������� ������� ���������� �����
                {
                    _selectedCell = null;
                    return;
                }

                _selectedCell = cell;
                _selectedChip = _selectedCell.Pair as ChipComponent;
                
                ChipOnSelect(_selectedChip);//��� ��������� �����
                return;
            }

            if (_selectedChip == null) return;//�� ���� ����� ��� �� ��������

            if (IsValidMove(_selectedChip, cell as CellComponent))//���� �������� ���
            {
                MoveChip(_selectedChip, cell as CellComponent);//�����
                DeselectAllChips();//������� ��������� � �����
                DeselectAllCells();//������� ��������� � ������

                if (ChipReachedLastRow(_selectedChip))//�������� ������� ������ 
                    Debug.Log("������ " + _currentPlayer);

                _selectedChip = null;//��������� � �����
                ChangePlayer();
                return;
            }

            if (isValidEat(_selectedChip, cell as CellComponent))//���� �������� ��������
            {
                EatChip(_selectedChip, cell as CellComponent, DetermineDirection(_selectedChip, cell as CellComponent, out bool plug));//����
                DeselectAllChips();//������� ��������� � �����
                DeselectAllCells();//������� ��������� � ������

                if (ChipReachedLastRow(_selectedChip) || ChipsCountLessThanZero(_currentPlayer))//�������� ������� ������ 
                    Debug.Log("������ " + _currentPlayer);

                _selectedChip = null;
                ChangePlayer();
            }
        }
        /// <summary>
        /// �������� �� ������� ���� ������
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
        /// ��������� ���� ������ � ��������� ������
        /// </summary>
        private void InitializeWinPosition()
        {
            _blackWinPositionCellComponents = _cellComponents.Where(t => t.transform.position.x > 60).ToList();
            _whiteWinPositionCellComponents = _cellComponents.Where(t => t.transform.position.x < 0).ToList();
        }
        /// <summary>
        /// ��������, �� ������� �� ��� ����� �������
        /// </summary>
        /// <param name="currentPlayer">�������</param>
        /// <returns></returns>
        private bool ChipsCountLessThanZero(ColorType currentPlayer)
        {
            if (currentPlayer == ColorType.Black)
                return _whiteChipComponents.Count() <= 0;
            else
                return _blackChipComponents.Count() <= 0;
        }
        /// <summary>
        /// �������� ���������� ���������� ����
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
        /// ����������� �� ���������� ����� ������
        /// </summary>
        /// <param name="chip">�����</param>
        /// <returns></returns>
        private bool SelectChipIsValid(ChipComponent chip)
        {
            return chip.GetColor == _currentPlayer;
        }
        /// <summary>
        /// ���������� ����������� ��� �������� �����
        /// </summary>
        /// <param name="chip">�����, ������� ���</param>
        /// <param name="cell">������, ���� ����� �����</param>
        /// <param name="isSuccess">������� �� ����� �����������</param>
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
        /// ��� ��������� �����
        /// </summary>
        /// <param name="chip">�����</param>
        private void ChipOnSelect(ChipComponent chip)
        {
            chip.AddAdditionalMaterial(_selectChipMaterial);
            SelectPossibleMoves(chip.Pair as CellComponent, chip.GetColor);
        }
        /// <summary>
        /// ��������, �������� �� ���
        /// </summary>
        /// <param name="chip">�����</param>
        /// <param name="cell">������</param>
        /// <returns>��� ��������</returns>
        private bool IsValidMove(ChipComponent chip, CellComponent cell)
        {
            var c = chip.Pair as CellComponent;
            if (_currentPlayer == ColorType.White)
                return (c.GetNeighbor(NeighborType.TopLeft) == cell || c.GetNeighbor(NeighborType.TopRight) == cell);
            return (c.GetNeighbor(NeighborType.BottomLeft) == cell || c.GetNeighbor(NeighborType.BottomRight) == cell);
        }
        /// <summary>
        /// ������� ����� �� ������
        /// </summary>
        /// <param name="chip">�����</param>
        /// <param name="cell">������</param>
        private void MoveChip(ChipComponent chip, CellComponent cell)
        {
            chip.Pair.Pair = null;
            StartCoroutine(chip.MoveFromTo(chip.transform.position, cell.transform.position + new Vector3(0, 0.5f, 0), 1f));
            chip.Pair = cell;
            cell.Pair = chip;
            //������� �������
        }
        /// <summary>
        /// ����� �������
        /// </summary>
        private void ChangePlayer()
        {
            _currentPlayer = (ColorType)((int)(_currentPlayer + 1) % 2);
            _playerCamera.RotateCam();
        }
        /// <summary>
        /// ���������� �������� ����������� ��� ��������
        /// </summary>
        /// <param name="neighborType"></param>
        /// <returns></returns>
        private NeighborType ReversedNeighborType(NeighborType neighborType)
        {
            return (NeighborType)((int)(neighborType + 2) % 4);
        }
        /// <summary>
        /// ��������, �������� �� ��������
        /// </summary>
        /// <param name="chip">�����</param>
        /// <param name="cell">������</param>
        /// <param name="neighborType">��� ������</param>
        /// <returns>�������� ��������</returns>
        private bool isValidEat(ChipComponent chip, CellComponent cell)
        {
            bool isValid = false;
            NeighborType neighborType = DetermineDirection(chip, cell, out isValid);
            if (!isValid) return false;
            /*
            * 1 ������ ������ � �������� ������� ������
            * 2 ����� ������ � ������� ���� �����
            * 3 ��� ���������
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

                _�hipComponents.Remove(c.GetNeighbor(neighborType).Pair as ChipComponent);
                c.GetNeighbor(neighborType).Pair = null;

                chip.Pair = cell;
                cell.Pair = chip;
            }
        }
        /// <summary>
        /// �������� �� ���������
        /// </summary>
        /// <param name="chip">�����</param>
        /// <param name="cell">������</param>
        /// <returns>�������� �� �������</returns>
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
        /// �������� �� ������� ����
        /// </summary>
        /// <param name="component">������</param>
        /// <returns>���� �� ����</returns>
        private bool IsCellEmpty(CellComponent component)
        {
            return component?.Pair == null;
        }
        /// <summary>
        /// ������� ��������� �� ���� �����
        /// </summary>
        private void DeselectAllChips()
        {
            foreach (var chip in _�hipComponents)
            {
                chip.RemoveAdditionalMaterial();
            }
        }
        /// <summary>
        /// ����� ��� ��� ��� �����
        /// </summary>
        private void PairAllChips()
        {
            foreach (var chip in _�hipComponents)
            {
                chip.Pair = _cellComponents.First(cell => (cell.transform.position.x == chip.transform.position.x) && (cell.transform.position.z == chip.transform.position.z));
                chip.Pair.Pair = chip;
            }
        }
        /// <summary>
        /// ����� ���� ������� ��� ������
        /// </summary>
        /// <param name="cell">������</param>
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
        /// ����� ������ ���� type ��� ������ cell
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
        /// �������� ��� ������� ��������� � ������ ��� ���������
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
                if (cell != _selectedCell && !_selectedCells.Contains(cell))//���������
                {
                    cell.RemoveAdditionalMaterial();
                    cell.Pair?.RemoveAdditionalMaterial();
                }
            }
        }
        /// <summary>
        /// ����� ��������� �� ���� ������� ������
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
        /// �������� ������ � ������� ������ � ����������� �� �������� �������
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