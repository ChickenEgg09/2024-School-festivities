using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Image fadePlane;
    public GameObject gameOverUI;

    public RectTransform newWaveBanner;
    public Text newWaveTitle;
    public Text newWaveEnemyCount;
    public Text scoreUI;
    public Text gameOverScoreUI;
    public Slider slider;  // Slider로 변경

    public Text ammoCountText; // 총알 수를 표시할 UI 텍스트
    public Text ammoCountShadow;
    public GameObject count;

    Spawner spawner;
    Player player;
    BackendManager backendManager; // BackendRank 참조 추가
    UserManager userManager;

    void Start()
    {
        player = FindObjectOfType<Player>();
        player.OnDeath += OnGameOver;

        backendManager = FindObjectOfType<BackendManager>(); // BackendRank 참조 초기화
        userManager = FindObjectOfType<UserManager>();

        // fadePlane 연결 확인
        if (fadePlane == null)
        {
            Debug.LogError("Fade Plane is not assigned!");
        }
    }

    void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }

    void Update()
    {
        scoreUI.text = ScoreKeeper.score.ToString("D6");

        float healthPercent = 0;
        if (player != null)
        {
            healthPercent = player.health / player.startingHealth;
        }

        // 체력 바를 Slider로 업데이트 (value를 설정)
        slider.value = healthPercent;  // Slider의 값을 체력 비율로 설정

        // 플레이어의 위치에 맞춰 체력 바를 위치시킴
        if (player != null)
        {
            // 플레이어의 위치를 화면 좌표로 변환
            Vector3 hpPosition = player.transform.position;
            hpPosition.y += 3f;  // 플레이어의 위에 배치 (값을 조정하여 위치를 fine-tune)

            // 월드 좌표를 화면 좌표로 변환하여 체력 바의 위치 설정
            Vector3 screenPos = Camera.main.WorldToScreenPoint(hpPosition);
            slider.transform.position = screenPos;
        }
    }

    public void GunCount(int remainingInMag, int permag)
    {
        ammoCountText.text = remainingInMag.ToString() + "/" + permag.ToString();
        ammoCountShadow.text = remainingInMag.ToString() + "/" + permag.ToString();
    }

    void OnNewWave(int waveNumber)
    {
        string[] numbers = { "One", "Two", "Three", "Four", "Five" };
        newWaveTitle.text = "- Wave " + numbers[waveNumber - 1] + " -";
        string enemyCountString = ((spawner.waves[waveNumber - 1].infinite) ? "∞" : spawner.waves[waveNumber - 1].enemyCount + "");
        newWaveEnemyCount.text = "Enemies : " + enemyCountString;

        StopCoroutine("AnimateNewWaveBanner");
        StartCoroutine("AnimateNewWaveBanner");
    }

    public void OnGameOver()
    {
        Cursor.visible = true;

        // Canvas가 비활성화되어 있으면 다시 활성화
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            canvas.gameObject.SetActive(true);  // Canvas 활성화
        }

        // Fade 효과 시작
        StartCoroutine(Fade(Color.clear, new Color(0, 0, 0, .95f), 1));

        // 플레이어 이름과 점수 가져오기
        string playerName = Player.playerName;
        int playerScore = ScoreKeeper.score;

        userManager.SignUpAndLogin(playerName);

        // 점수 초기화
        ScoreKeeper.score = 0;

        // 점수와 UUID, 이름을 Backend에 저장
        backendManager.SaveScore(playerName, playerScore);

        // 게임 오버 UI 업데이트
        gameOverScoreUI.text = playerScore.ToString("D6");

        // scoreUI, slider 숨기고, gameOverUI 활성화
        scoreUI.gameObject.SetActive(false);
        count.gameObject.SetActive(false);
        slider.gameObject.SetActive(false);
        gameOverUI.SetActive(true); // 게임 오버 UI 활성화
    }

    IEnumerator AnimateNewWaveBanner()
    {
        float delayTime = 1.5f;
        float speed = 3f;
        float animatePercent = 0;
        int dir = 1;

        float endDelayTime = Time.time + 1 / speed + delayTime;

        while (animatePercent >= 0)
        {
            animatePercent += Time.deltaTime * speed * dir;

            if (animatePercent >= 1)
            {
                animatePercent = 1;

                if (Time.time > endDelayTime)
                {
                    dir = -1;
                }
            }

            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-170, 45, animatePercent);
            yield return null;
        }
    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;

        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }

    public void StartNewGame()
    {
        // 게임을 새로 시작할 때 점수 초기화
        ScoreKeeper.score = 0;
        SceneManager.LoadScene("Game");
    }

    public void StartNewMenu()
    {
        Player.playerName = "";
        SceneManager.LoadScene("Menu");
    }
}
