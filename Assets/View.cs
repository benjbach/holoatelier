using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class View
{
    bool UNITY_GAME_OBJECTS_MODE = false;
    List<GameObject> visualObjects = new List<GameObject>();

    public enum VIEW_DIMENSION { X, Y, Z, LINKING_FIELD };
    private Mesh myMesh;

    public Mesh MyMesh
    {
        get { return myMesh; }
        set { myMesh = value; }
    }

    private MeshTopology myMeshTopoolgy;

    private List<Vector3> positions = new List<Vector3>();

    public View(MeshTopology type)
    {
        myMeshTopoolgy = type;
        myMesh = new Mesh();
    }

    private Vector3[] Colors;

    public Vector3[] ColorsProperty
    {
        get { return Colors; }
        set { Colors = value; }
    }
 
    public void initialiseDataView(int numberOfPoints, GameObject parent)
    {
        for (int i = 0; i < numberOfPoints-1; i++)
        {
            positions.Add(new Vector3());

            if (UNITY_GAME_OBJECTS_MODE)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.parent = parent.transform;
                visualObjects.Add(go);
            }
        }

        //Debug.Log("Created " + numberOfPoints +" data points");
    }

    public void setDataDimension(float[] dat, VIEW_DIMENSION dimension)
    {
        for (int i = 0; i < dat.Length; i++)
        {
            Vector3 p = positions[i];

            switch (dimension)
            {
                case VIEW_DIMENSION.X:
                    p.x = dat[i];
                    break;
                case VIEW_DIMENSION.Y:
                    p.y = dat[i];
                    break;
                case VIEW_DIMENSION.Z:
                    p.z = dat[i];
                    break;
            }
            positions[i] = p;
        }
    }

    public void updateView(float[] linking)
    {
        if (UNITY_GAME_OBJECTS_MODE)
            updateGameObjects(0.05f);
        else if (linking == null)
            updateMeshPositions(null);
        else //create the lines
        {
            updateMeshPositions(linking);
        }
    }

    int[] createIndicesScatterPlot(int numberOfPoints)
    {
        int[] indices = new int[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            indices[i] = i;
        }
        return indices;
    }

    int[] createIndicesLines(float[] linkingField)
    {
        List<int> indices = new List<int>();

        for (int i = 0; i < linkingField.Length-1; i++)
        {
            //Debug.Log(linkingField[i] + "    - - - -        " + linkingField[i + 1]);
            if (linkingField[i] == linkingField[i + 1])
            {
                indices.Add(i);
                indices.Add(i + 1);
            }
        }

        //foreach (int item in indices)
        //{
        //    Debug.Log(item);
        //}
        return indices.ToArray();
    }

    private void updateGameObjects(float scale)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            GameObject go = visualObjects[i];
            go.transform.localScale = new Vector3(scale, scale, scale);
            go.transform.localPosition = positions[i];
            go.GetComponent<Renderer>().material.color = Color.black;
        }
    }

    private void updateMeshPositions(float[] linkingField)
    {
        switch (myMeshTopoolgy)
        {
            case MeshTopology.LineStrip:
                myMesh.vertices = positions.ToArray();
                myMesh.SetIndices(createIndicesLines(linkingField), MeshTopology.LineStrip, 0);
                break;
            case MeshTopology.Lines:
                myMesh.vertices = positions.ToArray();
                myMesh.SetIndices(createIndicesLines(linkingField), MeshTopology.Lines, 0);
                break;
            case MeshTopology.Points:
                myMesh.vertices = positions.ToArray();
                myMesh.SetIndices(createIndicesScatterPlot(positions.Count), MeshTopology.Points, 0);
                break;
            case MeshTopology.Quads:
                break;
            case MeshTopology.Triangles:
                break;
            default:
                break;
        }

        

    }

    public void debugVector3List(List<Vector3> list)
    {
        foreach (Vector3 p in list)
        {
            Debug.Log(p.ToString());
        }
    }

    internal void debugVector3List(Vector3[] vector3)
    {
        foreach (Vector3 p in vector3)
        {
            Debug.Log(p.ToString());
        }
    }

    public void mapColorContinuous(float[] dat, Color fromColor, Color toColor)
    {
        List<Color> myColors = new List<Color>();
        for(int i=0;i<dat.Length;i++)
        {
            myColors.Add(Color.Lerp(fromColor, toColor, dat[i]));
        }
        //Debug.Log("vertices count: " + myMesh.vertices.Length + " colors count: " + myColors.Count);
        myMesh.colors = myColors.ToArray();
    }

    public void mapColorCategory(float[] dat, Color[] palette)
    {
        Color[] colorSet = new Color[dat.Length];
        int cat =0;
        colorSet[0] = palette[cat];
        for(int i=1; i<dat.Length; i++)
        {
            if (dat[i] == dat[i - 1]) colorSet[i] = palette[cat];
            else {
                cat++;
                Debug.Log(cat);
                colorSet[i] = palette[cat];
            }
        }
        setColors(colorSet);
    }

    public void setColors(Color[] colors)
    {
        myMesh.colors = colors;
//        myMesh.RecalculateNormals();
    }
}
