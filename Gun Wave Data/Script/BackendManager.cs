using UnityEngine;
using BackEnd;
using System;
using UnityEngine.SceneManagement;

public class BackendManager : MonoBehaviour
{
    string guestId = "guest-36d6f4ef-5d60-4a35-9c03-bdf679b5c607"; // 게스트 ID

    public delegate void RankDataLoaded(RankData[] rankData, int totalRankCount);
    public event RankDataLoaded OnRankDataLoaded;

    // Start 함수에서 Backend.Initialize() 호출
    void Start()
    {
        // Backend 초기화
        Backend.Initialize();

        // 로그인이 안 된 상태일 경우 게스트 로그인 처리
        if (!Backend.IsInitialized)  // 초기화되지 않으면 에러 로그 출력
        {
            Debug.LogError("Backend initialization failed.");
            return;
        }

        // 게스트 계정으로 로그인
        GuestLogin();
    }

    // 게스트 로그인 함수
    private void GuestLogin()
    {
        var loginResult = Backend.BMember.GuestLogin(guestId); // 게스트 계정으로 로그인

        if (loginResult.IsSuccess())
        {
            Debug.Log("게스트 로그인 성공");
        }
        else
        {
            Debug.LogError($"게스트 로그인 실패: {loginResult.GetErrorCode()}, {loginResult.GetMessage()}");
        }
    }

    public void SaveScore(string playerName, int playerScore)
    {
        string rankUUID = "0193be4f-c14d-7a99-914b-f8f50e7421cb"; // 기존 리더보드 UUID
        string tableName = "Ranking"; // 리더보드 테이블 이름
        string rowIndate = string.Empty;

        Param param = new Param();
        param.Add("Name", playerName); // 닉네임
        param.Add("Score", playerScore); // 점수

        int currentScore = -1; // 현재 저장된 점수를 추적

        // 기존 데이터를 확인
        var bro = Backend.GameData.Get(tableName, new Where());
        if (bro.IsSuccess())
        {
            if (bro.FlattenRows().Count > 0)
            {
                var row = bro.FlattenRows()[0];
                rowIndate = row["inDate"].ToString();

                // 기존 점수 가져오기
                if (row.ContainsKey("Score"))
                {
                    currentScore = int.Parse(row["Score"].ToString());
                }
            }
            else
            {
                // 기존 데이터가 없으면 새 데이터 삽입
                var bro2 = Backend.GameData.Insert(tableName, param);
                if (bro2.IsSuccess())
                {
                    rowIndate = bro2.GetInDate();
                    Debug.Log("새 데이터 삽입 성공");
                }
            }
        }

        // 점수를 비교하여 기존 점수보다 낮으면 업데이트하지 않음
        if (currentScore >= playerScore)
        {
            Debug.Log($"점수가 낮아 업데이트하지 않음 (현재 점수: {currentScore}, 새로운 점수: {playerScore})");
        }
        else
        {
            // 리더보드에 점수 등록
            var rankBro = Backend.URank.User.UpdateUserScore(rankUUID, tableName, rowIndate, param);
            if (rankBro.IsSuccess())
            {
                Debug.Log("랭킹 등록 성공");
            }
            else
            {
                Debug.LogError($"랭킹 등록 실패: {rankBro.GetErrorCode()}, {rankBro.GetMessage()}");
            }
        }

        // 로그아웃
        var logoutBro = Backend.BMember.Logout();
        if (logoutBro.IsSuccess())
        {
            Debug.Log("로그아웃 성공");
        }
    }

    public void LoadRankingData(int count, int offset)
    {
        string rankUUID = "0193be4f-c14d-7a99-914b-f8f50e7421cb"; // 리더보드 UUID

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

            // 랭킹 데이터를 UI로 전달
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