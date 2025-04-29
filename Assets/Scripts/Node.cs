using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCubeNode : MonoBehaviour
{
    public GameObject cube;
    public List<GameObject> neighbors = new List<GameObject>();
    public Vector2Int index;

    public Material redmaterial;
    public Material blueMaterial;
    public Material dirtMaterial;
    public bool isLedge = false;

    // A* variables
    public bool activated = true;
    public float gCost;
    public float hCost;
    public float fCost => gCost + hCost;
    public GameCubeNode parent;
    public LevelType levelType;
    public InhabitantType inhabitantType;

    public GameObject inhabitant = null;
    public float reachableValue = 0;


    public enum LevelType
    {
        Water,
        Ground1,
        Level2,
        Level3,
        Level4,
    }

    public enum InhabitantType
    {
        None,
        Player,
        Zombie,
        Defence,
        HalfDefence
    }

    public void EnterNode(GameObject gameObject, InhabitantType type)
    {
        inhabitant = gameObject;
        inhabitantType = type;
        SetColorBlue();
    }
    public void LeaveNode()
    {
        inhabitant = null;
        SetColor();
        inhabitantType = InhabitantType.None;
    }

    public GameObject GetInhabitant()
    {
        return inhabitant;
    }

    public InhabitantType GetInhabitantType()
    {
        return inhabitantType;
    }

    public void SetColor()
    {
        GetComponent<Renderer>().material = redmaterial;
    }
    public void SetColorBlue()
    {
        GetComponent<Renderer>().material = blueMaterial;
    }
    public void SetLevelColor(float value)
    {
        //value = 0.3f;
        //GetComponent<Renderer>().material = dirtMaterial;
        //GetComponent<Renderer>().material.SetFloat("_ColorMult", value);
    }
}