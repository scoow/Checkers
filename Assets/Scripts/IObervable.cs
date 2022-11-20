
using Checkers;

public interface IObervable
{
    public event MoveEventHandler OnMoveEventHandler;
    public delegate void MoveEventHandler(ColorType player, ActionType actionType, string cell);
}
