using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpMenuUnlockManager : MonoBehaviour
{
    public HelpMenuData[] helpData;

    public void UnlockPage(HelpSection s, string name)
    {
        for (int i = 0; i < helpData.Length; i++)
        {
            if (helpData[i].section == s)
            {
                for (int ii = 0; ii < helpData[i].pages.Length; ii++)
                {
                    if (helpData[i].pages[ii].name == name)
                    {
                        helpData[i].pages[ii].unlocked = true;

                        FindObjectOfType<HelpManager>().helpData[i].pages[ii].unlocked = true;
                        return;
                    }
                }
                return;
            }
        }
    }

    public void UnlockEction(HelpSection s)
    {
       
    }
}