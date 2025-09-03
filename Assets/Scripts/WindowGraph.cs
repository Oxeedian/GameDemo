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
    [SerializeField] private RectTransform labelTemplateX;
    [SerializeField] private RectTransform labelTemplateY;
    [SerializeField] private Color dotColor;
    [SerializeField] private Color lineColor;


    private void Awake()
    {
    }

    void Start()
    {
        //CreateCircle(new Vector2(200, 200));

        //List<int> valueList = new List<int>() { 1, 98, 12, 50, 12, 56, 2, 10, 12 };

        //List<HitChances> valueListHit = new List<HitChances>() { new HitChances(0, 1.0f), new HitChances(10, 1.0f), new HitChances(20, 1.0f), new HitChances(30, 1.0f), new HitChances(40, 1.0f) };

        //ShowGraph(valueListHit, 100.0f);
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

    //private void ShowGraph(List<int> valueList)
    //{
    //    float xSize = 50;
    //    float yMaximum = 100f;
    //    float graphHeight = graphContainter.sizeDelta.y;

    //    GameObject lastCircleGameObject = null;

    //    for (int i = 0; i < valueList.Count; i++)
    //    {
    //        float xPosition = xSize * i;
    //        float yPosition = (valueList[i] / yMaximum) * graphHeight;

    //        GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition));
    //        if (lastCircleGameObject != null)
    //        {
    //            CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);
    //        }
    //        lastCircleGameObject = circleGameObject;

    //        RectTransform labelX = Instantiate(labelTemplateX);
    //        labelX.SetParent(graphContainter);
    //        labelX.gameObject.SetActive(true);
    //        labelX.anchoredPosition = new Vector2(xPosition, -4.4f);
    //        labelX.GetComponent<TMP_Text>().text = i.ToString();
    //    }

    //    int separatorCount = 10;
    //    for (int i = 0; i <= separatorCount; i++)
    //    {
    //        float normalizedValue = i / 1f / separatorCount;
    //        RectTransform labelY = Instantiate(labelTemplateX);
    //        labelY.SetParent(graphContainter);
    //        labelY.gameObject.SetActive(true);
    //        labelY.anchoredPosition = new Vector2(-12.7f, normalizedValue * graphHeight);
    //        labelY.GetComponent<TMP_Text>().text = Mathf.RoundToInt((normalizedValue * yMaximum)).ToString();
    //    }
    //}


    public void ShowGraph(List<HitChances> valueList, float maxRange)
    {
        foreach (Transform child in graphContainter.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        float xSize = 50;
        float yMaximum = 100f;
        float graphHeight = graphContainter.sizeDelta.y;

        GameObject lastCircleGameObject = null;


        for (int i = 0; i < valueList.Count; i++)
        {
            float xPosition = xSize * i;
            float yPosition = (valueList[i].HitChance / yMaximum) * graphHeight;

            GameObject circleGameObject = CreateCircle(new Vector2(xPosition, yPosition));
            if (lastCircleGameObject != null)
            {
                CreateDotConnection(lastCircleGameObject.GetComponent<RectTransform>().anchoredPosition, circleGameObject.GetComponent<RectTransform>().anchoredPosition);
            }
            lastCircleGameObject = circleGameObject;

            RectTransform labelX = Instantiate(labelTemplateX);
            labelX.SetParent(graphContainter);
            labelX.gameObject.SetActive(true);
            labelX.anchoredPosition = new Vector2(xPosition, -4.4f);
            labelX.GetComponent<TMP_Text>().text = valueList[i].Distance.ToString();
            labelX.localScale = Vector3.one;
        }

        int separatorCount = 10;
        for (int i = 0; i <= separatorCount; i++)
        {
            float normalizedValue = i / 1f / separatorCount;
            RectTransform labelY = Instantiate(labelTemplateX);
            labelY.SetParent(graphContainter);
            labelY.gameObject.SetActive(true);
            labelY.anchoredPosition = new Vector2(-26f, normalizedValue * graphHeight);
            labelY.GetComponent<TMP_Text>().text = Mathf.RoundToInt((normalizedValue * yMaximum)).ToString();
            labelY.localScale = Vector3.one;
        }
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
}
