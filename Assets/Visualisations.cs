﻿using UnityEngine;
using System.Collections;
using DataBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using UtilGeometry;


public class Visualisations : MonoBehaviour
{

    //View v;
    public Material pointCloudMaterial;
    public Material linesGraphMaterial;

    public TextAsset dataFile;

    DataObject dataObject;
    GameObject MenuDimensions;
    View scatterplot3D;

    // Use this for initialization
    void Start()
    {
        //loads a dataset
        dataObject = new DataObject(dataFile.text);


        // Cubix
        float[] coloredData = dataObject.getDimension(7);
        Color[] cd = Colors.mapDiscreteColor(coloredData); 

        GameObject view = createSingle2DView(
            dataObject, // data points 
            1,2,3, 
            -1, // always leave -1 
            MeshTopology.Points, 
            pointCloudMaterial, 
            out scatterplot3D);

        scatterplot3D.setColors(cd);
                    

     
        
    }
    /// <summary>
    /// sets the position of the view
    /// </summary>
    /// <param name="view"></param>
    /// <param name="pos"></param>
    void SetViewPosition(ref GameObject view, Vector3 pos)
    {
        view.transform.position = pos;
    }

    /// <summary>
    /// sets the scale of the view
    /// </summary>
    /// <param name="view"></param>
    /// <param name="scale"></param>
    void SetViewScale(ref GameObject view, Vector3 scale)
    {
        view.transform.localScale= scale;
    }

    /// <summary>
    /// sets the orientation of the view
    /// </summary>
    /// <param name="view"></param>
    /// <param name="quat"></param>
    void SetViewRotation(ref GameObject view, Quaternion quat)
    {
        view.transform.rotation = quat;
    }

    /// <summary>
    /// Creates a textual menu with data dimensions 
    /// </summary>
    /// <param name="dobjs"></param>
    /// <returns></returns>
    // GameObject createTextMenu(DisplayMenu dm, DataObject dobjs)
    // {
    //     GameObject menu = new GameObject();
    //     dm.createTextMenu(menu, Color.black, Color.red);
    //     menu.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    //     return menu;
    // }
    /// <summary>
    /// creates a histogram view
    /// </summary>
    /// <param name="dobjs"></param>
    /// <param name="Dimension"></param>
    /// <param name="binSize"></param>
    /// <param name="smooth"></param>
    /// <param name="scale"></param>
    /// <returns></returns>
    GameObject createSingle1DView(DataObject dobjs, int Dimension, int binSize, bool smooth, float scale)
    {
        GameObject Snax = new GameObject();
        List<Vector3> l = new List<Vector3>();

        //get the array of dimension
        float[] values = dobjs.GetCol(dobjs.DataArray, Dimension);
        float[] bins = new float[binSize+1];
        
        //bin the values
        for (int i = 0; i < values.Length;i++)
        {
            int indexBin = Mathf.RoundToInt(values[i] * binSize);
            //Debug.Log(indexBin);
            bins[indexBin]+=0.05f;
        }

        float minBin = values.Min();
        float maxBin = values.Max();

        //create the data points height (~ histo)
        for (int i = 0; i < bins.Length;i++)
        {
//            l.Add(new Vector3(UtilMath.normaliseValue(i,0,bins.Length,0f,2f), UtilMath.normaliseValue(bins[i],minBin,maxBin,0f,1f), Dimension));
            l.Add(new Vector3(i,bins[i], 0));

            //Debug.Log("Dimension: " + Dimension + " bin " + i + " value: " + bins[i]);
        }

        Vector3[] pointCurved;
        if (smooth) pointCurved = Curver.MakeSmoothCurve(l.ToArray(), binSize);
        else pointCurved = l.ToArray();

        LineRenderer lineRenderer = Snax.gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Standard"));
        lineRenderer.material.color = Color.red;
        lineRenderer.SetColors(Color.red, Color.red);
        lineRenderer.SetWidth(0.025f, 0.025f);
        lineRenderer.SetVertexCount(pointCurved.Length);
        lineRenderer.SetPositions(pointCurved);
        lineRenderer.useWorldSpace = false;

        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(pointCurved);
        int[] indices = tr.Triangulate();

        Vector2[] UVs = new Vector2[4];
        UVs[0] = new Vector2(0, 1);
        UVs[1] = new Vector2(1, 1);
        UVs[2] = new Vector2(0, 0);
        UVs[3] = new Vector2(1, 0);

        //Create the indices
        for(int i=0; i<pointCurved.Length;i++)
        {

        }

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[pointCurved.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(pointCurved[i].x, pointCurved[i].y, 0);
        }

        Snax.transform.localScale *= scale;

        ////// Create the mesh
        //Mesh msh = new Mesh();
        //msh.vertices = vertices;
        //msh.triangles = indices;
        //msh.RecalculateNormals();
        //msh.RecalculateBounds();
        
        //// Set up game object with mesh;
        //Snax.gameObject.AddComponent(typeof(MeshRenderer));
        //Snax.GetComponent<MeshRenderer>().material = new Material(Shader.Find("GUI/Text Shader"));
        //Snax.GetComponent<MeshRenderer>().material.color = Color.blue;
        
        //MeshFilter filter = Snax.gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        
        //filter.mesh = msh;

        return Snax;
    }

    /// <summary>
    /// Creates a single view with dimensions x, y, z
    /// </summary>
    /// <param name="dobjs"></param>
    /// <param name="DimensionX"></param>
    /// <param name="DimensionY"></param>
    /// <param name="DimensionZ"></param>
    /// <param name="topology"></param>
    /// <param name="LinkIndex"> the linking field to create a graph; pass a negative value to ignore</param>
    /// <returns></returns>
    GameObject createSingle2DView(DataObject dobjs, int DimensionX, int DimensionY, int DimensionZ, int LinkIndex, MeshTopology topology, Material m, out View v)
    {
        v = new View(topology);
        string viewName = "";
        
        if (DimensionX >= 0) viewName += dobjs.indexToDimension(DimensionX) + " - ";
        if (DimensionY >= 0) viewName += dobjs.indexToDimension(DimensionY) + " - ";
        if (DimensionZ >= 0) viewName += dobjs.indexToDimension(DimensionZ);

        GameObject view = new GameObject(viewName);
        view.transform.parent = transform;

        v.initialiseDataView(dobjs.DataPoints, view);
        if (DimensionX >= 0)
        {
            v.setDataDimension(dobjs.getDimension(DimensionX), View.VIEW_DIMENSION.X);
            GameObject labelX = createLabel(dobjs.indexToDimension(DimensionX), view, new Vector3(0.1f, 0f,0f));
        }

        if (DimensionY >= 0)
        {
            v.setDataDimension(dobjs.getDimension(DimensionY), View.VIEW_DIMENSION.Y);
            GameObject labelY = createLabel(dobjs.indexToDimension(DimensionY), view, new Vector3(-0.1f, 0.1f, 0f));
            labelY.transform.Rotate(0f, 0f, 90f);
        }

        if (DimensionZ >= 0)
        {
            v.setDataDimension(dobjs.getDimension(DimensionZ), View.VIEW_DIMENSION.Z);
            GameObject labelZ = createLabel(dobjs.indexToDimension(DimensionY), view, new Vector3(-0.1f, 0.1f, 0.1f));
            labelZ.transform.Rotate(0f, -90f, 00f);
        }

        if (LinkIndex < 0)
            v.updateView(null);
        else
            v.updateView(dobjs.getDimension(LinkIndex));

        view.AddComponent<MeshFilter>();
        view.AddComponent<MeshRenderer>();

        view.GetComponent<MeshFilter>().mesh = v.MyMesh;
        view.GetComponent<Renderer>().material = m;

        //List<Color> myColors = new List<Color>();
        //for (int i = 0; i < v.MyMesh.vertices.Length; i++)
        //    myColors.Add(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)));

        return view;
    }
    /// <summary>
    /// Creates a 2D Diff view
    /// </summary>
    /// <param name="dobjs"></param>
    /// <param name="DimensionX"></param>
    /// <param name="DimensionY"></param>
    /// <param name="DimensionZ"></param>
    /// <param name="LinkIndex"></param>
    /// <param name="topology"></param>
    /// <param name="m"></param>
    /// <returns></returns>
    GameObject create2DDiffView(DataObject dobjs, int DimensionX, int DimensionY, int DimensionZ, int ToDimensionX, int ToDimensionY, int ToDimensionZ, int LinkIndex, MeshTopology topology, Material m)
    {
        View v = new View(topology);
        string viewName = "";

        if (DimensionX >= 0) viewName += dobjs.indexToDimension(DimensionX) + " - ";
        if (DimensionY >= 0) viewName += dobjs.indexToDimension(DimensionY) + " - ";
        if (DimensionZ >= 0) viewName += dobjs.indexToDimension(DimensionZ);

        GameObject view = new GameObject(viewName);
        view.transform.parent = transform;

        v.initialiseDataView(dobjs.DataPoints, view);
        
        //diff x
        //float[] dX =  dobjs.getDimension(DimensionX) - dobjs.getDimension(ToDimensionX);
        if (DimensionX >= 0)
        {
            v.setDataDimension(dobjs.getDimension(DimensionX), View.VIEW_DIMENSION.X);
            GameObject labelX = createLabel(dobjs.indexToDimension(DimensionX), view, new Vector3(0.1f, 0f, 0f));
        }
        if (DimensionY >= 0)
        {
            v.setDataDimension(dobjs.getDimension(DimensionY), View.VIEW_DIMENSION.Y);
            GameObject labelY = createLabel(dobjs.indexToDimension(DimensionY), view, new Vector3(-0.1f, 0.1f, 0f));
            labelY.transform.Rotate(0f, 0f, 90f);

        }
        if (DimensionZ >= 0)
        {
            v.setDataDimension(dobjs.getDimension(DimensionZ), View.VIEW_DIMENSION.Z);
            GameObject labelZ = createLabel(dobjs.indexToDimension(DimensionY), view, new Vector3(-0.1f, 0.1f, 0.1f));
            labelZ.transform.Rotate(0f, -90f, 00f);
        }
        if (LinkIndex < 0)
            v.updateView(null);
        else
            v.updateView(dobjs.getDimension(LinkIndex));

        view.AddComponent<MeshFilter>();
        view.AddComponent<MeshRenderer>();

        view.GetComponent<MeshFilter>().mesh = v.MyMesh;
        view.GetComponent<Renderer>().material = m;
        return view;
    }


    /// <summary>
    /// Creates a SPLOM from data
    /// </summary>
    /// <param name="dobjs">the DataObject</param>
    /// <returns>A 2D array of game objects views</returns>
    GameObject[,] createSPLOM2D(DataObject dobjs, int linkingField, MeshTopology topology, Material material, float spacing)
    {
        float[] coloredData = dobjs.getDimension(7);
        Color[] cd = Colors.mapDiscreteColor(coloredData); 

        GameObject[,] SPLOM = new GameObject[dobjs.Identifiers.Length, dobjs.Identifiers.Length];
        string[] descriptors = dobjs.Identifiers;

        for (int i = 0; i < descriptors.Length; i++)
        {
            for (int j = 0; j < descriptors.Length; j++)
            {
                {
                    View v;
                    GameObject view = createSingle2DView(dobjs, i, j, -1, linkingField, topology, material, out v);
                    
                    
                    //int categories = dobjs.getNumberOfCategories(coloredData);
                    //Debug.Log("found " + categories + " in origin");
                    //List<Color> lc = Colors.getPalette(categories);
                    //v.mapColorCategory(coloredData, lc.ToArray());
                    v.setColors(cd);
                    //v.mapColorContinuous(dataObject.getDimension(0), Color.black, Color.red);
                    view.transform.position = new Vector3((float)i * spacing, -(float)j *spacing, 0f);
                    SPLOM[i, j] = view;
                }
            }
        }
        return SPLOM;
    }

    GameObject[,] createSPLOM3D(DataObject dobjs, int linkingField, MeshTopology topology, Material material, float spacing)
    {
        GameObject[,] SPLOM = new GameObject[dobjs.Identifiers.Length, dobjs.Identifiers.Length];
        string[] descriptors = dobjs.Identifiers;
        for (int i = 0; i < descriptors.Length; i++)
        {
            for (int j = 0; j < descriptors.Length; j++)
            {
                for (int k = 0; k < descriptors.Length;k++ )
                {
                    if (i != j)
                    {
                        View v;
                        GameObject view = createSingle2DView(dobjs, i, j, k, linkingField, topology, material, out v);
                        view.transform.position = new Vector3((float)i * spacing, -(float)j * spacing, (float)k * spacing);                        
                        SPLOM[i, j] = view;
                    }
                }
            }
        }
        return SPLOM;
    }

    GameObject createLabel(string label, GameObject parent, Vector3 position)
    {
        GameObject TextObject = new GameObject(label);
        TextObject.AddComponent<TextMesh>();
        TextMesh tm = TextObject.GetComponent<TextMesh>();
        tm.text = label;
        TextObject.transform.localPosition = position;
        TextObject.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
        tm.fontSize = 108;
        tm.color = Color.black;
        TextObject.AddComponent<BoxCollider>();

        TextObject.transform.parent = parent.transform;

        return TextObject;
    }

    int dimension1 = -1;
    int dimension2 = -1;

    View snappedView;
    
    // Update is called once per frame
    void Update()
    {
        //BBACH: TODO MAP PLANE POSITIONS
        //Communication with the Cube Shader: sets the p0,p1,p2 points of the plane
        pointCloudMaterial.SetVector("p0Temp", new Vector4());
        pointCloudMaterial.SetVector("p1Temp", new Vector4());
        pointCloudMaterial.SetVector("p2Temp", new Vector4());



        //if (Input.GetMouseButtonDown(0))
        //{
        //    //empty RaycastHit object which raycast puts the hit details into
        //    RaycastHit hit;
        //    //ray shooting out of the camera from where the mouse is
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //    if (Physics.Raycast(ray, out hit))
        //    {
        //        //print out the name if the raycast hits something
        //        int k = (dataObject.dimensionToIndex(hit.transform.name));

        //        if (dimension1 < 0) dimension1 = k;
        //        if (dimension2 < 0 && dimension1 != k) dimension2 = k;

        //        if (dimension1 > 0 && dimension2 > 0)
        //        {
        //            View v;
        //            createSingle2DView(dataObject, dimension1, dimension2, -1, -1, MeshTopology.Points, pointCloudMaterial, out v);
        //        }

        //    }
        //}
        //if(Input.GetMouseButtonDown(1))
        //{
        //    dimension1 = -1;
        //    dimension2 = -1;
        //}


    }


}
