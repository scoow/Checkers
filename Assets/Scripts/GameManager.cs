using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Checkers
{
    public class GameManager : MonoBehaviour
    {
        private List<CellComponent> _cellComponents;
        private List<ChipComponent> _�hipComponents;

        private SelectManager selectManager;
        private GameInitializator gameInitializator;
        private CheckersLogic checkersLogic;
        private CameraManager cameraManager;

        private void Start()
        {
            cameraManager = FindObjectOfType<CameraManager>();

            _cellComponents = FindObjectsOfType<CellComponent>().ToList();
            _�hipComponents = FindObjectsOfType<ChipComponent>().ToList();

            gameInitializator = new(_cellComponents, _�hipComponents);

            checkersLogic = new (gameInitializator, cameraManager, _�hipComponents);

            selectManager = new(checkersLogic, _cellComponents, _�hipComponents);

            gameInitializator.InitializeWinPosition();
            gameInitializator.PairAllChips();//����� ���� ��� ���� ����� � �������
            gameInitializator.FindNeighbors();//����� ������� ��� ���� ������
            selectManager.SubscribeCells();//����������� �� ������� ���� ������
        }
    }
}