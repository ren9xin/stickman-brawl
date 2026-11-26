using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 刺刺生成 : MonoBehaviour
{
    public GameObject bulletPre;
    // 核心修改1：公共列表，用于挂载固定生成点（在Inspector中拖入即可）
    [Header("固定生成点（拖入你的生成点对象）")]
    public List<Transform> spawnPoints = new List<Transform>(); // 存储所有固定生成点
    float speed;
    [Header("生成间隔设置")]
    public float startDelay = 1f; // 延迟1秒开始第一次生成
    public float spawnInterval = 3f; // 每隔2秒生成一次
    public GameObject targetBackground; // 自动查找的所属背景
    private Coroutine spawnCoroutine; // 协程实例，用于后续停止生成
    // Start is called before the first frame update
    void Start()
    {
        // 核心步骤1：自动获取所有子物体，作为固定生成点
        CollectChildSpawnPoints();
        // 自动查找所属背景
        targetBackground = this.transform.parent.gameObject;
        // 读取子弹速度（原有逻辑不变）
        if (bulletPre != null)
        {
            子弹控制 bulletPrefabCache = bulletPre.GetComponent<子弹控制>();
            if (bulletPrefabCache != null)
            {
                speed = bulletPrefabCache.speed;
            }
        }

      
    }


    // Update is called once per frame
    void Update()
    {
        // 无需调用生成方法，由InvokeRepeating自动触发
    }

    public void StartSpawnBullet()
    {
        // 先判断当前「刺刺生成」对象是否激活，未激活直接返回，不启动协程
        if (!this.gameObject.activeSelf)
        {
            return;
        }

        //再判断目标背景是否激活，未激活直接返回
        if (targetBackground == null || !targetBackground.activeSelf)
        {
            return;
        }
        // 避免重复启动协程
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        // 启动生成协程
        spawnCoroutine = StartCoroutine(SpawnBulletCoroutine());
    }

    /// <summary>
    /// 协程：定时生成刺刺（先判断背景，再执行生成）
    /// </summary>
    private IEnumerator SpawnBulletCoroutine()
    {
        // 第一步：初始延迟
        yield return new WaitForSeconds(startDelay);

        // 第二步：循环生成（每次都先判断背景状态）
        while (true)
        {
            // 【核心判断】：背景激活才执行生成
            if (targetBackground != null && targetBackground.activeSelf)
            {
                generateBullet(); // 执行生成逻辑
            }

            // 第三步：等待下一个间隔时间
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void CollectChildSpawnPoints()
    {
        spawnPoints.Clear(); // 清空原有列表，避免重复

        // 遍历当前对象的所有子物体
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform childTrans = transform.GetChild(i);
            spawnPoints.Add(childTrans); // 将子物体加入生成点列表
        }


    }

    void generateBullet()
    {
        if (targetBackground == null || !targetBackground.activeSelf)
        {
            return;
        }
        // 容错：若未挂载任何生成点，直接返回，避免报错
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            return;
        }

        // 核心修改2：从固定生成点列表中随机选择一个
        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        // 直接使用生成点的位置（无需再计算x/y，生成点的位置就是固定位置）
        Vector3 spawnPos = randomSpawnPoint.position;

        // 计算发射方向（原有逻辑优化：基于生成点的位置判断左右，无需单独计算x）
        float horizontal = 1f;
        if (spawnPos.x > 0)
        {
            horizontal = -1f; // 右侧生成点：向左发射
        }
        // 左侧生成点：向右发射（无需else，默认horizontal=1f）

        // 生成子弹（原有逻辑不变，仅替换生成位置为固定生成点的位置）
        if (bulletPre != null)
        {
            GameObject bullet = Instantiate(bulletPre, spawnPos, Quaternion.identity);
            // 翻转子弹缩放（原有逻辑不变）
            bullet.transform.localScale = new Vector3(horizontal * bullet.transform.localScale.x, bullet.transform.localScale.y, 1);
            // 设置子弹旋转（原有逻辑不变）
            float rotationZ = horizontal > 0 ? 300f : 60f;
            bullet.transform.rotation = Quaternion.Euler(0, 0, rotationZ);
            // 设置子弹速度（原有逻辑不变）
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.velocity = new Vector3(horizontal, 0, 0) * speed * 2;
            }
        }
    }

    /// <summary>
    /// 外部调用：停止刺刺生成
    /// </summary>
    public void StopSpawnBullet()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    // 销毁时停止协程，避免内存泄漏
    void OnDestroy()
    {
        StopSpawnBullet();
    }
}
