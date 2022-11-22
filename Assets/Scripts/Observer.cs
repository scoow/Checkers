using System;
using System.Collections.Generic;
using System.IO;

namespace Checkers
{
    public class Observer : IObserver
    {
        private readonly bool _isReplayMode = false;
        private readonly string _path = "log.txt";
        private readonly Queue<string> _movesQueque = new();

        public Observer(bool isReplayMode)
        {
            _isReplayMode = isReplayMode;
            if (File.Exists(_path))
                if (!_isReplayMode)//Если в режиме записи - удалить старый файл перед записью
                {
                    File.Delete(_path);
                }
                else//иначе - закэшировать данные из файла в очередь
                {
                    _movesQueque = new Queue<string>();
                    ReadFromFileToQueque();
                }
        }

        private void ReadFromFileToQueque()
        {
            using (StreamReader sr = new(_path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    _movesQueque.Enqueue(line);
                }
            }
        }

        public bool HaveMoves()
        {
            return _movesQueque.Count > 0;
        }

        void IObserver.RecieveTurn(string move)
        {
            if (!_isReplayMode)
                using (StreamWriter sw = new(_path, true))
                {
                    sw.WriteLine(move);
                }
        }

        public string SendTurn()
        {
            return _movesQueque.Dequeue();
        }
    }
}