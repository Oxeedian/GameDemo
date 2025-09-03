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
    public static bool RollChance(int percent)
    {
        percent = Mathf.Clamp(percent, 0, 100);
        int roll = Random.Range(0, 100);

        return roll < percent;
    }

    public bool AttackUnit(Vector3 pos, float range, Unit enemy, int damage, int hitChance)
    {
        if(IsEnemyInRange(pos, enemy, range))
        {
            if(RollChance(hitChance))
            {
                enemy.health -= damage; // TODO: Damage calculerings function i Unit klassen.

                if (enemy.health <= 0)
                {
                    enemy.GetComponent<Enemy>().Kill();
                }
                return true;
            }
        }
        return false;
    }


    private float sweetSpot = 0.2f;// TODO: Ha som en värde man skickar in.
    private float fallOffStrength = 0.35f; // TODO: Ha som en värde man skickar in. Ökar drop off. Lägre värde striktare, Högre lättare.
    public int GetHitChance(Vector3 shooterPos, Vector3 targetPos, float maxRange, float shootEfficiency)
    {
        float distance = Vector3.Distance(shooterPos, targetPos);

        if (distance > maxRange)
            return 0;


        float sweetSpotRange = maxRange * sweetSpot;
        float spread = maxRange * fallOffStrength;
        float hitChance = Mathf.Exp(-Mathf.Pow(distance - sweetSpotRange, 2) / (2 * spread * spread)); //Gaussian (bell curve)

        int returnValue = (int)((Mathf.Clamp01(hitChance) * 100f) * shootEfficiency);

        return returnValue;
    }

    public int GetHitChance(float range, float maxRange, float shootEfficiency)
    {
        float distance = range;

        if (distance > maxRange)
            return 0;


        float sweetSpotRange = maxRange * sweetSpot;
        float spread = maxRange * fallOffStrength;
        float hitChance = Mathf.Exp(-Mathf.Pow(distance - sweetSpotRange, 2) / (2 * spread * spread)); //Gaussian (bell curve)

        int returnValue = (int)((Mathf.Clamp01(hitChance) * 100f) * shootEfficiency);

        return returnValue;
    }

    public void BuildGraph(float maxRange, float shootEfficeny)
    {
        List<HitChances> hitChanceList = new List<HitChances>();

     

        for (int i = 0; i <= maxRange; i += 2)
        {
            HitChances hitChancel = new HitChances(i, GetHitChance(i, maxRange, shootEfficeny));

            hitChanceList.Add(hitChancel);
        }

        PostMaster.uiController.windowGraph.ShowGraph(hitChanceList, maxRange);
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
