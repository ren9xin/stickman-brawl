using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 风控制 : MonoBehaviour
{
    [Header("多标签配置（目标物体标签）")]
    public List<string> targetTags = new List<string> { "Player", "gun","bullet" }; // Player和Gun标签
    [Header("位置范围筛选（可选，精准限定区域）")]
    public bool usePositionRange = false;
    public float minX = -10f;
    public float maxX = 10f;
    public float minY = -5f;
    public float maxY = 5f;

    [Header("施力核心参数")]
    public float forceMagnitude = 3f; // 力/速度大小
    public bool randomDirection = true; // 随机左右方向
    public bool isRightDirection = true; // 指定方向：true=右，false=左
    public bool useForce = true; // true=物理受力，false=强制位移

    [Header("间隔与随机时长配置")]
    public float fixedTriggerInterval = 5f; // 固定触发间隔（每隔X秒触发一次施力）
    public float minForceDuration = 2f; // 施力最短时长
    public float maxForceDuration = 6f; // 施力最长时长（随机值在min和max之间）

    private List<Rigidbody2D> targetRigidbodies = new List<Rigidbody2D>();
    private Coroutine outerTriggerCoroutine; // 外层间隔触发协程
    private Coroutine innerForceCoroutine; // 内层随机时长施力协程
    private GameObject targetBackground; // 自动查找的所属背景

    void Start()
    {
        targetBackground = this.transform.parent.gameObject;
        // 初始化收集Player和Gun物体
        CollectTargetRigidbodies();
    }

    /// <summary>
    /// 筛选标签为Player/Gun的所有带刚体物体
    /// </summary>
    private void CollectTargetRigidbodies()
    {
        targetRigidbodies.Clear();
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);

        foreach (var obj in allObjects)
        {
            if (obj == null || !obj.activeSelf) continue;
            if (!IsObjectInTargetTags(obj)) continue;

            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                continue;
            }

            if (usePositionRange)
            {
                Vector3 objPos = obj.transform.position;
                if (objPos.x < minX || objPos.x > maxX || objPos.y < minY || objPos.y > maxY)
                {
                    continue;
                }
            }

            targetRigidbodies.Add(rb);
        }


    }

    /// <summary>
    /// 判断物体标签是否在目标列表中
    /// </summary>
    private bool IsObjectInTargetTags(GameObject obj)
    {
        foreach (string tag in targetTags)
        {
            if (obj.CompareTag(tag))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 外部调用：启动「固定间隔+随机时长」施力逻辑
    /// </summary>
    public void StartPlayerGunRandomForce()
    {
        // 避免重复启动外层协程
        if (outerTriggerCoroutine != null)
        {
            StopCoroutine(outerTriggerCoroutine);
        }

        if (targetBackground == null || !targetBackground.activeSelf)
        {
            return;
        }

        // 刷新目标物体列表
        CollectTargetRigidbodies();
        if (targetRigidbodies.Count == 0) return;

        outerTriggerCoroutine = StartCoroutine(OuterFixedIntervalCoroutine());
    }

    /// <summary>
    /// 外层协程：按固定间隔循环触发内层施力协程
    /// </summary>
    private IEnumerator OuterFixedIntervalCoroutine(float delayTime=2f)
    {
        if (delayTime > 0f)
        {
            yield return new WaitForSeconds(delayTime);
        }
        // 无限循环，按固定间隔触发
        while (true)
        {
            // 1. 启动内层施力协程，并等待其完全结束（关键修改：使用yield return 等待协程完成）
            innerForceCoroutine = StartCoroutine(InnerRandomDurationForceCoroutine());
            yield return innerForceCoroutine; // 阻塞外层循环，直到内层协程执行完毕

            // 2. 内层施力已结束，等待固定间隔时间（此时无任何施力行为）
            yield return new WaitForSeconds(fixedTriggerInterval);
        }
    }

    /// <summary>
    /// 内层协程：随机生成施力时长，持续施力后停止
    /// </summary>
    private IEnumerator InnerRandomDurationForceCoroutine()
    {
        
        // 1. 随机生成本次施力持续时间（在min和max之间）
        float randomCurrentDuration = Random.Range(minForceDuration, maxForceDuration);

        // 2. 确定本次施力方向
        float direction = 1f;
        if (randomDirection)
        {
            direction = Random.Range(0, 2) == 1 ? 1f : -1f;
        }
        else
        {
            direction = isRightDirection ? 1f : -1f;
        }

        // 3. 按随机时长持续施力
        float startTime = Time.time;
        while (Time.time - startTime < randomCurrentDuration)
        {
            CollectTargetRigidbodies();
            foreach (var rb in targetRigidbodies)
            {
                if (rb != null && rb.gameObject.activeSelf)
                {
                    if (useForce)
                    {
                        // 物理受力（受质量、摩擦力影响
                        if(rb.gameObject.tag=="gun")
                        {
                            rb.AddForce(new Vector2(direction * forceMagnitude*0.1f, 0f), ForceMode2D.Force);
                        }
                        else
                        {
                            rb.AddForce(new Vector2(direction * forceMagnitude, 0f), ForceMode2D.Force);
                        }
                            
                    }
                    else
                    {
                        // 强制位移（匀速移动，不受物理参数影响）
                        rb.velocity = new Vector2(direction * forceMagnitude, rb.velocity.y);
                    }
                }
            }

            yield return null; // 每一帧更新施力
        }

        // 4. 本次施力时间结束，停止水平移动
        /*StopPlayerGunHorizontalMove();*/
        innerForceCoroutine = null;
    }

    /// <summary>
    /// 停止Player/Gun的水平移动
    /// </summary>
    private void StopPlayerGunHorizontalMove()
    {
        foreach (var rb in targetRigidbodies)
        {
            if (rb != null)
            {
                rb.velocity = new Vector2(0f, rb.velocity.y); // 保留垂直速度（如跳跃）
            }
        }
    }

    /// <summary>
    /// 外部调用：强制停止所有施力逻辑（外层间隔+内层施力）
    /// </summary>
    public void StopPlayerGunAllForce()
    {
        // 停止外层间隔触发协程
        if (outerTriggerCoroutine != null)
        {
            StopCoroutine(outerTriggerCoroutine);
            outerTriggerCoroutine = null;
        }

        // 停止内层施力协程
        if (innerForceCoroutine != null)
        {
            StopCoroutine(innerForceCoroutine);
            innerForceCoroutine = null;
        }

        // 停止物体水平移动
        StopPlayerGunHorizontalMove();
    }

    /// <summary>
    /// 外部调用：刷新Player/Gun列表（运行时新增物体后调用）
    /// </summary>
    public void RefreshTargetObjects()
    {
        CollectTargetRigidbodies();
    }
}
