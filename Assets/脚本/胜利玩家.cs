using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class 胜利玩家 : MonoBehaviour
{
    public 角色控制 Player1;
    public 角色控制 Player2;
    public GameObject icon1;
    public GameObject icon2;
    // Start is called before the first frame update



    public void ShowWinner()
    {
        icon1.SetActive(false);
        icon2.SetActive(false);
        if (游戏数据管理.Instance.WinnerId=="Player2")//玩家2胜利
        {
            
            icon2.SetActive(true);
        }
        else if (游戏数据管理.Instance.WinnerId == "Player1")
        {
            icon1.SetActive(true);
            
        }
    }
}
