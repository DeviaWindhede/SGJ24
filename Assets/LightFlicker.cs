using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

[ExecuteAlways]
public class LightFlicker : MonoBehaviour
{
    Light selfLight;

    [SerializeField] float baseIntensity = 1.0f;
    [SerializeField] float flickerIntensity = 0.1f;
    [SerializeField] float flickerSpeed = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        selfLight = GetComponent<Light>();
        
    }

    // Update is called once per frame
    void Update()
    {
        float intensityWave = Mathf.Sin(Time.time * flickerSpeed);

        float perlin = Mathf.PerlinNoise(Time.time * flickerSpeed, 2.0f);

        selfLight.intensity = baseIntensity + (perlin * 2.0f - 1.0f) * flickerIntensity;


    }
}
