using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightUtils { 
    public static float GetLightValue(Texture2D tex)
    {
        Color[] cols = tex.GetPixels();

        Color avg = new Color(0, 0, 0);
        for (int i = 0; i< cols.Length; i++)
        {
            avg += cols[i];
        }
        avg /= cols.Length;

        // https://www.w3.org/TR/AERT/#color-contrast
        return 0.299f * avg.r + 0.587f * avg.g + 0.114f * avg.b;
    }
}
