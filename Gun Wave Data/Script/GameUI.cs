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
    public Slider slider;  // Slider�� ����

    public Text ammoCountText; // �Ѿ� ���� ǥ���� UI �ؽ�Ʈ
    public Text ammoCountShadow;
    public GameObject count;

    Spawner spawner;
    Player player;
    BackendManager backendManager; // BackendRank ���� �߰�
    UserManager userManager;

    void Start()
    {
        player = FindObjectOfType<Player>();
        player.OnDeath += OnGameOver;

        backendManager = FindObjectOfType<BackendManager>(); // BackendRank ���� �ʱ�ȭ
        userManager = FindObjectOfType<UserManager>();

        // fadePlane ���� Ȯ��
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

        // ü�� �ٸ� Slider�� ������Ʈ (value�� ����)
        slider.value = healthPercent;  // Slider�� ���� ü�� ������ ����

        // �÷��̾��� ��ġ�� ���� ü�� �ٸ� ��ġ��Ŵ
        if (player != null)
        {
            // �÷��̾��� ��ġ�� ȭ�� ��ǥ�� ��ȯ
            Vector3 hpPosition = player.transform.position;
            hpPosition.y += 3f;  // �÷��̾��� ���� ��ġ (���� �����Ͽ� ��ġ�� fine-tune)

            // ���� ��ǥ�� ȭ�� ��ǥ�� ��ȯ�Ͽ� ü�� ���� ��ġ ����
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
        string enemyCountString = ((spawner.waves[waveNumber - 1].infinite) ? "��" : spawner.waves[waveNumber - 1].enemyCount + "");
        newWaveEnemyCount.text = "Enemies : " + enemyCountString;

        StopCoroutine("AnimateNewWaveBanner");
        StartCoroutine("AnimateNewWaveBanner");
    }

    public void OnGameOver()
    {
        Cursor.visible = true;

        // Canvas�� ��Ȱ��ȭ�Ǿ� ������ �ٽ� Ȱ��ȭ
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            canvas.gameObject.SetActive(true);  // Canvas Ȱ��ȭ
        }

        // Fade ȿ�� ����
        StartCoroutine(Fade(Color.clear, new Color(0, 0, 0, .95f), 1));

        // �÷��̾� �̸��� ���� ��������
        string playerName = Player.playerName;
        int playerScore = ScoreKeeper.score;

        userManager.SignUpAndLogin(playerName);

        // ���� �ʱ�ȭ
        ScoreKeeper.score = 0;

        // ������ UUID, �̸��� Backend�� ����
        backendManager.SaveScore(playerName, playerScore);

        // ���� ���� UI ������Ʈ
        gameOverScoreUI.text = playerScore.ToString("D6");

        // scoreUI, slider �����, gameOverUI Ȱ��ȭ
        scoreUI.gameObject.SetActive(false);
        count.gameObject.SetActive(false);
        slider.gameObject.SetActive(false);
        gameOverUI.SetActive(true); // ���� ���� UI Ȱ��ȭ
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
        // ������ ���� ������ �� ���� �ʱ�ȭ
        ScoreKeeper.score = 0;
        SceneManager.LoadScene("Game");
    }

    public void StartNewMenu()
    {
        Player.playerName = "";
        SceneManager.LoadScene("Menu");
    }
}
