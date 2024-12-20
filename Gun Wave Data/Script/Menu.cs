using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections; // 코루틴을 사용하기 위해 필요

public class Menu : MonoBehaviour
{
    public GameObject mainMenuHolder;
    public GameObject optionsMenuHolder;
    public GameObject nameInputHolder; // 이름 입력 UI
    public InputField nameInputField; // 이름 입력 필드
    public GameObject warningBar; // 경고 메시지 바

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
        // 입력된 이름 저장
        string playerName = nameInputField.text;

        if (IsValidName(playerName))
        {
            // 이름이 유효하면 Player 클래스에 저장
            Player.playerName = playerName;

            mainMenuHolder.SetActive(true);
            nameInputHolder.SetActive(false);
            SceneManager.LoadScene("Game"); // Game 씬으로 이동
        }
        else
        {
            // 경고 텍스트를 표시하고, 3초 후에 사라지게 설정
            warningBar.gameObject.SetActive(true);
            StartCoroutine(HideWarningAfterDelay(3f)); // 3초 후에 경고 메시지 사라짐
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

    // 이름 유효성 검사 함수
    private bool IsValidName(string name)
    {
        string pattern = @"^\d{5} [가-힣]{1,5}$"; // 정규식: 5자리 숫자 + 띄어쓰기 + 1~5자리 한글
        return Regex.IsMatch(name, pattern);
    }

    // 경고 메시지 3초 후에 사라지게 하는 코루틴
    private IEnumerator HideWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // 지정된 시간(초)만큼 대기
        warningBar.gameObject.SetActive(false); // 경고 메시지 숨기기
    }
}