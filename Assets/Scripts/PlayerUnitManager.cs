using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerUnitManager : MonoBehaviour
{
   [SerializeField] float visionRange = 100;
    List<GameObject> culledObjects = new List<GameObject>();


    public void CullOutOfRange(List<Unit> allPlayers)
    {
        return;

        // Re-enable rendering on previously culled objects
        foreach (GameObject gameObject in culledObjects)
        {
            Renderer rend = gameObject.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.enabled = true;
            }
            else
            {
                Debug.Log("Error: Object missing Renderer!");
            }
        }

        culledObjects.Clear();

        SphereCollider[] allSphereColliders = FindObjectsOfType<SphereCollider>();

        foreach (SphereCollider sphere in allSphereColliders)
        {
            GameObject obj = sphere.gameObject;
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend == null)
            {
                Debug.Log("Error: Object missing Renderer!");
                continue;
            }

            bool inRangeOfAnyPlayer = false;

            // Check if any player is within range
            foreach (PlayerUnit player in allPlayers)
            {
                float distance = Vector3.Distance(player.transform.position, obj.transform.position);
                if (distance <= visionRange)
                {
                    inRangeOfAnyPlayer = true;
                    break; // No need to check more players
                }
            }

            if (inRangeOfAnyPlayer)
            {
                rend.enabled = true;
            }
            else
            {
                rend.enabled = false;
                culledObjects.Add(obj);
            }
        }
    }
}
