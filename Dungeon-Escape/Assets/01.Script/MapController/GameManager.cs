using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
   // ----------------------------------------
   // * Public Area *
   // ----------------------------------------

   public static GameManager Instance()
   {
      if (_instance.Equals(null))
      {
         _instance = FindObjectOfType<GameManager>();
      }
      return _instance;
   }
   
   public enum State
   {
      GameOver,
      Pause,
      Gaiming,
      Hit
   };
   
   
   public State GameState = State.Gaiming;
   


   [Header("Object Control")] 
   public GameObject KeyPrefab;

   [Header("Heal Control")] 
   public GameObject HealEffectPrefab = null;
   public int WellHealAmount = 4;
   public int PortionHealAmount = 1;
   
   [Header("Damage")]
   public int SpikeDamage = 1;
   public int MonsterDamage = 1;
   
   [Header("UI Control")] 
   public Sprite[] HPSprite;
   public Image HpImage;
   public GameObject PauseUi;
   public Text SaveText;
   public Text MessageText;

   [Header("SFX and BGM")] 
   public AudioClip KeySFX;
   public AudioClip PortionSFX;
   public AudioClip DoorOpenSFX;
   public AudioClip BGM;
   // ----------------------------------------
   
   
   // ----------------------------------------
   // * Private Area *
   // ----------------------------------------

   private static GameManager _instance; // Singleton

   
   private enum MyPerfsName
   {
      Level_,
      IsGetKey,
      Respawn_
   };
      
   private static readonly int MAX_HP = 4;
   private static readonly int ORIGINAL_HP = 4;

   private PlayerController _playerController;

   private int _level;
   private int _playerHP = ORIGINAL_HP;
   
   // True  : 1
   // False : 0
   private bool _isGetKey = false;

   private GameObject _healEffect = null;
   
   // ----------------------------------------
   
   
   private void Awake()
   {
      _instance = GetComponent<GameManager>();
      On_Start_Setting();
      LoadSGame();
   }

   private void Start()
   {
      HpImage.GetComponent<Image>().sprite = HPSprite[_playerHP];
      _playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();


      Debug.Log("Has Key : " + PlayerPrefs.GetInt(nameof(MyPerfsName.IsGetKey)) +
                        "     Level : " + PlayerPrefs.GetInt(nameof(MyPerfsName.Level_)));
      _healEffect = Instantiate(HealEffectPrefab);
      _healEffect.SetActive(false);
   }

   private void FixedUpdate()
   {
      if (Input.GetButton("Cancel"))
      {
         Pause();
         PauseUi.SetActive(true);
      }
   }

   /// <summary>
   /// Heal the player HP
   /// </summary>
   /// <param name="tagName">Heal object's tag</param>
   public IEnumerator Heal(string tagName)
   {
      switch (tagName)
      {
         case "Well":
            _playerHP += WellHealAmount;
            break;
         case "Portion":
            _playerHP += PortionHealAmount;
            break;
      }

      if (_playerHP >= MAX_HP)
      {
         _playerHP = MAX_HP;
      }

      UpdateUi();
      _healEffect.transform.position = GameObject.FindWithTag("Player").transform.position;
      _healEffect.SetActive(true);
      yield return new WaitForSeconds(1.5f);
      _healEffect.SetActive(false);
   }

   /// <summary>
   /// Player Hit func
   /// Set Damage in GameManager
   /// </summary>
   /// <param name="objectTag">Put tag of object</param>
   public void Hit(string objectTag)
   {
      switch (objectTag)
      {
            case "Spike":
               _playerHP -= SpikeDamage;
               break;
            
            case "Monster":
               _playerHP -= MonsterDamage;
               break;
      }
      UpdateUi();
      GameState = State.Hit;
      if (_playerHP <= 0)
      {
         GameOver();
         
         return;
      }
      Debug.Log("Hit! Reamin : " + _playerHP);
      Invoke("Resume",1f);
   }
   
   
   // ----------------------------------------
   // Control the Game State
   // ----------------------------------------
   public void Resume()
   {
      GameState = State.Gaiming;
      PauseUi.SetActive(false);
      Cursor.visible = false;
   }
   
   public void GameOver()
   {
      GameState = State.GameOver;
      _playerController.DieAnimation();
      Invoke(nameof(GameRestart),2f);
   }

   public void Pause()
   {
      GameState = State.Pause;
      Cursor.visible = true;
   }
   
   void GameRestart()
   {
      _level = PlayerPrefs.GetInt(nameof(MyPerfsName.Level_),1);
      SceneManager.LoadScene(MyPerfsName.Level_ + _level.ToString());
   }
   // ----------------------------------------
   

   
   // ----------------------------------------
   // Quit Game
   // ----------------------------------------
   public void ExitGame()
   {
      Invoke(nameof(GameExit),1f);
   }
   
   void GameExit()
   {
      Application.Quit();
   }
   // ----------------------------------------
   
   
   // ----------------------------------------
   // Game Data Controller
   // ----------------------------------------
   public Vector3 LoadSGame()
   {
      // Get transform data to set respawn point
      float posX = PlayerPrefs.GetFloat(nameof(MyPerfsName.Respawn_)+'x', 0);
      float posY = PlayerPrefs.GetFloat(nameof(MyPerfsName.Respawn_)+'y', 0);
      float posZ = PlayerPrefs.GetFloat(nameof(MyPerfsName.Respawn_)+'z', 0);
      Debug.Log(PlayerPrefs.GetInt(nameof(MyPerfsName.IsGetKey)));
      
      if (PlayerPrefs.GetInt(nameof(MyPerfsName.IsGetKey)).Equals(0))
      {
         _isGetKey = false;
         MessageText.text = "Where is the key...?";
      }
      else
      {
         GetKey();
      }
      _level = PlayerPrefs.GetInt(nameof(MyPerfsName.Level_),1);
      Vector3 spawnPos = new Vector3(posX,posY,posZ);
      return spawnPos;
   }
   
   // Save Data and show Saving...
   IEnumerator SaveLevel()
   {
      PlayerPrefs.SetInt(nameof(MyPerfsName.Level_),_level);
      SaveText.enabled = true;
      SaveText.text = "Saved";
      yield return new WaitForSeconds(.7f);
      for (int i = 0; i < 3; i++)
      {
         SaveText.text = AddText(SaveText.text,".");
         yield return new WaitForSeconds(.7f);
      }

      SaveText.enabled = false;
      
      Debug.Log("Level : " + _level);
   }
   
   
   public void SaveGame(Vector3 spawnPosition)
   {
      int keyHashCode = 0;
      if (_isGetKey)
      {
         keyHashCode = 1;
      }
      else
      {
         keyHashCode = 0;
      }
      PlayerPrefs.SetInt(nameof(MyPerfsName.IsGetKey),keyHashCode);
      
      PlayerPrefs.SetFloat(nameof(MyPerfsName.Respawn_)+'x',spawnPosition.x + 1);
      PlayerPrefs.SetFloat(nameof(MyPerfsName.Respawn_)+'y',spawnPosition.y);
      PlayerPrefs.SetFloat(nameof(MyPerfsName.Respawn_)+'z',spawnPosition.z);
      Debug.Log("SavePos : " + spawnPosition + "\nHas Key : " + PlayerPrefs.GetInt(nameof(MyPerfsName.IsGetKey)));
      StartCoroutine(SaveLevel());
   }
   
   
   public IEnumerator NextLevel()
   {
      ++_level;
      Pause();
      StartCoroutine(SaveLevel());
      yield return new WaitForSeconds(2f);
      
      PlayerPrefs.SetInt(nameof(MyPerfsName.Level_),_level);
      PlayerPrefs.SetInt(nameof(MyPerfsName.IsGetKey),0);
      PlayerPrefs.SetFloat((nameof(MyPerfsName.Respawn_)+'x'),0);
      PlayerPrefs.SetFloat(nameof(MyPerfsName.Respawn_)+'y',0);
      PlayerPrefs.SetFloat(nameof(MyPerfsName.Respawn_)+'z',0); 
      GameState = State.Gaiming;

      SceneManager.LoadScene(nameof(MyPerfsName.Level_) + _level);
   }
   // ----------------------------------------
   
   
   // ----------------------------------------
   // Return the game data
   // ----------------------------------------
   public void GetKey()
   {
      _isGetKey = true;
      KeyPrefab.SetActive(false);
      MessageText.text = "I got a Key, Find Exit!";
   }
   
   public int Get_Player_Hp()
   {
      return _playerHP;
   }
   public bool If_I_Have_Key()
   {
      return _isGetKey;
   }
   // ----------------------------------------
   
   
   // ----------------------------------------
   // When Ui values are chaged, update on shot 
   // ----------------------------------------
   void UpdateUi()
   {
      HpImage.GetComponent<Image>().sprite = HPSprite[_playerHP];
   }
   // ----------------------------------------

   void On_Start_Setting()
   {
      // 16 : 9 해상도, 전체회면
      Screen.SetResolution(Screen.width,(Screen.width * 16)/9,true);
      // 마우스 숨기기
      Cursor.visible = false;
   }
   
   string AddText(string originalText,string add)
   {
      string text = originalText;
      text += add;
      return text;
   }
}
