using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class 子弹控制 : MonoBehaviour
{
    public float speed = 5f; 
    private Vector3 dir;     
    public 枪控制 gun;
    public float power;//子弹力道
    public bool IsShell=false;//判断是否是炮弹
    public GameObject BoomPre;//爆炸特效
    void Start()
    {
        power=0.05f*speed;
    }

    void Update()
    {
        fly(dir);
    }

    /*void UpdateBulletDirection()
    {
        if (枪控制.Instance == null) return; 

        float horizontal = 枪控制.Instance.transform.localScale.x;
        horizontal = Mathf.Sign(horizontal);

        transform.localScale = new Vector3(horizontal * 0.01860096f, 0.02139177f, 1);

        dir = new Vector3(horizontal, 0, 0);


        float rotationZ = horizontal > 0 ? 270f : 90f; 
        transform.rotation = Quaternion.Euler(0, 0, rotationZ);
    }
    */


    public void fly(Vector3 dir)
    {
        //transform.position += dir * speed * Time.deltaTime;


        if (transform.position.x > 8.6f || transform.position.x < -8.6f ||
            transform.position.y > 5 || transform.position.y < -5)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsShell)
        {
            GameObject boom = Instantiate(BoomPre, transform.position, Quaternion.identity);
        }
        if (collision.gameObject.CompareTag("ground"))
        {
            Destroy(gameObject);
        }
        
        /*else if (collision.gameObject.CompareTag("gun"))
        {
            Destroy(gameObject);
        }*/
    }
}
