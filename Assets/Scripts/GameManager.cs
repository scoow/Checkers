using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Checkers
{
    public class GameManager : MonoBehaviour
    {
        private List<CellComponent> _cellComponents;
        private List<ChipComponent> _�hipComponents;

        private SelectManager _selectManager;
        private GameInitializator _gameInitializator;
        private CheckersLogic _checkersLogic;
        private CameraManager _cameraManager;


       

        private void Start()
        {
            _cameraManager = FindObjectOfType<CameraManager>();

            _cellComponents = FindObjectsOfType<CellComponent>().ToList();
            _�hipComponents = FindObjectsOfType<ChipComponent>().ToList();

            _gameInitializator = new(_cellComponents, _�hipComponents);
            _gameInitializator.InitializeWinPosition();//����� ��������� ���� ������
            _gameInitializator.PairAllChips();//����� ���� ��� ���� ����� � �������
            _gameInitializator.FindNeighbors();//����� ������� ��� ���� ������

            _checkersLogic = new (_gameInitializator, _cameraManager, _�hipComponents);

            _selectManager = new(_checkersLogic, _cellComponents, _�hipComponents);
            _selectManager.SubscribeCells();//����������� �� ������� ���� ������
        }
    }
}