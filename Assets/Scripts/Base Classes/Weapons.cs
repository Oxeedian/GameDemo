using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons : MonoBehaviour
{
    public enum WeaponType
    {
        Pistol,
        Sniper
    }

    public float sweetSpot;
    public float sweetSpotDropOff;
    public float maxRange;
    public int damage;
    public WeaponType weaponType;

    public virtual void ShootWeapon()
    {

    }
}
