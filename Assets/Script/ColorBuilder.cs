using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorBuilder {

    static List<Color> listOfColor = new List<Color>();

    static ColorBuilder()
    {
        float base_ = 0.2f;
        int slice = 5;
        float step = (1 - base_) / slice;

        for (int r = 0; r <= slice; r++)
            for (int g = 0; g <= slice; g++)
                for (int b = 0; b <= slice; b++)
                    if (!( r == g && g == b)) // Ne legyen mindhárom komponens ugyan olyan értékű, mivel az a szürke egy árnyalatát jelenti
                        listOfColor.Add(new Color(base_ + r * step, base_ + g * step, base_ + b * step));
    }

    static public Color GetColor(string text)
    {
        int sum = 0;
        for (int i = 0; i < text.Length; i++)
            sum += text[i] * (i + 1);

        return listOfColor[sum % listOfColor.Count];
    }
}
