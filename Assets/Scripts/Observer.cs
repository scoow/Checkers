using Checkers;
using System.IO;
using UnityEngine;


namespace Checkers
{
    public class Observer : IObserver
    {
        private bool _isReplayMode = false;
        private string path = "log.txt";
        private bool _fileExists;
        public Observer(bool isReplayMode)
        {
            _isReplayMode = isReplayMode;
        }
        void IObserver.RecieveTurn(ColorType player, ActionType actionType, string cell)
        {
            //Debug.Log("GG");

            using (StreamWriter sw = new(path, true))
            {
                sw.WriteLine(player.ToString() + " " + actionType.ToString() + " " + cell);
            } 
        }
        void IObserver.RecieveTurn(ColorType player, ActionType actionType, string startCell, string endCell)
        {
            //
        }
    }
}