using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShopSoundByte
{
    ShopBell,
    CardDraw,
    CardShuffleIn,
    CardShuffleOut,
}

public class AudioManager : MonoBehaviour
{
    static AudioManager _instance;
    public static AudioManager Instance => _instance;

    [Header("Cards")]
    [SerializeField] private AudioSource _deckAudioSource;
    [SerializeField] private List<AudioClip> _cardDrawAudioClips;
    [SerializeField] private List<AudioClip> _cardShuffleInAudioClips;
    [SerializeField] private List<AudioClip> _cardShuffleOutAudioClips;

    [Header("Bell")]
    [SerializeField] private AudioSource _bellAudioSource;
    [SerializeField] private List<AudioClip> _shopBellAudioClips;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(ShopSoundByte aSoundByte)
    {
        switch (aSoundByte)
        {
            case ShopSoundByte.ShopBell:
                _bellAudioSource.PlayOneShot(GetRandomAudio(_shopBellAudioClips));
                break;
            case ShopSoundByte.CardDraw:
                _deckAudioSource.PlayOneShot(GetRandomAudio(_cardDrawAudioClips));
                break;
            case ShopSoundByte.CardShuffleIn:
                _deckAudioSource.PlayOneShot(GetRandomAudio(_cardShuffleInAudioClips));
                break;
            case ShopSoundByte.CardShuffleOut:
                _deckAudioSource.PlayOneShot(GetRandomAudio(_cardShuffleOutAudioClips));
                break;
            default:
                break;
        }
    }

    private AudioClip GetRandomAudio(List<AudioClip> aAudioClips)
    {
        return aAudioClips[Random.Range(0, aAudioClips.Count)];
    }
}
