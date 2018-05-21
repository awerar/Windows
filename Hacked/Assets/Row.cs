using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Row : MonoBehaviour {

    public LayoutElement text;
    public LayoutElement row;

    // Update is called once per frame
    private void Update()
    {
        row.preferredHeight = text.GetComponent<Text>().preferredHeight+4;
    }
}

//#===##===##===#
//| X || O || X |
//#===##===##===#
//| O || X || O |
//#===##===##===#
//| O || X || X |
//#===##===##===#