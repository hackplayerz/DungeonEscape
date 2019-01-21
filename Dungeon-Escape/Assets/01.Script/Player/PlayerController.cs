using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.PostProcessing;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    // ----------------------------------------
    /* Public Area */
    // ----------------------------------------

    [Header("Set Camera")]
    public Camera MainCamera;

    
    [Header("Player Setting")] 
    public GameObject PlayerPrefab;
    public float Speed = 0.9f;

    [Header("Post Processing Setting")] 
    public DepthOfFieldModel.Settings MyWellEnterSetting;
    // ----------------------------------------
    
    
    // ----------------------------------------
    /* Private Area */
    // ----------------------------------------
    
    private Transform _zoomInWellPosition;
    private Transform _zoomInFinishPosition;
    
    private static readonly float IntensityMax = .9f;
    private static readonly float IntensityMin = .7f;

    private float _intensity = .7f;
    
    private bool _isBloodying = false;
    private bool _isEnterWell = false;
    private bool _isIncraseing = false;
    
    private CameraController _cameraController;
    private Rigidbody _rigidbody = null;
    private Animator _animator = null;
    private AudioSource _audioSource = null;
    
    private Transform _originalCamTransform;
    
    // Post - Processing model Set
    private PostProcessingBehaviour _postProcessingBehaviour;
    
    private VignetteModel.Settings _hitVignetteModel;
    private VignetteModel.Settings _originalVignetteModel;

    private DepthOfFieldModel.Settings _dofSetting;
    // ----------------------------------------
    

    private void Start()
    {
        MainCamera = Camera.main;
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _cameraController = Camera.main.GetComponent<CameraController>();
        _audioSource = GetComponent<AudioSource>();
        
        _rigidbody.transform.position = GameManager.Instance().LoadSGame();
        _postProcessingBehaviour = MainCamera.GetComponent<PostProcessingBehaviour>();
        // Vignette setting
        Reset_Vignette_Setting();
        _hitVignetteModel = _postProcessingBehaviour.profile.vignette.settings;
        
        //depth of field Setting
        _dofSetting = _postProcessingBehaviour.profile.depthOfField.settings;
        _dofSetting.focusDistance = (transform.position - MainCamera.transform.position).magnitude;
        
        _postProcessingBehaviour.profile.depthOfField.settings = _dofSetting;
        
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance().GameState.Equals(GameManager.State.Gaiming))
        {
            Move();
        }

        if (_rigidbody.position.y < -10)
        {
            GameManager.Instance().GameOver();
        }
    }

    void Move()
    {
        float zAxisRaw = Input.GetAxisRaw("Horizontal");
        float xAxisRaw = -Input.GetAxisRaw("Vertical");
        Player_Animation_Controller(zAxisRaw, xAxisRaw);
        _rigidbody.transform.position += new Vector3(xAxisRaw, 0, zAxisRaw) * Speed * Time.fixedDeltaTime;
    }

    void Player_Animation_Controller(float forward, float left)
    {
        Vector2 move = new Vector2(forward,left);
        // ----------------------------------------
        // Player Look Rotation
        // ----------------------------------------

        if (!left.Equals(0))
        {
            _rigidbody.rotation = Quaternion.Euler(0 , 90 * left, 0);
        }
        else
        {
            if (forward>= .1f)
            {
                _rigidbody.rotation = Quaternion.identity;
            }
            else if(forward <= -.1f)
            {
                _rigidbody.rotation = Quaternion.Euler(0,180 * forward,0);
            }
        }
        // ----------------------------------------

        
        // ----------------------------------------
        // Move Animation
        // ----------------------------------------

        if (move != Vector2.zero)
        {
            _animator.SetBool("IsRun", true);
        }
        else
        {
            _animator.SetBool("IsRun",false);
        }
        // ----------------------------------------        
    }
    // ----------------------------------------
    
    private void OnTriggerEnter(Collider trigger)
    {
        if (trigger.tag.Equals("Spike"))
        {
            GameManager.Instance().Hit(trigger.tag);
            _cameraController.StartCameraShake(.2f,.2f,.8f);
            _rigidbody.AddForce(-_rigidbody.transform.forward * 200);
            _animator.SetTrigger("Hit");
            _animator.SetBool("IsRun",false);
            
            if (GameManager.Instance().Get_Player_Hp() < 2)
            {
                Start_Hit_Effect();
            }
        }

        if (trigger.tag.Equals("Finish"))
        {
            Zoom_In_Camera(true, trigger.tag, trigger.gameObject.GetComponent<CameraPositionController>().Get_CameraSetting_Position());
            transform.rotation = Quaternion.Euler(0, 90, 0);
            _animator.SetTrigger("Clear");
            StartCoroutine(GameManager.Instance().NextLevel());
        }
        
        
        if (trigger.tag.Equals("Well"))
        {
            _originalCamTransform = Camera.main.transform;
            // 카메라 ObjectEnter로 이동
           Zoom_In_Camera(true,trigger.tag,trigger.gameObject.GetComponent<CameraPositionController>().Get_CameraSetting_Position());
            
            _isEnterWell = true;
            // Set Post - Processing Profile

            _dofSetting.focusDistance = (trigger.gameObject.transform.position - MainCamera.transform.position).magnitude;
           // _dofSetting.focalLength = ((transform.position - trigger.gameObject.transform.position).magnitude) / 10;
                
            _postProcessingBehaviour.profile.depthOfField.settings = MyWellEnterSetting;
        }

        if (trigger.tag.Equals("Portion"))
        {
            StartCoroutine(GameManager.Instance().Heal(trigger.tag));
            trigger.gameObject.SetActive(false);
            _audioSource.PlayOneShot(GameManager.Instance().PortionSFX);
        }
        
        if (trigger.tag.Equals("Key"))
        {
            GameManager.Instance().GetKey();
            _audioSource.PlayOneShot(GameManager.Instance().KeySFX);
        }
    }

    private void OnTriggerStay(Collider trigger)
    {
        if (trigger.tag.Equals("Well"))
        {
            Show_Help_Key(trigger.gameObject);    
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(GameManager.Instance().Heal(trigger.tag));
                GameManager.Instance().SaveGame(transform.position);
                Reset_Vignette_Setting();
            }
        }
    }

    private void OnTriggerExit(Collider trigger)
    {
        if (trigger.tag.Equals("Well"))
        {
            // originalPosition으로 이동
            Quit_Help_Key(trigger.gameObject);
            Zoom_In_Camera(false,trigger.tag,_originalCamTransform);
            _isEnterWell = false;
            _postProcessingBehaviour.profile.depthOfField.settings = _dofSetting;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag.Equals("Water"))
        {
            GameManager.Instance().GameOver();
        }

        
        if (collision.collider.tag.Equals("Door"))
        {
            if (GameManager.Instance().If_I_Have_Key())
            {
                collision.gameObject.SetActive(false);
                _audioSource.PlayOneShot(GameManager.Instance().DoorOpenSFX);
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.tag.Equals("Moving"))
        {
            // 자연스러운 캐릭터 이동을 위해
            // 캐릭터를 이동체에 차일드화하여 이동
            transform.parent = collision.gameObject.transform;
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.tag.Equals("Moving"))
        {
            // 기존 부모프리팹에 종속
            transform.parent = PlayerPrefab.transform;
        }
    }


    /// <summary>
    /// Fix and Move Camera
    /// </summary>
    /// <param name="isEnter">is Enter to Object</param>
    /// <param name="tagName">Object's tag</param>
    /// <param name="zoomPosition">where the camera fix to position</param>
    /// <param name="velocity">damp velocity</param>
    /// <param name="dampTime">damp time</param>
    void Zoom_In_Camera(bool isEnter, string tagName, Transform zoomPosition)
    {
        if (isEnter)
        {
            MainCamera.transform.position = zoomPosition.position;
            MainCamera.transform.rotation = zoomPosition.rotation;
            _cameraController.IsCameraZooom(true, tagName);
        }
        else
        {
            MainCamera.transform.rotation = _originalCamTransform.rotation;
            _cameraController.IsCameraZooom(false, tagName);
        }
    }


    // ----------------------------------------
    // Play Die Animation with camera shake
    // ----------------------------------------
    public void DieAnimation()
    {
        _animator.SetTrigger("Die");
        _cameraController.StartCameraShake(.2f,.4f,1.5f);
    }
    // ----------------------------------------
    
    
    // ----------------------------------------
    // Help
    // ----------------------------------------
    void Show_Help_Key(GameObject helpKeyName)
    {
        if (helpKeyName.tag.Equals("Well"))
        {
            helpKeyName.GetComponentInChildren<TextMeshPro>().text = "E";
            helpKeyName.GetComponentInChildren<TextMeshPro>().enabled = true;
        }
    }  

    void Quit_Help_Key(GameObject helpKeyName)
    {
        if (helpKeyName.tag.Equals("Well"))
        {
            helpKeyName.GetComponentInChildren<TextMeshPro>().enabled = false;
        }
    }
    // ----------------------------------------

    // ----------------------------------------
    // Show Heart beat effect and vignette effect
    // ----------------------------------------
    void Start_Hit_Effect()
    {
        _hitVignetteModel.color = Color.red;
        _isBloodying = true;

        StartCoroutine(Bloody_Effect());
    }

    
    IEnumerator Bloody_Effect()
    {
        while (_isBloodying)
        {
            if (_intensity >= IntensityMax)
            {
                Debug.Log("Reatch");
                _intensity = IntensityMax;
                _isIncraseing = false;
            }
            
            else if(_intensity <= IntensityMin)
            {
                _intensity = IntensityMin;
                _isIncraseing = true;
            }

            if (_isIncraseing)
            {
                // intensity 증가중
                if (_intensity >= IntensityMax)
                {
                    _isIncraseing = false;
                    _intensity -= 0.01f;
                }
                else
                {
                    _intensity += 0.01f;
                }
            }
            else
            {
                // intensity 감소중
                if (_intensity <= IntensityMin)
                {
                    _isIncraseing = true;
                    _intensity += 0.01f;
                }
                else
                {
                    _intensity -= 0.01f;
                }
            }
            
            yield return new WaitForSeconds(.1f);

            _hitVignetteModel.intensity = _intensity;
            _postProcessingBehaviour.profile.vignette.settings = _hitVignetteModel;
        }
        // When heal, Set Bloody Effect false 
        Reset_Vignette_Setting();
    }
    // ----------------------------------------
    
    
    void Reset_Vignette_Setting()
    {
        _originalVignetteModel.color = Color.black;
        _originalVignetteModel.intensity = .5f;
        _originalVignetteModel.center = Vector3.one * .5f;
        _originalVignetteModel.smoothness = 1f;
        _originalVignetteModel.roundness = .5f;
        _isBloodying = false;
        
        _postProcessingBehaviour.profile.vignette.settings = _originalVignetteModel;
    }
}
