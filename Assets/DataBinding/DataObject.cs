using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using System.Linq;


namespace DataBinding
{

    public class DataObject
    {
        float[,] dataArray;
        int dataPoints;

        public int DataPoints
        {
            get { return dataPoints; }
            set { dataPoints = value; }
        }

        public float[,] DataArray
        {
            get { return dataArray; }
            set { dataArray = value; }
        }

        string[] identifiers;

        public string[] Identifiers
        {
            get { return identifiers; }
            set { identifiers = value; }
        }
        string[] typesToRead;

        public DataObject(string data)
        {
            loadCSV(data);
        }

        Dictionary<float, string> textualDimensions = new Dictionary<float, string>();

        public Dictionary<float, string> TextualDimensions
        {
            get { return textualDimensions; }
            set { textualDimensions = value; }
        }


        char[] split = new char[] { ',', '\t'};

        Dictionary<int, Vector2> dimensionsRange = new Dictionary<int, Vector2>(); 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void loadCSV(string data)
        {
            string[] lines = data.Split('\n');

            //1: read types
            identifiers = lines[0].Split(split);
            typesToRead = lines[1].Split(split);
            //clean identifiers strings
            for (int i = 0; i < identifiers.Length;i++)
            {
                string id = identifiers[i].Replace("\r", string.Empty);
                identifiers[i] = id;
            }

            dataArray = new float[lines.Length, identifiers.Length];
            dataPoints = dataArray.GetUpperBound(0);

            string[] theTypes = new string[typesToRead.Length];

            //type reading
            for (int i = 0; i < typesToRead.Length; i++)
            {
                if (isBool(typesToRead[i]))
                    theTypes[i] = "bool";
                else if (isFloat(typesToRead[i]))
                    theTypes[i] = "float";
                else theTypes[i] = "string";
            }
            
            float textualPointer = 0f;
            //line reading
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(split);
                
                //dimension reading
                for (int k = 0; k < identifiers.Length; k++)
                {
   
                    //1- get the corresponding type
                    if (isBool(values[k]))
                    {
                        dataArray[i - 1, k] = Convert.ToSingle(bool.Parse(values[k]));
                    }
                    else if(values[k].Contains(':'))// isDateTime(values[k]))
                    {
                        //DateTime dt = DateTime.Parse(values[k]);
                        string[] valH = values[k].Split(':');
                        dataArray[i - 1, k] = float.Parse(valH[0]) * 60 + float.Parse(valH[1]);// *60 + float.Parse(valH[2]);
                    }
                    else if(isInt(values[k]))
                    {
                        dataArray[i-1,k] = (float)int.Parse(values[k]);                     
                    }
                    else if (isFloat(values[k]))
                    {
                        //Debug.Log(k);
                        dataArray[i - 1, k] = float.Parse(values[k]);
                    }
                    else if (!String.IsNullOrEmpty(values[k]))
                    {
                        //lookup if already encoded
                        if (textualDimensions.ContainsValue(values[k]))
                        {
                            //Debug.Log(i + " " + k);
                            dataArray[i - 1, k] = textualDimensions.FirstOrDefault(x => x.Value == values[k]).Key;
                        }
                        else
                        {
                            //new key
                            textualPointer++;
                            textualDimensions.Add(textualPointer, values[k]);
                            dataArray[i - 1, k] = textualPointer;
                        }
                    }
                    else
                    { 
                        dataArray[i - 1, k] = 0f;
                    }
                }
            }

            normaliseArray();

        }
        /// <summary>
        /// internal function: normalises all the data input between 0 and 1
        /// </summary>
        private void normaliseArray()
        {
            //1 make a copy of the parsed array
            float[,] normArray = new float[dataArray.GetUpperBound(0),dataArray.GetUpperBound(1)+1];
            //for each dimensions (column) normalise all data
            for (int i = 0; i < normArray.GetUpperBound(1); i++)
            {
                float[] rawDimension = GetCol(dataArray, i);
                float minDimension = rawDimension.Min();
                float maxDimension = rawDimension.Max();

                float[] normalisedDimension = new float[rawDimension.Length];

                dimensionsRange.Add(i, new Vector2(minDimension, maxDimension));

                for(int j=0; j<rawDimension.Length;j++)
                {
                    normalisedDimension[j] = normaliseValue(rawDimension[j], minDimension, maxDimension, 0f, 1f);
                    //if (i == 13)
                    //    Debug.Log(rawDimension[j] + "    ---    " + normaliseValue(rawDimension[j], minDimension, maxDimension, 0f, 1f));
                }

                SetCol<float>(normArray, i, normalisedDimension);
                
            }
            dataArray = normArray;
        }

        /// <summary>
        /// debug function that prints the 2D array
        /// </summary>
        /// <param name="data"></param>
        public void Debug2DArray(object[,] data)
        {
            for (int i = 0; i < data.GetUpperBound(0); i++)
            {
                string line = "";
                for (int j = 0; j < data.GetUpperBound(1); j++)
                {
                    line += (data[i, j]) + " ";
                }
                Debug.Log(line);
            }
        }

        /// <summary>
        /// debugs one column
        /// </summary>
        /// <param name="col"></param>
        public void DebugArray(int col)
        {
            float[] selection = getDimension(identifiers[col]);

            for (int i = 0; i < selection.Length; i++)
                Debug.Log(selection[i]);
        }

        /// <summary>
        /// returns one row of 2D array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public T[] GetRow<T>(T[,] matrix, int row)
        {
            var rowLength = matrix.GetLength(1);
            var rowVector = new T[rowLength];

            for (var i = 0; i < rowLength; i++)
                rowVector[i] = matrix[row, i];

            return rowVector;
        }

        /// <summary>
        /// returns one column of the 2D array
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public float[] GetCol(float[,] matrix, int col)
        {
            var colLength = matrix.GetLength(0)-1;
            var colVector = new float[colLength];

            for (var i = 0; i < colLength; i++)
            {
                colVector[i] = matrix[i, col];
            }
            return colVector;
        }

        /// <summary>
        /// sets a vector of values into a specific column
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix"></param>
        /// <param name="col"></param>
        /// <param name="colVector"></param>
        public void SetCol<T>(T[,] matrix, int col, T[] colVector)
        {
            var colLength = matrix.GetLength(0);
            for (var i = 0; i < colLength; i++)
                matrix[i, col] = colVector[i];
        }

        /// <summary>
        /// returns an array of values corresponding to the  
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public float[] getDimension(int col)
        {
            return getDimension(identifiers[col]);
        }

        public float[] getDimension(string name)
        {

            // 1 bind name to position in array
            int selectCol = -1;
            for (int i = 0; i < identifiers.Length; i++)
            {
                if (identifiers[i] == name)
                    selectCol = i;
            }
            if (selectCol < 0)
                return null;
            else
            {
                return GetCol(dataArray, selectCol);
            }

        }

        public int dimensionToIndex(string dimension)
        {
            int id = -1;
            for(int i=0;i<identifiers.Length;i++)
            {
                
                if (dimension == identifiers[i])
                {
                    id = i;
                }
            }
            return id;
        }
        
        public string indexToDimension(int dimensionIndex)
        {
            return identifiers.ElementAt(dimensionIndex);
        }

        float stringToFloat(string value)
        {
            return BitConverter.ToSingle(Encoding.UTF8.GetBytes(value), 0);
        }
        string floatToString(float value)
        {
            return BitConverter.ToString(BitConverter.GetBytes(value));
        }
      
        float normaliseValue(float value, float i0, float i1, float j0, float j1)
        {
            float L = (j0 - j1) / (i0 - i1);
            return (j0 - (L * i0) + (L * value));
        }

        public bool isBool(string value)
        {
            bool res = false;
            return bool.TryParse(value, out res);
        }

        public bool isInt(string value)
        {
            int res=0;
            return int.TryParse(value, out res);
        }
        public bool isFloat(string value)
        {
            float res = 0f;
            return float.TryParse(value, out res);
        }

        public bool isDateTime(string value)
        {
            DateTime res = new DateTime();
            return DateTime.TryParse(value, out res);
        }

        public int getNumberOfCategories(float[] column)
        {
            List<float> values = new List<float>();
            int categories = 0;
            for(int i=0; i<column.Length;i++)
            {
                if (!values.Contains(column[i]))
                {
                    values.Add(column[i]);
                    Debug.Log(normaliseValue(column[i],0f,1f,dimensionsRange[7].x, dimensionsRange[7].y));
                }
                //if (column[i] != column[i + 1])
                //{ Debug.Log(column[i] + "       " + column[i + 1]); categories++; }
            }
                return values.Count;
        }

    }
}