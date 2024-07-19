using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TarotDeckBehaviour : MonoBehaviour
{
    [SerializeField] private List<GameObject> tarotCardMeshes;
    [SerializeField] private Transform _tarotSpawnPosition;
    [SerializeField] private Transform _tarotFinalCenterPosition;
    [SerializeField] private GameObject _tarotCardPrefab;
    [SerializeField] private int _tarotCardsToSpawn = 3;
    [SerializeField] private float _tarotCardSpacing = 0.5f;
    [SerializeField] private float _tarotCardMoveDelayOnSpawn = 0.5f;
    [SerializeField] private float _tarotCardMaxMoveTime = 1.0f;

    private List<CardType> _cards = new();
    private int _tarotCardsSpawned = 0;

    public int TarotCardsToSpawn => _tarotCardsToSpawn;

    private Vector3 GetTarotSpawnPosition(int aIndex)
    {
        return _tarotFinalCenterPosition.position + aIndex * _tarotCardSpacing * _tarotFinalCenterPosition.right;
    }

    private void Awake()
    {
        int cardCount = Enum.GetValues(typeof(CardType)).Length;
        for (int i = 0; i < cardCount; i++)
        {
            _cards.Add((CardType)i);
        }
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

    public void OnClick()
    {
        if (_tarotCardsSpawned >= _tarotCardsToSpawn) { return; }

        StartCoroutine(SpawnCard(_tarotCardsSpawned));
        ++_tarotCardsSpawned;
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
