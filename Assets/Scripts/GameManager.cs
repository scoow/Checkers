using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Checkers
{
    public class GameManager : MonoBehaviour
    {
        private List<CellComponent> _cellComponents;
        private List<ChipComponent> _сhipComponents;

        private SelectManager selectManager;
        private GameInitializator gameInitializator;
        private CheckersLogic checkersLogic;

        //private CameraManager _playerCamera;



        private void Start()
        {
            //_playerCamera = FindObjectOfType<CameraManager>();

            _cellComponents = FindObjectsOfType<CellComponent>().ToList();
            _сhipComponents = FindObjectsOfType<ChipComponent>().ToList();


            selectManager = new(_cellComponents, _сhipComponents);
            gameInitializator = new(selectManager, _cellComponents, _сhipComponents);
            checkersLogic = new (selectManager, gameInitializator, _cellComponents, _сhipComponents);
            selectManager.GetLogic(checkersLogic);

            gameInitializator.InitializeWinPosition();
            gameInitializator.PairAllChips();//найти пару для всех шашек и связать
            gameInitializator.FindNeighbors();//найти соседей для всех клеток
            gameInitializator.SubscribeCells();//подписаться на события всех клеток
        }
    }
}