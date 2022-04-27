using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Allow to manipulate UI elements

public class CopyManager : MonoBehaviour
{
    public GameObject codeTextField;
    public Animator animator;

    public void ClickCopyCode()
    {
        CopyToClipboard(codeTextField.GetComponent<TMP_Text>().text);
        animator.SetTrigger("copied");
    }

    public static void CopyToClipboard(string str)
    {
        GUIUtility.systemCopyBuffer = str;
    }
}