using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Attack : MonoBehaviour
{
    [SerializeField] private GameObject rangeIndicator;

    //List<Enemy> allEnemies = new List<Enemy>();


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

        rangeIndicator.transform.localScale = new Vector3(range * 2, 10f, range * 2);
        rangeIndicator.transform.position = pos;
        return enemiesInRange;
    }

    public void AttackUnit(Enemy enemy, int damage)
    {
        enemy.health -= damage;

        if (enemy.health <= 0)
        {
            enemy.isAlive = false;
        }
    }
}
