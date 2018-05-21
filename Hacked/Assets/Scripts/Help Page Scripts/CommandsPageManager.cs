using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandsPageManager : MonoBehaviour {

    [Header("Prefabs")]
    public GameObject text;

    [Header("Game Objects")]
    public GameObject content;
    public Transform advancedCommands;
    public Transform BasicCommands;

	// Use this for initialization
	void Start () {
		foreach (string doc in CommandHelper.GetAllFullDocumentationOfType(CommandType.Basic, MachineType.Shell))
        {
            GameObject g = Instantiate(text, content.transform);
            g.GetComponent<Text>().text = doc;
        }

        advancedCommands.SetAsLastSibling();

        foreach (string doc in CommandHelper.GetAllFullDocumentationOfType(CommandType.Advanced, MachineType.Shell))
        {
            GameObject g = Instantiate(text, content.transform);
            g.GetComponent<Text>().text = doc;
            g.GetComponent<RectTransform>().SetAsLastSibling();
        }
    }
}
