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
        private List<ChipComponent> _�hipComponents;

        private SelectManager _selectManager;
        private GameInitializator _gameInitializator;
        private CheckersLogic _checkersLogic;
        private CameraManager _cameraManager;

        private IObserver _observer;
        private void Start()
        {
            _observer = new Observer(inReplayMode);//�����������

            _cameraManager = FindObjectOfType<CameraManager>();

            _cellComponents = FindObjectsOfType<CellComponent>().ToList();
            _�hipComponents = FindObjectsOfType<ChipComponent>().ToList();

            _gameInitializator = new(_cellComponents, _�hipComponents);
            _gameInitializator.InitializeWinPosition();//����� ��������� ���� ������
            _gameInitializator.PairAllChips();//����� ���� ��� ���� ����� � �������
            _gameInitializator.FindNeighbors();//����� ������� ��� ���� ������

            _checkersLogic = new(_gameInitializator, _cameraManager, _�hipComponents);

            _selectManager = new(_checkersLogic, _observer, _cellComponents, _�hipComponents);
            _selectManager.SubscribeCells();//����������� �� ������� ���� ������

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