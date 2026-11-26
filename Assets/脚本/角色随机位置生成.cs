using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 双玩家生成管理器（精准适配：有效背景=SetActive(true)的背景）
/// 功能：先随机单个激活背景，再在该背景内生成玩家1/2（按键独立、唯一生成点）
/// </summary>
public class 角色随机位置生成 : MonoBehaviour
{
    [Header("双玩家预制体（绑定对应按键控制脚本）")]
    public GameObject player1Prefab; // 玩家1
    public GameObject player2Prefab; // 玩家2

    [Header("场景结构配置")]
    public Transform backgroundRoot; // 背景父物体（背景根对象）
    public string spawnPointGroupName = "角色生成点"; // 每个背景下的生成点父物体名称（需统一）

    // 私有变量：存储单个选中的激活有效背景 + 该背景下的可用具体生成点
    private Transform _selectedActiveBackground; // 唯一激活背景，有多个则随机选中
    private List<Transform> _availableSpawnPoints; // 选中背景下的【具体生成点】（不再是生成点组）
    GameObject 游戏主体;
    void Start()
    {
        _availableSpawnPoints = new List<Transform>();
    }

    public void TriggerPlayerSpawn()
    {
        // 状态判断：仅在游戏运行中允许生成（双重保障，可保留）
        if (游戏状态.Instance == null || 游戏状态.Instance.currentGameState != GameState.GamePlaying)
        {
            return;
        }

        // 1. 先筛选所有激活的有效背景，如果有多个则随机选中一个
        if (!SelectRandomActiveValidBackground())
        {
            return;
        }

        // 2. 从选中的单个激活背景中，收集可用【具体生成点】
        CollectSpawnPointsFromSelectedBackground();

        // 3. 生成玩家1和玩家2
        SpawnSpecificPlayer(1, player1Prefab);
        SpawnSpecificPlayer(2, player2Prefab);
    }
    /// <summary>
    /// 核心步骤1：筛选所有「激活状态」的有效背景，并随机选中一个
    /// 有效背景条件：1. SetActive(true)（激活） 2. 包含有效生成点
    /// </summary>
    /// <returns>是否成功选中激活的有效背景</returns>
    private bool SelectRandomActiveValidBackground()
    {
        List<Transform> activeValidBackgrounds = new List<Transform>();

        // 判空保护
        if (backgroundRoot == null)
        {
            return false;
        }

        // 遍历所有背景，筛选「激活状态」的有效背景
        foreach (Transform singleBackground in backgroundRoot)
        {
            // 先判断背景是否激活（SetActive(true)），未激活直接跳过
            if (!singleBackground.gameObject.activeSelf)
            {
                continue;
            }

            // 再判断该激活背景是否包含有效生成点
            // 核心重构1：替换Find()，仅查找「直接子物体」中的生成点组（非递归，杜绝跨层级）
            Transform spawnPointGroup = GetDirectChildByName(singleBackground, spawnPointGroupName);
            // 有效生成点条件：找到生成点父物体 + 父物体下有具体生成位置
            if (spawnPointGroup != null && spawnPointGroup.childCount > 0)
            {
                activeValidBackgrounds.Add(singleBackground);
            }
        }

        // 检查激活的有效背景数量
        if (activeValidBackgrounds.Count == 0)
        {
            return false;
        }

        // 随机选中一个激活的有效背景
        int randomBgIndex = Random.Range(0, activeValidBackgrounds.Count);
        _selectedActiveBackground = activeValidBackgrounds[randomBgIndex];
        return true;
    }

    /// <summary>
    /// 核心步骤2：从选中的单个激活背景中，收集可用【具体生成点】（修正核心：不再收集生成点组）
    /// </summary>
    private void CollectSpawnPointsFromSelectedBackground()
    {
        _availableSpawnPoints = new List<Transform>();

        // 判空保护
        if (_selectedActiveBackground == null)
        {
            return;
        }

        // 查找选中激活背景下的“角色生成点”父物体
        // 核心重构2：再次使用直接子物体查找，获取生成点组
        Transform spawnPointGroup = GetDirectChildByName(_selectedActiveBackground, spawnPointGroupName);
        if (spawnPointGroup == null || spawnPointGroup.childCount == 0)
        {
            return;
        }

        // 强制校验生成点组的父物体是否为选中的激活背景（杜绝跨背景引用）
        if (spawnPointGroup.parent != _selectedActiveBackground)
        {
            return;
        }

        // 遍历生成点父物体下的所有子物体，添加为【具体可用生成点】
        foreach(Transform singleSpawnPoint in spawnPointGroup)
        {
            // 双重保障：校验具体生成点是否属于当前生成点组（避免无效生成点混入）
            if (singleSpawnPoint.parent != spawnPointGroup)
            {
                continue;
            }
            _availableSpawnPoints.Add(singleSpawnPoint);
        }


    }

    /// <summary>
    /// 单个玩家生成逻辑（修正生成点占用逻辑，确保双玩家均可生成）
    /// </summary>
    /// <param name="playerIndex">玩家编号</param>
    /// <param name="playerPrefab">玩家预制体</param>
    private void SpawnSpecificPlayer(int playerIndex, GameObject playerPrefab)
    {
        // 容错保护
        if (playerPrefab == null)
        {
            return;
        }
        if (_availableSpawnPoints == null || _availableSpawnPoints.Count == 0)
        {
            return;
        }

        // 1. 随机挑选一个可用【具体生成点】
        int randomPosIndex = Random.Range(0, _availableSpawnPoints.Count);
        Transform targetSpawnPos = _availableSpawnPoints[randomPosIndex];

        

        // 额外校验：目标生成点是否属于选中背景的生成点组（最终兜底）
        bool isSpawnPointValid = false;
        Transform spawnPointGroup = _selectedActiveBackground.Find(spawnPointGroupName);
        if (spawnPointGroup != null)
        {
            isSpawnPointValid = targetSpawnPos.IsChildOf(spawnPointGroup);
        }
        if (!isSpawnPointValid)
        {
            return;
        }

        // 2. 生成玩家
        GameObject newPlayer = Instantiate(playerPrefab, targetSpawnPos.position, targetSpawnPos.rotation, null);
        newPlayer.name = $"Player_{playerIndex}";
        // 3. 标记该【具体生成点】为已占用，移除可用列表（确保唯一生成点，不浪费其他生成点）
        _availableSpawnPoints.RemoveAt(randomPosIndex);

        // 额外：注册玩家ID到血条匹配管理器（若你需要血条绑定，可保留；不需要可删除以下代码块）
        /*角色控制 playerCtrl = newPlayer.GetComponent<角色控制>();
        if (playerCtrl != null && 血条匹配.Instance != null)
        {
            playerCtrl.playerID = playerIndex;
            血条匹配.Instance.RegisterPlayer(playerIndex, newPlayer);
        }*/
    }

    /// <summary>
    /// 工具方法：仅查找目标物体的「直接子物体」（非递归，彻底替代Transform.Find()）
    /// 解决Find()递归查找导致的跨背景、跨层级引用问题
    /// </summary>
    /// <param name="parentTransform">父物体</param>
    /// <param name="childName">子物体名称</param>
    /// <returns>找到的直接子物体，未找到返回null</returns>
    private Transform GetDirectChildByName(Transform parentTransform, string childName)
    {
        if (parentTransform == null || string.IsNullOrEmpty(childName))
        {
            return null;
        }

        // 仅遍历直接子物体，不递归，精准定位
        foreach (Transform directChild in parentTransform)
        {
            if (directChild.name == childName)
            {
                return directChild;
            }
        }

        // 未找到直接子物体
        return null;
    }

}
