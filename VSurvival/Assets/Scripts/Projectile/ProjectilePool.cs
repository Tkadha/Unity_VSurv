using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private Projectile prefab;
    [SerializeField] private int initialSize = 30;
    [SerializeField] private int maxSize = 200;

    private readonly Queue<Projectile> pool = new Queue<Projectile>();

    private void Awake()
    {
        WarmUp();
    }

    private void WarmUp()
    {
        if (prefab == null) return;

        for (int i = 0; i < initialSize; i++)
        {
            var p = CreateNew();
            Return(p);
        }
    }

    private Projectile CreateNew()
    {
        var p = Instantiate(prefab, transform);
        p.gameObject.SetActive(false);
        p.SetPool(this);
        return p;
    }

    public Projectile Get(Vector3 position, Quaternion rotation)
    {
        Projectile p = null;

        if (pool.Count > 0)
        {
            p = pool.Dequeue();
        }
        else
        {
            // 풀이 비었으면 확장(최대치 고려)
            int totalActivePlusPooled = transform.childCount; // 대략적(자식으로 관리)
            if (totalActivePlusPooled < maxSize)
                p = CreateNew();
            else
                return null; // 최대치면 발사 생략(원하면 강제 재사용으로 바꿀 수 있음)
        }

        p.transform.SetPositionAndRotation(position, rotation);
        p.gameObject.SetActive(true);
        return p;
    }

    public void Return(Projectile p)
    {
        if (p == null) return;

        p.gameObject.SetActive(false);
        p.transform.SetParent(transform);

        pool.Enqueue(p);
    }
}
