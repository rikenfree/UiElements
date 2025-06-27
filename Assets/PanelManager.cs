using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject[] panels;
    public void NextPanel()
    {
        panels[0].SetActive(false);
        panels[1].SetActive(true);
    }

    public void PreviousPanel()
    {
        panels[0].SetActive(true);
        panels[1].SetActive(false);
    }

}
