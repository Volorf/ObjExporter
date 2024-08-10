using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Volorf.ObjExporter
{
    public class ObjExporter
    {
        Vector3[] _points = new Vector3[]
        {
            // Based on Blenders axis. Z up, Y forward.

            // Left side counter clockwise
            new Vector3(-1f, -1f, -1f),
            new Vector3(-1f, -1f, 1f),
            new Vector3(-1f, 1f, 1f),
            new Vector3(-1f, 1f, -1f),

            // Right side counter clockwise
            new Vector3(1f, -1f, -1f),
            new Vector3(1f, -1f, 1f),
            new Vector3(1f, 1f, 1f),
            new Vector3(1f, 1f, -1f)
        };

        Vector3[] _normals = {
            new (-1f, 0f, 0f),
            new (0f, 1f, 0f),
            new (1f, 0f, 0f),
            new (0f, -1f, 0f),
            new (0f, 0f, 1f),
            new (0f, 0f, -1f)
        };

        Vector2[] _uvs = {
            new (1f, 0f),
            new (0f, 0f),
            new (0f, 1f),
            new (1f, 1f)
        };

        StringBuilder _objData = new StringBuilder();
        StringBuilder _mtlData = new StringBuilder();

        Hashtable _colorPalette = new Hashtable();

        void AddComment(StringBuilder data, string c)
        {
            data.Append("# " + c + "\n");
        }

        private void AddLine(StringBuilder data, string l)
        {
            data.Append(l + "\n");
        }

        public async Task ExportAsync(
            string pathWithDirectory, 
            string appName, 
            string appVersion, 
            string sketchName,
            Vector3[] positions, 
            Vector3[] colors, 
            float voxelSize,
            Action onSuccess,
            Action onFailure
            )
        {
            try
            {
                Clear();
                AddComment(_objData, appName);
                AddComment(_objData, appVersion);

                AddLine(_objData, "mtllib " + sketchName + ".mtl");

                await Task.Run(() => Generate(positions, colors, voxelSize));

                // Debug.Log(_objData);
                // Debug.Log(pathWithDirectory);

                string pathWithSketchDir = pathWithDirectory + "/" + sketchName;

                await WriteFileAsync(pathWithSketchDir, sketchName + ".obj", _objData.ToString());
                await WriteFileAsync(pathWithSketchDir, sketchName + ".mtl", _mtlData.ToString());
                
                onSuccess();
            }
            catch (Exception e)
            {
                Debug.LogError("Export failed: " + e.Message);
                onFailure();
            }
        }

        private void Generate(Vector3[] positions, Vector3[] colors, float voxelSize)
        {
            _colorPalette.Clear();
            int colorCount = 0;

            for (int i = 0; i < positions.Length; i++)
            {
                _objData.Append(GetObjectTitle("Cube " + i));

                for (int j = 0; j < _points.Length; j++)
                {
                    _objData.Append(GetVertex(positions[i] * 2 + _points[j] * voxelSize));
                }

                for (int m = 0; m < _uvs.Length; m++)
                {
                    _objData.Append(GetUV(_uvs[m]));
                }

                for (int k = 0; k < _normals.Length; k++)
                {
                    _objData.Append(GetNormal(_normals[k]));
                }

                if (_colorPalette.ContainsKey(colors[i]))
                {
                    AddLine(_objData, "usemtl " + _colorPalette[colors[i]]);
                }
                else
                {
                    colorCount++;
                    _colorPalette.Add(colors[i], colorCount);
                    AddLine(_objData, "usemtl " + colorCount);

                    // Fill .mtl data
                    AddLine(_mtlData, "newmtl " + colorCount);
                    AddLine(_mtlData, "Ns 250.0");
                    AddLine(_mtlData, "Ka 1.0 1.0 1.0");
                    AddLine(_mtlData, $"Kd {colors[i].x} {colors[i].y} {colors[i].z}");
                    AddLine(_mtlData, "Ks 0.5 0.5 0.5");
                    AddLine(_mtlData, "Ke 0.0 0.0 0.0");
                    AddLine(_mtlData, "Ni 1.0");
                    AddLine(_mtlData, "d 1.0");
                    AddLine(_mtlData, "illum 2");
                    AddLine(_mtlData, "");
                }

                AddLine(_objData, "s 0");

                _objData.Append(GetFaces(i));
            }
        }
        
        private string GetObjectTitle(string t)
        {
            return "o " + t + "\n";
        }

        private string GetVertex(Vector3 vec)
        {
            string vertexString = "v ";

            vertexString += vec.x + " ";
            vertexString += vec.y + " ";
            vertexString += vec.z + "\n";

            return vertexString;
        }

        private string GetUV(Vector2 vec)
        {
            string uv = "vt ";

            uv += vec.x + " ";
            uv += vec.y + "\n";

            return uv;
        }

        private string GetNormal(Vector3 vec)
        {
            string normalString = "vn ";

            normalString += vec.x + " ";
            normalString += vec.y + " ";
            normalString += vec.z + "\n";

            return normalString;
        }

        private string GetFaces(int boxelIndex)
        {
            string faceSequence = "";
            int iOff = boxelIndex * 8;

            // https://www.figma.com/file/9SBJSDKK13UkWwfSzT1ZOh/BOXEL?node-id=6922%3A4049&t=aaCs9xiZ0eUqQpOg-1

            // Left Side
            faceSequence += "f ";
            faceSequence += (1 + iOff) + "/" + 1 + "/" + 1 + " ";
            faceSequence += (2 + iOff) + "/" + 2 + "/" + 1 + " ";
            faceSequence += (3 + iOff) + "/" + 3 + "/" + 1 + " ";
            faceSequence += (4 + iOff) + "/" + 4 + "/" + 1 + "\n";

            // Front Side
            faceSequence += "f ";
            faceSequence += (4 + iOff) + "/" + 1 + "/" + 2 + " ";
            faceSequence += (3 + iOff) + "/" + 2 + "/" + 2 + " ";
            faceSequence += (7 + iOff) + "/" + 3 + "/" + 2 + " ";
            faceSequence += (8 + iOff) + "/" + 4 + "/" + 2 + "\n";

            // Right Side
            faceSequence += "f ";
            faceSequence += (5 + iOff) + "/" + 1 + "/" + 3 + " ";
            faceSequence += (8 + iOff) + "/" + 2 + "/" + 3 + " ";
            faceSequence += (7 + iOff) + "/" + 3 + "/" + 3 + " ";
            faceSequence += (6 + iOff) + "/" + 4 + "/" + 3 + "\n";

            // Back Side
            faceSequence += "f ";
            faceSequence += (1 + iOff) + "/" + 1 + "/" + 4 + " ";
            faceSequence += (5 + iOff) + "/" + 2 + "/" + 4 + " ";
            faceSequence += (6 + iOff) + "/" + 3 + "/" + 4 + " ";
            faceSequence += (2 + iOff) + "/" + 4 + "/" + 4 + "\n";

            // Top Side
            faceSequence += "f ";
            faceSequence += (2 + iOff) + "/" + 1 + "/" + 5 + " ";
            faceSequence += (6 + iOff) + "/" + 2 + "/" + 5 + " ";
            faceSequence += (7 + iOff) + "/" + 3 + "/" + 5 + " ";
            faceSequence += (3 + iOff) + "/" + 4 + "/" + 5 + "\n";

            // Bottom Side
            faceSequence += "f ";
            faceSequence += (1 + iOff) + "/" + 1 + "/" + 6 + " ";
            faceSequence += (4 + iOff) + "/" + 2 + "/" + 6 + " ";
            faceSequence += (8 + iOff) + "/" + 3 + "/" + 6 + " ";
            faceSequence += (5 + iOff) + "/" + 4 + "/" + 6 + "\n";

            return faceSequence;
        }

        private void Clear()
        {
            _objData.Clear();
            _mtlData.Clear();
        }

        private async Task WriteFileAsync(string pathWithSketchDir, string fileNameWithExtention, string data)
        {
            if (!Directory.Exists(pathWithSketchDir))
            {
                Directory.CreateDirectory(pathWithSketchDir);
            }

            string filePath = pathWithSketchDir + "/" + fileNameWithExtention;
            await File.WriteAllTextAsync(filePath, data);
        }
    }
}
