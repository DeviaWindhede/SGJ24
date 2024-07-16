using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TarotGameBehaviour : MonoBehaviour
{
    private TarotCardBehaviour _hoveredCard;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane));

        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit))
        {
            if (_hoveredCard != null)
            {
                _hoveredCard.SetHovered(false);
            }
            return;
        }

        if (hit.collider.gameObject.CompareTag("TarotCard"))
        {
            TarotCardBehaviour tarotCard = hit.collider.gameObject.GetComponent<TarotCardBehaviour>();

            if (_hoveredCard != null && _hoveredCard != tarotCard)
            {
                _hoveredCard.SetHovered(false);
            }

            _hoveredCard = tarotCard;
            tarotCard.SetHovered(true);

            if (Input.GetMouseButtonDown(0))
            {
                tarotCard.Select();
            }
        }
        else
        {
            if (hit.collider.gameObject.CompareTag("TarotDeck") && Input.GetMouseButtonDown(0))
            {
                TarotDeckBehaviour tarotDeck = hit.collider.gameObject.GetComponent<TarotDeckBehaviour>();
                tarotDeck.OnClick();
            }

            if (_hoveredCard != null)
            {
                _hoveredCard.SetHovered(false);
            }
        }
    }
}
