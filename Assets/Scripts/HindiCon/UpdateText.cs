using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UpdateText : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI text;
    [SerializeField] TMPro.TMP_InputField textF;
    void Update()
    {
        //text.SetText(UnicodeToKrutidev.UnicodeToKrutiDev(textF.text));
        //text.SetText(textF.text);
    }
}
