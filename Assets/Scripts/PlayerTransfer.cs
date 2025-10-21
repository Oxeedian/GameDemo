using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class PlayerData
    {
        public string charName;
        public Weapons.WeaponType weapon;
    }

public static class PlayerTransfer
{
    static List<PlayerData> savedunits = new List<PlayerData>();


    public static void SaveUnits(List<PlayerData> units)
    {
        foreach (PlayerData unit in units)
        {
            PlayerData data = new PlayerData();

            data.charName = unit.charName;
            data.weapon = unit.weapon;

            savedunits.Add(data);
        }
    }

    public static List<PlayerData> GetUnits()
    {
        return savedunits;
    }
}
