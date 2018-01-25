//----------------------------------------------
//            TSSM: TimeSaver SoundManager
// 		   Created by: Murillo Pugliesi Lopes
//----------------------------------------------

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Used to control all sound stuff, such as Volume, Mute, and much more.
/// </summary>

[RequireComponent (typeof (AudioListener))]
[System.Serializable] 
public class SoundManager : MonoBehaviour {
	
	[SerializeField]
	[HideInInspector]public List<Sound> AllSoundsHolder = new List<Sound>();
	
	[SerializeField]
	[HideInInspector]public List<Sound> AllStreamSoundsHolder = new List<Sound>();

	[SerializeField]
	[HideInInspector]public List<Sound> AllResourceSoundsHolder = new List<Sound>();

	[SerializeField]
	[HideInInspector]public List<Sound> AllLanguages = new List<Sound>();
	
	[SerializeField]
	[HideInInspector]public Sound[] AllSounds = new Sound[0];
	
	[SerializeField][HideInInspector]public static List<string> StartingBkgsSounds;
	[SerializeField][HideInInspector]public List<string> StartingBkgsSoundsHolder;
	[SerializeField][HideInInspector]public List<int> sceneBkgSoundsNumberHolder;
	[SerializeField][HideInInspector]public bool dontStopIfSameSoundHolder;
	[SerializeField][HideInInspector]public static bool dontStopIfSameSound;
	//SM PROPERTIES
	[SerializeField]
	 [HideInInspector]public bool usePool;
	[SerializeField]
	 [HideInInspector]public bool downloadAtStart;
	
	[SerializeField]
	 [HideInInspector]public int qntPool;
	
	[SerializeField]
	 [HideInInspector]public string startingLanguage;
	[SerializeField]
	 [HideInInspector]public int startingLanguageIndex;
	
	[SerializeField]
	 [HideInInspector]public List<GameObject> pool;
	[SerializeField]
	 [HideInInspector]public bool useSameSM;
	[SerializeField]
	 [HideInInspector]public bool useMultiLanguage;
	
	//BKG PROPERTIES
	[SerializeField]
	 [HideInInspector]public float bkgVolume = 1;
	[SerializeField]
	 [HideInInspector]public bool playAtStart;
	[SerializeField]
	 [HideInInspector]public string nameBkgStart;
	[SerializeField]
	 [HideInInspector]public int fpsInitial = 60;
	
	//EFX PROPERTIES
	[SerializeField]
	 [HideInInspector]public float efxVolume = 1;
	
	//VOICE PROPERTIES
	[SerializeField]
	 [HideInInspector]public float voiVolume = 1;
	
	//private
	private static Sound[] _AllSounds;
	private static bool _usePool;
	private static int _qntPool;
	private static List<GameObject> _pool;
	private static float _bkgVolumeHold;
	private static float _efxVolumeHold;
	private static float _voiceVolumeHold;
	private static bool _playStreamSound;
	private static List<AudioSource> _streamSoundSource;
	private static List<AudioClip> _streamSoundClip;
	
	public static SoundManager SMInstance;

	/// <summary>
	/// Use this to check the background track volume, or to set background track volume.
	/// </summary>
	public static float backgroundVolume{
		set{
			List<Sound> hold = new List<Sound>();
			for(int i = 0; i < _AllSounds.Length;i++){
				if(string.Equals(_AllSounds[i].track.ToString(), track.BackgroundSound.ToString())){
					hold.Add(_AllSounds[i]);	
				}
			}
			if(hold.Count == 0) return;
			_bkgVolumeHold = BetweenZeroOne(value);Volume(BetweenZeroOne(value), track.BackgroundSound);}
		get{return _bkgVolumeHold;}
	}
	/// <summary>
	/// Use this to check the effect track volume, or to set effect track volume.
	/// </summary>
	public static float effectsVolume{
		set{
			List<Sound> hold = new List<Sound>();
			for(int i = 0; i < _AllSounds.Length;i++){
				if(string.Equals(_AllSounds[i].track.ToString(), track.EffectSound.ToString())){
					hold.Add(_AllSounds[i]);	
				}
			}
			if(hold.Count == 0) return;
			_efxVolumeHold = BetweenZeroOne(value);
			Volume(BetweenZeroOne(value), track.EffectSound);}
		get{return _efxVolumeHold;}
	}
	/// <summary>
	/// Use this to check the voice track volume, or to set background voice volume.
	/// </summary>
	public static float voiceVolume{
		set{
			List<Sound> hold = new List<Sound>();
			for(int i = 0; i < _AllSounds.Length;i++){
				if(string.Equals(_AllSounds[i].track.ToString(), track.VoiceSound.ToString())){
					hold.Add(_AllSounds[i]);	
				}
			}
			if(hold.Count == 0) return;
			_voiceVolumeHold = BetweenZeroOne(value);Volume (BetweenZeroOne(value), track.VoiceSound);}
		get{return _voiceVolumeHold;}
	}
	
	/// <summary>
	/// Use this to change the TSSM language. Use a valid string, otherwise it will print an error.
	/// </summary>
	public static string language{
		set{
			bool has = false;
			for(int i=0;i<_AllLanguages.Length;i++){
				if(_AllLanguages[i] == value)
					has = true;
			}
			if(!has){
				Debug.LogWarning("Couldn't find "+value+" language. Please check the typo.");
				return;
			}
			_currentLanguage = value;}
		get{return _currentLanguage;}
	}
	
	private static int _currentPoolIndex;
	private static GameObject _currentPlaying;
	private static float _timeToFadeIn;
	private static int _controllerFadeIn;
	private static GameObject _thisGameObject;
	private static List<Sound> _AllPlaying = new List<Sound>();
	private static int _fps, _totalPool;
	private static bool _bkgMuted = false, _efxMuted = false, _allMuted = false, _useMultiLanguage, _voiceMuted = false;
	private static string oldLevel;
	
	/// <summary>
	/// Use this to mute or unmute background track, or to check if it's muted.
	/// </summary>
	public static bool bkgMuted{
		set{Mute(value, track.BackgroundSound);}
		get{return _bkgMuted;}
	}
	/// <summary>
	/// Use this to mute or unmute effect track, or to check if it's muted.
	/// </summary>
	public static bool efxMuted{
		set{Mute(value, track.EffectSound);}
		get{return _efxMuted;}
	}
	/// <summary>
	/// Use this to mute or unmute voice track, or to check if it's muted.
	/// </summary>
	public static bool voiceMuted{
		set{Mute(value, track.VoiceSound);}
		get{return _voiceMuted;}	
	}
	/// <summary>
	/// Use this to mute or unmute all tracks, or to check if it's muted.
	/// </summary>
	public static bool allMuted{
		set{Mute(value, track.All);}
		get{return _allMuted;}	
	}
	
	private static string _startingLanguage, _currentLanguage;
	private static string[] _AllLanguages;
	/// <summary>
	/// Set the TSSM Fps manually, or check it.
	/// </summary>
	public static int fps {
		set {_fps = (value > 0) ? value : 1;}
		get {return _fps;}
	}
	
	private void OnApplicationPause (bool isPaused){
	    if(isPaused)
			StopAllSounds();
	    else
			PlayAllSounds();
 	}
	
	private static float BetweenZeroOne(float value){
		float toReturn;
		toReturn = (value < 0)? 0 : value;
		toReturn = (toReturn > 1)? 1: toReturn;
		return toReturn;
	}
	void Awake(){
		
		if(useSameSM){
			if (SMInstance != null && SMInstance != this) {
		        Destroy(this.gameObject);
		        return;
		    } else {
		        SMInstance = this;
		    }
			GameObject.DontDestroyOnLoad(this.gameObject);
		}else{
			SMInstance = this;	
		}
		
		AudioListener[] lst = FindObjectsOfType(typeof(AudioListener)) as AudioListener[];
		for(int i = 0; i < lst.Length; i++){
			if(lst[i].gameObject == gameObject) continue;
			Destroy(lst[i]);
		}

		_AllPlaying = new List<Sound>();

		AllSounds = new Sound[AllSoundsHolder.Count+AllStreamSoundsHolder.Count+AllResourceSoundsHolder.Count];
		AllSoundsHolder.CopyTo(AllSounds, 0);
		AllStreamSoundsHolder.CopyTo(AllSounds, AllSoundsHolder.Count);
		AllResourceSoundsHolder.CopyTo(AllSounds, AllSoundsHolder.Count+AllStreamSoundsHolder.Count);
		
		_AllSounds = AllSounds;

        _pool = pool;
		_usePool = usePool;
		_qntPool = qntPool;
		backgroundVolume = bkgVolume;
		effectsVolume = efxVolume;
        voiceVolume = voiVolume;
		_currentPoolIndex = 0;
		_currentPlaying = null;
		_playStreamSound = false;
		_thisGameObject = gameObject;
		_useMultiLanguage = useMultiLanguage;
		_AllLanguages = new string[AllLanguages.Count];
		_streamSoundSource = new List<AudioSource>();
		_streamSoundClip = new List<AudioClip>();
		for(int i = 0;i< AllLanguages.Count;i++){
			_AllLanguages[i] = AllLanguages[i].name;
		}
		
		if(useMultiLanguage && _currentLanguage == null){
			_currentLanguage = startingLanguage;	
		}

		SoundManager.fps = fpsInitial;
		
		if(downloadAtStart){
			for(int i = 0; i < _AllSounds.Length;i++){
				if(_AllSounds[i].isStreamSound){
					StartCoroutine(DownloadStreamSound(_AllSounds[i], i));	
				}
			}
		}

		dontStopIfSameSound = dontStopIfSameSoundHolder;
		StartingBkgsSounds = StartingBkgsSoundsHolder;
		oldLevel = "";

		if(playAtStart){
			SceneManager.sceneLoaded += OnSceneLoadPlayBackgroundMusic;
			//OnSceneLoadPlayBackgroundMusic(SceneManager.GetActiveScene(), LoadSceneMode.Single);
		}
	}
	
	private IEnumerator DownloadStreamSound(Sound snd, int index){
		AudioClip clip = null;
		Debug.Log("TSSM: Starting downloading "+snd.name+".");
		WWW www = new WWW(snd.URL);
		clip = www.GetAudioClip(snd._3D);
		_AllSounds[index].clip = clip;
		yield return www;
		_AllSounds[index].clip = www.GetAudioClip();
		Debug.Log("TSSM: Finished downloading "+snd.name+" with success.");
	}

	public static void OnSceneLoadPlayBackgroundMusic(Scene scene, LoadSceneMode m){
		string soundToStop = "";
		string soundToPlay = "";
		for(int i = 0; i < StartingBkgsSounds.Count; i++){
			string allNames = StartingBkgsSounds[i];
			string sceneName = allNames.Split(';')[0];
			string soundName = allNames.Split(';')[1];

			string currentScene = SceneManager.GetActiveScene().name.ToLower();

			string[] hold = currentScene.Split(' ');
			string final = "";
			for(int j = 0; j< hold.Length;j++){
				final += hold[j];
			}
			currentScene = final;

			//Debug.Log("oldScene : "+oldLevel+"  ||sceneName: "+sceneName.ToLower()+"   ||soundName: "+soundName+"   ||loaded name: "+currentScene);
			if(oldLevel != "" && oldLevel.ToLower() == sceneName.ToLower()){
				soundToStop = soundName;
			}
			if(currentScene == sceneName.ToLower()){
				soundToPlay = soundName;
			}
		}

		if (!dontStopIfSameSound || soundToPlay != soundToStop ){
			Stop(soundToStop);
			if(soundToPlay != "None")
				Play(soundToPlay);
		}	
		oldLevel = SceneManager.GetActiveScene().name.ToLower();
	}
	
	void LateUpdate(){
		if(_pool != null){
			for(int i = 0;i < _pool.Count;i++){
				if(!_pool[i].GetComponent<AudioSource>().isPlaying){
					_pool[i].GetComponent<AudioSource>().clip = null;
				}
			}
		}
		if(_playStreamSound){
			for(int i = 0; i < _streamSoundSource.Count;i++){
				if(_streamSoundClip[i].loadState == AudioDataLoadState.Loaded && _streamSoundSource[i].clip != _streamSoundClip[i]){
					_streamSoundSource[i].clip = _streamSoundClip[i];
					_streamSoundSource[i].Play();
					_streamSoundSource.RemoveAt(i);	
					_streamSoundClip.RemoveAt(i);
					if(_streamSoundSource.Count <= 0) _playStreamSound = false;
				}
			}
		}
		for(int i = 0; i < _AllPlaying.Count;i++){
			if(!_AllPlaying[i].pauseEffect){
				if(_AllPlaying[i].currentSource == null && !_AllPlaying[i].loop){
					_AllPlaying.RemoveAt(i); i--;
					continue;
				}
				if(_AllPlaying[i].currentSource != null){
					if(_AllPlaying[i].fadeIn){
						_AllPlaying[i].controllerFadeIn++;
						_AllPlaying[i].currentSource.volume = _AllPlaying[i].controllerFadeIn/(_AllPlaying[i].timeToFadeIn*SoundManager.fps);
						if(_AllPlaying[i].currentSource.volume >= 1) _AllPlaying[i].fadeIn = false;
					}
				}
				if(_AllPlaying[i].currentSource != null){
					if(_AllPlaying[i].stopAt != 0){
						if(_AllPlaying[i].currentSource.time >= _AllPlaying[i].stopAt)
							_AllPlaying[i].currentSource.Stop();
					}
				}
				if(_AllPlaying[i].currentSource != null){
					if(_AllPlaying[i].fadeOut){
						if(_AllPlaying[i].currentSource.time >= _AllPlaying[i].controllerFadeOut){
							_AllPlaying[i].fadeOutTimer++;
							_AllPlaying[i].currentSource.volume = 1 - _AllPlaying[i].fadeOutTimer/(_AllPlaying[i].timeToFadeOut*SoundManager.fps);
							if(_AllPlaying[i].currentSource.volume <= 0) _AllPlaying[i].fadeOut = false;
						}
					}
				}
				if(_AllPlaying[i].currentSource != null){
					if(System.Math.Round(_AllPlaying[i].currentSource.time,2) >= System.Math.Round(_AllPlaying[i].timeToTrigger,2) && _AllPlaying[i].canShootEvent){
						GameObject toRecive = (_AllPlaying[i].targetForTrigger != null) ? _AllPlaying[i].targetForTrigger : gameObject;
						string funcToTrigger = (_AllPlaying[i].functionForTrigger != null) ? _AllPlaying[i].functionForTrigger : "OnSoundTrigger";
						toRecive.SendMessage(funcToTrigger, _AllPlaying[i].name, SendMessageOptions.DontRequireReceiver);
						_AllPlaying[i].canShootEvent = false;
					}
				}
				if(_AllPlaying[i].currentSource != null){
					if(!_AllPlaying[i].currentSource.isPlaying && !_AllPlaying[i].isMuted && !_AllPlaying[i].canShootEvent){
						_AllPlaying[i].currentSource.clip = null;
						_AllPlaying.RemoveAt(i);
					}
				}
			}
		}
	}
	
	/// <summary>
	/// Use this to add a sound manually, also return the created Sound.
	/// </summary>
	static public Sound AddSound(AudioClip clip, string name, soundTrack track, string language ,float volume, bool loop, bool fadeIn, float timeToFadeIn, bool fadeOut, float timeToFadeOut, bool is3D, Vector3 posFor3D, bool isTrigger, GameObject triggerTarget, float triggerTime, string functionForTrigger){
		Sound newSound = ScriptableObject.CreateInstance<Sound>();
		newSound.clip = clip;
		newSound.name = name;
		newSound.track = track;
		newSound.language = language;
		newSound.volume = volume;
		newSound.maxVolume = volume;
		newSound.loop = loop;
		newSound.fadeIn = fadeIn;
		newSound.timeToFadeIn = timeToFadeIn;
		newSound.fadeOut = fadeOut;
		newSound.timeToFadeOut = timeToFadeOut;
		newSound._3D = is3D;
		newSound.v3Target = posFor3D;
		newSound.trigger = isTrigger;
		newSound.targetForTrigger = triggerTarget;
		newSound.timeToTrigger = triggerTime;
		newSound.functionForTrigger = functionForTrigger;

		newSound.isStreamSound = false;
		newSound.isResourceSound = false;
		
		List<Sound> holder = new List<Sound>();
		for(int i = 0; i < _AllSounds.Length;i++){
			holder.Add(_AllSounds[i]);
		}
		holder.Add(newSound);
		
		_AllSounds = holder.ToArray();
		return newSound;
	}

	static public Sound AddStreamSound(string URL, string name, soundTrack track, string language ,float volume, bool loop, bool fadeIn, float timeToFadeIn, bool fadeOut, float timeToFadeOut, bool is3D, Vector3 posFor3D, bool isTrigger, GameObject triggerTarget, float triggerTime, string functionForTrigger){
		Sound newSound = ScriptableObject.CreateInstance<Sound>();
		newSound.URL = URL;
		newSound.name = name;
		newSound.track = track;
		newSound.language = language;
		newSound.volume = volume;
		newSound.maxVolume = volume;
		newSound.loop = loop;
		newSound.fadeIn = fadeIn;
		newSound.timeToFadeIn = timeToFadeIn;
		newSound.fadeOut = fadeOut;
		newSound.timeToFadeOut = timeToFadeOut;
		newSound._3D = is3D;
		newSound.v3Target = posFor3D;
		newSound.trigger = isTrigger;
		newSound.targetForTrigger = triggerTarget;
		newSound.timeToTrigger = triggerTime;
		newSound.functionForTrigger = functionForTrigger;

		newSound.isStreamSound = true;
		newSound.isResourceSound = false;

		List<Sound> holder = new List<Sound>();
		for(int i = 0; i < _AllSounds.Length;i++){
			holder.Add(_AllSounds[i]);
		}
		holder.Add(newSound);

		_AllSounds = holder.ToArray();
		return newSound;
	}

	static public Sound AddResourceSound(string resourcePath, string name, soundTrack track, string language ,float volume, bool loop, bool fadeIn, float timeToFadeIn, bool fadeOut, float timeToFadeOut, bool is3D, Vector3 posFor3D, bool isTrigger, GameObject triggerTarget, float triggerTime, string functionForTrigger){
		Sound newSound = ScriptableObject.CreateInstance<Sound>();
		newSound.nameInResourceFolder = resourcePath;
		newSound.name = name;
		newSound.track = track;
		newSound.language = language;
		newSound.volume = volume;
		newSound.maxVolume = volume;
		newSound.loop = loop;
		newSound.fadeIn = fadeIn;
		newSound.timeToFadeIn = timeToFadeIn;
		newSound.fadeOut = fadeOut;
		newSound.timeToFadeOut = timeToFadeOut;
		newSound._3D = is3D;
		newSound.v3Target = posFor3D;
		newSound.trigger = isTrigger;
		newSound.targetForTrigger = triggerTarget;
		newSound.timeToTrigger = triggerTime;
		newSound.functionForTrigger = functionForTrigger;

		newSound.isStreamSound = false;
		newSound.isResourceSound = true;

		List<Sound> holder = new List<Sound>();
		for(int i = 0; i < _AllSounds.Length;i++){
			holder.Add(_AllSounds[i]);
		}
		holder.Add(newSound);

		_AllSounds = holder.ToArray();
		return newSound;
	}

	static public void RemoveSoundByName(string n){
		List<Sound> sHolder = new List<Sound>();

		for(int i = 0; i < _AllSounds.Length;i++){
			if(_AllSounds[i].name != n){
				sHolder.Add(_AllSounds[i]);
			}
		}
		_AllSounds = sHolder.ToArray();
		sHolder.Clear();
		for(int i = 0; i < _AllPlaying.Count;i++){
			if(_AllPlaying[i].name != n){
				sHolder.Add(_AllPlaying[i]);
			}
		}
		_AllPlaying = sHolder;
	}

	static public void RemoveAllNormalSounds(){
		List<Sound> sHolder = new List<Sound>();

		for(int i = 0; i < _AllSounds.Length;i++){
			if(_AllSounds[i].isStreamSound && _AllSounds[i].isResourceSound){
				sHolder.Add(_AllSounds[i]);
			}else{
				ScriptableObject.Destroy(_AllSounds[i]);
			}
		}
		_AllSounds = sHolder.ToArray();
		sHolder.Clear();
		for(int i = 0; i < _AllPlaying.Count;i++){
			if(_AllPlaying[i].isStreamSound && _AllPlaying[i].isResourceSound){
				sHolder.Add(_AllPlaying[i]);
			}else{
				ScriptableObject.Destroy(_AllPlaying[i]);
			}
		}
		_AllPlaying = sHolder;
	}
	static public void RemoveAllStreamSounds(){
		List<Sound> sHolder = new List<Sound>();

		for(int i = 0; i < _AllSounds.Length;i++){
			if(!_AllSounds[i].isStreamSound){
				sHolder.Add(_AllSounds[i]);
			}else{
				ScriptableObject.Destroy(_AllSounds[i]);
			}
		}
		_AllSounds = sHolder.ToArray();
		sHolder.Clear();
		for(int i = 0; i < _AllPlaying.Count;i++){
			if(!_AllPlaying[i].isStreamSound){
				sHolder.Add(_AllPlaying[i]);
			}else{
				ScriptableObject.Destroy(_AllPlaying[i]);
			}
		}
		_AllPlaying = sHolder;
	}
	static public void RemoveAllResourceSounds(){
		List<Sound> sHolder = new List<Sound>();

		for(int i = 0; i < _AllSounds.Length;i++){
			if(!_AllSounds[i].isResourceSound){
				sHolder.Add(_AllSounds[i]);
			}else{
				ScriptableObject.Destroy(_AllSounds[i]);
			}
		}
		_AllSounds = sHolder.ToArray();
		sHolder.Clear();
		for(int i = 0; i < _AllPlaying.Count;i++){
			if(!_AllPlaying[i].isResourceSound){
				sHolder.Add(_AllPlaying[i]);
			}else{
				ScriptableObject.Destroy(_AllPlaying[i]);
			}
		}
		_AllPlaying = sHolder;
	}
	static public void RemoveAllSounds(){
		for(int i = 0; i < _AllSounds.Length;i++){
			ScriptableObject.Destroy(_AllSounds[i]);
		}
		for(int i = 0; i < _AllPlaying.Count;i++){
			ScriptableObject.Destroy(_AllPlaying[i]);
		}
		_AllSounds = new Sound[0];
		_AllPlaying.Clear();
	}

	static private Sound GetSound(string clipName)
	{
		Sound toReturn = null;
		for(int i = 0; i < _AllSounds.Length;i++){
			if(_AllSounds[i].name == clipName)
				toReturn = _AllSounds[i];
		}
		
		if(toReturn == null) Debug.LogWarning("There's no: "+clipName+" AudioClip set. Check the typo");
		return toReturn;
	}
	static private Sound[] GetSounds(string clipName)
	{
		Sound[] toReturn;
		List<Sound> hold = new List<Sound>();
		for(int i = 0; i < _AllSounds.Length;i++){
			if(string.Equals(_AllSounds[i].name, clipName))
				hold.Add(_AllSounds[i]);
		}
		toReturn = new Sound[hold.Count];
		for(int i = 0; i<hold.Count;i++){
			toReturn[i] = hold[i];
		}
		if(toReturn == null || toReturn.Length == 0)
			Debug.LogWarning("There's no: "+clipName+" AudioClip set. Check the typo");
		
		return toReturn;
	}
	static private Sound[] GetSounds(track trackCompare)
	{
		Sound[] toReturn;
		List<Sound> hold = new List<Sound>();
		switch(trackCompare){
		case track.BackgroundSound:
		case track.EffectSound:
		case track.VoiceSound:
			for(int i = 0; i < _AllSounds.Length;i++){
				if(string.Equals(_AllSounds[i].track.ToString(), trackCompare.ToString())){
					hold.Add(_AllSounds[i]);	
				}
			}
			break;
		case track.All:
			for(int i = 0; i < _AllSounds.Length;i++){
				hold.Add(_AllSounds[i]);
			}
			break;
		}
		toReturn = new Sound[hold.Count];
		for(int i = 0; i<hold.Count;i++){
			toReturn[i] = hold[i];
		}
		
		if(toReturn == null || toReturn.Length == 0)
			Debug.LogWarning("There's no: "+trackCompare.ToString()+" Track set. Check the typo");
		
		return toReturn;
		
	}
	
	/// <summary>
	/// Return an Sound Array of the playing name with the string passed as Param.
	/// </summary>
	static public Sound[] GetSoundPlaying(string clipName){
		Sound[] toReturn;
		List<Sound> hold = new List<Sound>();
		for(int i = 0; i < _AllPlaying.Count;i++){
			if(string.Equals(_AllPlaying[i].name, clipName))
				hold.Add(_AllPlaying[i]);
		}
		toReturn = new Sound[hold.Count];
		for(int i = 0; i<hold.Count;i++){
			toReturn[i] = hold[i];
		}
		if(toReturn == null || toReturn.Length == 0)
			Debug.LogWarning("There's no: "+clipName+" AudioClip set. Check the typo");
		
		return toReturn;
	}
	/// <summary>
	/// Return an Sound Array of the playing name with the track passed as Param.
	/// </summary>
	static public Sound[] GetSoundPlaying(track trackCompare){
		Sound[] toReturn;
		List<Sound> hold = new List<Sound>();
		switch(trackCompare){
		case track.BackgroundSound:
		case track.EffectSound:
		case track.VoiceSound:
			for(int i = 0; i < _AllPlaying.Count;i++){
				if(string.Equals(_AllPlaying[i].track.ToString(), trackCompare.ToString())){
					hold.Add(_AllSounds[i]);	
				}
			}
			break;
		case track.All:
			for(int i = 0; i < _AllPlaying.Count;i++){
				hold.Add(_AllPlaying[i]);
			}
			break;
		}
		toReturn = new Sound[hold.Count];
		for(int i = 0; i<hold.Count;i++){
			toReturn[i] = hold[i];
		}
		
		if(toReturn == null || toReturn.Length == 0)
			Debug.LogWarning("There's no: "+trackCompare.ToString()+" Track set. Check the typo");
		
		return toReturn;
	}
	
	/// <summary>
	/// Set the volume of the track passed as Param.
	/// </summary>
	static public void Volume(float volume, track trackCompare){
		Sound[] snds = GetSounds(trackCompare);
		for(int i = 0; i<snds.Length;i++){
            if(volume >= snds[i].maxVolume)
            {
                snds[i].volume = snds[i].maxVolume;
                snds[i].lastVolumeSetted = snds[i].maxVolume;
            }
            else
            {
                snds[i].volume = volume;
                snds[i].lastVolumeSetted = volume;
            }
		}
		for(int i=0;i<_AllPlaying.Count;i++){
			if(string.Equals(_AllPlaying[i].track.ToString(), trackCompare.ToString()) || trackCompare == track.All)
			{
				if(_AllPlaying[i].track == soundTrack.BackgroundSound && !_bkgMuted ||
					_AllPlaying[i].track == soundTrack.EffectSound && !_efxMuted ||
					_AllPlaying[i].track == soundTrack.VoiceSound && !_voiceMuted || !_allMuted){
                    
					if(volume <= 1.0f){
                        if (volume > _AllPlaying[i].maxVolume)
                        {
                            _AllPlaying[i].currentSource.volume = _AllPlaying[i].maxVolume;
                            _AllPlaying[i].lastVolumeSetted = _AllPlaying[i].maxVolume;
                        }
                        else
                        {
                            _AllPlaying[i].currentSource.volume = volume;
                            _AllPlaying[i].lastVolumeSetted = volume;
                        }
						if(volume == 0.0f){ 
							_AllPlaying[i].isMuted = true;
							_AllPlaying[i].pauseEffect = true;
						}else{ 
							_AllPlaying[i].isMuted = false;
							_AllPlaying[i].pauseEffect = false;
						}
					}else{
						_AllPlaying[i].isMuted = false;
						_AllPlaying[i].currentSource.volume = _AllPlaying[i].lastVolumeSetted;
					}
				}
			}
		}
	}
	/// <summary>
	/// Set the volume of the clip name passed as Param.
	/// </summary>
	static public void Volume(float volume, string clipName){
		for(int i=0;i<_AllPlaying.Count;i++){
			if(string.Equals(clipName, _AllPlaying[i].name)){
				if(_AllPlaying[i].track == soundTrack.BackgroundSound && !bkgMuted ||
				_AllPlaying[i].track == soundTrack.EffectSound && !efxMuted ||
				_AllPlaying[i].track == soundTrack.VoiceSound && !voiceMuted){
                    if (volume == 0.0f)
                    {
                        _AllPlaying[i].isMuted = true;
                        _AllPlaying[i].pauseEffect = true;
                    }
                    else
                    {
                        _AllPlaying[i].isMuted = false;
                        _AllPlaying[i].pauseEffect = false;
                    }

                    if(volume > _AllPlaying[i].maxVolume)
                    {
                        _AllPlaying[i].currentSource.volume = _AllPlaying[i].maxVolume;
                        _AllPlaying[i].lastVolumeSetted = _AllPlaying[i].maxVolume;
                    }
                    else
                    {
                        _AllPlaying[i].currentSource.volume = volume;
                        _AllPlaying[i].lastVolumeSetted = volume;
                    }
				}
			}
		}
		Sound clip = GetSound(clipName);
		if(clip == null){
			Debug.LogWarning("There's no: "+clipName+" AudioClip set to change it's volume. Check the typo");
			return;
		}
        if(volume > clip.maxVolume)
        {
            clip.volume = clip.maxVolume;
            clip.lastVolumeSetted = clip.maxVolume;
        }else
        {
            clip.volume = volume;
            clip.lastVolumeSetted = volume;
        }
	}
	
	/// <summary>
	/// Mute the track passed as param.
	/// </summary>
	static public void Mute(bool mute, track compareTrack){
		if(compareTrack == track.BackgroundSound)
			_bkgMuted = mute;
		else if(compareTrack == track.EffectSound)
			_efxMuted = mute;
		else if(compareTrack == track.VoiceSound)
			_voiceMuted = mute;
		else if(compareTrack == track.All){
			_efxMuted = mute;
			_bkgMuted = mute;
			_voiceMuted = mute;
		}
        if (_efxMuted && _voiceMuted && _bkgMuted)
            _allMuted = true;
        else
            _allMuted = false;
		
			Volume((mute)? 0 : 1, compareTrack);		
	}
	/// <summary>
	/// Mute a single clip, passed as Param.
	/// </summary>
	static public void Mute(bool mute, string clipName){
		Volume((mute)? 0 : 1,clipName);
	}
	/// <summary>
	/// Stop every single sound.
	/// </summary>
	static public void StopAllSounds(){
		for(int i=0;i<_AllPlaying.Count;i++){
			if(_AllPlaying[i].currentSource == null){
				_AllPlaying.RemoveAt(i--);
				continue;
			}
            if (_AllPlaying[i].currentSource.isPlaying)
            {
                _AllPlaying[i].currentSource.Stop();
            }else
            {
                _AllPlaying[i].currentSource.clip = null;
            }
		}		
	}
	
	static public void PlayAllSounds(){
		for(int i=0;i<_AllPlaying.Count;i++){
			if(_AllPlaying[i].currentSource == null){
				_AllPlaying.RemoveAt(i--);
				continue;
			}
			_AllPlaying[i].currentSource.PlayDelayed(0);
		}		
	}
	/// <summary>
	/// Stop a single sound, passed as Param as String.
	/// </summary>
	static public void Stop(string name){
		for(int i=0;i<_AllPlaying.Count;i++){
			if(string.Equals(name, _AllPlaying[i].name)){
				_AllPlaying[i].currentSource.Stop();
			}
		}	
	}
	
	/// <summary>
	/// Play the (name) sound.
	/// </summary>
	static public Sound Play(string name){
		return SoundManager.Play(name, 0.0f);
	}
	/// <summary>
	/// Wait for (delay) seconds, than play the (name) sound.
	/// </summary>
	static public Sound Play(string name, float delay){
		return SoundManager.Play(name, delay, 0.0f);
	}
	/// <summary>
	/// Wait for (delay) seconds, than play the (param) sound at (playAt) seconds.
	/// </summary>
	static public Sound Play(string name, float delay, float playAt){
		return SoundManager.Play(name, delay, playAt, -1.0f);
	}
	/// <summary>
	/// Wait for (delay) seconds, than play the (param) sound at (playAt) seconds, and than stop at (stopAt) seconds.
	/// </summary>
	static public Sound Play(string name, float delay, float playAt, float stopAt){
		Sound playingSound = null;
		List<Sound> soundsFound = new List<Sound>();
		for(int i = 0; i < _AllSounds.Length; i++){
            if (string.Equals(_AllSounds[i].name, name))           
                soundsFound.Add(_AllSounds[i]);
		}
		
		if(soundsFound.Count == 0){
			Debug.LogWarning("You sent the wrong name, there's none : "+name+" Sound");
			return null;
		}
		
		if(soundsFound.Count > 1){
			int index = (int)Mathf.Round(Random.Range(0, soundsFound.Count));
			playingSound = soundsFound[index];

            if (_useMultiLanguage && _AllLanguages.Length == 0)
                Debug.LogWarning("You are using Multi Language Feature, but you have no language created.");

			if(_useMultiLanguage){
				int total = soundsFound.Count;
				int counter = 0;
				while(playingSound.language != _currentLanguage && playingSound.language != "All"){
					index++;
					if(index >= total)
						index = 0;
					playingSound = soundsFound[index];
					
					counter++;
					if(counter >= total)
						return null;
				}
			}
		}else{
			playingSound = soundsFound[0];	
			if(_useMultiLanguage){
				if(playingSound.language != _currentLanguage && playingSound.language != "All")
					return null;
			}
		}

		AudioClip toPlay = (!playingSound.isResourceSound) ? playingSound.clip : Resources.Load(playingSound.nameInResourceFolder, typeof(AudioClip)) as AudioClip;

		if(_usePool){
			_totalPool = 0;
			for(int i = 0; i<_pool.Count;i++){
				if(_pool[i].GetComponent<AudioSource>().isPlaying)
					_totalPool++;
			}
			if(_totalPool == _pool.Count)
				return null;
			if(_currentPoolIndex >= _qntPool)
				_currentPoolIndex = 0;
			_currentPlaying = _pool[_currentPoolIndex];
			while(_pool[_currentPoolIndex].GetComponent<AudioSource>().isPlaying){
				_currentPoolIndex++;
				if(_currentPoolIndex >= _qntPool)
					_currentPoolIndex = 0;
				_currentPlaying = _pool[_currentPoolIndex];
			}
			_currentPlaying.transform.parent = _thisGameObject.transform;
			_currentPlaying.transform.localPosition = new Vector3(0,0,0);
			_currentPoolIndex++;
		}else{
			GameObject holderSound = new GameObject("AudioEmitter");
			holderSound.transform.parent = _thisGameObject.transform;
			holderSound.AddComponent<AudioSource>();
			holderSound.transform.localPosition = new Vector3(0,0,0);

			if(!playingSound.loop){
					GameObject.Destroy(holderSound, toPlay.length+5.0f);
			}
			_currentPlaying = holderSound;
		}
		AudioSource AS = _currentPlaying.GetComponent<AudioSource>();
		if(string.Equals(playingSound.track.ToString(), track.EffectSound.ToString()) && efxMuted)
			playingSound.volume = 0;
		if(string.Equals(playingSound.track.ToString(), track.BackgroundSound.ToString()) && bkgMuted)
			playingSound.volume = 0;
		if(string.Equals(playingSound.track.ToString(), track.VoiceSound.ToString()) && voiceMuted)
			playingSound.volume = 0;
		if(allMuted)
			playingSound.volume = 0;

        AS.volume = playingSound.volume;
		
		if(playingSound._3D){
			_currentPlaying.transform.parent = null;
			if(playingSound.followObject){
				_currentPlaying.transform.parent = playingSound.tTarget.transform;
				_currentPlaying.transform.localPosition = new Vector3(0,0,0);
			}
			if(playingSound.v3Target.x != 0 || playingSound.v3Target.y != 0 || playingSound.v3Target.y != 0)
			{
				_currentPlaying.transform.position = playingSound.v3Target;	
			}else if(playingSound.tTarget != null){
				_currentPlaying.transform.position = playingSound.tTarget.transform.position;	
			}
		}
		if(playingSound.fadeIn && playingSound.timeToFadeIn > 0){
			AS.volume = 0;
			playingSound.controllerFadeIn = 0;	
		}
		
		playingSound.controllerFadeOut = 0;
		if(playingSound.fadeOut && playingSound.timeToFadeOut > 0 && playingSound.clip.length > playingSound.timeToFadeOut){
			playingSound.controllerFadeOut = playingSound.clip.length - playingSound.timeToFadeOut;
			playingSound.fadeOutTimer = 0;
		}
		//playingSound.maxVolume = playingSound.volume;
		playingSound.lastVolumeSetted = playingSound.volume;
		AS.loop = playingSound.loop;
		playingSound.stopAt = (stopAt > 0 && stopAt < playingSound.clip.length) ? stopAt : 0;
		playingSound.isMuted = false;
		AS.clip = toPlay;
		playingSound.currentSource = AS;
		if(playingSound._3D && playingSound.followObject){
			playingSound.currentSource.spread = 180.0f;
			playingSound.currentSource.dopplerLevel = 0;
		}
		if(playingSound.timeToTrigger < -1)playingSound.canShootEvent = false;
		else playingSound.canShootEvent = true;
		if(playingSound.timeToTrigger == -1)playingSound.timeToTrigger = playingSound.clip.length;
		_AllPlaying.Add(playingSound);
		if(playingSound.isStreamSound){
			_streamSoundSource.Add(AS);
			_streamSoundClip.Add(playingSound.clip);
			_playStreamSound = true;
			return playingSound;
		}
        AS.PlayScheduled((AudioSettings.dspTime + delay));
		AS.time = playAt;

		playingSound.clip = toPlay;

		//Debug.Log(string.Format("PLAYING: {0}, WITH VOLUME: {1}", playingSound.name, playingSound.volume));
		
		return playingSound;
	}
}
