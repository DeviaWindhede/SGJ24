using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

[RequireComponent(typeof(CharacterController))]
public class PlayerBehaviour : MonoBehaviour
{
    public delegate void OnInteractableChanged(PlayerInteractionType aType);
    public event OnInteractableChanged OnInteractableChangedEvent;
    //ShopperWorldCanvas
    [SerializeField] private Transform _model;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _maxSpeed = 5f;
    [SerializeField] private float _acceleration = 1f;
    [SerializeField] private float _decceleration = 1f;
    [SerializeField] private float _turnSpeed = 1f;

    private float _speed = 0.0f;
    private Camera _camera;
    private CharacterController _controller;
    private PlayerInputActions _inputActions;
    private Vector2 _moveInput;
    private Vector3 _previousPosition;
    private Vector3 _previousInput = Vector3.forward;
    private Vector3 _movementDirection = Vector3.forward;
    private bool _shouldMove = true;

    private PlayerInteractable _currentInteractable;

    // Start is called before the first frame update
    void Start()
    {
        _controller = GetComponent<CharacterController>();

        _camera = FindObjectOfType<PixelCamRaycast>().GetComponent<Camera>();

        _inputActions = new();
        _inputActions.Enable();

        _inputActions.ShopControls.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _inputActions.ShopControls.Move.canceled += ctx => _moveInput = Vector2.zero;

        _inputActions.ShopControls.Interact.performed += _ => OnInteract();
        _inputActions.ShopControls.Deny.performed += _ => OnDeny();

        gameObject.SetActive(false);
        transform.SetPositionAndRotation(
            PersistentShopData.Instance.playerTransform.position,
            PersistentShopData.Instance.playerTransform.rotation
        );
        gameObject.SetActive(true);

        _previousPosition = transform.position;
        ResetPosition();
    }

    public void ShouldEnableMovement(bool aValue)
    {
        _shouldMove = aValue;
    }

    public void ResetPosition()
    {
        _controller.enabled = false;
        transform.SetPositionAndRotation(
            transform.position = -Vector3.forward,
            Quaternion.identity
        );
        _controller.enabled = true;
    }

    private void OnDestroy()
    {
        _inputActions.ShopControls.Move.performed -= ctx => _moveInput = ctx.ReadValue<Vector2>();
        _inputActions.ShopControls.Move.canceled -= ctx => _moveInput = Vector2.zero;

        _inputActions.ShopControls.Interact.performed -= _ => OnInteract();

        _inputActions.Disable();


        PersistentShopData.Instance.playerTransform = new TransformData
        {
            position = transform.position,
            rotation = _model.rotation
        };
    }

    private void OnInteract()
    {
        print($"Interact!!! {_currentInteractable}");
        _currentInteractable?.OnInteract();
    }

    private void OnDeny()
    {
        print("Deny!!!");
        _currentInteractable?.OnDeny();
    }

    Vector3 GetMoveDir()
    {
        Vector3 cameraForward = _camera.transform.forward;
        Vector3 cameraRight = _camera.transform.right;

        Vector3 result = cameraForward * _moveInput.y + cameraRight * _moveInput.x;

        return new Vector3(result.x, 0, result.z).normalized;
    }

    //_animator.SetBool("IsTalking", true);


    // Update is called once per frame
    void Update()
    {
        float speed = Mathf.Lerp(0, 2, _speed / _maxSpeed);
        _animator.SetFloat("Speed", _speed);
        _previousPosition = transform.position;

        float dot = Vector3.Dot(_previousInput, _movementDirection);
        if (dot <= -0.9f)
        {
            _movementDirection += Vector3.Cross(_movementDirection, Vector3.up) * 10.0f;
            _movementDirection.Normalize();
        }
        _movementDirection = Vector3.MoveTowards(_movementDirection, _previousInput, _turnSpeed * Time.deltaTime);
        _model.forward = _movementDirection;

        if (!_shouldMove) { return; }

        var inputDir = GetMoveDir();
        if (inputDir.sqrMagnitude > 0)
        {
            _speed = Mathf.MoveTowards(_speed, _maxSpeed, _acceleration * Time.deltaTime);
            _previousInput = inputDir;
        }
        else
        {
            _speed = Mathf.MoveTowards(_speed, 0, _decceleration * Time.deltaTime);
        }

        Vector3 moveAmount = _speed * Time.deltaTime * _movementDirection;
        _controller.Move(moveAmount);
    }

    private void OnTriggerStay(Collider collision)
    {
        if (!collision.gameObject.CompareTag("PlayerInteractable")) { return; }

        PlayerInteractable interactable = collision.gameObject.GetComponent<PlayerInteractable>();
        if (interactable != _currentInteractable) 
        {
            OnInteractableChangedEvent?.Invoke(interactable.PlayerInteractionType);
        }
        _currentInteractable = interactable;
    }

    private void OnTriggerExit(Collider collision)
    {
        if (!collision.gameObject.CompareTag("PlayerInteractable")) { return; }

        if (_currentInteractable != null)
        {
            OnInteractableChangedEvent?.Invoke(PlayerInteractionType.None);
        }
        _currentInteractable = null;
    }
}
