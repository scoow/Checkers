namespace Checkers
{
    public interface IObserver
    {
        void RecieveTurn(string move);
        string SendTurn();
        bool HaveMoves();
    }

    public enum ActionType
    {
        selects,
        moves,
        takes
    }
}