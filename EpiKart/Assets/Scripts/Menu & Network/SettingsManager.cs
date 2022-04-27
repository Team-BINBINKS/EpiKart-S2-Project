using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    #region Script Arguments
    public TMP_Text screenModeLabel;
    #endregion

    #region Scripts Vars
    private FullScreenMode actualScreenMode;
    #endregion

    public string FullScreenModeToString(FullScreenMode fullScreenMode)
    {
        // Swtich through all the window's states
        switch (fullScreenMode)
        {
            case FullScreenMode.FullScreenWindow:
                return "Fullscreen";
            case FullScreenMode.Windowed:
                return "Windowed";
            case FullScreenMode.MaximizedWindow:
                return "Windowed fullscreen";
        }

        // Just in case
        return "ERROR";
    }

    void Start()
    {
        // Initialize the default window mode to be fullscreen
        actualScreenMode = FullScreenMode.FullScreenWindow;
        screenModeLabel.text = FullScreenModeToString(actualScreenMode);
    }

    public void ApplyScreenMode(FullScreenMode fullScreenMode)
    {
        if (fullScreenMode == FullScreenMode.Windowed)
        {
            // Change the resolution when windowed
            Screen.SetResolution(1280, 720, fullScreenMode);
        }
        else
        {
            // Get the current resolution width & height
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, fullScreenMode);
        }
    }
    public void ChangeScreenMode(int step)
    {
        // Calculate the next or previous one in the list to make it loop
        FullScreenMode newScreenMode = (FullScreenMode)(((int)actualScreenMode - 1 + step) % 3) + 1;
        newScreenMode = (int)newScreenMode >= 1 ? newScreenMode : FullScreenMode.Windowed;

        // Update the text label
        screenModeLabel.text = FullScreenModeToString(newScreenMode);

        // Apply the screen mode
        actualScreenMode = newScreenMode;
    }

    public void PreviousScreenMode()
    {
        ChangeScreenMode(-1);
    }
    public void NextScreenMode()
    {
        ChangeScreenMode(1);
    }
    public void ValidNewSettings()
    {
        ApplyScreenMode(actualScreenMode);
    }
}
