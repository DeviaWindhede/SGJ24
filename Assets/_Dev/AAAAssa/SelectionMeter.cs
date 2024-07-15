using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionMeter : MonoBehaviour
{
    [SerializeField] private GameObject _YellowSelection;
    [SerializeField] private GameObject _GreenSelection;
    [SerializeField] private GameObject _SelectionTarget;
    
    [SerializeField] public float GreenMin;
    [SerializeField] public float GreenMax;

    [SerializeField] public float YellowMin;
    [SerializeField] public float YellowMax;

    [SerializeField] public float SelectionSpeed;
    private float _SelectorPosition;

    [SerializeField] private bool _Press;

    [SerializeField] private float SelectorMin;
    [SerializeField] private float SelectorMax;


    // Start is called before the first frame update
    void Start()
    {
        _Press = false;

        ConfigureElements();
    }

    void ConfigureElements()
    {
        float minScale = 7.0f;
        float maxScale = 51.0f;

        float minX = -810.0f;
        float maxX = 810.0f;
        
        float yellowMinX = Mathf.Lerp(minX, maxX, YellowMin);
        float yellowMaxX = Mathf.Lerp(minX, maxX, YellowMax);
        float yellowWidth = Mathf.Lerp(minScale, maxScale, YellowMax - YellowMin);
        float yellowCenter = Mathf.Lerp(yellowMinX, yellowMaxX, 0.5f);


        float greenMinX = Mathf.Lerp(minX, maxX, GreenMin);
        float greenMaxX = Mathf.Lerp(minX, maxX, GreenMax);
        float greenWidth = Mathf.Lerp(minScale, maxScale, GreenMax - GreenMin);
        float greenCenter = Mathf.Lerp(greenMinX, greenMaxX, 0.5f);

        _YellowSelection.transform.localScale = new Vector3(yellowWidth, yellowWidth, yellowWidth);
        _YellowSelection.transform.localPosition = new Vector3(yellowCenter, _YellowSelection.transform.localPosition.y, _YellowSelection.transform.localPosition.z);

        _GreenSelection.transform.localScale = new Vector3(greenWidth, greenWidth, greenWidth);
        _GreenSelection.transform.localPosition = new Vector3(greenCenter, _GreenSelection.transform.localPosition.y, _GreenSelection.transform.localPosition.z);
    }

    float GetSelectorT()
    {
        float t = Mathf.Sin(Time.time * SelectionSpeed);
        t = (t + 1) / 2;
        return t;
    }

    // Update is called once per frame
    void Update()
    {
        float t = GetSelectorT();

        _SelectorPosition = Mathf.Lerp(SelectorMin, SelectorMax, t);
        _SelectionTarget.transform.localPosition = new Vector3(_SelectorPosition, _SelectionTarget.transform.localPosition.y, _SelectionTarget.transform.localPosition.z);


        if (_Press) { PressInput(); _Press = false; }
    }

    bool IsWithinYellow()
    {
        float t = GetSelectorT();
        return (t >= YellowMin && t <= YellowMax);
    }

    bool IsWithinGreen()
    {
        float t = GetSelectorT();
        return (t >= GreenMin && t <= GreenMax);
    }


    void PressInput()
    {
        Debug.Log("Pressed");
        if (IsWithinYellow()) { Debug.Log("Within Yellow"); }
        if (IsWithinGreen()) { Debug.Log("Within Green"); }

    }

    private void OnValidate()
    {
        ConfigureElements();
    }
}
