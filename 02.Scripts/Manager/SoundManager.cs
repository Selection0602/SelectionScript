using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class SoundManager : MonoBehaviour
{
    private AudioSource _bgmSource;

    private AudioMixer _audioMixer;
    private const string MASTER_VOLUME_PARAM = "MasterVolume";
    private const string BGM_VOLUME_PARAM = "BGMVolume";
    private const string SFX_VOLUME_PARAM = "SFXVolume";

    private Dictionary<BGMType, AudioClip> _bgmClips = new Dictionary<BGMType, AudioClip>();
    private Dictionary<SFXType, AudioClip> _sfxClips = new Dictionary<SFXType, AudioClip>();

    // 오디오 소스 풀
    private const int SFX_SOURCE_COUNT = 10;
    private Queue<AudioSource> _sfxSourcePool = new Queue<AudioSource>();
    private List<AudioSource> _activeSfxSources = new List<AudioSource>();

    public float MasterVolume { get; private set; } = 1f;
    public float BGMVolume { get; private set; } = 1f;
    public float SFXVolume { get; private set; } = 1f;

    private Dictionary<string, BGMType> _sceneBGMDict = new Dictionary<string, BGMType>()
    {
        {"StartScene", BGMType.BGM_Main},
        {"MapScene", BGMType.BGM_Main},
        {"EndingScene", BGMType.None},
        {"ResultScene", BGMType.None},
        {"RestScene", BGMType.BGM_Battle},
        {"BattleScene", BGMType.BGM_Battle},
        {"MiniGameScene_01", BGMType.BGM_MiniGame},
        {"MiniGameScene_02", BGMType.BGM_MiniGame},
        {"MiniGameScene_03", BGMType.BGM_MiniGame},
        {"RandomEventScene", BGMType.BGM_Battle},
    };
    
    private void Start()
    {
        // 오디오 믹서 가져오기
        _audioMixer = Resources.Load("AudioMixer/AudioMixer") as AudioMixer;

        // 씬 로드 및 언로드 이벤트 등록
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // BGM 소스 생성
        if (_bgmSource == null)
        {
            GameObject bgmObject = new GameObject("BGMSource");
            bgmObject.transform.SetParent(transform);
            _bgmSource = bgmObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;

            if (_audioMixer != null)
            {
                // 오디오 믹서와 오디오 소스 연결
                AudioMixerGroup[] groups = _audioMixer.FindMatchingGroups("BGM");
                if (groups.Length > 0)
                    _bgmSource.outputAudioMixerGroup = groups[0];
            }
        }

        // 효과음 소스 풀 초기화
        InitializeSfxPool();

        // 볼륨 설정 로드 및 적용
        LoadVolumeSetting();
        
        SetMasterVolume(MasterVolume);
        SetBGMVolume(BGMVolume);
        SetSFXVolume(SFXVolume);

        if(SceneManager.GetActiveScene().buildIndex != 1)
            InitializeAudioClips(SceneManager.GetActiveScene().name);
    }

    #region 어드레서블 데이터 로드
    // 어드레서블에서 오디오 클립 로드
    private async void InitializeAudioClips(string sceneName)
    {
        // 로딩씬을 통해 전달 받은 데이터가 있다면 어드레서블을 사용하지 않고 바로 적용
        if (Manager.Instance.AddressableManager.HasHandle($"BGM, {sceneName}") && _sceneBGMDict[sceneName] != BGMType.None)
        {
            var handle = Manager.Instance.AddressableManager.GetHandle($"BGM, {sceneName}");
            SetBGMClips(handle);
        }
        else if(_sceneBGMDict[sceneName] != BGMType.None)
        // 로딩씬을 통해 전달 받은 데이터가 없다면 직접 불러옴
             await LoadBGMClips(sceneName);

        if (Manager.Instance.AddressableManager.HasHandle($"SFX, {sceneName}"))
        {
            var handle = Manager.Instance.AddressableManager.GetHandle($"SFX, {sceneName}");
            SetSFXClips(handle);
        }
        else
            await LoadSFXClips(sceneName);

        if(_sceneBGMDict.ContainsKey(sceneName))
            PlayBGM(_sceneBGMDict[sceneName]);
    }

    private void SetBGMClips(AsyncOperationHandle handle)
    {
        if(handle.Result is not IList<Object> clips) return;
            
        foreach (var clip in clips)
        {
            if(clip is AudioClip audioClip && Enum.TryParse(audioClip.name, true, out BGMType audioType))
                _bgmClips.TryAdd(audioType, audioClip);
        }
    }

    private void SetSFXClips(AsyncOperationHandle handle)
    {
        if(handle.Result is not IList<Object> clips) return;

        foreach (var clip in clips)
        {
            if(clip is AudioClip audioClip && Enum.TryParse(audioClip.name, true, out SFXType audioType))
                _sfxClips.TryAdd(audioType, audioClip);
        }
    }

    private async Task LoadBGMClips(string sceneName)
    {
        var bgmLabels = new List<string> { "BGM", sceneName };

        List<AudioClip> bgmList =
            await Manager.Instance.AddressableManager.LoadGroupAssetsAsync<AudioClip>(bgmLabels,
                Addressables.MergeMode.Intersection);

        foreach (var clip in bgmList)
        {
            if (Enum.TryParse(clip.name, out BGMType bgm))
                _bgmClips[bgm] = clip;
            else
                Debug.LogWarning("BGM 클립 이름이 enum과 일치하지 않습니다: " + clip.name);
        }

        Manager.Instance.AddressableManager.UnloadAssets(bgmLabels);
        
        Debug.Log($"BGM {_bgmClips.Count}개 로드 완료");
    }

    // SFX 클립 로드
    private async Task LoadSFXClips(string sceneName)
    {
        var sfxLabel = new List<string> { "SFX", sceneName };
        
        List<AudioClip> sfxList =
            await Manager.Instance.AddressableManager.LoadGroupAssetsAsync<AudioClip>(sfxLabel,
                Addressables.MergeMode.Intersection);

        foreach (var clip in sfxList)
        {
            if (Enum.TryParse(clip.name, out SFXType sfx))
                _sfxClips[sfx] = clip;
            else
                Debug.LogWarning("SFX 클립 이름이 enum과 일치하지 않습니다: " + clip.name);
        }

        Manager.Instance.AddressableManager.UnloadAssets(sfxLabel);
        
        Debug.Log($"효과음 {_sfxClips.Count}개 로드 완료");
    }
    #endregion

    #region 효과음 오브젝트 풀 관리
    // 효과음 소스 풀 초기화
    private void InitializeSfxPool()
    {
        // COUNT 만큼 오디오 소스 생성
        for (int i = 0; i < SFX_SOURCE_COUNT; i++)
        {
            GameObject sfxObject = new GameObject($"SFXSource_{i}");
            sfxObject.transform.SetParent(transform);
            AudioSource source = sfxObject.AddComponent<AudioSource>();
            source.playOnAwake = false;

            if (_audioMixer != null)
            {
                // 오디오 믹서와 오디오 소스 연결
                AudioMixerGroup[] groups = _audioMixer.FindMatchingGroups("SFX");
                if (groups.Length > 0)
                    source.outputAudioMixerGroup = groups[0];
            }

            _sfxSourcePool.Enqueue(source);
        }
    }
    
    // 오디오 소스 풀에서 가져오기
    private AudioSource GetSfxSourceFromPool()
    {
        if (_sfxSourcePool.Count == 0)
        {
            // 활성 오디오 소스 중 재생이 끝난 것이 있는지 확인
            for (int i = 0; i < _activeSfxSources.Count; i++)
            {
                if (!_activeSfxSources[i].isPlaying)
                {
                    AudioSource source = _activeSfxSources[i];
                    _activeSfxSources.RemoveAt(i);
                    return source;
                }
            }

            // 없으면 새로 생성
            GameObject sfxObject = new GameObject($"SFX_Source_Extra");
            sfxObject.transform.SetParent(transform);
            AudioSource sfxSource = sfxObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            _activeSfxSources.Add(sfxSource);
            return sfxSource;
        }

        AudioSource pooledSource = _sfxSourcePool.Dequeue();
        _activeSfxSources.Add(pooledSource);
        return pooledSource;
    }

    // 재생 완료된 소스 풀에 반환
    private IEnumerator ReturnToPoolWhenFinished(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);

        if (_activeSfxSources.Contains(source))
        {
            _activeSfxSources.Remove(source);
            _sfxSourcePool.Enqueue(source);
        }
    }
    #endregion

    #region BGM/SFX 재생 및 일시정지
    /// <summary>
    /// BGM 재생
    /// </summary>
    /// <param name="clipType">재생할 BGM Enum값</param>
    /// <param name="fadeTime">페이드 인/아웃 시간 (기본값 : 1초)</param>
    public void PlayBGM(BGMType clipType, float fadeTime = 1.0f)
    {
        if (SceneManager.GetActiveScene().buildIndex == 1 || !_bgmClips.ContainsKey(clipType)) return;
        
        if (clipType == BGMType.None)
            return;
        
        StartCoroutine(FadeBGM(_bgmClips[clipType], fadeTime));
    }

    // BGM 페이드 인/아웃
    private IEnumerator FadeBGM(AudioClip newClip, float fadeTime)
    {
        // 기존 BGM 페이드 아웃
        if (_bgmSource.isPlaying && fadeTime > 0)
        {
            float startVolume = _bgmSource.volume;
            float timer = 0;

            while (timer < fadeTime / 2f)
            {
                timer += Time.deltaTime;
                _bgmSource.volume = Mathf.Lerp(startVolume, 0, timer / (fadeTime / 2f));
                yield return null;
            }

            _bgmSource.Stop();
        }

        // 음악 변경
        _bgmSource.clip = newClip;
        _bgmSource.volume = 0;
        _bgmSource.Play();

        // 페이드 인
        float fadeInTimer = 0;
        while (fadeInTimer < fadeTime)
        {
            fadeInTimer += Time.deltaTime;
            _bgmSource.volume = Mathf.Lerp(0, BGMVolume, fadeInTimer / fadeTime);
            yield return null;
        }
        _bgmSource.volume = BGMVolume;
    }
    
    private IEnumerator FadeOutBGM()
    {
        float fadeTime = 0.5f;
        
        if (_bgmSource.isPlaying && fadeTime > 0)
        {
            float startVolume = _bgmSource.volume;
            float timer = 0;

            while (timer < fadeTime)
            {
                timer += Time.deltaTime;
                _bgmSource.volume = Mathf.Lerp(startVolume, 0, timer / fadeTime);
                yield return null;
            }

            _bgmSource.Stop();
        }
    }
    
    /// <summary>
    /// 효과음 재생
    /// </summary>
    /// <param name="sfxType">재생할 효과음 Enum값</param>
    public void PlaySFX(SFXType sfxType)
    {
        if (!_sfxClips.ContainsKey(sfxType)) return;
        
        AudioSource source = GetSfxSourceFromPool();
        if (source == null) return;

        source.clip = _sfxClips[sfxType];
        source.volume = SFXVolume;
        source.Play();

        StartCoroutine(ReturnToPoolWhenFinished(source));
    }
    
    /// <summary>
    /// BGM 일시 정지
    /// </summary>
    public void PauseBGM() => _bgmSource.Pause();
    
    /// <summary>
    /// 일시 정지된 BGM 다시 재생
    /// </summary>
    public void ResumeBGM() => _bgmSource.UnPause();
    #endregion

    #region 볼륨 설정 관련
    /// <summary>
    /// 마스터 볼륨 설정
    /// </summary>
    /// <param name="volume">설정할 볼륨</param>
    public void SetMasterVolume(float volume)
    {
        // 볼륨을 0과 1 사이로 제한
        volume = Mathf.Clamp01(volume);
        MasterVolume = volume;

        // 마스터 볼륨을 데시벨로 변환하여 오디오 믹서에 설정 후 PlayerPrefs에 저장
        float dbValue = ConvertToDecibel(volume);
        _audioMixer.SetFloat(MASTER_VOLUME_PARAM, dbValue);
        PlayerPrefs.SetFloat(MASTER_VOLUME_PARAM, MasterVolume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 배경음 볼륨 설정
    /// </summary>
    /// <param name="volume">설정할 볼륨</param>
    public void SetBGMVolume(float volume)
    {
        // 볼륨을 0과 1 사이로 제한
        volume = Mathf.Clamp01(volume);
        BGMVolume = volume;

        // 배경음 볼륨을 데시벨로 변환하여 오디오 믹서에 설정 후 PlayerPrefs에 저장
        float dbValue = ConvertToDecibel(volume);
        _audioMixer.SetFloat(BGM_VOLUME_PARAM, dbValue);
        PlayerPrefs.SetFloat(BGM_VOLUME_PARAM, BGMVolume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 효과음 볼륨 설정
    /// </summary>
    /// <param name="volume">설정할 볼륨</param>
    public void SetSFXVolume(float volume)
    {
        // 볼륨을 0과 1 사이로 제한
        volume = Mathf.Clamp01(volume);
        SFXVolume = volume;

        // 효과음 볼륨을 데시벨로 변환하여 오디오 믹서에 설정 후 PlayerPrefs에 저장
        float dbValue = ConvertToDecibel(volume);
        _audioMixer.SetFloat(SFX_VOLUME_PARAM, dbValue);
        PlayerPrefs.SetFloat(SFX_VOLUME_PARAM, SFXVolume);
        PlayerPrefs.Save();
    }

    // PlayerPrefs에 저장된 볼륨 설정 가져오기
    private void LoadVolumeSetting()
    {
        MasterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_PARAM, 1);
        BGMVolume = PlayerPrefs.GetFloat(BGM_VOLUME_PARAM, 1);
        SFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME_PARAM, 1);
    }
    
    // 볼륨(0-1)을 데시벨(-80-0)로 변환
    private float ConvertToDecibel(float linearVolume) => linearVolume > 0.0001f ? Mathf.Log10(linearVolume) * 20f : -80f;
    #endregion
    
    #region 씬 이벤트
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 로드 후 BGM과 SFX 클립 초기화 및 BGM 재생
        if (SceneManager.GetActiveScene().buildIndex != 1)
        {
            InitializeAudioClips(SceneManager.GetActiveScene().name);
            PlayBGM(_sceneBGMDict[SceneManager.GetActiveScene().name]);
        }
    }
    
    private void OnSceneUnloaded(Scene scene)
    {
        StartCoroutine(FadeOutBGM());
        ClearClips();
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    #endregion
    
    // 씬이 언로드 될 때 BGM과 SFX 클립 초기화
    private void ClearClips()
    {
        if(SceneBase.Current is LoadingScene) return;
        if (_bgmClips.Count == 0 && _sfxClips.Count == 0) return;

        _bgmClips.Clear();
        _sfxClips.Clear();
    }
}