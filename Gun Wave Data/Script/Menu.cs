using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections; // �ڷ�ƾ�� ����ϱ� ���� �ʿ�

public class Menu : MonoBehaviour
{
    public GameObject mainMenuHolder;
    public GameObject optionsMenuHolder;
    public GameObject nameInputHolder; // �̸� �Է� UI
    public InputField nameInputField; // �̸� �Է� �ʵ�
    public GameObject warningBar; // ��� �޽��� ��

    public Slider[] volumeSliders;

    void Start()
    {
        volumeSliders[0].value = AudioManager.instance.masterVolumePercent;
        volumeSliders[1].value = AudioManager.instance.musicVolumePercent;
        volumeSliders[2].value = AudioManager.instance.sfxVolumePercent;
    }

    public void Play()
    {
        mainMenuHolder.SetActive(false);
        nameInputHolder.SetActive(true);
    }

    public void SubmitName()
    {
        // �Էµ� �̸� ����
        string playerName = nameInputField.text;

        if (IsValidName(playerName))
        {
            // �̸��� ��ȿ�ϸ� Player Ŭ������ ����
            Player.playerName = playerName;

            mainMenuHolder.SetActive(true);
            nameInputHolder.SetActive(false);
            SceneManager.LoadScene("Game"); // Game ������ �̵�
        }
        else
        {
            // ��� �ؽ�Ʈ�� ǥ���ϰ�, 3�� �Ŀ� ������� ����
            warningBar.gameObject.SetActive(true);
            StartCoroutine(HideWarningAfterDelay(3f)); // 3�� �Ŀ� ��� �޽��� �����
        }
    }

    public void Rank()
    {
        SceneManager.LoadScene("Rank");
    }

    public void NameMenu()
    {
        mainMenuHolder.SetActive(true);
        nameInputHolder.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OptionsMenu()
    {
        mainMenuHolder.SetActive(false);
        optionsMenuHolder.SetActive(true);
    }

    public void MainMenu()
    {
        mainMenuHolder.SetActive(true);
        optionsMenuHolder.SetActive(false);
    }

    public void SetMasterVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Master);
    }

    public void SetMusicVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Music);
    }

    public void SetSfxVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Sfx);
    }

    // �̸� ��ȿ�� �˻� �Լ�
    private bool IsValidName(string name)
    {
        string pattern = @"^\d{5} [��-�R]{1,5}$"; // ���Խ�: 5�ڸ� ���� + ���� + 1~5�ڸ� �ѱ�
        return Regex.IsMatch(name, pattern);
    }

    // ��� �޽��� 3�� �Ŀ� ������� �ϴ� �ڷ�ƾ
    private IEnumerator HideWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // ������ �ð�(��)��ŭ ���
        warningBar.gameObject.SetActive(false); // ��� �޽��� �����
    }
}