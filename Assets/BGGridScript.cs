using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGGridScroll : MonoBehaviour
{
    public float scroll_speed = 0.1f;
    public MeshRenderer mesh_Renderer;
    private float x_Scroll;

    //Start is called before the first frame update

    void Awake()
    {
        mesh_Renderer= GetComponent<MeshRenderer>();
        Time.timeScale = 1F;
    }

    //Update for once per frame

    void Update()
    {
        Scroll();
    }

    void Scroll()
    {
        x_Scroll = Time.time * scroll_speed;
        Vector2 offset = new Vector2(0f, x_Scroll);
        mesh_Renderer.sharedMaterial.SetTextureOffset("_MainTex", offset);
    }
}

