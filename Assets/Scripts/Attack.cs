using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;


public class Attack : MonoBehaviour
{
    [SerializeField] private GameObject rangeIndicator;

    //List<Enemy> allEnemies = new List<Enemy>();

    public void SetRangeIndicatorPosition(Vector3 pos, float range)
    {
        rangeIndicator.transform.localScale = new Vector3(range * 2, 10f, range * 2);
        rangeIndicator.transform.position = pos;
    }

    public List<Enemy> GatherAllEnemiesInRange(Vector3 pos, float range, List<Enemy> allEnemies)
    {
        List<Enemy> enemiesInRange = new List<Enemy>();

        foreach (Enemy enemy in allEnemies)
        {
            if (Vector3.Distance(pos, enemy.transform.position) < range && enemy.isAlive)
            {
                enemiesInRange.Add(enemy);

                Debug.DrawLine(new Vector3(pos.x, pos.y + 1, pos.z), new Vector3(enemy.transform.position.x, enemy.transform.position.y + 1, enemy.transform.position.z) , Color.green);
            }
        }

        return enemiesInRange;
    }

    public bool AttackUnit(Vector3 pos, float range,Unit enemy, int damage)
    {
        if(IsEnemyInRange(pos, enemy, range))
        {
            enemy.health -= damage;

            if (enemy.health <= 0)
            {
                enemy.isAlive = false;
            }
            return true;
        }
        return false;
    }

    private bool IsEnemyInRange(Vector3 pos, Unit enemy, float range)
    {
        if (Vector3.Distance(pos, enemy.transform.position) < range && enemy.isAlive)
        {
            return true;
        }
        return false;
    }
}
