using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    [SerializeField] private Gradient _gradient;
    [SerializeField] private float _timeToFill = 0.5f;

    private Coroutine _updateCoroutine;

    private float _targetFillPercentage = 0.0f;

    public void SetTargetFillPercentage(float aValue)
    {
        _targetFillPercentage = aValue;

        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
        }
        _updateCoroutine = StartCoroutine(UpdateProgress());
    }

    void Start()
    {
        SetTargetFillPercentage(0);
        _fillImage.fillMethod = Image.FillMethod.Horizontal;
        _fillImage.fillAmount = _targetFillPercentage;
    }

    private IEnumerator UpdateProgress()
    {
        float fillAmount = _fillImage.fillAmount;
        Color currentColor = _fillImage.color;

        float elapsedTime = 0.0f;
        while (elapsedTime < _timeToFill)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / _timeToFill;
            _fillImage.fillAmount = Mathf.Lerp(fillAmount, _targetFillPercentage, t);
            _fillImage.color = _gradient.Evaluate(_fillImage.fillAmount);

            yield return null;
        }
    }
}
