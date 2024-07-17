using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

class QueueState : IState<ShopperBehaviour>
{
    private void LeaveAndPay(ShopperBehaviour aShopper, int aAmount)
    {
        aShopper.Pay(aAmount);
        aShopper.ChangeState(typeof(LeavingState));
    }

    void IState<ShopperBehaviour>.Enter(ShopperBehaviour aShopper)
    {
        aShopper.EnterQueue();

        switch (aShopper.currentShopDestination)
        {
            case ShopLocationType.TarotReading:
            {
                switch (aShopper.currentQueueType)
                {
                    case ShopperQueueType.Purchase:
                        aShopper.currentQueueType = ShopperQueueType.Action;
                        break;
                    case ShopperQueueType.Action:
                    case ShopperQueueType.PostActionPay:
                    default:
                        aShopper.currentQueueType = ShopperQueueType.PostActionPay;
                        break;
                }
                break;
            }
            case ShopLocationType.Potions:
            case ShopLocationType.GooberAdoption:
            case ShopLocationType.Enchanting:
            default:
                break;
        }
    }

    void IState<ShopperBehaviour>.Exit(ShopperBehaviour aShopper)
    {
        aShopper.LeaveQueue();
    }

    void IState<ShopperBehaviour>.Update(ShopperBehaviour aShopper)
    {
        if (!aShopper.HasReachedDestination) { return; }
        if (!aShopper.HasInteractedWithPlayer) { return; }

        switch (aShopper.currentQueueType)
        {
            case ShopperQueueType.Action:
                aShopper.ChangeState(typeof(ActionState));
                return;
            case ShopperQueueType.Purchase:
                // TODO: REMOVE RESOURCES
                break;
            case ShopperQueueType.PostActionPay:
            default:
                break;
        }
        LeaveAndPay(aShopper, 10); // TODO: REMOVE PROPER AMOUNT
    }
}

class ActionState : IState<ShopperBehaviour>
{
    void IState<ShopperBehaviour>.Enter(ShopperBehaviour aShopper)
    {
        aShopper.SetDestination(aShopper.currentShopDestination, true);
    }

    void IState<ShopperBehaviour>.Exit(ShopperBehaviour aShopper)
    {
        aShopper.currentQueueType = ShopperQueueType.PostActionPay;
    }

    void IState<ShopperBehaviour>.Update(ShopperBehaviour aShopper)
    {
        if (!aShopper.HasReachedDestination) { return; }
        if (!aShopper.HasPerformedAction) { return; }

        aShopper.ChangeState(typeof(QueueState));
    }
}

class LeavingState : IState<ShopperBehaviour>
{
    void IState<ShopperBehaviour>.Enter(ShopperBehaviour aShopper)
    {
        aShopper.LeaveShop();
    }

    void IState<ShopperBehaviour>.Exit(ShopperBehaviour aShopper) { }

    void IState<ShopperBehaviour>.Update(ShopperBehaviour aShopper)
    {
        if (!aShopper.HasReachedDestination) { return; }
        aShopper.DestroySelf();
    }
}

class DecisionState : IState<ShopperBehaviour>
{
    private static readonly float MIN_DECISION_INTERVAL = 1.0f;
    private static readonly float MAX_DECISION_INTERVAL = 5.0f;
    private static readonly float DECISION_ODDS = 0.35f;
    private static readonly float SATISFIED_ODDS = 0.25f;

    private float _decideTimer = 0.0f;
    private float _decideInterval = 5.0f;

    private void RandomizeDecision(ShopperBehaviour aShopper)
    {
        _decideTimer = 0.0f;
        _decideInterval = UnityEngine.Random.Range(MIN_DECISION_INTERVAL, MAX_DECISION_INTERVAL);
        aShopper.currentShopDestination = ShopManager.GetRandomType(); // its fine if we randomize to the same destination, since we'll just decide again later
    }

    void IState<ShopperBehaviour>.Enter(ShopperBehaviour aShopper)
    {
        RandomizeDecision(aShopper);
    }

    void IState<ShopperBehaviour>.Exit(ShopperBehaviour aShopper) { }

    void IState<ShopperBehaviour>.Update(ShopperBehaviour aShopper)
    {
        if (!aShopper.HasReachedDestination) { return; }

        _decideTimer += Time.deltaTime;
        if (_decideTimer < _decideInterval) { return; }

        if (UnityEngine.Random.Range(0, 1.0f) < DECISION_ODDS)
        {
            aShopper.ChangeState(typeof(QueueState));
            return;
        }

        if (UnityEngine.Random.Range(0, 1.0f) < SATISFIED_ODDS)
        {
            aShopper.ChangeState(typeof(LeavingState));
            return;
        }

        RandomizeDecision(aShopper);
    }
}


public enum ShopperQueueType
{
    Purchase, // eg potions, goober rental
    Action, // eg request to tarot read
    PostActionPay, // eg pay for tarot reading action
}

[RequireComponent(typeof(NavMeshAgent))]
public class ShopperBehaviour : MonoBehaviour
{
    private StateMachine<ShopperBehaviour> _stateMachine;
    private NavMeshAgent _agent;

    private Vector3 _currentDestination;
    private ShopManager _shopManager;
    private bool _hasReachedDestination;
    private bool _isInQueue;
    private bool _hasInteractedWithPlayer;
    private bool _hasPerformedAction;

    [HideInInspector] public ShopperQueueType currentQueueType;
    [HideInInspector] public ShopLocationType currentShopDestination;
    public ShopManager ShopManager => _shopManager;
    public bool HasReachedDestination => _hasReachedDestination;
    public bool HasInteractedWithPlayer => _hasInteractedWithPlayer;
    public bool HasPerformedAction => _hasPerformedAction;

    public void DestroySelf()
    {
        _shopManager.DestroyShopper(gameObject);
    }

    public void Pay(int aAmount)
    {
        print("Paying " + aAmount + " coins");
    }

    public void Interact()
    {
        _hasInteractedWithPlayer = true;
    }

    public void ChangeState(System.Type aStateType)
    {
        _stateMachine.ChangeState(aStateType);
    }

    public void LeaveShop()
    {
        _currentDestination = _shopManager.GetEntrancePosition();
        _agent.SetDestination(_currentDestination);
        _hasReachedDestination = false;
    }
    public void EnterQueue()
    {
        int index = _shopManager.Queue.Enter(this);
        OnQueueUpdate(index);
        _isInQueue = true;
        _hasInteractedWithPlayer = false;
    }
    public void LeaveQueue()
    {
        if (!_isInQueue) { return; }

        _shopManager.Queue.Leave();
        _isInQueue = false;
    }

    public void OnQueueUpdate(int aValue)
    {
        _currentDestination = _shopManager.Queue.GetQueuePosition(aValue);
        _agent.SetDestination(_currentDestination);
    }

    public void Init(ShopManager aShopManager)
    {
        _stateMachine = new StateMachine<ShopperBehaviour>(this);
        {
            _stateMachine.AddState(typeof(DecisionState), new DecisionState());
            _stateMachine.AddState(typeof(QueueState), new QueueState());
            _stateMachine.AddState(typeof(ActionState), new ActionState());
            _stateMachine.AddState(typeof(LeavingState), new LeavingState());
        }
        _stateMachine.ChangeState(typeof(DecisionState));

        _agent = GetComponent<NavMeshAgent>();
        _shopManager = aShopManager;
        _agent.avoidancePriority = 0;
    }

    public void SetDestination(ShopLocationType aType, bool aUseInteractLocation = false)
    {
        Vector3 destination = aUseInteractLocation ? 
            _shopManager.GetShopPosition(aType).NavAgentInteractLocation.position :
            _shopManager.GetShopPosition(aType).NavAgentWindowShopLocation.position;

        if (_currentDestination == destination) { return; }

        _hasReachedDestination = false;
        _currentDestination = destination;
        currentShopDestination = aType;

        _agent.SetDestination(_currentDestination);
    }

    public void PerformAction()
    {
        _hasPerformedAction = true; // TEMP?
        print("Performing action");
    }

    void Update()
    {
        _stateMachine.Update();
        print(_stateMachine.CurrentState);

        var delta = _currentDestination - transform.position;
        _hasReachedDestination = delta.magnitude < 1.0f;
    }
}
