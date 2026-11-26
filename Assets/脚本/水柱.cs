using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class 水柱 : MonoBehaviour
{
    public 法杖控制 staff;
    public 角色控制 player;
    public float power;
    public float moveSpeed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (staff == null)
        {
            if(player != null)
            {
                player.ismove = true;
                player.isjump = true;
            }
            Destroy(gameObject);
        }
    }
}
