using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour {
     public float minIntensity = 0.25f;
     public float maxIntensity = 0.5f;
     
    
    private float _random_Noise_Range;
    private float _random_Shadow_Range;
    
    private Light _light; 
     
     void Start()
     {
         _light = GetComponent<Light>();
         _random_Noise_Range = Random.Range(0.0f, 65535.0f);
     }
 
     void Update(){
         float noise = Mathf.PerlinNoise(_random_Noise_Range, Time.time);
         _random_Shadow_Range = Random.Range(0.6f, 1f);
         _light.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
         _light.shadows = LightShadows.Soft;
         _light.shadowRadius = _random_Shadow_Range;
     }
}
