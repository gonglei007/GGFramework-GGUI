using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountDown : MonoBehaviour
{
    private float GameTime;
    private Coroutine m_coroutine;
    bool IsStart = false;
    private UILabel Lb_Cd = null;
    private string content;

    void Start()
    {
        Lb_Cd = GetComponent<UILabel>();
        GameTime = 0;
        IsStart = false;
        Lb_Cd.text = "请稍后....";
        if (m_coroutine != null)
        {
            StopCoroutine(m_coroutine);
        }
    }

    public void StartCountDown(Color color, string txt, float time)
    {
        Stop();
        IsStart = true;
        GameTime = time;
        content = txt;
        Lb_Cd.color = color;
        if (time == 0)
        {
            Lb_Cd.text = "";
        }
        else
        {
            Lb_Cd.text = string.Format("{0}:{1}", txt, time);
        }
  
        m_coroutine = StartCoroutine(CalcTime());
    }

    public void Stop()
    {
        GameTime = 0;
        Lb_Cd.text = "";
        IsStart = false;
        if (m_coroutine != null)
        {
            StopCoroutine(m_coroutine);
        }
    }

    IEnumerator CalcTime()
    {
        while (IsStart)
        {
            yield return new WaitForSeconds(0.95f);
            GameTime--;
            if (GameTime < 0)
            {
                GameTime = 0;
                Stop();
            }
            else
            {
                Lb_Cd.text = string.Format("{0}:{1}", content,GameTime);
            }
        }
    }
}
