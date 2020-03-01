using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController: MonoBehaviour
{
    //public GameObject box;

    public void HideBox(GameObject box)
    {
        box.SetActive(false);
    }
    
    public void ShowBox(GameObject box)
    {
        box.SetActive(true);
    }
    

}
