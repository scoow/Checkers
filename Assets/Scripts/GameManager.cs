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

        //private CameraManager _playerCamera;



        private void Start()
        {
            //_playerCamera = FindObjectOfType<CameraManager>();

            _cellComponents = FindObjectsOfType<CellComponent>().ToList();
            _�hipComponents = FindObjectsOfType<ChipComponent>().ToList();


            selectManager = new(_cellComponents, _�hipComponents);
            gameInitializator = new(selectManager, _cellComponents, _�hipComponents);
            checkersLogic = new (selectManager, gameInitializator, _cellComponents, _�hipComponents);
            selectManager.GetLogic(checkersLogic);

            gameInitializator.InitializeWinPosition();
            gameInitializator.PairAllChips();//����� ���� ��� ���� ����� � �������
            gameInitializator.FindNeighbors();//����� ������� ��� ���� ������
            gameInitializator.SubscribeCells();//����������� �� ������� ���� ������
        }
    }
}