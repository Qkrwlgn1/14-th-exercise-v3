using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PickaxeController : CloseWeaponController
{
    public static bool isActive = true;

    void Start()
    {
        WeaponManager.currentWeapon = currentCloseWeapon.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentCloseWeapon.anim;

    }


    void Update()
    {
        if (isActive)
            TryAttack();
    }


    protected override IEnumerator HitCorutine()
    {
        while (isSwing)
        {
            if (CheckObject())
            {
                if (hitInfo.transform.tag == "Rock")
                    hitInfo.transform.GetComponent<Rock>().Mining();
                else if (hitInfo.transform.tag == "WeakAnimal")
                    SoundManager.instance.PlaySE("Animal_Hit");
                    hitInfo.transform.GetComponent<WeakAnimal>().Damage(currentCloseWeapon.damage, transform.position);


                isSwing = false;
                Debug.Log(hitInfo.transform.name);
            }
            yield return null;
        }
    }


    public override void CloseWeaponChange(CloseWeapon _closeWeapon)
    {
        base.CloseWeaponChange(_closeWeapon);
        isActive = true;
    }
}
