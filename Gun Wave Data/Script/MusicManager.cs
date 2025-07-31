using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioClip mainTheme; // 게임 음악
    public AudioClip menuTheme; // 메뉴 음악

    private string sceneName; // 현재 씬 이름
    private AudioSource audioSource; // 음악을 재생할 AudioSource

    void Awake()
    {
        // AudioSource가 없으면 추가
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true; // 음악 반복 재생
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 오브젝트 유지
        }
    }

    void OnEnable()
    {
        // 씬 로드 이벤트에 메서드 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 씬 로드 이벤트에서 메서드 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드될 때 호출되는 메서드
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string newSceneName = scene.name;

        // 씬이 변경된 경우에만 음악 재생
        if (newSceneName != sceneName)
        {
            sceneName = newSceneName;
            PlayMusicForScene(sceneName);
        }
    }

    // 씬에 따라 적절한 음악 재생
    void PlayMusicForScene(string currentScene)
    {
        AudioClip clipToPlay = null;

        if (currentScene == "Menu")
        {
            clipToPlay = menuTheme;
        }
        else if (currentScene == "Game")
        {
            clipToPlay = mainTheme;
        }
        else if (currentScene == "Rank")
        {
            clipToPlay = menuTheme;
        }

        if (clipToPlay != null && audioSource.clip != clipToPlay)
        {
            audioSource.clip = clipToPlay;
            audioSource.Play(); // 음악 재생
        }
    }
}
