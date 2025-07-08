using UnityEngine;
using BackEnd;
using System;
using UnityEngine.SceneManagement;

public class BackendManager : MonoBehaviour
{
    string guestId = "guest-36d6f4ef-5d60-4a35-9c03-bdf679b5c607"; // �Խ�Ʈ ID

    public delegate void RankDataLoaded(RankData[] rankData, int totalRankCount);
    public event RankDataLoaded OnRankDataLoaded;

    // Start �Լ����� Backend.Initialize() ȣ��
    void Start()
    {
        // Backend �ʱ�ȭ
        Backend.Initialize();

        // �α����� �� �� ������ ��� �Խ�Ʈ �α��� ó��
        if (!Backend.IsInitialized)  // �ʱ�ȭ���� ������ ���� �α� ���
        {
            Debug.LogError("Backend initialization failed.");
            return;
        }

        // �Խ�Ʈ �������� �α���
        GuestLogin();
    }

    // �Խ�Ʈ �α��� �Լ�
    private void GuestLogin()
    {
        var loginResult = Backend.BMember.GuestLogin(guestId); // �Խ�Ʈ �������� �α���

        if (loginResult.IsSuccess())
        {
            Debug.Log("�Խ�Ʈ �α��� ����");
        }
        else
        {
            Debug.LogError($"�Խ�Ʈ �α��� ����: {loginResult.GetErrorCode()}, {loginResult.GetMessage()}");
        }
    }

    public void SaveScore(string playerName, int playerScore)
    {
        string rankUUID = "0193be4f-c14d-7a99-914b-f8f50e7421cb"; // ���� �������� UUID
        string tableName = "Ranking"; // �������� ���̺� �̸�
        string rowIndate = string.Empty;

        Param param = new Param();
        param.Add("Name", playerName); // �г���
        param.Add("Score", playerScore); // ����

        int currentScore = -1; // ���� ����� ������ ����

        // ���� �����͸� Ȯ��
        var bro = Backend.GameData.Get(tableName, new Where());
        if (bro.IsSuccess())
        {
            if (bro.FlattenRows().Count > 0)
            {
                var row = bro.FlattenRows()[0];
                rowIndate = row["inDate"].ToString();

                // ���� ���� ��������
                if (row.ContainsKey("Score"))
                {
                    currentScore = int.Parse(row["Score"].ToString());
                }
            }
            else
            {
                // ���� �����Ͱ� ������ �� ������ ����
                var bro2 = Backend.GameData.Insert(tableName, param);
                if (bro2.IsSuccess())
                {
                    rowIndate = bro2.GetInDate();
                    Debug.Log("�� ������ ���� ����");
                }
            }
        }

        // ������ ���Ͽ� ���� �������� ������ ������Ʈ���� ����
        if (currentScore >= playerScore)
        {
            Debug.Log($"������ ���� ������Ʈ���� ���� (���� ����: {currentScore}, ���ο� ����: {playerScore})");
        }
        else
        {
            // �������忡 ���� ���
            var rankBro = Backend.URank.User.UpdateUserScore(rankUUID, tableName, rowIndate, param);
            if (rankBro.IsSuccess())
            {
                Debug.Log("��ŷ ��� ����");
            }
            else
            {
                Debug.LogError($"��ŷ ��� ����: {rankBro.GetErrorCode()}, {rankBro.GetMessage()}");
            }
        }

        // �α׾ƿ�
        var logoutBro = Backend.BMember.Logout();
        if (logoutBro.IsSuccess())
        {
            Debug.Log("�α׾ƿ� ����");
        }
    }

    public void LoadRankingData(int count, int offset)
    {
        string rankUUID = "0193be4f-c14d-7a99-914b-f8f50e7421cb"; // �������� UUID

        var bro = Backend.URank.User.GetRankList(rankUUID, count, offset);

        if (bro.IsSuccess())
        {
            LitJson.JsonData rankListJson = bro.GetFlattenJSON();
            RankData[] rankData = new RankData[rankListJson["rows"].Count];
            for (int i = 0; i < rankListJson["rows"].Count; i++)
            {
                rankData[i] = new RankData
                {
                    Rank = int.Parse(rankListJson["rows"][i]["rank"].ToString()),
                    Name = rankListJson["rows"][i]["nickname"].ToString(),
                    Score = int.Parse(rankListJson["rows"][i]["score"].ToString())
                };
            }

            int totalRankCount = int.Parse(rankListJson["totalCount"].ToString());

            // ��ŷ �����͸� UI�� ����
            OnRankDataLoaded?.Invoke(rankData, totalRankCount);
        }
        else
        {
            Debug.LogError($"Failed to load ranking data: {bro.GetErrorCode()}, {bro.GetMessage()}");
        }
    }

    public void Menu()
    {
        SceneManager.LoadScene("Menu");
    }
}

[System.Serializable]
public class RankData
{
    public int Rank;
    public string Name;
    public int Score;
}