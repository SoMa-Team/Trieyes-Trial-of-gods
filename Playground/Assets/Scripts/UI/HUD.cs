using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class HUD : MonoBehaviour
{
    public enum InfoType {Time, Health, Kill};
    public InfoType type;

    private Slider mySlider;
    private Text myText;

    private void Awake()
    {
        myText = GetComponent<Text>();
        mySlider = GetComponent<Slider>();        
    }

    private void LateUpdate()
    {
        var player = GameManager.instance.player;

        switch (type)
        {
            case InfoType.Time:
                int min = Mathf.FloorToInt(GameManager.instance.gameTime / 60);
                int sec = Mathf.FloorToInt(GameManager.instance.gameTime % 60);
                if (myText != null)
                    myText.text = string.Format("{0:D2}:{1:D2}", min, sec);
                break;

            case InfoType.Health:
                if (player != null && player.statManager != null && mySlider != null)
                {
                    float maxHp = player.statManager.vitalStats.maxHealth;
                    float curHp = player.currentHealth;
                    mySlider.value = Mathf.Clamp01(curHp / maxHp);
                }
                break;

            case InfoType.Kill:
                if (myText != null)
                    myText.text = GameManager.instance.killCount.ToString("F0");
                break;
        }
    }
}
