using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TarotCardBehaviour : MonoBehaviour
{
    private Animator _animator;
    private BoxCollider _boxCollider;
    private bool _isHovered = false;
    private bool _isFlipped = false;
    private bool _hasInitialized = false;

    public void Init()
    {
        _boxCollider = GetComponent<BoxCollider>();
        _animator = transform.GetChild(0).GetComponent<Animator>();
        _boxCollider.enabled = true;
        _animator.enabled = true;
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
    }
}
