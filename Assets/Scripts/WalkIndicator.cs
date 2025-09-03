using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class WalkIndicator : MonoBehaviour
{
    [SerializeField] private  UnityEngine.UI.Image progressBar;
    [SerializeField] private Color barColor;
    [SerializeField] private TMP_Text walkPointText;

    public float targetFillPerentage = 0;
    private float currentFillPercentage = 0;
    private float startValue = 0f;
    private float endValue = 10f;
    private float duration = 0.2f;
    private float elapsed = 0f;
    private bool lerpNewValue = false;

    void Start()
    {

    }

    private void Update()
    {
        if (lerpNewValue)
        {
            //elapsed += Time.deltaTime;
            // float t = Mathf.Clamp01(elapsed / duration);
            //float currentValue = Mathf.Lerp(currentFillPercentage, targetFillPerentage, t);
            currentFillPercentage = Mathf.MoveTowards(currentFillPercentage, targetFillPerentage, 0.005f);
            progressBar.fillAmount = currentFillPercentage;

           // if (t >= 1f)
            //{
                //currentFillPercentage = currentValue;
                //lerpNewValue = false;
            //}
        }
    }

    public void PrepareWalkBar(float percentage, Color barColor, int walkPoints, int maxWalk)
    {
        lerpNewValue = true;

        if (percentage > 1)
        {
            Debug.Log("Värde över 1");
            targetFillPerentage = 1;
        }
        else if (percentage < 0)
        {
            Debug.Log("Värde under 0");
            targetFillPerentage = 0;
        }

        progressBar.color = barColor;
        targetFillPerentage = percentage;

        elapsed = 0;
        string text = string.Empty;

        if(walkPoints > maxWalk)
        {
            text = "   Unreachable";
        }
        else
        {
            text = $"         {walkPoints} / {maxWalk}";

        }
        walkPointText.text = text;
    }
}
