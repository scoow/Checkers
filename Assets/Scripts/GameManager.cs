using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Checkers
{
    public class GameManager : MonoBehaviour
    {
        private List<CellComponent> _cellComponents;
        private List<CellComponent> _blackWinPositionCellComponents;
        private List<CellComponent> _whiteWinPositionCellComponents;

        private List<ChipComponent> _whiteChipComponents;
        private List<ChipComponent> _blackChipComponents;
        private List<ChipComponent> _�hipComponents;

        private SelectManager selectManager;
        private GameInitializator gameInitializator;

        private ColorType _currentPlayer = ColorType.White;

        private CameraManager _playerCamera;


        private ChipComponent _selectedChip = null;//������ �� ���������� �����
        private void Start()
        {
            

            _playerCamera = FindObjectOfType<CameraManager>();

            _�hipComponents = FindObjectsOfType<ChipComponent>().ToList();
            _cellComponents = FindObjectsOfType<CellComponent>().Where(t => t.GetColor == ColorType.Black).ToList();

            _whiteChipComponents = _�hipComponents.Where(t => t.GetColor == ColorType.White).ToList();
            _blackChipComponents = _�hipComponents.Where(t => t.GetColor == ColorType.Black).ToList();

            selectManager = new(_cellComponents, _�hipComponents);
            gameInitializator = new(_cellComponents, _�hipComponents);

            InitializeWinPosition();

            gameInitializator.PairAllChips();//����� ���� ��� ���� ����� � �������
            gameInitializator.FindNeighbors();//����� ������� ��� ���� ������
            SubscribeCells();//����������� �� ������� ���� ������
        }
        /// <summary>
        /// ��������� ������� �� ������
        /// </summary>
        /// <param name="cell">������</param>
        private void CellOnClick(CellComponent cell)
        {
            if (!cell.IsEmpty())//���� �� ������ ����� �����
            {
                if (!SelectChipIsValid(cell.Pair as ChipComponent))//���� ����� �� ����������� ������
                {
                    Debug.Log("�� ��� ���!");
                    return;
                }
                selectManager.DeselectAllChips();
                selectManager.DeselectAllCells();

                if (cell == selectManager.SelectedCell)//���� �������� ������� ���������� �����
                {
                    selectManager.SelectedCell = null;
                    return;
                }

                selectManager.SelectedCell = cell;
                _selectedChip = selectManager.SelectedCell.Pair as ChipComponent;

                selectManager.ChipOnSelect(_selectedChip);//��� ��������� �����
                return;
            }

            if (_selectedChip == null) return;//�� ���� ����� ��� �� ��������

            if (IsValidMove(_selectedChip, cell as CellComponent))//���� �������� ���
            {
                MoveChip(_selectedChip, cell as CellComponent);//�����
                selectManager.DeselectAllChips();//������� ��������� � �����
                selectManager.DeselectAllCells();//������� ��������� � ������

                if (ChipReachedLastRow(_selectedChip))//�������� ������� ������ 
                    Debug.Log("������ " + _currentPlayer);

                _selectedChip = null;//��������� � �����
                ChangePlayer();
                return;
            }

            if (isValidEat(_selectedChip, cell as CellComponent))//���� �������� ��������
            {
                EatChip(_selectedChip, cell as CellComponent, DetermineDirection(_selectedChip, cell as CellComponent, out bool plug));//����
                selectManager.DeselectAllChips();//������� ��������� � �����
                selectManager.DeselectAllCells();//������� ��������� � ������

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
                cell.OnFocusEventHandler += selectManager.CellFocus;
                cell.OnClickEventHandler += CellOnClick;///////////////
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
            if (cell.IsEmpty() && IsNeighbor(cell.GetNeighbor(ReversedNeighborType(neighborType)), chip))
                if (!c.GetNeighbor(neighborType).IsEmpty() && c.GetNeighbor(neighborType).Pair.GetColor != chip.GetColor)
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



        
    }
}