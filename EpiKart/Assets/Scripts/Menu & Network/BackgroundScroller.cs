using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundScroller : MonoBehaviour
{
    #region Script Arguments
    public RawImage image;

    [Header("Scrolling Speed")]
    public float xSpeed = 0.025f, ySpeed = 0.025f;
    #endregion

    // Scrolles the given image (towards bottom-left)
    void Update()
    {
        image.uvRect = new Rect(image.uvRect.position + new Vector2(xSpeed, ySpeed) * Time.deltaTime, image.uvRect.size);
    }
}
