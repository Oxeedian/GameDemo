using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    [SerializeField] private RectTransform uiIndicator;
    [SerializeField] private WalkIndicator walkIndicator;
    [SerializeField] private Button shootButton;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private TMP_Text hitChancePercentageText;

    private bool shootButtonPressed = false;
    private TurnManager turnManager;

    public WindowGraph windowGraph;


    public void Initialize(TurnManager aTurnManager)
    {
        turnManager = aTurnManager;
        uiIndicator.gameObject.SetActive(false);
        shootButton.gameObject.SetActive(false);
    }

    public void EndTurnButton()
    {
        turnManager.EndTurn();
    }

    public void SetTargetIndicatiorPosition(Camera camera, Vector3 position, int hitChance)
    {
        position.y += 2;
        Vector3 screenPos = camera.WorldToScreenPoint(position);

        if (screenPos.z > 0)
        {
            uiIndicator.position = screenPos;
            uiIndicator.gameObject.SetActive(true);
            shootButton.gameObject.SetActive(true);
            Debug.Log("Target Found");
        }
        else
        {
            uiIndicator.gameObject.SetActive(false);
            shootButton.gameObject.SetActive(false);
            Debug.Log("Target NOT Found");

        }

        hitChancePercentageText.text = hitChance.ToString() + "%";        
    }

    public void HideTargetIndicator()
    {
        uiIndicator.gameObject.SetActive(false);
        shootButton.gameObject.SetActive(false);
    }

    public void PressShootButton()
    {
        shootButtonPressed = true;
    }
    public bool IsShootButtonPressed()
    {
        if (shootButtonPressed == true)
        {
            shootButtonPressed = false;
            return true;
        }


        return shootButtonPressed;
    }

    public void ActivateWalkBar(float percentage, Color barColor)
    {
        walkIndicator.enabled = true;
        //walkIndicator.PrepareWalkBar(percentage, barColor);
    }
    public void DeactivateWalkBar()
    {
        walkIndicator.enabled = false;
    }

    public void DrawPath(List<GameCubeNode> path, float walkRange, int actionPoints)
    {
        if (path == null)
            return;
        if (path.Count < 2)
            return;

        lineRenderer.positionCount = path.Count; 
        float movementPointsRequiered = 0;
        lineRenderer.SetPosition(0, path[0].transform.position + new Vector3(0, 1, 0));

        for (int i = 1; i < lineRenderer.positionCount; i++)
        {
            if(path[i].transform.position.y > path[i-1].transform.position.y)
            {
                movementPointsRequiered += 3;
            }
            else
            {
                movementPointsRequiered += 1;
            }

            if(movementPointsRequiered > walkRange)
            {
                lineRenderer.positionCount = i;
                //movementPointsRequiered = walkRange;
                break;
            }
            else if(actionPoints == 1 && movementPointsRequiered > walkRange * 0.5)
            {
                lineRenderer.positionCount = i;
                break;
            }


                lineRenderer.SetPosition(i, path[i].transform.position + new Vector3(0, 1, 0));
        }

        float walkPercentageBarValue = movementPointsRequiered / walkRange;

        if(actionPoints == 1)
        {
            walkPercentageBarValue = movementPointsRequiered / (walkRange * 0.5f);
        }

        Debug.Log(walkPercentageBarValue);

        if(walkPercentageBarValue > 1 && actionPoints >= 1)
        {
            lineRenderer.material.color = Color.black;
            Color emissionColor = Color.black * 2.0f;
            lineRenderer.material.SetColor("_EmissionColor", emissionColor);
            walkIndicator.PrepareWalkBar(walkPercentageBarValue, Color.black, 1, 0); //1 och 0 förr att det alltid kommer vara unreachable om man kommer hit. Skippa onödig matte.
        }
        else if (walkPercentageBarValue > 0.5 && actionPoints >= 2)
        {
            lineRenderer.material.color = Color.red;
            Color emissionColor = Color.red * 2.0f;
            lineRenderer.material.SetColor("_EmissionColor", emissionColor);
            walkIndicator.PrepareWalkBar(walkPercentageBarValue, Color.red, (int)movementPointsRequiered, (int)walkRange);
        }
        else if(actionPoints >= 1)
        {
            lineRenderer.material.color = Color.cyan;
            Color emissionColor = Color.cyan * 2.0f;
            lineRenderer.material.SetColor("_EmissionColor", emissionColor);
            walkIndicator.PrepareWalkBar(walkPercentageBarValue, Color.cyan, (int)movementPointsRequiered, (int)walkRange);
        }
        else
        {

        }
    }
}
