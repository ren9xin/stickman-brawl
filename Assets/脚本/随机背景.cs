using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class 随机背景 : MonoBehaviour
{
    public GameObject[] background;


    void Start()
    {
        
    }

    public void TriggerRandomBackground()
    {
        if (background != null && background.Length > 0)
        {
            foreach (var t in background)
            {
                t.SetActive(false);
            }
            GameObject bg = background[UnityEngine.Random.Range(0, background.Length)];
            bg.SetActive(true);
        }
    }
        // Update is called once per frame
    void Update()
    {

    }
    
}
