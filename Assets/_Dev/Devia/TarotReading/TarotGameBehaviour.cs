using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TarotGameBehaviour : MonoBehaviour
{
    [SerializeField] private Camera _pixelCam;
    private TarotCardBehaviour _hoveredCard;
    private PlayerInputActions _inputActions;
    private RaycastHit _hit;


    // Start is called before the first frame update
    void Start()
    {
        _inputActions = new();
        _inputActions.Enable();

        _inputActions.MiniGameControls.Enable();
        _inputActions.MiniGameControls.MouseDown.performed += _ => OnMouseClick();
    }



    private void OnDestroy()
    {
        _inputActions.MiniGameControls.MouseDown.performed -= _ => OnMouseClick();

        _inputActions.MiniGameControls.Disable();
        _inputActions.Disable();
    }

    private void SetHovered(bool aValue)
    {
        if (!_hoveredCard) { return; }
        _hoveredCard.SetHovered(aValue);
    }

    private void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        Ray ray = _pixelCam.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, _pixelCam.nearClipPlane));

        if (!Physics.Raycast(ray, out _hit))
        {
            SetHovered(false);
            _hoveredCard = null;
            return;
        }

        if (_hit.collider.gameObject.CompareTag("TarotDeck")) { return; }

        TarotCardBehaviour tarotCard = _hit.collider.gameObject.GetComponent<TarotCardBehaviour>();

        if (_hoveredCard != tarotCard)
        {
            SetHovered(false);
        }

        _hoveredCard = tarotCard;
        SetHovered(true);
    }

    private void OnMouseClick()
    {
        if (_hit.collider != null && _hit.collider.gameObject.CompareTag("TarotDeck"))
        {
            _hit.collider.gameObject.GetComponent<TarotDeckBehaviour>().OnClick();
            return;
        }

        if (!_hoveredCard) { return; }
        _hoveredCard.Select();
    }
}
