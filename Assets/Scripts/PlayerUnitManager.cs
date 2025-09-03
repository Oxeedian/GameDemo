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
        allRenderers = FindObjectsOfType<Renderer>();
        allPlayers = players;
    }

    public void ReGatherRenderers()
    {
        allRenderers = FindObjectsOfType<Renderer>();
    }


    public void CullOutOfRange()
    {
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

            if (obj.tag.Contains("Enemy"))
            {
                continue;
            }

            if (!rend.enabled && culledObjects.ContainsKey(obj))
                continue;


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
                if (!culledObjects.ContainsKey(obj))
                {
                    culledObjects[obj] = rend.material.color;
                    rend.material.color = rend.material.color * 0.5f;
                }
            }


            foreach (Enemy enemy in PostMaster.allEnemies)
            {
                enemy.cullChecked = false;
            }
            foreach (PlayerUnit player in allPlayers)
            {
                CullEnemyOutOfRange(player);
            }
        }
    }




    public void CullEnemyOutOfRange(Enemy enemy)
    {
        foreach (PlayerUnit player in allPlayers)
        {
            float distance = Vector3.Distance(player.transform.position, enemy.transform.position);
            if (distance <= visionRange)
            {
                enemy.renderer.enabled = true;
                return;
            }
        }

        enemy.renderer.enabled = false;
    }

    public void CullEnemyOutOfRange(PlayerUnit player)
    {
        foreach (Enemy enemy in PostMaster.allEnemies)
        {
            if (enemy.cullChecked)
                continue;

            enemy.renderer.enabled = false;
            float distance = Vector3.Distance(player.transform.position, enemy.transform.position);

            if (distance <= visionRange)
            {
                enemy.cullChecked = true;
                enemy.renderer.enabled = true;
            }
        }

    }
  

}


