using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Timer : NetworkBehaviour
{
    [SerializeField] private Text timerText;
    private NetworkVariable<int> timeLeft = new(25);
    private WaitForSeconds waitForASecond = new WaitForSeconds(1);

    private void Start()
    {
        timerText.text = timeLeft.Value.ToString();
        timeLeft.OnValueChanged += (int prev, int current) => { timerText.text =  $"Timer: {current}"; };
        GameManager.Instance.OnGameStarted += () => { StartCoroutine(CountDown()); };
    }

    private IEnumerator CountDown()
    {
        while (timeLeft.Value > 0)
        {
            yield return waitForASecond;
            if (IsServer)
            {
                timeLeft.Value--;
            }
            timerText.text = $"Timer: {timeLeft.Value}";
        }
        GameManager.Instance.GameOver();
    }
}
