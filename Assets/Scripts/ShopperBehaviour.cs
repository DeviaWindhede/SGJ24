using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

class QueueState : IState<ShopperBehaviour>
{
    private readonly static int MIN_GOOBER_PRICE = 10;
    private readonly static int MAX_GOOBER_PRICE = 50;

    private void Leave(ShopperBehaviour aShopper)
    {
        aShopper.LeaveQueue();
        aShopper.ChangeState(typeof(LeavingState));
    }

    private void LeaveAndPay(ShopperBehaviour aShopper)
    {
        int amount = 0;

        switch (aShopper.state.currentShopDestination)
        {
            case ShopLocationType.TarotReading:
                if (aShopper.state.currentQueueType == ShopperQueueType.PostActionPay)
                {
                    aShopper.Pay(PersistentShopData.Instance.shopManagerState.tarotPrice);
                }

                Leave(aShopper);
                return;
            case ShopLocationType.Potions:
                amount = 10;
                break;
            case ShopLocationType.GooberAdoption:
                float percentage = 0.0f;
                for (int i = 0; i < PersistentShopData.Instance.shopResources.goobers.Count; i++)
                {
                    var goober = PersistentShopData.Instance.shopResources.goobers[i];
                    if (!goober.isUnlocked) { continue; }
                    if (goober.isClaimed) { continue; }

                    percentage = goober.HappinessPercentage;
                    goober.isClaimed = true;

                    break;
                }
                amount = (int)Mathf.Lerp(MIN_GOOBER_PRICE, MAX_GOOBER_PRICE, percentage);
                break;
            case ShopLocationType.Enchanting:
                amount = 30;
                break;
            default:
                break;
        }

        aShopper.Pay(amount);
        Leave(aShopper);
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
                    default:
                        break;
                }
                break;
            }
            case ShopLocationType.Potions:
            {
                aShopper.state.potionType = (PotionType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(PotionType)).Length);
                break;
            }
            case ShopLocationType.GooberAdoption:
            case ShopLocationType.Enchanting:
            default:
                break;
        }
        aShopper.SetAvoidancePriority(3);
    }

    void IState<ShopperBehaviour>.Exit(ShopperBehaviour aShopper)
    {
        aShopper.SetAvoidancePriority(ShopperBehaviour.DEFAULT_AVOIDANCE_PRIORITY);
        aShopper.ShopperWorldCanvas.SetSpeechBubbleIcon(ShopperIconType.None);
    }

    void IState<ShopperBehaviour>.Update(ShopperBehaviour aShopper)
    {
        if (!aShopper.state.hasReachedDestination) { return; }

        {
            ShopperIconType type = (ShopperIconType)aShopper.state.currentShopDestination;
            if (!aShopper.ShouldDisplayIcon) { type = ShopperIconType.None; }
            else if (aShopper.state.currentQueueType == ShopperQueueType.PostActionPay) { type = ShopperIconType.Paying; }

            if (aShopper.state.currentShopDestination == ShopLocationType.Potions)
            {
                aShopper.ShopperWorldCanvas.SetSpeechBubblePotionIcon(aShopper.state.potionType);
            }
            else
            {
                aShopper.ShopperWorldCanvas.SetSpeechBubbleIcon(type);
            }
        }

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
        LeaveAndPay(aShopper); // TODO: REMOVE PROPER AMOUNT
    }
}

class ActionState : IState<ShopperBehaviour>
{
    void IState<ShopperBehaviour>.Enter(ShopperBehaviour aShopper)
    {
        aShopper.SetDestination(aShopper.state.currentShopDestination, true);
        aShopper.state.currentQueueType = ShopperQueueType.PostActionPay;
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
        if (!aShopper.state.hasReachedDestination)
        {
            aShopper.SetAvoidancePriority(0);
            return;
        }
        aShopper.SetAvoidancePriority(99);
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
    public PotionType potionType;
    public System.Type currentStateType = typeof(DecisionState);
    public int queueIndex = -1;

    public int spendingGold = 0;
    public float decideTimer = 0.0f;
    public float decideInterval = 5.0f;
}

[RequireComponent(typeof(NavMeshAgent))]
public class ShopperBehaviour : MonoBehaviour
{
    public static readonly int DEFAULT_AVOIDANCE_PRIORITY = 10;

    [SerializeField] private Animator _animator;
    [SerializeField] private float _stoppingTime = 0.5f;

    private ShopperWorldCanvas _shopperWorldCanvas;
    private StateMachine<ShopperBehaviour> _stateMachine;
    private NavMeshAgent _agent;
    private ShopManager _shopManager;
    public ShopperState state = new();
    public float distanceToReachDestination = 0.75f;
    private float _agentSpeed;

    public ShopperWorldCanvas ShopperWorldCanvas => _shopperWorldCanvas;

    public ShopManager ShopManager => _shopManager;
    public System.Type CurrentStateType => _stateMachine.CurrentState.GetType();
    public bool ShouldDisplayIcon => _shopManager.IsStandingInRegister && state.queueIndex == 0;

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
        PersistentShopData.Instance.shopResources.AddCoins(aAmount);
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
        _shopperWorldCanvas = GetComponent<ShopperWorldCanvas>();
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

    // TODO: OM N�GON V�NTAR P� TAROT CARD OCH MAN �PPNAR ETT MINIGAME S� CRASHAR DET P� V�GEN TBX
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
