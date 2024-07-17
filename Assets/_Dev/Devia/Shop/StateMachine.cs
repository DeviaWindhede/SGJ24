
using System.Collections.Generic;

public interface IState<T> where T : class
{
    public void Enter(T aOwner);
    public void Exit(T aOwner);
    public void Update(T aOwner);
}

public class StateMachine<T> where T : class
{
    private T _owner;
    private Dictionary<System.Type, IState<T>> _states = new();
    private IState<T> _currentState;

    public IState<T> CurrentState => _currentState;

    public StateMachine(T aOwner)
    {
        _owner = aOwner;
    }

    public void AddState(System.Type aStateType, IState<T> aState)
    {
        _states.Add(aStateType, aState);
    }

    public void ChangeState(System.Type newState)
    {
        _currentState?.Exit(_owner);
        _currentState = _states[newState];
        _currentState.Enter(_owner);
    }

    public void OnDestroy()
    {
        _currentState?.Exit(_owner);
        _states.Clear();
    }

    public void Update()
    {
        _currentState?.Update(_owner);
    }
}
