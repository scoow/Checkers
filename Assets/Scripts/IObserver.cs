namespace Checkers
{
    public interface IObserver
    {
        void RecieveTurn(string move);//получить информацию о сделанном ходе
        string SendTurn();//передать информацию о сделанном ходе
        bool HaveMoves();//есть ли записанные ходы
    }

    public enum ActionType
    {
        selects,
        moves,
        takes
    }
}