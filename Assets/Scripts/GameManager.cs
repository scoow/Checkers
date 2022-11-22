using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Checkers
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private bool inReplayMode;

        private List<CellComponent> _cellComponents;
        private List<ChipComponent> _сhipComponents;

        private SelectManager _selectManager;
        private GameInitializator _gameInitializator;
        private CheckersLogic _checkersLogic;
        private CameraManager _cameraManager;

        private IObserver _observer;
        private void Start()
        {
            _observer = new Observer(inReplayMode);//наблюдатель

            _cameraManager = FindObjectOfType<CameraManager>();

            _cellComponents = FindObjectsOfType<CellComponent>().ToList();
            _сhipComponents = FindObjectsOfType<ChipComponent>().ToList();

            _gameInitializator = new(_cellComponents, _сhipComponents);
            _gameInitializator.InitializeWinPosition();//найти последние ряды клеток
            _gameInitializator.PairAllChips();//найти пару для всех шашек и связать
            _gameInitializator.FindNeighbors();//найти соседей для всех клеток

            _checkersLogic = new(_gameInitializator, _cameraManager, _сhipComponents);

            _selectManager = new(_checkersLogic, _observer, _cellComponents, _сhipComponents);
            _selectManager.SubscribeCells();//подписаться на события всех клеток

            if (inReplayMode)
                _cameraManager.DisableRaycaster();
        }
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                if (inReplayMode)
                    if (_observer.HaveMoves())
                    {
                        _selectManager.ReplayMove();
                    }
            }
        }
    }
}