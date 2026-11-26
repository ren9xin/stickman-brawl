using UnityEngine;

/// <summary>
/// 移动平台父物体控制脚本：控制两个子物体沿X轴反向、不同速度移动并边界折返
/// </summary>
public class 移动平台 : MonoBehaviour
{
    public float leftBoundary = -8.7f; // 左边界X坐标
    public float rightBoundary = 8.8f; // 右边界X坐标
    public float platform1Speed = 1f; // 第一个子平台移动速度
    public float platform2Speed = 2f; // 第二个子平台移动速度（与第一个反向）

    private Transform _platform1; // 第一个子物体Transform
    private Transform _platform2; // 第二个子物体Transform
    private int _platform1MoveDir = 1; // 第一个平台移动方向（1=右，-1=左）
    private int _platform2MoveDir = -1; // 第二个平台移动方向（与第一个相反，初始=-1）

    void Start()
    {
        // 初始化：获取父物体下的前两个子物体（确保父物体只有两个子平台）
        Transform[] childTransforms = GetComponentsInChildren<Transform>();
        // 跳过父物体自身（索引0是父物体，索引1和2是两个子物体）
        if (childTransforms.Length >= 3)
        {
            _platform1 = childTransforms[1];
            _platform2 = childTransforms[2];
        }
        else
        {
            enabled = false; // 禁用脚本，避免报错
            return;
        }
    }

    void Update()
    {
        // 若子平台未获取成功，直接返回
        if (_platform1 == null || _platform2 == null) return;

        // 1. 更新两个平台的位置（沿X轴移动）
        MoveSinglePlatform(_platform1, platform1Speed, ref _platform1MoveDir);
        MoveSinglePlatform(_platform2, platform2Speed, ref _platform2MoveDir);
    }

    /// <summary>
    /// 单个平台的移动逻辑（包含边界检测与折返）
    /// </summary>
    /// <param name="platform">要移动的平台Transform</param>
    /// <param name="speed">移动速度</param>
    /// <param name="moveDir">移动方向（引用传递，可修改值）</param>
    private void MoveSinglePlatform(Transform platform, float speed, ref int moveDir)
    {
        // 计算X轴移动增量
        float xMoveDelta = moveDir * speed * Time.deltaTime;
        // 更新平台位置（仅沿X轴移动，Y、Z轴保持不变）
        platform.position += new Vector3(xMoveDelta, 0, 0);

        // 边界检测与方向反转
        // 触达右边界，且当前方向是向右
        if (platform.position.x >= rightBoundary && moveDir == 1)
        {
            moveDir = -1; // 反转方向为向左
            // 修正位置，防止平台超出边界
            platform.position = new Vector3(rightBoundary, platform.position.y, platform.position.z);
        }
        // 触达左边界，且当前方向是向左
        else if (platform.position.x <= leftBoundary && moveDir == -1)
        {
            moveDir = 1; // 反转方向为向右
            // 修正位置，防止平台超出边界
            platform.position = new Vector3(leftBoundary, platform.position.y, platform.position.z);
        }
    }
}
