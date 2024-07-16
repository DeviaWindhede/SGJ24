using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerBehaviour : MonoBehaviour
{
    [SerializeField] private Transform _model;
    [SerializeField] private float _speed = 5f;

    private Rigidbody _rigidBody;
    private PlayerInputActions _inputActions;
    private Vector2 _moveInput;


    // Start is called before the first frame update
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();

        _inputActions = new();
        _inputActions.Enable();

        _inputActions.ShopControls.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _inputActions.ShopControls.Move.canceled += ctx => _moveInput = Vector2.zero;
    }

    private void OnDestroy()
    {
        _inputActions.ShopControls.Move.performed -= ctx => _moveInput = ctx.ReadValue<Vector2>();
        _inputActions.ShopControls.Move.canceled -= ctx => _moveInput = Vector2.zero;
     
        _inputActions.Disable();
    }

    Vector2 GetMoveDir()
    {
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;

        Vector3 result = cameraForward * _moveInput.y + cameraRight * _moveInput.x;

        return new Vector2(result.x, result.z);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 velocity = _speed * Time.deltaTime * GetMoveDir();

        _rigidBody.velocity = new Vector3(velocity.x, 0, velocity.y);

        if (velocity.magnitude == 0) { return; }

        _model.forward = new Vector3(velocity.x, 0, velocity.y);
    }
}
