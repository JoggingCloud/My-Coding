using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public float timeLeft = 0;
    public Text TimerText;
    public GameObject loserScreen;
    private bool isGameOver;
    public AudioSource Audio;

    public AudioSource CountDown;
    bool isCountDownPlaying;

    private void Update()
    {
        if(timeLeft > 0)
        {
            timeLeft -=Time.deltaTime;
            if (timeLeft < 10)
            {
                TimerText.color = LerpRed();
                if (isCountDownPlaying == false)
                {
                    isCountDownPlaying = true;
                    CountDown.Play();
                }
            }
        }
        else if (isGameOver == false)
        {
            timeLeft += 0;
            isGameOver = true;
            EndScreen();
            Audio.Stop();
            CountDown.Stop();
        }
        DisplayTime(timeLeft);
    }

    void DisplayTime(float timeToDisplay)
    {
        if(timeToDisplay < 0)
        {
            timeToDisplay = 0;
        }
        else if (timeToDisplay > 0)
        {
            timeToDisplay += 1;
        }

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        TimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public Color LerpRed()
    {
        return Color.Lerp(a: Color.white, b: Color.red, Mathf.Sin(Time.time * 6));
    }

    public void EndScreen()
    {
        loserScreen.SetActive(true);
        Time.timeScale = 0;
    }
}