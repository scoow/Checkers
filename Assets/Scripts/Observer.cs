using System.IO;
using UnityEngine;


namespace Checkers
{
    public class Observer : IObserver
    {
        private string path = "log.txt";
        public Observer()
        {
            if (!File.Exists(path))
            {
                File.Create(path);
            }
            using (File.OpenRead(path))
            using (StreamReader sr = new StreamReader(path))
            {
                sr.ReadToEnd();
            }

        }
        public void RecieveTurn()
        {
            Debug.Log("GG");
        }
    }
}