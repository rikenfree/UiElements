using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject[] panels;

    int currentPanelIndex = 0;

    public void ShowPanel(int index)
    {
        if (panels == null || panels.Length == 0) return;
        panels[currentPanelIndex].SetActive(false);
        currentPanelIndex = index;
        panels[currentPanelIndex].SetActive(true);
    }   
    
    public void ShowNextPanel()
    {
        if (panels == null || panels.Length == 0) return;
        if (currentPanelIndex < panels.Length - 1)
        {
            ShowPanel(currentPanelIndex + 1);
        }
        // else: do nothing, already at last panel
    }

    public void ShowPreviousPanel()
    {
        if (panels == null || panels.Length == 0) return;
        if (currentPanelIndex > 0)
        {
            ShowPanel(currentPanelIndex - 1);
        }
        // else: do nothing, already at first panel
    }
}
