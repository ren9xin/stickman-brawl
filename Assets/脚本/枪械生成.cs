using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 枪械生成 : MonoBehaviour
{
    public int SetGunNumber;//游戏内设定最多的枪数
    public float spawnInterval = 5f; // 生成间隔（比如每5秒检查并生成1把）
    public GameObject[] gunPrefabs; // 拖拽赋值：枪支预制体
    private float timer; // 生成冷却定时器
    private bool isSpawning = false; // 防止重复启动定时器
    public GameObject 游戏主体;
    public string gunTag = "gun"; // 枪支的标签（需给所有枪支预制体打此标签）
    public int blstaffnumber = 1;//场上最多一把爆裂法杖
    public GameObject blStaffPrefab; // 单独赋值：爆裂法杖预制体
    public float minRandomDelay = 5f; // 爆裂法杖最小随机生成时间
    public float maxRandomDelay = 20f; // 爆裂法杖最大随机生成时间
    public 咏唱结束事件 blstaff;
    // Start is called before the first frame update
    void Start()
    {
        SetGunNumber = 3;

    }

    // Update is called once per frame
    void Update()
    {
        if (游戏主体.activeSelf)
        {
            CheckGunCountAndSpawn();
        }
    }

    // 检测枪支数量并触发生成
    void CheckGunCountAndSpawn()
    {
        // 统计场景中所有激活的枪支数量（通过标签筛选）
        int currentGunCount = GameObject.FindGameObjectsWithTag(gunTag).Length;

        // 数量不足且未在生成冷却中 → 启动定时器
        if (currentGunCount < SetGunNumber && !isSpawning)
        {
            StartCoroutine(SpawnGunAfterDelay());
        }
    }

    // 协程：延迟生成枪支（避免帧内重复生成）
    IEnumerator SpawnGunAfterDelay()
    {
        isSpawning= true;

        //先随机确定本次要生成的预制体，再根据类型选择延迟时间
        GameObject targetGunPrefab = GetTargetGunPrefabBeforeDelay();
        if (targetGunPrefab == blStaffPrefab)
        {
            // 仅当本次确定生成爆裂法杖时，才使用随机延迟
            timer= Random.Range(minRandomDelay, maxRandomDelay);
        }
        else
        {
            // 生成普通枪支，仍使用原有固定间隔
            timer= spawnInterval;
        }

        while (timer > 0)
        {
            timer-= Time.deltaTime;
            yield return null;
        }

        // 用提前确定好的预制体生成枪支
        SpawnSpecifiedGun(targetGunPrefab);
        isSpawning= false;
    }

    // 生成前先确定要生成的枪支（先筛选后随机，避免延迟后条件变化）
    GameObject GetTargetGunPrefabBeforeDelay()
    {
        // 1. 统计当前爆裂法杖数量
        int currentBlStaffCount = 0;
        GameObject[] allGuns = GameObject.FindGameObjectsWithTag(gunTag);
        foreach (var gun in allGuns)
        {
            if (gun.name == blStaffPrefab.name + "(Clone)")
            {
                currentBlStaffCount++;
            }
        }

        // 2. 筛选可用预制体
        List<GameObject> availablePrefabs = new List<GameObject>(gunPrefabs);
        if (currentBlStaffCount >= blstaffnumber && availablePrefabs.Contains(blStaffPrefab))
        {
            availablePrefabs.Remove(blStaffPrefab); // 达上限则移除爆裂法杖
        }

        // 3. 随机选择本次要生成的预制体并返回
        return availablePrefabs[Random.Range(0, availablePrefabs.Count)];
    }

    // 生成指定的枪支（不再随机，使用提前确定的预制体）
    void SpawnSpecifiedGun(GameObject specifiedGunPrefab)
    {
        GameObject newGun = Instantiate(specifiedGunPrefab, new Vector2(Random.Range(-6.6f, 5.8f), 4.5f), Quaternion.identity);
        if (specifiedGunPrefab == blStaffPrefab)
        {
           blstaff.staff=newGun.GetComponent<法杖控制>();
        }
        newGun.tag = gunTag;
    }
}
