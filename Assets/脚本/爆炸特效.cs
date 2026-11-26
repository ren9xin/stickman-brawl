using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 爆炸特效 : MonoBehaviour
{
    // 物体原始大小
    public Vector3 originalScale = 0.015f*Vector3.one;
    // 缩放时长
    public float scaleDuration = 0.1f;
    public float power;//横向
    public float moveSpeed;//倍率
    void Start()
    {
        // 物体生成后，先将缩放设为0
        transform.localScale = Vector3.zero;
        // 启动协程，执行1秒缩放动画
        StartCoroutine(ScaleUpFromZero());
        Destroy(gameObject, scaleDuration+0.1f);
    }

    /// 协程：1秒内从0缩放至原始大小
    IEnumerator ScaleUpFromZero()
    {
        float elapsedTime = 0f; // 已流逝的时间
        Vector3 startScale = Vector3.zero; // 起始缩放（0尺寸）

        // 当已流逝时间 < 目标时长时，持续执行缩放
        while (elapsedTime < scaleDuration)
        {
            // 计算缩放进度
            float progress = elapsedTime / scaleDuration;
            // 线性插值计算当前缩放值，实现平滑过渡
            transform.localScale = Vector3.Lerp(startScale, originalScale, progress);
            // 累加帧间隔时间，确保进度计算准确
            elapsedTime += Time.deltaTime;
            // 暂停协程，下一帧继续执行
            yield return null;
        }

        // 确保最终缩放精准等于原始大小（避免因帧率误差导致偏差）
        transform.localScale = originalScale;
    }
}
