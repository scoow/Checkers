using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Checkers
{
    public class GameManager : MonoBehaviour
    {
        private List<CellComponent> _cellComponents;
        private List<ChipComponent> _сhipComponents;

        private SelectManager _selectManager;
        private GameInitializator _gameInitializator;
        private CheckersLogic _checkersLogic;
        private CameraManager _cameraManager;


       

        private void Start()
        {
            _cameraManager = FindObjectOfType<CameraManager>();

            _cellComponents = FindObjectsOfType<CellComponent>().ToList();
            _сhipComponents = FindObjectsOfType<ChipComponent>().ToList();

            _gameInitializator = new(_cellComponents, _сhipComponents);
            _gameInitializator.InitializeWinPosition();//найти последние ряды клеток
            _gameInitializator.PairAllChips();//найти пару для всех шашек и связать
            _gameInitializator.FindNeighbors();//найти соседей для всех клеток

            _checkersLogic = new (_gameInitializator, _cameraManager, _сhipComponents);

            _selectManager = new(_checkersLogic, _cellComponents, _сhipComponents);
            _selectManager.SubscribeCells();//подписаться на события всех клеток
        }
    }
}