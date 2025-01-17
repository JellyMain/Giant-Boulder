using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

// Face remove by angle
namespace RayFire
{
    public class RFCombineMesh
    {
        // Combined mesh data
        List<int>       trianglesSubId;
        List<List<int>> triangles;
        List<Vector3>   vertices;
        List<Vector3>   normals;
        List<Vector2>   uv;
        List<Vector2>   uv2;
        List<Color>     colors;
        List<Vector4>   tangents;
        
        // Constructor
        RFCombineMesh()
        {
            trianglesSubId = new List<int>();
            triangles      = new List<List<int>>();
            vertices       = new List<Vector3>();
            normals        = new List<Vector3>();
            uv             = new List<Vector2>();
            uv2            = new List<Vector2>();
            colors         = new List<Color>();
            tangents       = new List<Vector4>();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Combine
        /// /////////////////////////////////////////////////////////
        
        // Set combined mesh data
        public static RFCombineMesh GetCombinedMesh(Transform transForm, List<Mesh> meshList, List<Transform> transList, List<List<int>> matIdList, List<bool> invertNormals)
        {
            // Check all meshes and convert to tris
            int meshVertIdOffset = 0;
            RFCombineMesh cMesh  = new RFCombineMesh();
            
            Mesh mesh;
            
            for (int m = 0; m < meshList.Count; m++)
            {
                // Get local mesh
                mesh             = meshList[m];

                // Collect combined vertices list
                cMesh.vertices.AddRange(mesh.vertices.Select(t => transForm.InverseTransformPoint(transList[m].TransformPoint(t))));
                
                // Collect combined normals list
                for (int i = 0; i < mesh.normals.Length; i++)
                    if (invertNormals[m] == false)
                        cMesh.normals.Add (mesh.normals[i]);
                    else
                        cMesh.normals.Add (-mesh.normals[i]);

                // Collect combined uvs list
                cMesh.uv.AddRange(mesh.uv.ToList());
                cMesh.uv2.AddRange(mesh.uv2.ToList());

                cMesh.colors.AddRange(mesh.colors.ToList());
                
                // Collect combined tangents list TODO FLIP NORMAL FOR INVERTED
                cMesh.tangents.AddRange(mesh.tangents.ToList());
                
                // Iterate every submesh
                for (int s = 0; s < mesh.subMeshCount; s++)
                {
                    // Get all triangles verts ids
                    int[] tris = mesh.GetTriangles(s);

                    // Invert normals
                    if (invertNormals[m] == true)
                        tris = tris.Reverse().ToArray();

                    // Increment by mesh vertices id offset
                    for (int i = 0; i < tris.Length; i++)
                        tris[i] += meshVertIdOffset;
                    
                    // Collect triangles with material which already has other triangles. >> add to existing list
                    if (cMesh.trianglesSubId.Contains(matIdList[m][s]) == true) // TODO change to hashset
                    {
                        int ind = cMesh.trianglesSubId.IndexOf(matIdList[m][s]);
                        cMesh.triangles[ind].AddRange(tris.ToList());
                    }
                    else
                    {
                        // Collect sub mesh triangles >> Create new list
                        cMesh.triangles.Add(tris.ToList());
                                            
                        // Check every triangle and collect tris material id
                        cMesh.trianglesSubId.Add(matIdList[m][s]);
                    }
                }

                // Offset verts ids per mesh
                meshVertIdOffset += mesh.vertices.Length;
            }

            return cMesh;
        }
        
        // Create combined mesh
        public static Mesh CreateMesh (RFCombineMesh cMesh, string name, IndexFormat indexFormat = IndexFormat.UInt16)
        {
            // Create combined mesh
            Mesh newMesh = new Mesh();
            newMesh.indexFormat = indexFormat;
            newMesh.name        = name + "_Comb";
            newMesh.SetVertices(cMesh.vertices);
            
            // Set triangles by submeshes
            newMesh.subMeshCount = cMesh.trianglesSubId.Count;
            for (int i = 0; i < cMesh.trianglesSubId.Count; i++)
                newMesh.SetTriangles(cMesh.triangles[i], cMesh.trianglesSubId[i]);
        
            // Normals IMPORTANT Do not RecalculateNormals(), cause normal artifacts
            newMesh.SetNormals(cMesh.normals); 
            // newMesh.RecalculateNormals();
            
            // UVs
            newMesh.SetUVs(0, cMesh.uv);
            newMesh.SetUVs(1, cMesh.uv2);
            newMesh.SetColors (cMesh.colors);
            
            // Tangents
            newMesh.SetTangents(cMesh.tangents);
            newMesh.RecalculateTangents();
            
            // Bounds
            newMesh.RecalculateBounds();
            
            return newMesh;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Shatter
        /// /////////////////////////////////////////////////////////
                      
        // Combine list of meshfilters
        public static Mesh CombineShatter (RayfireShatter shatter, Transform root, List<MeshFilter> filters)
        {
            // Get meshes and tms
            List<Mesh>           meshList  = new List<Mesh>();
            List<Transform>      transList = new List<Transform>();
            List<List<Material>> matList   = new List<List<Material>>();
            
            // Verts amount // TODO max vert amount check
            // int totalVerts = 0;

            // Collect meshes, transforms and materials by meshfilter list
            GetMeshTransMatLists (filters, ref meshList, ref transList, ref matList, 4, 0.05f);
            
            // Get all materials list
            List<Material> allMaterials = GetAllMaterials(transList, matList);

            // Set materials list to shatter
            shatter.materials = allMaterials.ToArray();
            
            // Collect material ids per submesh
            List<List<int>> matIdList = GetMatIdList(transList, matList, allMaterials);
            
            // Get invert list
            List<bool> invertNormals = GetInvertList(transList);
            
            // Create combined mesh data
            RFCombineMesh cMesh = GetCombinedMesh(root, meshList, transList, matIdList, invertNormals);
            
            // Create combined mesh and return TODO input index format in case of more than 65k vertices
            return CreateMesh (cMesh, root.name);
        }

        // Collect meshes, transforms and materials by meshfilter list
        static void GetMeshTransMatLists (List<MeshFilter> filters, ref List<Mesh> meshList, ref List<Transform> transList, ref List<List<Material>> matList, int verts, float size)
        {
            // Collect mesh, tm and mats for meshfilter
            foreach (var mf in filters)
            {
                // Filters
                if (mf.sharedMesh.vertexCount < verts)
                    continue;
                MeshRenderer mr = mf.GetComponent<MeshRenderer>();
                if (mr != null && mr.bounds.size.magnitude < size)
                    continue;
                
                // Collect mats
                List<Material> mats = new List<Material>();
                if (mr != null)
                    mats = mr.sharedMaterials.ToList();
                
                // Collect
                meshList.Add(mf.sharedMesh);
                transList.Add(mf.transform);
                matList.Add(mats);
            }
        }
        
        // Get all materials list
        public static List<Material> GetAllMaterials(List<Transform> transList, List<List<Material>> matList)
        {
            List<Material> allMaterials = new List<Material>();
            for (int f = 0; f < transList.Count; f++)
                for (int m = 0; m < matList[f].Count; m++)
                    if (allMaterials.Contains(matList[f][m]) == false)
                        allMaterials.Add(matList[f][m]);
            return allMaterials;
        }
        
        // Collect material ids per submesh
        public static List<List<int>> GetMatIdList(List<Transform> transList, List<List<Material>> matList, List<Material> allMaterials)
        {
            List<List<int>> matIdList = new List<List<int>>();
            for (int f = 0; f < transList.Count; f++)
                matIdList.Add(matList[f].Select(t => allMaterials.IndexOf(t)).ToList());
            return matIdList;
        }

        // Get invert list by transforms
        public static List<bool> GetInvertList(List<Transform> transList)
        {
            List<bool> invertNormals = new List<bool>();
            for (int f = 0; f < transList.Count; f++)
            {
                // Get invert normals because of negative scale
                bool invert = false;
                if (transList[f].localScale.x < 0) 
                    invert = !invert;
                if (transList[f].localScale.y < 0) 
                    invert = !invert;
                if (transList[f].localScale.z < 0) 
                    invert = !invert;
                invertNormals.Add(invert);
            }
            return invertNormals;
        }
    }
}


