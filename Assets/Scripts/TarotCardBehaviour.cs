using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public enum CardType
{
    Fool,
    Magician,
    HighPriestess,
    Empress,
    Emperor,
    Hierophant,
    Lovers,
    Chariot,
    Strength,
    Hermit,
    WheelOfFortune,
    Justice,
    HangedMan,
    Death,
    Temperance,
    Devil,
    Tower,
    Star,
    Moon,
    Sun,
    Judgement,
    World
}

[RequireComponent(typeof(BoxCollider))]
public class TarotCardBehaviour : MonoBehaviour
{
    [SerializeField] private AnimatorController _animatorController;

    private Animator _animator;
    private BoxCollider _boxCollider;
    private bool _isHovered = false;
    private bool _isFlipped = false;
    private bool _hasInitialized = false;
    private CardType _type;
    private GameObject _instatiatedCard;

    public void Init(CardType aType, GameObject aCard)
    {
        _type = aType;
        _boxCollider = GetComponent<BoxCollider>();
        _boxCollider.enabled = false;

        bool shouldFlip = Random.Range(0, 2) == 0;

        _instatiatedCard = Instantiate(aCard, transform);
        _instatiatedCard.transform.localRotation = Quaternion.Euler(0, shouldFlip ? 180 : 0, -180);
        _animator = _instatiatedCard.AddComponent<Animator>();
        _animator.runtimeAnimatorController = _animatorController;
        _animator.enabled = true;
    }

    public void FinishMove()
    {
        _boxCollider.enabled = true;
        _hasInitialized = true;
    }

    public void SetHovered(bool aValue)
    {
        if (!_hasInitialized) { return; }
        if (_isFlipped && aValue) { return; }

        _isHovered = aValue;
        _animator.SetBool("Hovered", _isHovered);
    }

    public void Select()
    {
        if (!_hasInitialized) { return; }
        if (_isFlipped) { return; }

        _isFlipped = true;
        _isHovered = false;
        _animator.SetTrigger("Select");
        AudioManager.Instance.PlaySound(ShopSoundByte.CardDraw);
    }
}
