using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShopSoundByte
{
    ShopBell,
    CardDraw,
    CardShuffleIn,
    CardShuffleOut,
    Click,
    CharacterCreated,
    MajorPurchase,
    Bunny,
    Pant,
    Sheep,
    Placeholder,
}

public class AudioManager : MonoBehaviour
{
    static AudioManager _instance;
    public static AudioManager Instance => _instance;

    [SerializeField] private AudioSource _miniGameAudioSource;
    [SerializeField] private AudioSource _sfxAudioSource;

    [Header("Cards")]
    [SerializeField] private List<AudioClip> _cardDrawAudioClips;
    [SerializeField] private List<AudioClip> _cardShuffleInAudioClips;
    [SerializeField] private List<AudioClip> _cardShuffleOutAudioClips;

    [Header("Bell")]
    [SerializeField] private AudioSource _bellAudioSource;
    [SerializeField] private List<AudioClip> _shopBellAudioClips;

    [Header("UI")]
    [SerializeField] private AudioClip _clickAudioClip;
    [SerializeField] private AudioClip _characterCreatedAudioClip;
    [SerializeField] private AudioClip _majorPurchaseAudioClip;

    [Header("Goober")]
    [SerializeField] private List<AudioClip> _gooberAudioClip;

    [Header("Placeholder")]
    [SerializeField] private AudioSource _placeholderAudioSource;
    [SerializeField] private AudioClip _placeholderAudioClip;

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
            case ShopSoundByte.Placeholder:
                _placeholderAudioSource.PlayOneShot(_placeholderAudioClip);
                break;
            case ShopSoundByte.ShopBell:
                _bellAudioSource.PlayOneShot(GetRandomAudio(_shopBellAudioClips));
                break;
            case ShopSoundByte.CardDraw:
                _miniGameAudioSource.PlayOneShot(GetRandomAudio(_cardDrawAudioClips));
                break;
            case ShopSoundByte.CardShuffleIn:
                _miniGameAudioSource.PlayOneShot(GetRandomAudio(_cardShuffleInAudioClips));
                break;
            case ShopSoundByte.CardShuffleOut:
                _miniGameAudioSource.PlayOneShot(GetRandomAudio(_cardShuffleOutAudioClips));
                break;
            case ShopSoundByte.Click:
                _sfxAudioSource.PlayOneShot(_clickAudioClip);
                break;
            case ShopSoundByte.CharacterCreated:
                _miniGameAudioSource.PlayOneShot(_characterCreatedAudioClip);
                break;
            case ShopSoundByte.MajorPurchase:
                _miniGameAudioSource.PlayOneShot(_majorPurchaseAudioClip);
                break;
            case ShopSoundByte.Bunny:
            case ShopSoundByte.Pant:
            case ShopSoundByte.Sheep:
                _miniGameAudioSource.PlayOneShot(GetRandomAudio(_gooberAudioClip));
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
