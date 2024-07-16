using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TarotDeckBehaviour : MonoBehaviour
{
    [SerializeField] private Transform _tarotSpawnPosition;
    [SerializeField] private Transform _tarotFinalCenterPosition;
    [SerializeField] private GameObject _tarotCardPrefab;
    [SerializeField] private int _tarotCardsToSpawn = 3;
    [SerializeField] private float _tarotCardSpacing = 0.5f;
    [SerializeField] private float _tarotCardMoveDelayOnSpawn = 0.5f;
    [SerializeField] private float _tarotCardMaxMoveTime = 1.0f;

    private int _tarotCardsSpawned = 0;

    private Vector3 GetTarotSpawnPosition(int aIndex)
    {
        return _tarotFinalCenterPosition.position + aIndex * _tarotCardSpacing * Vector3.right;
    }

    private IEnumerator SpawnCard(int aIndex)
    {
        GameObject tarotCard = Instantiate(
            _tarotCardPrefab,
            _tarotSpawnPosition.position,
            _tarotSpawnPosition.rotation
        );

        yield return new WaitForSeconds(_tarotCardMoveDelayOnSpawn);

        Vector3 startPosition = _tarotSpawnPosition.position;
        Vector3 endPosition = GetTarotSpawnPosition(aIndex);
        float distance = Vector3.Distance(startPosition, endPosition);

        Vector3 delta = startPosition - GetTarotSpawnPosition(0);
        float maxDistance = delta.magnitude;
        float moveTime = Mathf.Lerp(0.0f, _tarotCardMaxMoveTime, distance / maxDistance);

        float elapsedTime = 0.0f;
        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / moveTime;
            tarotCard.transform.position = Vector3.Slerp(startPosition, endPosition, t);

            yield return null;
        }

        tarotCard.transform.position = endPosition;
        tarotCard.GetComponent<TarotCardBehaviour>().Init();

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
            Gizmos.DrawWireCube(GetTarotSpawnPosition(i), new Vector3(0.07f, 0.01f, 0.12f));
        }
        Gizmos.DrawWireSphere(_tarotFinalCenterPosition.position, 0.01f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_tarotSpawnPosition.position, 0.01f);
    }
}
