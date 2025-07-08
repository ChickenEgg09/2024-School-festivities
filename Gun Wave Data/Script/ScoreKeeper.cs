using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static int score { get; set; }  // set을 public으로 변경하여 외부에서 점수를 수정할 수 있게 함
    float lastEnemyKillTime;
    int streakCount;
    float streakExpiryTime = 1f; // 1초 이내에 연속 처치할 때 점수 상승
    int maxStreakBonus = 5; // 최대 연속 처치 보너스

    void Start()
    {
        Enemy.OnDeathStatic += OnEnemyKilled;
        FindObjectOfType<Player>().OnDeath += OnPlayerDeath;
    }

    void OnEnemyKilled()
    {
        // 연속 처치 체크
        if (Time.time < lastEnemyKillTime + streakExpiryTime)
        {
            streakCount = Mathf.Min(streakCount + 1, maxStreakBonus); // streakCount 제한
        }
        else
        {
            streakCount = 0;
        }

        lastEnemyKillTime = Time.time;

        // 기본 점수 + 연속 처치 점수
        score += 5 + (int)Mathf.Pow(2, streakCount); // 연속 처치에 따른 점수 증가
    }

    void OnPlayerDeath()
    {
        Enemy.OnDeathStatic -= OnEnemyKilled;
    }
}
