using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RankingManager : MonoBehaviour
{
    public GameObject[] rankSlots; // ���� UI ���� ������Ʈ��
    public Button prevButton; // ���� ��ư
    public Button nextButton; // ���� ��ư

    private BackendManager backendManager;
    private int currentPage = 1; // ���� ������ (1���������� ����)
    private int itemsPerPage = 5; // �������� �׸� ��
    private int totalRanks = 0; // ��ü ���� ����

    void Start()
    {
        // BackendManager�� ã�� �̺�Ʈ ����
        backendManager = FindObjectOfType<BackendManager>();
        backendManager.OnRankDataLoaded += UpdateUI;

        // ù ������ ������ �ε�
        LoadPage(currentPage);
    }

    public void LoadPage(int page)
    {
        backendManager.LoadRankingData(itemsPerPage, (page - 1) * itemsPerPage);
    }

    void UpdateUI(RankData[] rankData, int totalRankCount)
    {
        totalRanks = totalRankCount;

        for (int i = 0; i < rankSlots.Length; i++)
        {
            if (rankSlots[i] == null) continue;

            Transform rankSlot = rankSlots[i].transform;

            UnityEngine.UI.Text rankText = rankSlot.Find("Rank")?.GetComponent<UnityEngine.UI.Text>();
            UnityEngine.UI.Text nameText = rankSlot.Find("Name")?.GetComponent<UnityEngine.UI.Text>();
            UnityEngine.UI.Text scoreText = rankSlot.Find("Score")?.GetComponent<UnityEngine.UI.Text>();

            if (i < rankData.Length)
            {
                rankText.text = rankData[i].Rank.ToString();
                nameText.text = rankData[i].Name;
                scoreText.text = rankData[i].Score.ToString();
            }
            else
            {
                rankText.text = "-";
                nameText.text = "-";
                scoreText.text = "-";
            }
        }

        UpdateButtonVisibility();
    }

    private void UpdateButtonVisibility()
    {
        prevButton.gameObject.SetActive(currentPage > 1);
        nextButton.gameObject.SetActive(currentPage * itemsPerPage < totalRanks);
    }

    public void OnNextPage()
    {
        if (currentPage * itemsPerPage < totalRanks)
        {
            currentPage++;
            LoadPage(currentPage);
        }
    }

    public void OnPrevPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            LoadPage(currentPage);
        }
    }
}
