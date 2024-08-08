using System;
using System.Collections;
using System.IO;
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

        string _objData;
        string _mtlData;

        Hashtable _colorPalette = new Hashtable();

        void AddComment(ref string data, string c)
        {
            data += "# " + c + "\n";
        }

        private void AddLine(ref string data, string l)
        {
            data += l + "\n";
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
                AddComment(ref _objData, appName);
                AddComment(ref _objData, appVersion);

                AddLine(ref _objData, "mtllib " + sketchName + ".mtl");

                Generate(positions, colors, voxelSize);

                // Debug.Log(_objData);
                // Debug.Log(pathWithDirectory);

                string pathWithSketchDir = pathWithDirectory + "/" + sketchName;

                await WriteFileAsync(pathWithSketchDir, sketchName + ".obj", _objData);
                await WriteFileAsync(pathWithSketchDir, sketchName + ".mtl", _mtlData);
                
                onSuccess();
            }
            catch
            {
                onFailure();
            }
        }

        private void Generate(Vector3[] positions, Vector3[] colors, float voxelSize)
        {
            _colorPalette.Clear();
            int colorCount = 0;

            for (int i = 0; i < positions.Length; i++)
            {
                _objData += GetObjectTitle("Cube " + i);

                for (int j = 0; j < _points.Length; j++)
                {
                    _objData += GetVertex(positions[i] * 2 + _points[j] * voxelSize);
                }

                for (int m = 0; m < _uvs.Length; m++)
                {
                    _objData += GetUV(_uvs[m]);
                }

                for (int k = 0; k < _normals.Length; k++)
                {
                    _objData += GetNormal(_normals[k]);
                }

                if (_colorPalette.ContainsKey(colors[i]))
                {
                    AddLine(ref _objData, "usemtl " + _colorPalette[colors[i]]);
                }
                else
                {
                    colorCount++;
                    _colorPalette.Add(colors[i], colorCount);
                    AddLine(ref _objData, "usemtl " + colorCount);

                    // Fill .mtl data
                    AddLine(ref _mtlData, "newmtl " + colorCount);
                    AddLine(ref _mtlData, "Ns 250.0");
                    AddLine(ref _mtlData, "Ka 1.0 1.0 1.0");
                    AddLine(ref _mtlData, $"Kd {colors[i].x} {colors[i].y} {colors[i].z}");
                    AddLine(ref _mtlData, "Ks 0.5 0.5 0.5");
                    AddLine(ref _mtlData, "Ke 0.0 0.0 0.0");
                    AddLine(ref _mtlData, "Ni 1.45");
                    AddLine(ref _mtlData, "d 1.0");
                    AddLine(ref _mtlData, "illum 2");
                    AddLine(ref _mtlData, "");
                }

                AddLine(ref _objData, "s off");

                _objData += GetFaces(i);
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
            _objData = "";
            _mtlData = "";
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
