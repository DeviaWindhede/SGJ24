using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TarotDeckBehaviour : MonoBehaviour
{
    private static readonly float WORST_ZONE = 450.0f / 800.0f;
    private static readonly float OKAY_ZONE = 250.0f / 800.0f;
    private static readonly float GOOD_ZONE = 150.0f / 800.0f;
    private static readonly float BEST_ZONE = 50.0f / 800.0f;

    [SerializeField] private List<GameObject> tarotCardMeshes;
    [SerializeField] private Transform _tarotSpawnPosition;
    [SerializeField] private Transform _tarotFinalCenterPosition;
    [SerializeField] private GameObject _tarotCardPrefab;
    [SerializeField] private int _tarotCardsToSpawn = 3;
    [SerializeField] private float _tarotCardSpacing = 0.5f;
    [SerializeField] private float _tarotCardMoveDelayOnSpawn = 0.5f;
    [SerializeField] private float _tarotCardMaxMoveTime = 1.0f;

    [Header("Indicator")]
    [SerializeField] private GameObject _progressBarGameObject;
    [SerializeField] private GameObject _innerBar;
    [SerializeField] private GameObject _indicator;
    [SerializeField] private float _width = 800.0f;
    [SerializeField] private float _indicatorSpeed = 1.0f;

    private Animator _animator;

    private List<CardType> _cards = new();
    private int _tarotCardsSpawned = 0;
    private bool _inGame;
    private float _targetPercentage;
    private float _indicatorPercentage;
    private float _totalHitPercentage;

    public int TarotCardsToSpawn => _tarotCardsToSpawn;
    public float TotalHitPercentage => _totalHitPercentage;

    private Vector3 GetTarotSpawnPosition(int aIndex)
    {
        return _tarotFinalCenterPosition.position + aIndex * _tarotCardSpacing * _tarotFinalCenterPosition.right;
    }

    private void Awake()
    {
        _animator = _progressBarGameObject.GetComponent<Animator>();

        int cardCount = Enum.GetValues(typeof(CardType)).Length;
        for (int i = 0; i < cardCount; i++)
        {
            _cards.Add((CardType)i);
        }

        RandomizeIndicator();
    }

    private void OnOutsideDeckClick()
    {
        if (!_inGame) { return; }

        _inGame = false;
        StartCoroutine(SpawnCard(_tarotCardsSpawned));
        ++_tarotCardsSpawned;
        _animator.SetTrigger("Disable");

        float delta = Mathf.Abs(_indicatorPercentage - _targetPercentage);

        if (delta > WORST_ZONE)
            delta = 0.0f;
        else if (delta > OKAY_ZONE)
            delta = 0.25f;
        else if (delta > GOOD_ZONE)
            delta = 0.5f;
        else if (delta > BEST_ZONE)
            delta = 0.75f;
        else
            delta = 1.0f;

        _totalHitPercentage += delta;

        if (_tarotCardsSpawned < _tarotCardsToSpawn) { return; }

        _totalHitPercentage = _totalHitPercentage / _tarotCardsToSpawn;
    }

    private void RandomizeIndicator()
    {
        _targetPercentage = UnityEngine.Random.Range(0.0f, 1.0f);
        RectTransform rect = _innerBar.GetComponent<RectTransform>();
        var position = rect.localPosition;
        position.x = Mathf.Lerp(-_width / 2.0f, _width / 2.0f, _targetPercentage);
        rect.localPosition = position;
    }

    private IEnumerator SpawnCard(int aIndex)
    {
        GameObject tarotCard = Instantiate(
            _tarotCardPrefab,
            _tarotSpawnPosition.position,
            _tarotSpawnPosition.rotation
        );


        TarotCardBehaviour cardBehaviour = tarotCard.GetComponent<TarotCardBehaviour>();
        {
            int index = UnityEngine.Random.Range(0, _cards.Count);
            CardType type = _cards[index];
            _cards.RemoveAt(index);
            cardBehaviour.Init(type, tarotCardMeshes[(int)type]);
        }

        AudioManager.Instance.PlaySound(ShopSoundByte.CardDraw);
        yield return new WaitForSeconds(_tarotCardMoveDelayOnSpawn);

        Vector3 endPosition = GetTarotSpawnPosition(aIndex);
        float distance = Vector3.Distance(_tarotSpawnPosition.position, endPosition);

        Vector3 delta = _tarotSpawnPosition.position - GetTarotSpawnPosition(0);
        float maxDistance = delta.magnitude;
        float moveTime = Mathf.Lerp(0.0f, _tarotCardMaxMoveTime, distance / maxDistance);

        float elapsedTime = 0.0f;
        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / moveTime;
            tarotCard.transform.SetPositionAndRotation(
                Vector3.Slerp(_tarotSpawnPosition.position, endPosition, t),
                Quaternion.Slerp(_tarotSpawnPosition.rotation, Quaternion.identity, t)
            );

            yield return null;
        }

        tarotCard.transform.position = endPosition;
        cardBehaviour.FinishMove();

        yield return null;
    }

    public void OnClick(bool aIsOnDeck)
    {
        if (!_inGame && aIsOnDeck)
        {
            OnDeckClick();
            return;
        }

        OnOutsideDeckClick();
    }

    private void OnDeckClick()
    {
        if (_tarotCardsSpawned >= _tarotCardsToSpawn) { return; }
        if (_inGame) {  return; }


        RandomizeIndicator();
        _inGame = true;
        _animator.SetTrigger("Enable");
    }

    private void Update()
    {
        if (!_inGame) { return; }

        _indicatorPercentage = Mathf.PingPong(Time.time * _indicatorSpeed, 1.0f);
        {
            RectTransform rect = _indicator.GetComponent<RectTransform>();
            var position = rect.localPosition;
            position.x = Mathf.Lerp(-_width / 2.0f, _width / 2.0f, _indicatorPercentage);
            rect.localPosition = position;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        for (int i = 0; i < _tarotCardsToSpawn; i++)
        {
            Gizmos.DrawWireCube(GetTarotSpawnPosition(i), new Vector3(0.11f, 0.01f, 0.21f));
        }
        Gizmos.DrawWireSphere(_tarotFinalCenterPosition.position, 0.01f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_tarotSpawnPosition.position, 0.01f);
    }
}
