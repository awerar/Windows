using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public enum HelpSection
{
    Shell, MainMenu, Database
}

public class HelpManager : MonoBehaviour {

    [Header("Prefabs")]
    public GameObject helpButton;
    public GameObject navigationButton;

    [Header("Game Objectes")]
    public GameObject content;
    private GameObject helpSelection;
    private GameObject help;
    private GameObject helpSelectionContent;
    public GameObject navigationTab;
    public GameObject pageContainer;
    
    [HideInInspector]
    public HelpMenuData[] helpData;
    public Dictionary<HelpSection, HelpMenuData> sectionToHelpdata;

    private HelpMenuData currentHelpMenu;
    private GameObject currentPage;
    public Dictionary<string, GameObject> nameToPage;

    private Animator anim;

    private void Start()
    {
        helpData = FindObjectOfType<HelpMenuUnlockManager>().helpData;

        anim = GetComponent<Animator>();

        helpSelection = content.transform.Find("Help Selection").Find("ViewPort").gameObject;
        helpSelectionContent = helpSelection.transform.Find("Content").gameObject;
        help = content.transform.Find("Help").gameObject;

        for (int child = 0; child < content.transform.childCount; child++)
        {
            content.transform.GetChild(child).GetComponent<LayoutElement>().preferredWidth = transform.parent.GetComponent<RectTransform>().rect.width;
            content.transform.GetChild(child).GetComponent<LayoutElement>().preferredHeight = transform.parent.GetComponent<RectTransform>().rect.height;
        }

        content.GetComponent<RectTransform>().anchoredPosition = new Vector2(transform.parent.GetComponent<RectTransform>().rect.width, content.GetComponent<RectTransform>().anchoredPosition.y);

        sectionToHelpdata = new Dictionary<HelpSection, HelpMenuData>();

        foreach (HelpMenuData h in helpData)
        {
            if (h.unlocked)
            {
                GameObject g = Instantiate(helpButton, helpSelectionContent.transform) as GameObject;
                g.transform.Find("Image").GetComponent<Image>().sprite = h.sprite;
                g.transform.Find("Text").GetComponent<Text>().text = h.section.ToString();
                g.GetComponent<Button>().onClick.AddListener(() => { ShowHelpMenu(h.section); });

                sectionToHelpdata.Add(h.section, h);
            }
        }
    }

    public void ShowHelpMenu(HelpSection hs)
    {
        //Clear the help menu
        foreach (Transform child in navigationTab.transform)
        {
            if (child.tag == "Option")
            {
                Destroy(child.gameObject);
            }
        }

        foreach (Transform child in pageContainer.transform)
        {
            Destroy(child.gameObject);
        }

        anim.SetBool("OnHelpMenu", true);

        nameToPage = new Dictionary<string, GameObject>();

        currentHelpMenu = sectionToHelpdata[hs];

        foreach (HelpPage p in currentHelpMenu.pages)
        {
            if (p.unlocked)
            {
                GameObject btn = Instantiate(navigationButton, navigationTab.transform);
                btn.transform.Find("Text").GetComponent<Text>().text = p.name;
                btn.GetComponent<Button>().onClick.AddListener(() => OpenPage(p.name));

                GameObject page = Instantiate(p.page, pageContainer.transform);
                page.SetActive(false);
                nameToPage.Add(p.name, page);
            }
        }

        OpenPage(currentHelpMenu.pages[0].name);
    }

    public void ShowHelpSelectMenu()
    {
        anim.SetBool("OnHelpMenu", false);
    }

    public void OpenPage (string name)
    {
        //Turn off current page
        if (currentPage != null)
        {
            currentPage.SetActive(false);
        }

        currentPage = nameToPage[name];
        currentPage.SetActive(true);
    }
}


    [System.Serializable]
public struct HelpMenuData
{
    [Header("General Info")]
    public Sprite sprite;
    public HelpSection section;
    public bool unlocked;

    [Header("Pages")]
    public HelpPage[] pages;

    public void Unlock()
    {
        unlocked = true;
    }
}

[System.Serializable]
public struct HelpPage {
    public GameObject page;
    public string name;
    public bool unlocked;

    public void Unlock()
    {
        unlocked = true;
    }
}
