using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

class QueueState : IState<ShopperBehaviour>
{
    private void LeaveAndPay(ShopperBehaviour aShopper, int aAmount)
    {
        aShopper.Pay(aAmount);
        aShopper.LeaveQueue();
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
        aShopper.SetAvoidancePriority(0);
    }

    void IState<ShopperBehaviour>.Exit(ShopperBehaviour aShopper)
    {
        aShopper.SetAvoidancePriority(ShopperBehaviour.DEFAULT_AVOIDANCE_PRIORITY);
    }

    void IState<ShopperBehaviour>.Update(ShopperBehaviour aShopper)
    {
        if (!aShopper.state.hasReachedDestination) { return; }
        if (!aShopper.state.hasInteractedWithPlayer) { return; }

        switch (aShopper.state.currentQueueType)
        {
            case ShopperQueueType.Action:
                aShopper.LeaveQueue();
                aShopper.ChangeState(typeof(ActionState));
                return;
            case ShopperQueueType.Purchase:
                // TODO: REMOVE RESOURCES
                aShopper.LeaveQueue();
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
        aShopper.SetAvoidancePriority(99);
    }

    void IState<ShopperBehaviour>.Exit(ShopperBehaviour aShopper)
    {
        aShopper.state.currentQueueType = ShopperQueueType.PostActionPay;
        
        var interactLocation = aShopper.ShopManager.GetShopPosition(aShopper.state.currentShopDestination).NavAgentInteractLocation;
        aShopper.transform.position = interactLocation.position;
        aShopper.SetAvoidancePriority(ShopperBehaviour.DEFAULT_AVOIDANCE_PRIORITY);
    }

    void IState<ShopperBehaviour>.Update(ShopperBehaviour aShopper)
    {
        if (!aShopper.state.hasReachedDestination) { return; }
        if (!aShopper.state.hasPerformedAction)
        { 
            if (aShopper.state.currentShopDestination == ShopLocationType.TarotReading)
            {
                aShopper.transform.position = aShopper.ShopManager.ShopLocations.TarotShopperSeat.position;
                aShopper.SetCurrentDestination(aShopper.transform.position);
            }
            return;
        }

        aShopper.state.hasPerformedAction = false;
        aShopper.ChangeState(typeof(QueueState));
    }
}

class LeavingState : IState<ShopperBehaviour>
{
    void IState<ShopperBehaviour>.Enter(ShopperBehaviour aShopper)
    {
        aShopper.LeaveShop();
        aShopper.distanceToReachDestination = 0.75f;

        if (PersistentShopData.Instance.shopTime.IsNight)
        {
            aShopper.SetAvoidancePriority(99);
        }
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
    private static readonly float DECISION_ODDS = 0.5f;
    private static readonly float SATISFIED_ODDS = 0.25f;

    private void RandomizeDecision(ShopperBehaviour aShopper)
    {
        aShopper.state.decideTimer = 0.0f;
        aShopper.state.decideInterval = UnityEngine.Random.Range(MIN_DECISION_INTERVAL, MAX_DECISION_INTERVAL);
        aShopper.state.hasReachedDestination = false;
        aShopper.SetDestination(ShopManager.GetRandomType(), false); // its fine if we randomize to the same destination, since we'll just decide again later
    }

    // we dont set destination here, it is done in the SpawnShopper function within the manager
    void IState<ShopperBehaviour>.Enter(ShopperBehaviour aShopper) { }

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
    public System.Type currentStateType = typeof(DecisionState);
    public int queueIndex = -1;

    public float decideTimer = 0.0f;
    public float decideInterval = 5.0f;
}

[RequireComponent(typeof(NavMeshAgent))]
public class ShopperBehaviour : MonoBehaviour
{
    public static readonly int DEFAULT_AVOIDANCE_PRIORITY = 10;

    [SerializeField] private Animator _animator;
    [SerializeField] private float _stoppingTime = 0.5f;

    private StateMachine<ShopperBehaviour> _stateMachine;
    private NavMeshAgent _agent;
    private ShopManager _shopManager;
    public ShopperState state = new();
    public float distanceToReachDestination = 0.75f;
    private float _agentSpeed;

    public ShopManager ShopManager => _shopManager;
    public System.Type CurrentStateType => _stateMachine.CurrentState.GetType();

    public void SetAvoidancePriority(int aPriority)
    {
        _agent.avoidancePriority = aPriority;
    }

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
        if (CurrentStateType == typeof(ActionState))
            state.hasPerformedAction = true;
        else
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

    public void Init(ShopManager aShopManager, ShopperState aState)
    {
        _agent = GetComponent<NavMeshAgent>();
        _agentSpeed = _agent.speed;
        _shopManager = aShopManager;
        SetAvoidancePriority(DEFAULT_AVOIDANCE_PRIORITY);
        state = aState;

        _stateMachine = new StateMachine<ShopperBehaviour>(this);
        {
            _stateMachine.AddState(typeof(DecisionState), new DecisionState());
            _stateMachine.AddState(typeof(QueueState), new QueueState());
            _stateMachine.AddState(typeof(ActionState), new ActionState());
            _stateMachine.AddState(typeof(LeavingState), new LeavingState());
        }
        ChangeState(state.currentStateType);
    }

    // TODO: OM NÅGON VÄNTAR PÅ TAROT CARD OCH MAN ÖPPNAR ETT MINIGAME SÅ CRASHAR DET PÅ VÄGEN TBX
    public void SetDestination(ShopLocationType aType, bool aUseInteractLocation = false)
    {
        Vector3 destination = aUseInteractLocation ? 
            _shopManager.GetShopPosition(aType).NavAgentInteractLocation.position :
            _shopManager.GetShopPosition(aType).NavAgentWindowShopLocation.position;

        if (state.currentDestination == destination) { return; }

        state.hasReachedDestination = false;
        state.currentShopDestination = aType;
        _agent.speed = _agentSpeed;

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
        state.hasReachedDestination = delta.magnitude < distanceToReachDestination;
        if (state.hasReachedDestination)
        {
            float maxDelta = Time.deltaTime / _stoppingTime;
            _agent.velocity = Vector3.MoveTowards(_agent.velocity, Vector3.zero, maxDelta);
        }

        _animator.SetFloat("Speed", _agent.velocity.magnitude);
    }

    private void OnDestroy()
    {
        _stateMachine.OnDestroy();
    }
}
