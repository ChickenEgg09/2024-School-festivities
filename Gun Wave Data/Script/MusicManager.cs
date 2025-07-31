using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AudioClip mainTheme; // ���� ����
    public AudioClip menuTheme; // �޴� ����

    private string sceneName; // ���� �� �̸�
    private AudioSource audioSource; // ������ ����� AudioSource

    void Awake()
    {
        // AudioSource�� ������ �߰�
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true; // ���� �ݺ� ���
            DontDestroyOnLoad(gameObject); // ���� �ٲ� ������Ʈ ����
        }
    }

    void OnEnable()
    {
        // �� �ε� �̺�Ʈ�� �޼��� ���
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // �� �ε� �̺�Ʈ���� �޼��� ����
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ���� �ε�� �� ȣ��Ǵ� �޼���
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string newSceneName = scene.name;

        // ���� ����� ��쿡�� ���� ���
        if (newSceneName != sceneName)
        {
            sceneName = newSceneName;
            PlayMusicForScene(sceneName);
        }
    }

    // ���� ���� ������ ���� ���
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
            audioSource.Play(); // ���� ���
        }
    }
}
