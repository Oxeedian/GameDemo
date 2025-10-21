using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public struct HitChances
{
    public int Distance;
    public float HitChance;

    public HitChances(int distance, float hitChance)
    {
        Distance = distance;
        HitChance = hitChance;
    }
}
public class WindowGraph : MonoBehaviour
{
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private RectTransform graphContainter;
    [SerializeField] private RectTransform redDotContainter;
    [SerializeField] private RectTransform labelTemplateX;
    [SerializeField] private RectTransform labelTemplateY;
    [SerializeField] private Color dotColor;
    [SerializeField] private Color lineColor;

    private GameObject redDot;


    private void Awake()
    {
    }

    void Start()
    {
        CreateRedCircle();
        redDot.SetActive(false);
    }

    void Update()
    {

    }

    private GameObject CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainter, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        gameObject.GetComponent<Image>().color = dotColor;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        return gameObject;
    }

    private void CreateRedCircle()
    {
        redDot = new GameObject("redCircle", typeof(Image));
        redDot.transform.SetParent(redDotContainter, false);
        redDot.GetComponent<Image>().sprite = circleSprite;
        redDot.GetComponent<Image>().color = Color.red;
        RectTransform rectTransform = redDot.GetComponent<RectTransform>();
        //rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(40, 40);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
    }


    private void CreateDotConnection(Vector2 dotPositionA, Vector2 dotPositionB)
    {
        GameObject gameObject = new GameObject("dotConnetcion", typeof(Image));
        gameObject.transform.SetParent(graphContainter, false);
        gameObject.GetComponent<Image>().color = lineColor;
        Vector2 dir = (dotPositionB - dotPositionA).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float distance = Vector2.Distance(dotPositionB, dotPositionA);

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(distance * 2, 11);
        rectTransform.anchorMin = new Vector2(-0.01f, -0.0001f);
        rectTransform.anchorMax = new Vector2(0.01f, 0.0001f);
        rectTransform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        rectTransform.anchoredPosition = dotPositionA + dir * (distance * 0.5f);
        rectTransform.eulerAngles = new Vector3(0, 0, angle);
    }

    public void ShowGraph(List<HitChances> valueList, float maxRange)
    {
        foreach (Transform child in graphContainter.transform)
            GameObject.Destroy(child.gameObject);

        float yMaximum = 100f;
        float graphH = graphContainter.sizeDelta.y;
        float graphW = graphContainter.sizeDelta.x;

        if (valueList == null || valueList.Count == 0) return;

        GameObject last = null;
        int n = valueList.Count;

        for (int i = 0; i < n; i++)
        {
            // X from index => always evenly spaced across width
            float t = (n == 1) ? 0f : i / (float)(n - 1);
            float x = t * graphW;

            float y = Mathf.Clamp01(valueList[i].HitChance / yMaximum) * graphH;

            GameObject circle = CreateCircle(new Vector2(x, y));
            if (last != null)
            {
                CreateDotConnection(
                    last.GetComponent<RectTransform>().anchoredPosition,
                    circle.GetComponent<RectTransform>().anchoredPosition
                );
            }
            last = circle;

            // X-axis label uses rounded distance
            RectTransform labelX = Instantiate(labelTemplateX);
            labelX.SetParent(graphContainter, false);  // keep local coords
            labelX.gameObject.SetActive(true);
            labelX.anchoredPosition = new Vector2(x, -4.4f);
            labelX.GetComponent<TMP_Text>().text = valueList[i].Distance.ToString();
            labelX.localScale = Vector3.one;
        }

        // Y-axis labels (unchanged)
        int separatorCount = 10;
        for (int i = 0; i <= separatorCount; i++)
        {
            float nt = i / (float)separatorCount;
            RectTransform labelY = Instantiate(labelTemplateX);
            labelY.SetParent(graphContainter, false);
            labelY.gameObject.SetActive(true);
            labelY.anchoredPosition = new Vector2(-26f, nt * graphH);
            labelY.GetComponent<TMP_Text>().text = Mathf.RoundToInt(nt * yMaximum).ToString();
            labelY.localScale = Vector3.one;
        }
    }

    public void ShowSingleDot(HitChances hitChance, float maxRange)
    {
        float yMaximum = 100f;
        float graphH = graphContainter.sizeDelta.y;
        float graphW = graphContainter.sizeDelta.x;

        // Convert X (distance) into normalized 0..1 range
        float t = Mathf.Clamp01(hitChance.Distance / maxRange);
        float x = t * graphW;

        // Convert Y (hit chance) into normalized 0..1 range
        float y = Mathf.Clamp01(hitChance.HitChance / yMaximum) * graphH;


        redDot.SetActive(true);

        redDot.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        //rectTransform.anchoredPosition = anchoredPosition;
    }
    public void HideSingleDot()
    {

    }
}
