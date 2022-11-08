namespace Checkers
{
    public interface IObserver
    {
        void RecieveTurn(ColorType player, ActionType actionType, string cell);
        void RecieveTurn(ColorType player, ActionType actionType, string startCell, string endCell);
    }

    public enum ActionType
    {
        selects,
        moves,
        takes
    }
}