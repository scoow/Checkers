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
        private CameraManager cameraManager;

        private void Start()
        {
            cameraManager = FindObjectOfType<CameraManager>();

            _cellComponents = FindObjectsOfType<CellComponent>().ToList();
            _сhipComponents = FindObjectsOfType<ChipComponent>().ToList();

            gameInitializator = new(_cellComponents, _сhipComponents);

            checkersLogic = new (gameInitializator, cameraManager, _сhipComponents);

            selectManager = new(checkersLogic, _cellComponents, _сhipComponents);

            gameInitializator.InitializeWinPosition();
            gameInitializator.PairAllChips();//найти пару для всех шашек и связать
            gameInitializator.FindNeighbors();//найти соседей для всех клеток
            selectManager.SubscribeCells();//подписаться на события всех клеток
        }
    }
}