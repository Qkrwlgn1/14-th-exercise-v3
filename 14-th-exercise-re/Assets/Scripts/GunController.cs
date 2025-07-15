using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public static bool isActive = false; 

    [SerializeField]
    private Gun currentGun;

    private float currentFireRate;

    private bool isReloading = false;
    [HideInInspector]
    public bool isFineSightMode = false;

    private Vector3 originPos;

    private AudioSource audioSource;

    private RaycastHit hitInfo;
    [SerializeField]
    protected LayerMask layerMask;

    [SerializeField]
    private Camera theCam;
    private CrossHair crosshair;

    [SerializeField]
    private GameObject hit_effect_Prefab;


    void Start()
    {
        originPos = Vector3.zero;
        audioSource = GetComponent<AudioSource>();
        crosshair = FindObjectOfType<CrossHair>();

      
    }


    void Update()
    {
        if (isActive)   
        {
            GunFireRateCalc();
            TryFire();
            TryReload();
            TryFineSight();
        }
        
    }


    private void GunFireRateCalc()
    {
        if(currentFireRate > 0)
        {
            currentFireRate -= Time.deltaTime; // 60분의 1 = 1
        }
    }


    private void TryFire()
    {
        if (Input.GetButton("Fire1") && currentFireRate <= 0 && !isReloading)
        {
            Fire();
        }
    }


    private void Fire()
    {
        if (!isReloading)
        {
            if (currentGun.currentBulletCount > 0)
            {
                Shoot();
            }
            else
            {
                CancelFineSight();
                StartCoroutine(ReloadCoroutine());
            }
        }
    }


    private void Shoot()
    {
        crosshair.FireAnimation();
        currentGun.currentBulletCount--;
        currentFireRate = currentGun.fireRate;
        PlaySE(currentGun.fireSound); 
        currentGun.muzzleFlash.Play();
        Hit();

        StopAllCoroutines();
        StartCoroutine(RetroActionCoroutine());
    }


    private void Hit()
    {
        if(Physics.Raycast(theCam.transform.position, theCam.transform.forward + 
            new Vector3(Random.Range(-crosshair.GetAccuracy() - currentGun.accuracy, crosshair.GetAccuracy() + currentGun.accuracy),
                        Random.Range(-crosshair.GetAccuracy() - currentGun.accuracy, crosshair.GetAccuracy() + currentGun.accuracy), 0)
                        , out hitInfo, currentGun.range, layerMask))
        {
            GameObject clone = Instantiate(hit_effect_Prefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            Destroy(clone, 2f);
        }
    }


    private void TryReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentGun.carryBulletCount > currentGun.reloadBulletCount)
        {
            CancelFineSight();
            StartCoroutine(ReloadCoroutine());
        }
    }


    public void CancelReload()
    {
        if (isReloading)
        {
            StopAllCoroutines();
            isReloading = false;
        }
    }


    IEnumerator ReloadCoroutine()
    { 
        if(currentGun.carryBulletCount > 0)
        {
            isReloading = true; 
            currentGun.anim.SetTrigger("Reload");

            currentGun.carryBulletCount += currentGun.currentBulletCount; 
            currentGun.currentBulletCount = 0; 

            yield return new WaitForSeconds(currentGun.reloadTime);

            if (currentGun.carryBulletCount >= currentGun.reloadBulletCount)
            {
                currentGun.currentBulletCount = currentGun.reloadBulletCount;
                currentGun.carryBulletCount -= currentGun.reloadBulletCount;    
            }
            else
            {
                currentGun.currentBulletCount += currentGun.carryBulletCount;
                currentGun.carryBulletCount = 0;
            }

            isReloading = false; 
        }
        else
        {
            Debug.Log("소유한 총알이 없습니다.");
        }
    }


    private void TryFineSight()
    {
        if(Input.GetButtonDown("Fire2") && !isReloading)
        {
            FineSight();
        }
    }


    public void CancelFineSight()
    {
        if (isFineSightMode)
        {
            FineSight();
        }
    }


    private void FineSight()
    {
        isFineSightMode = !isFineSightMode;
        currentGun.anim.SetBool("FineSightMode", isFineSightMode);
        crosshair.FineSightAnimation(isFineSightMode);

        if (isFineSightMode)
        {
            StopAllCoroutines();
            StartCoroutine(FineSightCoroutine());
        }
        else
        {
            StartCoroutine(FineSightDeactivateCoroutine());
        }

    }


    IEnumerator FineSightCoroutine()
    {
        while(currentGun.transform.localPosition != currentGun.fineSightOriginPos)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.2f);
            yield return null;
        }
    }


    IEnumerator FineSightDeactivateCoroutine()
    {
        while (currentGun.transform.localPosition != originPos)
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.2f);
            yield return null;
        }
    }

 
    IEnumerator RetroActionCoroutine()
    {
        Vector3 recoilBack = new Vector3 (currentGun.retroActionForce, originPos.y, originPos.z);
        Vector3 retroActionRecoilBack = new Vector3(currentGun.retroActionFineSightForce, currentGun.fineSightOriginPos.y, currentGun.fineSightOriginPos.z);

        if (!isFineSightMode)
        {
            currentGun.transform.localPosition = originPos;
            while(currentGun.transform.localPosition.x <= currentGun.retroActionForce - 0.02f)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, recoilBack, 0.4f);
                yield return null;
            }

            while (currentGun.transform.localPosition != originPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.1f);
                yield return null;
            }

        }
        else
        {
            currentGun.transform.localPosition = currentGun.fineSightOriginPos;
            // 반동 시작
            while (currentGun.transform.localPosition.x <= currentGun.retroActionFineSightForce - 0.02f)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, retroActionRecoilBack, 0.4f);
                yield return null;
            }

            // 반동 끝
            while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.1f);
                yield return null;
            }
        }
    }


    private void PlaySE(AudioClip _clip)    
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }


    public Gun GetGun()
    {
        return currentGun;
    }


    public bool GetFineSightMode()
    {
        return isFineSightMode;
    }


    public void GunChange(Gun _gun)
    {
        if(WeaponManager.currentWeapon != null)
        {
            WeaponManager.currentWeapon.gameObject.SetActive(false);
        }

        currentGun = _gun;
        WeaponManager.currentWeapon = currentGun.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentGun.anim;


        currentGun.transform.localPosition = Vector3.zero;
        currentGun.gameObject.SetActive(true);
        isActive = true;
    }
}
