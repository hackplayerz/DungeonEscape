using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    /* Public Area */
    [Header("Set Camera Following Object")]
    public Transform TargetToFollow;
    public float DampTime = 0.15f;
    [Header("Set Camera Transform")] 
    public Vector3 CamTransform = Vector3.zero;
    public Vector3 CamRotation = Vector3.zero;
    public bool IsBeltScroll = false;
    



    /* Private Area */
    private float ShakeAmount = .7f;
    private float DecreaseFactor = 1f;
    private float ShakeTime = .8f;

    private float _deltaTime = 0;
    private Vector3 _originalPosition = Vector3.zero;
    private Vector3 _velocity = Vector3.zero;
    
    private bool _isEnterWell = false;
    private bool _isEnterFinish = false;
    
    
    
    enum CameraState
    {
        Stop,
        Moving,
        Shake
    };
    
    [SerializeField]
    private CameraState CamState = CameraState.Moving;
    
    private void OnEnable()
    {
        transform.rotation = Quaternion.Euler(0,-90,0);
    }

    private void Update()
    {
        if (CamState.Equals(CameraState.Shake))
        {
            CameraShake();
        }
    }

    private void FixedUpdate ()
    {
        Vector3 destination = TargetToFollow.position;
        if (IsBeltScroll)
        {
            if (CamState.Equals(CameraState.Moving))
            {
                destination.z += 1f;
                destination.y = CamTransform.y;
                destination.x = CamTransform.x;
                transform.position = Vector3.SmoothDamp(transform.position, destination, ref _velocity, DampTime);
                transform.rotation = Quaternion.Euler(CamRotation);
            }
        }
        else
        {
            if (CamState.Equals(CameraState.Moving))
            {
                destination.y = CamTransform.y;
                transform.position = Vector3.SmoothDamp(transform.position, destination, ref _velocity, DampTime);
                transform.rotation = Quaternion.Euler(CamRotation);
            }
        }
    }
    
    // Shake Camera
    void CameraShake()
    {
        if (_deltaTime < ShakeTime)
        {
            transform.position = _originalPosition + Random.insideUnitSphere * ShakeAmount;
            _deltaTime += Time.fixedDeltaTime * DecreaseFactor;
        }
        else
        {
            _deltaTime = 0;
            CamState = CameraState.Moving;
        }
    }
    /// <summary>
    /// Camera Shake
    /// </summary>
    /// <param name="shakeTime">Set Shake time</param>
    /// <param name="shakeAmount">How much power of Shaking</param>
    /// <param name="decreaseFactor">Set smoothing of Shaking</param>
    public void StartCameraShake(float shakeTime, float shakeAmount,float decreaseFactor)
    {
        ShakeTime = shakeTime;
        ShakeAmount = shakeAmount;
        DecreaseFactor = decreaseFactor;
        _originalPosition = transform.position;
        CamState = CameraState.Shake;
    }
    
    /// <summary>
    /// Setting Behavior of Zooming of Camera
    /// </summary>
    /// <param name="isEnter">if is enter object : true</param>
    /// <param name="tagName">object's tag name</param>
    public void IsCameraZooom(bool isEnter,string tagName)
    {
        switch (tagName)
        {
            case "Well" :
                _isEnterWell = isEnter;
                break;
            case "Finish" :
                _isEnterFinish = isEnter;
                break;
        }

        if (_isEnterWell || _isEnterFinish)
        {
            CamState = CameraState.Stop;
        }
        else
        {
            CamState = CameraState.Moving;
        }
    }
}