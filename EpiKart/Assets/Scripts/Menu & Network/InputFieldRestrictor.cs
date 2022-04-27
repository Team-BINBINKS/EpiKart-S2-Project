using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Allow to manipulate UI elements
using System.Text.RegularExpressions; // Allows for Regex implementations

public class InputFieldRestrictor : MonoBehaviour
{
    #region Script Arguments
    public TMP_InputField joinInputField;
    public TMP_Text warningInputField;
    #endregion

    public void onChangeCall()
    {
        warningInputField.text = "";
        joinInputField.text = Regex.Replace(joinInputField.text, @"[a-z]", m => m.ToString().ToUpper());
    }
}
