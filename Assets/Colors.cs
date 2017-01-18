using UnityEngine;
using System.Collections;
using DataBinding;
using System.Collections.Generic;
using System.Linq;
using UtilGeometry;

public class Colors
{
    public static List<Color> getPalette(int N_STEPS)
    {
        List<Color> colors = new List<Color>();

        for (int i = 0; i < N_STEPS; i++)
        {
            Color c = Random.ColorHSV();
            c.a = 1f;
            colors.Add(c);
        }
        return colors;
    }

    public static Color[] mapDiscreteColor(float[] values)
    {
        Color[] colors = new Color[values.Length];

     
        Dictionary<float, Color> mapping = new Dictionary<float,Color>();

        for (int i = 0; i < values.Length; i++)
        {
            if(!mapping.ContainsKey((values[i])))
            {
                Color c = Random.ColorHSV();
                mapping.Add(values[i], c);
                colors[i] = c;
            }
            else
            {
                colors[i] = mapping[values[i]];
            }
        }
        

        return colors;

    }

}