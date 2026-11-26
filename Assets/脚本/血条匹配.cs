using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 全局玩家管理器：统一管理所有玩家实例，供血条查找
/// </summary>
public class 血条匹配 : MonoBehaviour
{
    public static 血条匹配 Instance; // 单例引用
    private Dictionary<int, GameObject> playerDict = new Dictionary<int, GameObject>(); // 玩家编号-玩家实例字典

    void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 玩家注册（玩家实例化时调用）
    /// </summary>
    /// <param name="playerID">唯一玩家编号</param>
    /// <param name="playerObj">玩家实例</param>
    public void RegisterPlayer(int playerID, GameObject playerObj)
    {
        if (playerDict.ContainsKey(playerID))
        {
            playerDict[playerID] = playerObj;
        }
        else
        {
            playerDict.Add(playerID, playerObj);
        }
    }

    /// <summary>
    /// 根据编号获取玩家实例
    /// </summary>
    /// <param name="playerID">玩家编号</param>
    /// <returns>玩家实例</returns>
    public GameObject GetPlayerByID(int playerID)
    {
        if (playerDict.ContainsKey(playerID))
        {
            return playerDict[playerID];
        }
        else
        {
            return null;
        }
    }
}
