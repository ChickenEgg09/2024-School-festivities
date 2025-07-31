using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static int score { get; set; }  // set�� public���� �����Ͽ� �ܺο��� ������ ������ �� �ְ� ��
    float lastEnemyKillTime;
    int streakCount;
    float streakExpiryTime = 1f; // 1�� �̳��� ���� óġ�� �� ���� ���
    int maxStreakBonus = 5; // �ִ� ���� óġ ���ʽ�

    void Start()
    {
        Enemy.OnDeathStatic += OnEnemyKilled;
        FindObjectOfType<Player>().OnDeath += OnPlayerDeath;
    }

    void OnEnemyKilled()
    {
        // ���� óġ üũ
        if (Time.time < lastEnemyKillTime + streakExpiryTime)
        {
            streakCount = Mathf.Min(streakCount + 1, maxStreakBonus); // streakCount ����
        }
        else
        {
            streakCount = 0;
        }

        lastEnemyKillTime = Time.time;

        // �⺻ ���� + ���� óġ ����
        score += 5 + (int)Mathf.Pow(2, streakCount); // ���� óġ�� ���� ���� ����
    }

    void OnPlayerDeath()
    {
        Enemy.OnDeathStatic -= OnEnemyKilled;
    }
}
