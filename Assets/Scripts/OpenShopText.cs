using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenShopText : MonoBehaviour
{
    [SerializeField] private float _speed = 1.0f;
    [SerializeField] private float _amplitude = 0.5f;
    [SerializeField] private float _offset = 0.0f;

    void Update()
    {
        var localPosition = transform.localPosition;
        localPosition.z = Mathf.Sin(Time.time * _speed) * _amplitude + _offset;
        transform.localPosition = localPosition;
    }
}
