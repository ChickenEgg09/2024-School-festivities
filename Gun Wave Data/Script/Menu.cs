using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections; // ÄÚ·çÆ¾À» »ç¿ëÇÏ±â À§ÇØ ÇÊ¿ä

public class Menu : MonoBehaviour
{
    public GameObject mainMenuHolder;
    public GameObject optionsMenuHolder;
    public GameObject nameInputHolder; // ÀÌ¸§ ÀÔ·Â UI
    public InputField nameInputField; // ÀÌ¸§ ÀÔ·Â ÇÊµå
    public GameObject warningBar; // °æ°í ¸Ş½ÃÁö ¹Ù

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
        // ÀÔ·ÂµÈ ÀÌ¸§ ÀúÀå
        string playerName = nameInputField.text;

        if (IsValidName(playerName))
        {
            // ÀÌ¸§ÀÌ À¯È¿ÇÏ¸é Player Å¬·¡½º¿¡ ÀúÀå
            Player.playerName = playerName;

            mainMenuHolder.SetActive(true);
            nameInputHolder.SetActive(false);
            SceneManager.LoadScene("Game"); // Game ¾ÀÀ¸·Î ÀÌµ¿
        }
        else
        {
            // °æ°í ÅØ½ºÆ®¸¦ Ç¥½ÃÇÏ°í, 3ÃÊ ÈÄ¿¡ »ç¶óÁö°Ô ¼³Á¤
            warningBar.gameObject.SetActive(true);
            StartCoroutine(HideWarningAfterDelay(3f)); // 3ÃÊ ÈÄ¿¡ °æ°í ¸Ş½ÃÁö »ç¶óÁü
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

    // ÀÌ¸§ À¯È¿¼º °Ë»ç ÇÔ¼ö
    private bool IsValidName(string name)
    {
        string pattern = @"^\d{5} [°¡-ÆR]{1,5}$"; // Á¤±Ô½Ä: 5ÀÚ¸® ¼ıÀÚ + ¶ç¾î¾²±â + 1~5ÀÚ¸® ÇÑ±Û
        return Regex.IsMatch(name, pattern);
    }

    // °æ°í ¸Ş½ÃÁö 3ÃÊ ÈÄ¿¡ »ç¶óÁö°Ô ÇÏ´Â ÄÚ·çÆ¾
    private IEnumerator HideWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // ÁöÁ¤µÈ ½Ã°£(ÃÊ)¸¸Å­ ´ë±â
        warningBar.gameObject.SetActive(false); // °æ°í ¸Ş½ÃÁö ¼û±â±â
    }
}