using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public string menuName;
    public bool open;

    public void OpenMenu()
    {
        open = true;
        gameObject.SetActive(true);
    }

    public void CloseMenu()
    {
        open = false;
        gameObject.SetActive(false);
    }
}
