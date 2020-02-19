using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.RemoteConfig;

namespace MS
{
	[ExecuteInEditMode]
	public class UpdateGameManager : MonoBehaviour 
	{
		
		[Header("UpdateSetting")]
		[SerializeField] int currentVersion;

		[Space(4)][SerializeField]string minVersionRemoteName="Minimum_Version";
		[SerializeField]int minVersion;

		[Space(4)][SerializeField]string letestVersionRemoteName="Letest_Version";
		[SerializeField]int letestVersion;

		[Space(4)][SerializeField]string letestAndroidURLName="Letest_Android_URL";
		[SerializeField]string letestAndroidURL="";

		[Space(4)][SerializeField]string letestIosURLName="Letest_Ios_URL";
		[SerializeField]string letestIosURL="";

		[Space(4)][SerializeField]string UpdateAvalibleMsgName="Update_Avalible_Msg";
		[SerializeField]string UpdateAvalibleMsg ="New Update is Avalible with more fetures";

		[Space(4)][SerializeField]string UpdateNeedMsgName="Update_Need_Msg";
		[SerializeField]string UpdateNeedMsg ="Update Game to move Ahaed";

		[Space(10)][Header("NewsSetting")]

		[Space(4)][SerializeField]string ShowNewsName="Show_News";
		[SerializeField]bool ShowNews =false;
		[Space(4)][SerializeField]string NewsImageURLName="News_Image_URL";
		[SerializeField]string NewsImageURL ="";
		[Space(4)][SerializeField]string NewsClickAndroidURLName="News_Click_Android_URL";
		[SerializeField]string NewsClickAndroidURL ="";
		[Space(4)][SerializeField]string NewsClickIosURLName="News_Click_Ios_URL";
		[SerializeField]string NewsClickIosURL ="";


		[Space(20)][Header("Dialog Reference")]
		[SerializeField] GameObject _updateDialog;
		[SerializeField] Text _updateText;
		[SerializeField] Button _updateActionBtn,_updateCloseBtn;
		[Space][SerializeField] GameObject _newsDialog;
		[SerializeField] RawImage _newsImage;
		[SerializeField] Button _newsActionBtn;

		public static UpdateGameManager intance;
		public const string dataPrefix="UpdateGameManager_";
		void Awake()
		{
			if (intance == null) {
				#if !UNITY_EDITOR
					intance = this;
					DontDestroyOnLoad (this.gameObject);
					_updateDialog.SetActive (false);
					_newsDialog.SetActive (false);
					ConfigManager.FetchCompleted+=onUpdateSetting;
					ConfigManager.FetchConfigs<int,int>(0,0);
				#endif
			} else {
				DestroyImmediate (this.gameObject);
			}
		}
		void Start()
		{
			
			// RemoteSettings.Updated += onUpdateSetting;
			// RemoteSettings.ForceUpdate ();
		}
		void onUpdateSetting(ConfigResponse config)
		{
			if(config.status==ConfigRequestStatus.Success)
			{
				minVersion = ConfigManager.appConfig.GetInt (minVersionRemoteName, 0);
				letestVersion = ConfigManager.appConfig.GetInt (letestVersionRemoteName, currentVersion);

				letestAndroidURL = ConfigManager.appConfig.GetString (letestAndroidURLName, letestAndroidURL);
				letestIosURL = ConfigManager.appConfig.GetString (letestIosURLName, letestIosURL);

				UpdateAvalibleMsg = ConfigManager.appConfig.GetString (UpdateAvalibleMsgName, UpdateAvalibleMsg);
				UpdateNeedMsg = ConfigManager.appConfig.GetString (UpdateNeedMsgName, UpdateNeedMsg);

				ShowNews = ConfigManager.appConfig.GetBool (ShowNewsName, ShowNews);
				NewsImageURL = ConfigManager.appConfig.GetString (NewsImageURLName, NewsImageURL);
				NewsClickAndroidURL = ConfigManager.appConfig.GetString (NewsClickAndroidURLName, NewsClickAndroidURL);
				NewsClickIosURL = ConfigManager.appConfig.GetString (NewsClickIosURLName, NewsClickIosURL);

				if (currentVersion < minVersion) {
					ShowUpdateNeedPopup ();
				} else if (currentVersion < letestVersion) {
					ShowUpdateAvaliblePopup ();
				} else if(ShowNews) {			
					StartCoroutine (setupNewsImage ());
				}
			}



		}
		void ShowUpdateAvaliblePopup()
		{
			_updateDialog.SetActive(true);
			_updateText.text=UpdateAvalibleMsg;
			_updateCloseBtn.gameObject.SetActive (true);
			_updateActionBtn.onClick.RemoveAllListeners ();
			_updateActionBtn.onClick.AddListener(()=>{
				_updateDialog.SetActive(false);
				#if UNITY_ANDROID
					Application.OpenURL(letestAndroidURL);
				#else
					Application.OpenURL(letestIosURL);
				#endif
			});
		}
		void ShowUpdateNeedPopup()
		{
			_updateDialog.SetActive(true);
			_updateText.text=UpdateNeedMsg;
			_updateActionBtn.onClick.RemoveAllListeners ();
			_updateCloseBtn.gameObject.SetActive (false);
			_updateActionBtn.onClick.AddListener(()=>{
				#if UNITY_ANDROID
					Application.OpenURL(letestAndroidURL);
				#else
					Application.OpenURL(letestIosURL);
				#endif
			});

		}
		IEnumerator setupNewsImage()
		{
			if (!NewsImageURL.Equals ("")) {
				WWW request = new WWW (NewsImageURL.Replace ("\\", ""));
				yield return request;
				if (request.error == null && request.texture != null) 
				{
					_newsImage.texture = request.texture;
						showNewsPopup ();
				} else {
					ShowNews = false;
				}
			} else {
				ShowNews = false;
			}
		}
		public void showNewsPopup()
		{
			if (!ShowNews)
				return;
			_newsDialog.SetActive (true);
			_newsActionBtn.onClick.RemoveAllListeners ();
			_newsActionBtn.onClick.AddListener(()=>{
				_newsDialog.SetActive (false);
				#if UNITY_ANDROID
				Application.OpenURL(NewsClickAndroidURL);
				#else
				Application.OpenURL(NewsClickIosURL);
				#endif
			});

		}
		void Update ()
		{
			#if UNITY_EDITOR
			PlayerPrefs.SetInt(dataPrefix+"CurrentVersion",currentVersion);
			if(GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>()==null)
			{
				Debug.LogError("Add Event System");
				UnityEditor.EditorApplication.ExecuteMenuItem("GameObject/UI/Event System");
			}
			#endif
			
		}



		#if UNITY_EDITOR
		[UnityEditor.Callbacks.PostProcessBuild]
				public static void OnPostProcessBuild(UnityEditor.BuildTarget target,string path)
		{
			Debug.Log ("Update Game Vestion " + PlayerPrefs.GetInt(dataPrefix+"CurrentVersion",0));
		}
		[UnityEditor.MenuItem("Window/Add Update Game Manger")]
		static void AddUpdateGameManger()
		{
			if(GameObject.FindObjectOfType<UpdateGameManager>()==null)
			{
				UnityEditor.PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("UpdateGameManger"));
			}
			else{
				Debug.LogError("Already Added");
			}
		}
		#endif

	}
}