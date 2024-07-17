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
        // fixes percistent state bug
        if (aShopper.PreviousStateType != null) 
        { 
            aShopper.EnterQueue();
        }

        switch (aShopper.state.currentShopDestination)
        {
            case ShopLocationType.TarotReading:
            {
                switch (aShopper.state.currentQueueType)
                {
                    case ShopperQueueType.Purchase:
                        aShopper.state.currentQueueType = ShopperQueueType.Action;
                        break;
                    case ShopperQueueType.Action:
                    case ShopperQueueType.PostActionPay:
                    default:
                        aShopper.state.currentQueueType = ShopperQueueType.PostActionPay;
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
        if (!aShopper.state.hasReachedDestination) { return; }
        if (!aShopper.state.hasInteractedWithPlayer) { return; }

        switch (aShopper.state.currentQueueType)
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
        aShopper.SetDestination(aShopper.state.currentShopDestination, true);
    }

    void IState<ShopperBehaviour>.Exit(ShopperBehaviour aShopper)
    {
        aShopper.state.currentQueueType = ShopperQueueType.PostActionPay;
    }

    void IState<ShopperBehaviour>.Update(ShopperBehaviour aShopper)
    {
        if (!aShopper.state.hasReachedDestination) { return; }
        if (!aShopper.state.hasPerformedAction) { return; }

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
        if (!aShopper.state.hasReachedDestination) { return; }
        aShopper.DestroySelf();
    }
}

class DecisionState : IState<ShopperBehaviour>
{
    private static readonly float MIN_DECISION_INTERVAL = 1.0f;
    private static readonly float MAX_DECISION_INTERVAL = 5.0f;
    private static readonly float DECISION_ODDS = 0.35f;
    private static readonly float SATISFIED_ODDS = 0.25f;

    private void RandomizeDecision(ShopperBehaviour aShopper)
    {
        aShopper.state.decideTimer = 0.0f;
        aShopper.state.decideInterval = UnityEngine.Random.Range(MIN_DECISION_INTERVAL, MAX_DECISION_INTERVAL);
        aShopper.state.currentShopDestination = ShopManager.GetRandomType(); // its fine if we randomize to the same destination, since we'll just decide again later
    }

    void IState<ShopperBehaviour>.Enter(ShopperBehaviour aShopper)
    {
        // fixes percistent state bug
        if (aShopper.PreviousStateType == null) { return; }
        RandomizeDecision(aShopper);
    }

    void IState<ShopperBehaviour>.Exit(ShopperBehaviour aShopper) { }

    void IState<ShopperBehaviour>.Update(ShopperBehaviour aShopper)
    {
        if (!aShopper.state.hasReachedDestination) { return; }

        aShopper.state.decideTimer += Time.deltaTime;
        if (aShopper.state.decideTimer < aShopper.state.decideInterval) { return; }

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

public class ShopperState
{
    public Vector3 currentDestination;
    public bool hasReachedDestination;
    public bool isInQueue;
    public bool hasInteractedWithPlayer;
    public bool hasPerformedAction;
    public ShopperQueueType currentQueueType;
    public ShopLocationType currentShopDestination;
    public System.Type currentStateType;
    public int queueIndex;

    public float decideTimer = 0.0f;
    public float decideInterval = 5.0f;
}

[RequireComponent(typeof(NavMeshAgent))]
public class ShopperBehaviour : MonoBehaviour
{
    private StateMachine<ShopperBehaviour> _stateMachine;
    private NavMeshAgent _agent;
    private ShopManager _shopManager;
    public ShopperState state = new();

    public ShopManager ShopManager => _shopManager;
    public System.Type CurrentStateType => _stateMachine.CurrentState.GetType();

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
        state.hasInteractedWithPlayer = true;
    }

    public System.Type PreviousStateType => _stateMachine.PreviousStateType;

    public void ChangeState(System.Type aStateType)
    {
        _stateMachine.ChangeState(aStateType);
        state.currentStateType = aStateType;
    }

    public void LeaveShop()
    {
        state.currentDestination = _shopManager.GetEntrancePosition();
        _agent.SetDestination(state.currentDestination);
        state.hasReachedDestination = false;
    }
    public void EnterQueue()
    {
        state.queueIndex = _shopManager.Queue.Enter(this);
        OnQueueUpdate(state.queueIndex);
        state.isInQueue = true;
        state.hasInteractedWithPlayer = false;
    }
    public void LeaveQueue()
    {
        if (!state.isInQueue) { return; }

        _shopManager.Queue.Leave();
        state.isInQueue = false;
        state.queueIndex = -1;
    }

    public void OnQueueUpdate(int aValue)
    {
        SetCurrentDestination(_shopManager.Queue.GetQueuePosition(aValue));
        state.queueIndex = aValue;
    }

    public void SetCurrentDestination(Vector3 aDestination)
    {
        state.currentDestination = aDestination;
        _agent.SetDestination(state.currentDestination);
    }

    public void Init(ShopManager aShopManager, System.Type aInitialState)
    {
        _agent = GetComponent<NavMeshAgent>();
        _shopManager = aShopManager;
        _agent.avoidancePriority = 0;
        state.queueIndex = -1;

        _stateMachine = new StateMachine<ShopperBehaviour>(this);
        {
            _stateMachine.AddState(typeof(DecisionState), new DecisionState());
            _stateMachine.AddState(typeof(QueueState), new QueueState());
            _stateMachine.AddState(typeof(ActionState), new ActionState());
            _stateMachine.AddState(typeof(LeavingState), new LeavingState());
        }
        _stateMachine.ChangeState(aInitialState);
    }

    public void SetDestination(ShopLocationType aType, bool aUseInteractLocation = false)
    {
        Vector3 destination = aUseInteractLocation ? 
            _shopManager.GetShopPosition(aType).NavAgentInteractLocation.position :
            _shopManager.GetShopPosition(aType).NavAgentWindowShopLocation.position;

        if (state.currentDestination == destination) { return; }

        state.hasReachedDestination = false;
        state.currentShopDestination = aType;

        SetCurrentDestination(destination);
    }

    public void PerformAction()
    {
        state.hasPerformedAction = true; // TEMP?
        print("Performing action");
    }

    public void DoUpdate()
    {
        _stateMachine.Update();

        var delta = state.currentDestination - transform.position;
        state.hasReachedDestination = delta.magnitude < 1.0f;
    }
}
