using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    [SerializeField]
    private int hp;

    [SerializeField]
    private float destroyTime;

    [SerializeField]
    private SphereCollider col;

    [SerializeField]
    private GameObject go_rock;
    [SerializeField]
    private GameObject go_debris;
    [SerializeField]
    private GameObject go_effect_prefabs;
    [SerializeField]
    private GameObject go_rock_item_prefab;

    [SerializeField]
    private int count;

    [SerializeField]
    private string strike_Sound;
    [SerializeField]
    private string destroy_Sound;


    public void Mining()
    {
        SoundManager.instance.PlaySE(strike_Sound);

        var clone = Instantiate(go_effect_prefabs, col.bounds.center, Quaternion.identity);
        Destroy(clone, destroyTime); // 이펙트 제거 시간


        hp--;
        if(hp <= 0)
        {
            Destruction();
        }
    }


    private void Destruction()
    {
        SoundManager.instance.PlaySE(destroy_Sound);

        col.enabled = false; // 콜라이더 비활성화
        for (int i = 0; i <= count; i++)
        {
            Instantiate(go_rock_item_prefab, go_rock.transform.position, Quaternion.identity);
        }
        Destroy(go_rock);
        go_debris.SetActive(true); // 파편 활성화
        Destroy(go_debris, destroyTime);

    }
}
