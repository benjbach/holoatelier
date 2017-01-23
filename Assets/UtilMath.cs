using UnityEngine;
using System.Collections;
using System.Linq; 
using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System;
using System.Xml.Linq;

public class UtilMath {

	//constants
    public static float FL_TO_M = 0.3048f/2f;

	//Unity max vertices count per mesh
	public static int MAXIMUM_VERTICES_COUNT = 65534; 

	// scsale data between 2 spaces
	public static float normaliseValue(float value, float i0, float i1, float j0, float j1)
	{
		float L = (j0 - j1) / (i0 - i1);
		return (j0 - (L * i0) + (L * value));
	}

	public static float animateSlowInSlowOut(float t)
	{
		if (t <= 0.5f)
			return 2.0f * t * t;

		else
			return 1.0f - 2.0f * (1.0f - t) * (1.0f - t);            
	}

	//format fileName : @"C:\Users\maxc\Documents\Maxime\DATA FOR VISUALISATION\TEST.BIN"
	//public static void SerializeVector3(Vector3[] data, string fileName)
	//{
	//	using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
	//	{
	//		BinaryFormatter bf = new BinaryFormatter();
	//		bf.Serialize(fs, data);
	//	}
	//}

	////format fileName : @"C:\Users\maxc\Documents\Maxime\DATA FOR VISUALISATION\TEST.BIN"
	//public static Vector3[] DeserializeVector3(string fileName)
	//{
	//	using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
	//	{

	//		BinaryFormatter bf = new BinaryFormatter();
	//		Vector3[] result = (Vector3[])bf.Deserialize(fs);		
			
	//		return result;
	//	}
	//}

	////format fileName : @"C:\Users\maxc\Documents\Maxime\DATA FOR VISUALISATION\TEST.BIN"
	//public static void SerializeInt(int[] data, string fileName)
	//{
	//	using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
	//	{
	//		BinaryFormatter bf = new BinaryFormatter();
	//		bf.Serialize(fs, data);
	//	}
	//}
	
	////format fileName : @"C:\Users\maxc\Documents\Maxime\DATA FOR VISUALISATION\TEST.BIN"
	//public static int[] DeserializeInt(string fileName)
	//{
	//	using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
	//	{
			
	//		BinaryFormatter bf = new BinaryFormatter();
	//		int[] result = (int[])bf.Deserialize(fs);		
			
	//		return result;
	//	}
	//}

	/// <summary>
	/// Projects a point on sphere.
	/// </summary>
	/// <returns>The on sphere.</returns>
	/// <param name="center">Center.</param>
	/// <param name="radius">Radius.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
    /// 
	public static Vector3 projectOnSphere (Vector3 center, float radius, float x, float y)
	{
		float theta = 2f * Mathf.PI * x;
		float phi = Mathf.Acos(2f * y - 1f);
		
		float xS = center.x + (radius * Mathf.Sin(theta) * Mathf.Cos(phi));
		float yS = center.y + (radius * Mathf.Sin(phi) * Mathf.Sin(theta));
		float zS = ( center.z + (radius * Mathf.Cos(theta)));
		
		return new Vector3(xS,yS,zS);
	}

    public static Vector3 GPS_to_Spherical(Vector3 center, float _lat, float _lon, float earthRadius, float altitude)
    {

      //excentricity correction...
      // % WGS84 ellipsoid constants:
      //  a = 6378137;
      // e = 8.1819190842622e-2;

      // % intermediate calculation
      //% (prime vertical radius of curvature)
      //N = a ./ sqrt(1 - e^2 .* sin(lat).^2);

      /*  Vector3 spherical = new Vector3(((_lat) * Mathf.Deg2Rad), (_lon * Mathf.Deg2Rad - Mathf.PI / 2f), earthRadius);
          float xS = (earthRadius + altitude) * Mathf.Sin(spherical.x) * Mathf.Cos(spherical.y);
          float yS = (earthRadius + altitude) * Mathf.Sin(spherical.x) * Mathf.Sin(spherical.y);
          float zS = (earthRadius + altitude) * Mathf.Cos(spherical.x);
      */

        //Vector3 spherical = new Vector3(((_lat) * Mathf.Deg2Rad), (_lon * Mathf.Deg2Rad - Mathf.PI / 2f), earthRadius);
        var lat = Mathf.Deg2Rad*_lat;
        var lon = Mathf.Deg2Rad*_lon;

        float xS = (earthRadius + altitude) * Mathf.Cos(lat) * Mathf.Cos(lon);
        float yS = (earthRadius + altitude) * Mathf.Cos(lat) * Mathf.Sin(lon);
        float zS = (earthRadius + altitude) * Mathf.Sin(lat);

       /* float xS = (earthRadius + altitude) * Mathf.Cos(spherical.y) * Mathf.Sin(spherical.x);
        float yS = (earthRadius + altitude) * Mathf.Sin(spherical.y) * Mathf.Sin(spherical.x);
        float zS = (earthRadius + altitude) * Mathf.Cos(spherical.x);*/

       // Vector3 p;
        
        return new Vector3(xS,yS,zS);

    }

    public static float[] diffArray(float[] from, float[] to )
    {
        List<float> diff_ = new List<float>();

        for (int i = 0; i < from.Length; i++)
            diff_.Add(from[i] - to[i]);

        return diff_.ToArray();
    }

    //public static int[] GetAngleHistogram(ref Vector3[] p1, Vector3[] p2, Vector2 axis, int resolution)
    //{
    //    int[] histo = new int[360];
    //    for (int i = 0; i < p1.Length; i++)
    //    {
    //        float angle = (float)getAngleWithAxis(new Vector2(p1[i].x, p1[i].y), new Vector2(p2[i].x, p2[i].y), axis);
    //        //  float length = (float)lengthVector(new Vector2(p2[i].X - p1[i].X, p2[i].Y - p1[i].Y));

    //        if (!angle.Equals(float.NaN))// || !length.Equals(float.NaN))
    //        {
    //            //to degree
    //            int degAngle = (int)(angle * 180f / (float)Math.PI);
    //            if (degAngle < 0)
    //            {
    //                degAngle = degAngle + 360;
    //                if (degAngle < 360 - 2)
    //                {
    //                    histo[degAngle - 2] += 1; // histo[degAngle - 2];// +length;
    //                    histo[degAngle - 1] += 1; // histo[degAngle - 1];// +length;
    //                    histo[degAngle] += 1; // histo[degAngle];// +length;
    //                    histo[degAngle + 1] += 1; // histo[degAngle + 1];// +length;
    //                    histo[degAngle + 2] += 1; // histo[degAngle + 2];// +length;
    //                }
    //            }
    //            if (degAngle >= 0 && degAngle < 180)
    //            {
    //                if (degAngle - 2 > 0 && degAngle + 2 < histo.Length)
    //                {
    //                    histo[degAngle - 2] += 1; // histo[degAngle - 2];// +length;
    //                    histo[degAngle - 1] += 1; // histo[degAngle - 1];// +length;
    //                    histo[degAngle] += 1; // histo[degAngle];// +length;
    //                    histo[degAngle + 1] += 1; // histo[degAngle + 1];// +length;
    //                    histo[degAngle + 2] += 1; // histo[degAngle + 2];// +length;
    //                }
    //            }
    //            // float val = -normaliseValue(degAngle, 0f, 360f, 0f, 2 * (float)Math.PI);
    //            p1[i].z = degAngle / 10;// normaliseValue(degAngle, 0f, 360f, 0f, 1f);
    //        }
    //    }
    //}

    //public static double lengthVector(Vector2 V)
    //{
    //    return Math.Sqrt((double)(V.X * V.X) + (double)(V.Y * V.Y));
    //}

    //public static Vector2 normalizeVector(Vector2 V)
    //{
    //    double lenght = Math.Sqrt((double)(V.X * V.X) + (double)(V.Y * V.Y));

    //    return new Vector2(V.X / (float)lenght, V.Y / (float)lenght);
    //}

    ///// <summary>
    ///// Get the angle between a vector and an axis
    ///// </summary>
    ///// <param name="A"> A is a first point</param>
    ///// <param name="B"> B is a second point</param>
    ///// <param name="C"> C is the Axis: (1,0) for Ox and (0,1) for Oy</param>
    ///// <returns>returns the axis between the vector AB and the axis C</returns>
    ///// 
    //public static double getAngleWithAxis(Vector2 A, Vector2 B, Vector2 C)
    //{
    //    double angle = 0f;

    //    Vector2 AB = (new Vector2(B.X - A.X, B.Y - A.Y));
    //    Vector2 AC = (new Vector2(C.X, C.Y));

    //    AB = normalizeVector(AB);
    //    AC = normalizeVector(AC);

    //    angle = Math.Atan2(AC.Y, AC.X) - Math.Atan2(AB.Y, AB.X);

    //    return angle;
    //}

	/*public static float animateFastInFastOut(float t)
	{
		if (t <= 0.5f)
			return Mathf.Pow(2.0f * t, 0.75f) / 2f;
		else
			return 1.0f - (Mathf.Pow((2.0f * (1.0 - t)), 0.75f)) / 2f;
	}*/

}
