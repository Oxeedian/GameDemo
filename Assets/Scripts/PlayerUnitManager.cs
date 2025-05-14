using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerUnitManager : MonoBehaviour
{
    [SerializeField] private float visionRange = 100f;

    private Dictionary<GameObject, Color> culledObjects = new Dictionary<GameObject, Color>();
    private Renderer[] allRenderers;
    private List<Unit> allPlayers;

    public void Initialize(List<Unit> players)
    {
        // Cache all renderers once at startup (you can make this dynamic if needed)
        allRenderers = FindObjectsOfType<Renderer>();
        allPlayers = players;
    }

    public void ReGatherRenderers()
    {
        allRenderers = FindObjectsOfType<Renderer>();
    }


    public void CullOutOfRange()
    {
        // Restore previously culled objects
        foreach (var entry in culledObjects)
        {
            Renderer rend = entry.Key.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.enabled = true;

                if (entry.Key.GetComponent<Enemy>() != null)
                {
                    rend.enabled = false;
                }
                else
                {
                    rend.material.color = entry.Value;
                }
            }
        }

        culledObjects.Clear();

        foreach (Renderer rend in allRenderers)
        {
            GameObject obj = rend.gameObject;

            if (!rend.enabled && culledObjects.ContainsKey(obj))
                continue;

            // Skip static or irrelevant objects if you tag them (optional)
            // if (obj.CompareTag("Static")) continue;

            bool inRangeOfAnyPlayer = false;

            foreach (PlayerUnit player in allPlayers)
            {
                float distance = Vector3.Distance(player.transform.position, obj.transform.position);
                if (distance <= visionRange)
                {
                    inRangeOfAnyPlayer = true;
                    break;
                }
            }

            if (inRangeOfAnyPlayer)
            {
                rend.enabled = true;
            }
            else
            {
                Enemy enemy = obj.GetComponent<Enemy>();

                if (enemy != null)
                {
                    rend.enabled = false;
                }
                else
                {
                    // Only cache and darken if not already processed
                    if (!culledObjects.ContainsKey(obj))
                    {
                        // Make sure we're not modifying shared materials
                        culledObjects[obj] = rend.material.color;
                        rend.material.color = rend.material.color * 0.5f;
                    }
                }
            }
        }
    }
}


