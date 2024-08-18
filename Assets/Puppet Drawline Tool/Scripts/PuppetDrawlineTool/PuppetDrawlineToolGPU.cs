using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using SpRiseMChen.Helper;


namespace SpRiseMChen.PuppetDrawline
{
    /// <summary>
    /// Puppet Drawline GPU Tool Class
    /// </summary>
    [Serializable]
    public class PuppetDrawlineToolGPU
    {
        /// <summary>
        /// compute shader
        /// </summary>
        private ComputeShader puppetWarpCS;


        #region ----- kernels of the compute shader. ------

        private int kernel_Matrix_Multiply;
        private int kernel_Matrix_TransposeMultiply;

        private int kernel_Matrix_GaussianElimination_CalEachRowDiv;
        private int kernel_Matrix_GaussianElimination_Elimination;

        private int kernel_Matrix_UpperTriangleMatrix_Solver;
        private int kernel_Matrix_LowerTriangleMatrix_Solver;

        private int kernel_Matrix_LDLT_CalDii;
        private int kernel_Matrix_LDLT;
        private int kernel_Matrix_LDLT_ClearLUpperTriValue;
        private int kernel_Matrix_LDLT_MergeLDandLT;
        private int kernel_Matrix_LDLT_ResetDiag;

        private int kernel_Matrix_LDLT_UpperTriangle_Solver;
        private int kernel_Matrix_LDLT_LowerTriangle_Solver;

        #endregion -----------


        #region ------- Matrix Info ---------
        /// <summary>
        /// handle weight, 1000 by default.
        /// </summary>
        private float w = 1000f;

        /// <summary>
        /// Part of the A1 matrix with respect to the edge error
        /// </summary>
        private Matrix A1_edge;

        /// <summary>
        /// The part of the A2 matrix about the edge error
        /// </summary>
        private Matrix A2_edge;

        /// <summary>
        /// The part of vector b1 about the edge error
        /// </summary>
        private Matrix b1_edge;

        /// <summary>
        /// The part of the b2 vector that deals with the edge error
        /// </summary>
        private Matrix b2_edge;

        /// <summary>
        /// Parts of A1 and A2 matrices with handle constraints (same matrix)
        /// </summary>
        private Matrix A_con;

        /// <summary>
        /// handle constraint in vectors b1 and b2 (same vector)
        /// </summary>
        private Matrix b_con;

        /// <summary>
        /// Complete A1 matrix
        /// </summary>
        private Matrix A1;

        /// <summary>
        /// Complete A2 matrix
        /// </summary>
        private Matrix A2;

        /// <summary>
        /// Complete b1 vector
        /// </summary>
        private Matrix b1;

        /// <summary>
        /// Complete b2 vector
        /// </summary>
        private Matrix b2;

        /// <summary>
        /// b2 vector error section, store scaled adjusted v' data
        /// </summary>
        private Matrix b2_edge_prime;

        /// <summary>
        /// A1 side error matrix ^T * A1 side error matrix
        /// </summary>
        private Matrix A1_edgeT_mul_A1_edge;

        /// <summary>
        /// A2 side error matrix ^T * A2 side error matrix
        /// </summary>
        private Matrix A2_edgeT_mul_A2_edge;

        /// <summary>
        /// A1 side error matrix ^T * b1 side error vector
        /// Used to record the value on the right side of the equation A1T*A1*v = A1T*b1
        /// </summary>
        private Matrix A1_edgeT_mul_b1_edge;

        /// <summary>
        /// Complete matrix A1^T*b1;
        /// </summary>
        private Matrix A1T_mul_b1;

        /// <summary>
        /// Complete matrix A1^T*A1;
        /// </summary>
        private Matrix A1T_mul_A1;

        /// <summary>
        /// Complete matrix A2^T*A2;
        /// </summary>
        private Matrix A2T_mul_A2;

        /// <summary>
        /// Record the rotation of each side and apply this rotation to each vertex after obtaining v'
        /// </summary>
        private List<Matrix> G_ckskList;

        /// <summary>
        /// Each edge is calculated only once, and the adjacency list is used to record whether the edge has been visited
        /// </summary>
        private HashSet<int> edgeVisited;

        /// <summary>
        /// Records the neighbor vertices of each edge. The neighbor vertices are defined as follows
        /// vl---vj
        ///  \   / \
        ///   \ /   \
        ///    vi---vr
        /// </summary>
        private Dictionary<int, EdgeNeighbors> edgeNeighborVertIndex;

        #endregion ----------------------------------------------


        #region -------- Mesh Info ----------------
        /// <summary>
        /// Records the two-dimensional coordinates of the vertices
        /// Index Indicates the ID of a vertex
        /// </summary>
        public List<Vector2> vertList;

        /// <summary>
        /// Record the mesh vertex ID corresponding to each triangle vertex
        /// The three vertex ids of each triangle are recorded in xyz of a Vector3Int
        /// </summary>
        private List<Vector3Int> triangleToVertList;


        /// <summary>
        /// The xy of Vector3 represents the coordinates of the control point,
        /// and z represents the subscript of the triangle in the triangleToVertList where the 
        /// current control point with subscript i resides
        /// </summary>
        [SerializeField] public List<Vector3> handleAtTriangle;


        /// <summary>
        /// Mesh Center.
        /// </summary>
        public Vector2 meshCenter;


        #endregion ----------------------------------------------

        #region -------------- Drwline Info ---------------

        /// <summary>
        /// List which contains all drawline objects.
        /// </summary>
        [SerializeField] private List<LineRenderer> linePrefabs;

        /// <summary>
        /// the position of every drawline samplers.
        /// </summary>
        [SerializeField] private List<Vector4> SamplerInterpolation;

        #endregion -----------------------------------------


        #region -------- Construction ---------

        /// <summary>
        /// Constructor - Generates a specific grid based on a given list of triangles and vertices
        /// </summary>
        /// <param name="pwCS">compute shader</param>
        /// <param name="delaunayTriangles">delaunay triangle vertices' infomations</param>
        /// <param name="delaunayVerts">all vertices infomations</param>
        public PuppetDrawlineToolGPU(ComputeShader pwCS, ref List<Vector3Int> delaunayTriangles, ref List<Vector2> delaunayVerts,
                                     ref List<LineRenderer> linePrefabs, int lineSamplePosCount)
        {
            edgeNeighborVertIndex = new Dictionary<int, EdgeNeighbors>();
            edgeVisited = new HashSet<int>();


            handleAtTriangle = new List<Vector3>();
            G_ckskList = new List<Matrix>();


            vertList = delaunayVerts;
            triangleToVertList = delaunayTriangles;

            this.linePrefabs = linePrefabs;
            this.SamplerInterpolation = new List<Vector4>(lineSamplePosCount);


            RegisterDrawlines();


            meshCenter = Vector2.zero;
            foreach (Vector2 v in vertList)
            {
                meshCenter += v;
            }
            meshCenter /= vertList.Count;

            for (int i = 0; i < vertList.Count; ++i)
            {
                vertList[i] -= meshCenter;
            }

            PuppetWarpComputeShader_Init(pwCS);

        }


        /// <summary>
        /// Init ComputeShader
        /// </summary>
        /// <param name="pwCS"></param>
        private void PuppetWarpComputeShader_Init(ComputeShader pwCS)
        {
            puppetWarpCS = pwCS;

            kernel_Matrix_Multiply = puppetWarpCS.FindKernel("Matrix_Multiply");
            kernel_Matrix_TransposeMultiply = puppetWarpCS.FindKernel("Matrix_TransposeMultiply");

            kernel_Matrix_GaussianElimination_CalEachRowDiv = puppetWarpCS.FindKernel("Matrix_GaussianElimination_CalEachRowDiv");
            kernel_Matrix_GaussianElimination_Elimination = puppetWarpCS.FindKernel("Matrix_GaussianElimination_Elimination");

            //kernel_Matrix_Solver_GetXValue = puppetWarpCS.FindKernel("Matrix_Solver_GetXValue");
            kernel_Matrix_UpperTriangleMatrix_Solver = puppetWarpCS.FindKernel("Matrix_UpperTriangleMatrix_Solver");
            kernel_Matrix_LowerTriangleMatrix_Solver = puppetWarpCS.FindKernel("Matrix_LowerTriangleMatrix_Solver");

            kernel_Matrix_LDLT_CalDii = puppetWarpCS.FindKernel("Matrix_LDLT_CalDii");
            kernel_Matrix_LDLT = puppetWarpCS.FindKernel("Matrix_LDLT");
            kernel_Matrix_LDLT_ClearLUpperTriValue = puppetWarpCS.FindKernel("Matrix_LDLT_ClearLUpperTriValue");

            kernel_Matrix_LDLT_MergeLDandLT = puppetWarpCS.FindKernel("Matrix_LDLT_MergeLDandLT");
            kernel_Matrix_LDLT_ResetDiag = puppetWarpCS.FindKernel("Matrix_LDLT_ResetDiag");
            kernel_Matrix_LDLT_UpperTriangle_Solver = puppetWarpCS.FindKernel("Matrix_LDLT_UpperTriangle_Solver");
            kernel_Matrix_LDLT_LowerTriangle_Solver = puppetWarpCS.FindKernel("Matrix_LDLT_LowerTriangle_Solver");
        }


        /// <summary>
        /// Register all stroke sampling points and record their barycentric coordinates within a triangle
        /// </summary>
        private void RegisterDrawlines()
        {
            for (int i = 0; i < linePrefabs.Count; i++)
            {
                for (int j = 0; j < linePrefabs[i].positionCount; j++)
                {
                    Vector3 pos = linePrefabs[i].GetPosition(j);
                    //Vector3 pos = new Vector2(v.x, v.y);

                    for (int k = 0; k < triangleToVertList.Count; ++k)
                    {
                        Vector3 v0 = vertList[triangleToVertList[k].x];
                        Vector3 v1 = vertList[triangleToVertList[k].y];
                        Vector3 v2 = vertList[triangleToVertList[k].z];


                        float Sdouble = Vector3.Cross(v1 - v0, v2 - v0).z;
                        float S0double = Vector3.Cross(pos - v1, pos - v2).z;
                        float S1double = Vector3.Cross(pos - v0, pos - v2).z;
                        float S2double = Vector3.Cross(pos - v1, pos - v0).z;

                        // Debug.LogError("V list -- v0: " + v0 + ", v1: " + v1 + ", v2: " + v2);

                        float fac0 = Mathf.Abs(S0double / Sdouble);
                        float fac1 = Mathf.Abs(S1double / Sdouble);
                        float fac2 = Mathf.Abs(S2double / Sdouble);

                        if (!Mathf.Approximately(1, fac0 + fac1 + fac2)) continue;


                        Vector4 vec4 = new Vector4(fac0, fac1, fac2, k);
                        SamplerInterpolation.Add(vec4);

                        break;
                    }

                }

            }

            Debug.Log("Lerp Count :" + SamplerInterpolation.Count);
        }


        /// <summary>
        /// Adjust Drawlines after the mesh is deformed.
        /// </summary>
        public void AdjustDrawlines()
        {
            int globalIndex = 0;

            for (int i = 0; i < linePrefabs.Count; i++)
            {
                for (int j = 0; j < linePrefabs[i].positionCount; j++)
                {
                    if (globalIndex >= SamplerInterpolation.Count)
                    {
                        Debug.Log("Index out of range: " + globalIndex);
                        break;
                    }

                    Vector4 interInfo = SamplerInterpolation[globalIndex];

                    Vector2 v0 = vertList[triangleToVertList[(int)interInfo.w].x];
                    Vector2 v1 = vertList[triangleToVertList[(int)interInfo.w].y];
                    Vector2 v2 = vertList[triangleToVertList[(int)interInfo.w].z];

                    // Debug.Log("v0: " + v0 + ", v1: " + v1 + ", v2: " + v2);

                    Vector2 pos = v0 * interInfo.x + v1 * interInfo.y + v2 * interInfo.z;
                    pos += meshCenter;

                    linePrefabs[i].SetPosition(j, new Vector3(pos.x, pos.y, 0));

                    globalIndex++;
                }

            }

            // Debug.Log("Global Index: " + globalIndex);
        }

        #endregion ----------------------------------------------

        /// <summary>
        /// Finds the vertex closest to the input position, and returns -1 if not found
        /// </summary>
        /// <param name="inputPosition"> Input position </param>
        /// <returns> The vertex closest to the input position. -1</returns>
        public int FindNearestVert(Vector2 inputPosition)
        {
            int ans = -1;
            float dis = 1e9f;
            inputPosition -= meshCenter;

            for (int i = 0; i < vertList.Count; ++i)
            {
                float tmp = (inputPosition - vertList[i]).sqrMagnitude;
                if (tmp < dis)
                {
                    dis = tmp;
                    ans = i;
                }
            }
            return ans;
        }


        /// <summary>
        /// Determine the effective interaction range between the pointer and handle, and the operation takes effect only within the range
        /// </summary>
        private readonly float epsilon = 0.25f;


        /// <summary>
        /// Searches for the handle closest to the input position whose distance is less than the threshold epsilon. 
        /// If the handle cannot be found or exceeds the threshold distance, -1 is returned
        /// </summary>
        /// <param name="inputPosition"> The current world coordinates of the pointer </param>
        /// <returns> ID of the vertex corresponding to the handle of epsilon that is closest to 
        /// the input location and less than the threshold. If the handle cannot be 
        /// found/exceeds the threshold distance, -1 is returned.</returns>
        public int FindNearestHandle(Vector2 inputPosition)
        {
            int ans = -1;
            float dis = 1e9f;
            inputPosition -= meshCenter;

            // check all handles.
            for (int i = 0; i < handleAtTriangle.Count; ++i)
            {
                float tmp = (inputPosition - new Vector2(handleAtTriangle[i].x, handleAtTriangle[i].y)).magnitude;

                if (tmp < dis)
                {
                    dis = tmp;
                    ans = i;
                }
            }

            if (dis > epsilon) ans = -1;

            return ans;
        }


        /// <summary>
        /// Move a handle point
        /// </summary>
        /// <param name="index">handle vertex ID</param>
        /// <param name="inputPosition"> World coordinates to move handle to </param>
        public void MoveHandle(int index, Vector2 inputPosition)
        {
            inputPosition -= meshCenter;

            if (index > handleAtTriangle.Count) return;
            handleAtTriangle[index] = new Vector3(inputPosition.x, inputPosition.y, handleAtTriangle[index].z);
        }


        /// <summary>
        /// Move entire mesh.
        /// </summary>
        /// <param name="inputPosition">input position</param>
        /// <param name="centerToInputpos">center of the mesh</param>
        public void MoveEntireMesh(Vector2 inputPosition, Vector2 centerToInputpos)
        {
            meshCenter = inputPosition - centerToInputpos;
        }


        /// <summary>
        /// Add a free position handle
        /// </summary>
        /// <param name="inputPosition"> World coordinates where you want to create handle </param>
        public void AddFreeHandle(Vector2 inputPosition)
        {
            inputPosition -= meshCenter;

            // Check each triangle to see if the handle is inside the current triangle (including the boundary)
            for (int i = 0; i < triangleToVertList.Count; ++i)
            {
                // Gets the vertex of the current triangle
                Vector2 v0 = vertList[triangleToVertList[i][0]];
                Vector2 v1 = vertList[triangleToVertList[i][1]];
                Vector2 v2 = vertList[triangleToVertList[i][2]];

                // Determine whether the handle is inside the triangle
                float Sdouble = Vector3.Cross(v1 - v0, v2 - v0).z;
                float S0double = Vector3.Cross(inputPosition - v1, inputPosition - v2).z;
                float S1double = Vector3.Cross(inputPosition - v0, inputPosition - v2).z;
                float S2double = Vector3.Cross(inputPosition - v1, inputPosition - v0).z;

                // Debug.LogError("V list -- v0: " + v0 + ", v1: " + v1 + ", v2: " + v2);

                float fac0 = Mathf.Abs(S0double / Sdouble);
                float fac1 = Mathf.Abs(S1double / Sdouble);
                float fac2 = Mathf.Abs(S2double / Sdouble);

                if (!Mathf.Approximately(1, fac0 + fac1 + fac2)) continue;


                handleAtTriangle.Add(new Vector3(inputPosition.x, inputPosition.y, i));

                Debug.LogWarning("Input Position: " + inputPosition);
                Debug.LogWarning("V0: " + v0 + ", V1: " + v1 + ", V2: " + v2);
                Debug.Log("ADD NEW HANDLE. Triangle ID: " + i);

                return;
            }
        }


        /// <summary>
        /// Removes the handle that is closest to the pointer and less than the specified threshold
        /// </summary>
        /// <param name="inputPosition"> World coordinates where the pointer is currently located </param>
        public void RemoveHandle(Vector2 inputPosition)
        {
            int ans = -1;
            float dis = 1e9f;
            inputPosition -= meshCenter;

            // Check all control points
            for (int i = 0; i < handleAtTriangle.Count; ++i)
            {
                // Calculate the distance from the control point to the mouse pointer
                float tmp = (inputPosition - new Vector2(handleAtTriangle[i].x, handleAtTriangle[i].y)).magnitude;

                // 
                if (tmp < dis)
                {
                    dis = tmp;
                    ans = i;
                }
            }

            if (ans == -1) return;

            if (dis <= epsilon) handleAtTriangle.RemoveAt(ans);
        }


        /// <summary>
        /// Number of current handle control points
        /// </summary>
        /// <returns> Number of current handle control points </returns>
        public int HandleCount()
        {
            // return handleList.Count;
            return handleAtTriangle.Count;
        }


        /// <summary>
        /// The correlation matrix is calculated from the current vertex and triangle sequence
        /// </summary>
        public void GenerateMatrixFromVertices()
        {
            if (vertList == null || triangleToVertList == null) return;

            int row = (vertList.Count + triangleToVertList.Count - 1) * 2;

            int col = vertList.Count * 2;

            A1_edge = new Matrix(row, col);
            b1_edge = new Matrix(row, 1);

            // record all neighbor infomations.
            for (int i = 0; i < triangleToVertList.Count; ++i)
            {
                Vector3Int t = triangleToVertList[i];

                for (int k = 0; k < 3; ++k)
                {
                    int v1 = t[k];
                    int v2 = t[(k + 1) % 3];
                    int v3 = t[(k + 2) % 3];
                    int hash = EdgeHashFunction(v1, v2);

                    if (!edgeNeighborVertIndex.ContainsKey(hash))
                    {
                        edgeNeighborVertIndex[hash] = new EdgeNeighbors();
                    }
                    edgeNeighborVertIndex[hash].AddNeighbor(v3);

                    // Debug.Log("edge neighbor size: " + edgeNeighborVertIndex[hash].Size);
                }
            }


            int globalRow = 0;

            // Fow every triangle
            for (int i = 0; i < triangleToVertList.Count; ++i)
            {
                Vector3Int t = triangleToVertList[i];

                for (int k = 0; k < 3; ++k)
                {
                    int v1 = t[k];
                    int v2 = t[(k + 1) % 3];
                    int hash = EdgeHashFunction(v1, v2);

                    if (edgeVisited.Contains(hash)) continue;

                    // Debug.Log("Hash Func: start = " + v1 + ", end = " + v2 + ", hashFunc = " + hash);

                    edgeVisited.Add(hash);

                    // There are two neighbor vertices around the current edge
                    if (edgeNeighborVertIndex[hash].Size > 1)
                    {
                        int v3 = edgeNeighborVertIndex[hash][0];
                        int v4 = edgeNeighborVertIndex[hash][1];

                        float[] vx = new float[] { vertList[v1].x, vertList[v2].x, vertList[v3].x, vertList[v4].x };
                        float[] vy = new float[] { vertList[v1].y, vertList[v2].y, vertList[v3].y, vertList[v4].y };

                        float ex = vx[1] - vx[0];
                        float ey = vy[1] - vy[0];

                        // æÿ’ÛGk
                        float[] gk = new float[] {vx[0], vy[0], vy[0], -vx[0],
                                              vx[1], vy[1], vy[1], -vx[1],
                                              vx[2], vy[2], vy[2], -vx[2],
                                              vx[3], vy[3], vy[3], -vx[3]};
                        Matrix Gk = new Matrix(gk, 8, 2);

                        //Debug.Log("Gk === ");
                        //Debug.Log(Gk);


                        Matrix G_cksk = (Gk.T * Gk).Inverse() * Gk.T;
                        //Debug.Log("Gk: \n" + Gk + "\n Inverse of :\n" + (Gk.T * Gk) + "\n  is \n" + (Gk.T * Gk).Inverse());
                        //Debug.LogWarning("G_cksk " + G_cksk);

                        G_ckskList.Add(G_cksk);


                        float[] e = new float[] { ex, ey, ey, -ex };
                        Matrix E = new Matrix(e, 2, 2);

                        Matrix H = -E * G_cksk;

                        H[0, 0] += -1;
                        H[1, 1] += -1;
                        H[0, 2] += 1;
                        H[1, 3] += 1;

                        // x
                        A1_edge[globalRow, v1 * 2] = H[0, 0];
                        A1_edge[globalRow, v1 * 2 + 1] = H[0, 1];
                        A1_edge[globalRow, v2 * 2] = H[0, 2];
                        A1_edge[globalRow, v2 * 2 + 1] = H[0, 3];
                        A1_edge[globalRow, v3 * 2] = H[0, 4];
                        A1_edge[globalRow, v3 * 2 + 1] = H[0, 5];
                        A1_edge[globalRow, v4 * 2] = H[0, 6];
                        A1_edge[globalRow, v4 * 2 + 1] = H[0, 7];


                        // y
                        A1_edge[globalRow + 1, v1 * 2] = H[1, 0];
                        A1_edge[globalRow + 1, v1 * 2 + 1] = H[1, 1];
                        A1_edge[globalRow + 1, v2 * 2] = H[1, 2];
                        A1_edge[globalRow + 1, v2 * 2 + 1] = H[1, 3];
                        A1_edge[globalRow + 1, v3 * 2] = H[1, 4];
                        A1_edge[globalRow + 1, v3 * 2 + 1] = H[1, 5];
                        A1_edge[globalRow + 1, v4 * 2] = H[1, 6];
                        A1_edge[globalRow + 1, v4 * 2 + 1] = H[1, 7];

                    }
                    else   // When this edge is a grid boundary, there is only one neighbor node
                    {
                        int v3 = edgeNeighborVertIndex[hash][0];

                        float[] vx = new float[] { vertList[v1].x, vertList[v2].x, vertList[v3].x };
                        float[] vy = new float[] { vertList[v1].y, vertList[v2].y, vertList[v3].y };

                        float ex = vx[1] - vx[0];
                        float ey = vy[1] - vy[0];

                        float[] gk = new float[] {vx[0], vy[0], vy[0], -vx[0],
                                              vx[1], vy[1], vy[1], -vx[1],
                                              vx[2], vy[2], vy[2], -vx[2]};
                        Matrix Gk = new Matrix(gk, 6, 2);

                        Matrix G_cksk = (Gk.T * Gk).Inverse() * Gk.T;
                        // Debug.Log("Gk: \n" + Gk + "\n Inverse of :\n" + (Gk.T * Gk) + "\n  is \n" + (Gk.T * Gk).Inverse());

                        //Debug.LogWarning("G_cksk " + G_cksk);

                        G_ckskList.Add(G_cksk);

                        float[] e = new float[] { ex, ey, ey, -ex };
                        Matrix E = new Matrix(e, 2, 2);

                        Matrix H = -E * G_cksk;

                        H[0, 0] += -1;
                        H[1, 1] += -1;
                        H[0, 2] += 1;
                        H[1, 3] += 1;

                        // x
                        A1_edge[globalRow, v1 * 2] = H[0, 0];
                        A1_edge[globalRow, v1 * 2 + 1] = H[0, 1];
                        A1_edge[globalRow, v2 * 2] = H[0, 2];
                        A1_edge[globalRow, v2 * 2 + 1] = H[0, 3];
                        A1_edge[globalRow, v3 * 2] = H[0, 4];
                        A1_edge[globalRow, v3 * 2 + 1] = H[0, 5];

                        // y
                        A1_edge[globalRow + 1, v1 * 2] = H[1, 0];
                        A1_edge[globalRow + 1, v1 * 2 + 1] = H[1, 1];
                        A1_edge[globalRow + 1, v2 * 2] = H[1, 2];
                        A1_edge[globalRow + 1, v2 * 2 + 1] = H[1, 3];
                        A1_edge[globalRow + 1, v3 * 2] = H[1, 4];
                        A1_edge[globalRow + 1, v3 * 2 + 1] = H[1, 5];

                    }

                    globalRow += 2;
                }
            }

            //Debug.LogError("----- A1 error -----");
            //Debug.LogError(A1_edge);

            globalRow = 0;
            A2_edge = new Matrix(row, col);
            b2_edge = new Matrix(row, 1);

            edgeVisited.Clear();
            for (int i = 0; i < triangleToVertList.Count; ++i)
            {
                Vector3Int t = triangleToVertList[i];

                for (int k = 0; k < 3; ++k)
                {
                    int v1 = t[k];
                    int v2 = t[(k + 1) % 3];
                    int hash = EdgeHashFunction(v1, v2);

                    if (edgeVisited.Contains(hash)) continue;

                    edgeVisited.Add(hash);

                    // x
                    A2_edge[globalRow, v1 * 2] = -1;
                    A2_edge[globalRow, v2 * 2] = 1;
                    b2_edge[globalRow, 0] = vertList[v2].x - vertList[v1].x;

                    ++globalRow;

                    // y
                    A2_edge[globalRow, v1 * 2 + 1] = -1;
                    A2_edge[globalRow, v2 * 2 + 1] = 1;
                    b2_edge[globalRow, 0] = vertList[v2].y - vertList[v1].y;

                    ++globalRow;
                }
            }


            //Matrix A1_edge_T = A1_edge.T;
            Matrix_TransposeMultiplyGPU(ref A1_edge, ref A1_edge, ref A1_edgeT_mul_A1_edge);

            //Matrix A2_edge_T = A2_edge.T;
            Matrix_TransposeMultiplyGPU(ref A2_edge, ref A2_edge, ref A2_edgeT_mul_A2_edge);

            Matrix_TransposeMultiplyGPU(ref A1_edge, ref b1_edge, ref A1_edgeT_mul_b1_edge);

            //A1_edgeT_mul_A1_edge = A1_edge.T * A1_edge;
            //A2_edgeT_mul_A2_edge = A2_edge.T * A2_edge;
            //A1_edgeT_mul_b1_edge = A1_edge.T * b1_edge;

            // Debug.LogError("[GPU] A1edgeT * A1edge: " + A1_edgeT_mul_A1_edge.ToString());

            b2_edge_prime = new Matrix(b2_edge.rows, b2_edge.cols);
        }


        // -------------------------------------------------------------------------------
        /// <summary>
        /// Update the relevant matrix based on the current handle Settings
        /// </summary>
        public void UpdateHandleConstraints()
        {
            int m = handleAtTriangle.Count * 2;
            int n = vertList.Count * 2;

            A_con = new Matrix(m, n);
            b_con = new Matrix(m, 1);

            // Process the parts of matrix A and vector b that are related to constraint based on the handle collection
            int globalRow = 0;
            for (int i = 0; i < handleAtTriangle.Count; ++i)
            {
                int id0 = triangleToVertList[(int)handleAtTriangle[i].z][0];
                int id1 = triangleToVertList[(int)handleAtTriangle[i].z][1];
                int id2 = triangleToVertList[(int)handleAtTriangle[i].z][2];

                Vector3 handlePos = handleAtTriangle[i];

                Vector3 v0 = vertList[id0];
                Vector3 v1 = vertList[id1];
                Vector3 v2 = vertList[id2];

                float Sdouble = Vector3.Cross(v1 - v0, v2 - v0).z;
                float S0double = Vector3.Cross(handlePos - v1, handlePos - v2).z;
                float S1double = Vector3.Cross(handlePos - v0, handlePos - v2).z;
                float S2double = Vector3.Cross(handlePos - v1, handlePos - v0).z;

                // Debug.LogError("V list -- v0: " + v0 + ", v1: " + v1 + ", v2: " + v2);

                float fac0 = Mathf.Abs(S0double / Sdouble);
                float fac1 = Mathf.Abs(S1double / Sdouble);
                float fac2 = Mathf.Abs(S2double / Sdouble);

                Debug.LogWarning("fac0: " + fac0 + ", fac1: " + fac1 + ", fac2: " + fac2);

                A_con[globalRow, id0 * 2] = w * fac0;
                A_con[globalRow + 1, id0 * 2 + 1] = w * fac0;
                A_con[globalRow, id1 * 2] = w * fac1;
                A_con[globalRow + 1, id1 * 2 + 1] = w * fac1;
                A_con[globalRow, id2 * 2] = w * fac2;
                A_con[globalRow + 1, id2 * 2 + 1] = w * fac2;

                globalRow += 2;
            }

            A1 = new Matrix(true, A1_edge, A_con);
            A2 = new Matrix(true, A2_edge, A_con);

            b1 = new Matrix(true, b1_edge, b_con);
            b2 = new Matrix(true, b2_edge, b_con);


            //Matrix AcT = A_con.Transpose();
            Matrix AcTAc = null;
            Matrix_TransposeMultiplyGPU(ref A_con, ref A_con, ref AcTAc);

            A1T_mul_A1 = A1_edgeT_mul_A1_edge + AcTAc;
            A2T_mul_A2 = A2_edgeT_mul_A2_edge + AcTAc;

            //Debug.LogWarning("A1_consT*A1_cons ");
            //Debug.LogWarning(A_con);

            /*
            Debug.LogWarning("A1 " + A1);
            Debug.LogWarning("A2 " + A2);
            Debug.LogWarning("b1 " + b1);
            Debug.LogWarning("b2 " + b2);
            Debug.LogWarning("A1T*A1 " + A1T_mul_A1);
            Debug.LogWarning("A2T*A2 " + A2T_mul_A2);
            */
        }


        /// <summary>
        /// Solve the position of each vertex after deformation
        /// </summary>
        public void SolveVerticesPositions()
        {
            if (handleAtTriangle.Count < 1) return;

            // After the handle position changes, the b vector section about the constraint is also modified based on the handle position
            int globalRow = 0;
            for (int i = 0; i < handleAtTriangle.Count; ++i)
            {
                b_con[globalRow, 0] = w * handleAtTriangle[i].x;
                b_con[globalRow + 1, 0] = w * handleAtTriangle[i].y;

                globalRow += 2;
            }

            // Debug.LogWarning("--- b_constraint ----- ");
            // Debug.LogWarning(b_con);


            //Matrix AcT = A_con.Transpose();
            Matrix AcTbc = null;
            Matrix_TransposeMultiplyGPU(ref A_con, ref b_con, ref AcTbc);

            A1T_mul_b1 = A1_edgeT_mul_b1_edge + AcTbc;


            Matrix Vprime = null;
            // Matrix.EquationSolver_LDLT(ref A1T_mul_A1, out Vprime, ref A1T_mul_b1);
            // Matrix_GaussianTriangleSolver(ref A1T_mul_A1, ref Vprime, ref A1T_mul_b1);


            // GPU_EquationSolver_LDLT(ref A1T_mul_A1, ref Vprime, ref A1T_mul_b1);
            GPU_EquationSolver_LDLT_InOne(ref A1T_mul_A1, ref Vprime, ref A1T_mul_b1);
            //Debug.Log("V prime: \n" + Vprime.ToString());


            edgeVisited.Clear();

            globalRow = 0;


            for (int i = 0; i < triangleToVertList.Count; i++)
            {
                Vector3Int t = triangleToVertList[i];

                for (int k = 0; k < 3; ++k)
                {
                    int v1 = t[k];
                    int v2 = t[(k + 1) % 3];
                    int hash = EdgeHashFunction(v1, v2);

                    if (edgeVisited.Contains(hash)) continue;

                    edgeVisited.Add(hash);

                    // There are two neighbor vertices around the current edge
                    if (edgeNeighborVertIndex[hash].Size > 1)
                    {
                        int v3 = edgeNeighborVertIndex[hash][0];
                        int v4 = edgeNeighborVertIndex[hash][1];

                        float[] vprimex = new float[] { Vprime[v1 * 2, 0], Vprime[v2 * 2, 0], Vprime[v3 * 2, 0], Vprime[v4 * 2, 0] };
                        float[] vprimey = new float[] { Vprime[v1 * 2 + 1, 0], Vprime[v2 * 2 + 1, 0], Vprime[v3 * 2 + 1, 0], Vprime[v4 * 2 + 1, 0] };


                        float[] varr = new float[] {vprimex[0], vprimey[0],
                                                vprimex[1], vprimey[1],
                                                vprimex[2], vprimey[2],
                                                vprimex[3], vprimey[3]};
                        Matrix vprime = new Matrix(varr, 8, 1);


                        Matrix Tk = G_ckskList[globalRow / 2] * vprime;

                        // normalize ck sk
                        float ck = Tk[0, 0];
                        float sk = Tk[1, 0];
                        float normalizeVal = Mathf.Sqrt(ck * ck + sk * sk);

                        //Debug.LogWarning("normalizeVal: " + normalizeVal);
                        ck /= (0.0001f + normalizeVal);
                        sk /= (0.0001f + normalizeVal);

                        float ekx = b2_edge[globalRow, 0];
                        float eky = b2_edge[globalRow + 1, 0];

                        b2_edge_prime[globalRow, 0] = ck * ekx + sk * eky;
                        b2_edge_prime[globalRow + 1, 0] = -sk * ekx + ck * eky;

                    }
                    else   // Only have one neighbor vertex
                    {
                        int v3 = edgeNeighborVertIndex[hash][0];

                        float[] vprimex = new float[] { Vprime[v1 * 2, 0], Vprime[v2 * 2, 0], Vprime[v3 * 2, 0] };
                        float[] vprimey = new float[] { Vprime[v1 * 2 + 1, 0], Vprime[v2 * 2 + 1, 0], Vprime[v3 * 2 + 1, 0] };


                        float[] varr = new float[] {vprimex[0], vprimey[0],
                                                vprimex[1], vprimey[1],
                                                vprimex[2], vprimey[2]};
                        Matrix vprime = new Matrix(varr, 6, 1);


                        Matrix Tk = G_ckskList[globalRow / 2] * vprime;

                        // normalize ck sk 
                        float ck = Tk[0, 0];
                        float sk = Tk[1, 0];
                        float normalizeVal = Mathf.Sqrt(ck * ck + sk * sk);
                        ck /= (0.0001f + normalizeVal);
                        sk /= (0.0001f + normalizeVal);

                        float ekx = b2_edge[globalRow, 0];
                        float eky = b2_edge[globalRow + 1, 0];

                        b2_edge_prime[globalRow, 0] = ck * ekx + sk * eky;
                        b2_edge_prime[globalRow + 1, 0] = -sk * ekx + ck * eky;

                    }

                    globalRow += 2;
                }
            }

            b2 = new Matrix(true, b2_edge_prime, b_con);
            //Debug.Log("b2 : \n" + b2.ToString());


            Matrix A2T_mul_b2 = null;
            // Matrix A2T = A2.T;

            // Matrix_MultiplyGPU(ref A2T, ref b2, ref A2T_mul_b2);
            Matrix_TransposeMultiplyGPU(ref A2, ref b2, ref A2T_mul_b2);

            // Debug.Log("A2T * b2: \n" + A2T_mul_b2);

            Matrix Vfinal = null;
            // Debug.Log("A2.T * A2: \n" + A2T_mul_A2.ToString());


            // GPU_EquationSolver_LDLT(ref A2T_mul_A2, ref Vfinal, ref A2T_mul_b2);
            GPU_EquationSolver_LDLT_InOne(ref A2T_mul_A2, ref Vfinal, ref A2T_mul_b2);
            //Debug.Log("V final: \n" + Vfinal.ToString());


            //string str = "";
            for (int i = 0; i < vertList.Count; ++i)
            {
                // Debug.Log("X " + Vfinal[i / 2, 0] + " Y" + Vfinal[i / 2 + 1, 0]);
                // if (Vfinal[i * 2, 0] == 0 || Vfinal[i * 2 + 1, 0] == 0) continue;

                if (float.IsNaN(Vfinal[i * 2, 0]) || float.IsNaN(Vfinal[i * 2 + 1, 0]))
                {
                    throw new Exception("LDLT TWO -- Encount NAN ISSUE.");
                }

                vertList[i] = new Vector2(Vfinal[i * 2, 0], Vfinal[i * 2 + 1, 0]);
                //str += vertList[i].ToString() + ", ";
            }

            // Debug.LogWarning(str);
            // Debug.LogError("V Final: \n" + Vfinal.ToString());
        }



        /// <summary>
        /// Store the hash function used by the side
        /// The number of vertices should not exceed 1e4, otherwise this hash function will fail
        /// </summary>
        /// <param name="startID"> A vertex of an edge </param>
        /// <param name="endID"> Another vertex of the edge </param>
        /// <returns> The hash result of this side </returns>
        public int EdgeHashFunction(int startID, int endID)
        {
            return 10000 * Mathf.Min(startID, endID) + Mathf.Max(startID, endID);
        }


        /// <summary>
        /// GPU method: Solve the product of two matrices A*B = Res
        /// </summary>
        /// <param name="A"> Matrix A</param>
        /// <param name="B"> Matrix B</param>
        /// <param name="Res"> Matrix Res</param>
        public void Matrix_MultiplyGPU(ref Matrix A, ref Matrix B, ref Matrix Res)
        {
            int Arow = A.mat.GetLength(0);
            int Acol = A.mat.GetLength(1);
            int Brow = B.mat.GetLength(0);
            int Bcol = B.mat.GetLength(1);

            if (Acol != Brow)
            {
                Debug.LogError("Matirx Multiply: Dimension not match.");
                Res = null;
                return;
            }

            int Crow = Arow, Ccol = Bcol;

            //Debug.LogWarning("A size " + Arow + ", " + Acol + " , val " + A[0, 0]);
            //Debug.LogWarning("B size " + Brow + ", " + Bcol + " , val " + B[0, 0]);

            var ABuf = new ComputeBuffer(Arow * Acol, Marshal.SizeOf(typeof(float)));
            var BBuf = new ComputeBuffer(Brow * Bcol, Marshal.SizeOf(typeof(float)));
            var CBuf = new ComputeBuffer(Crow * Ccol, Marshal.SizeOf(typeof(float)));

            ABuf.SetData(A.mat);
            BBuf.SetData(B.mat);


            puppetWarpCS.SetBuffer(kernel_Matrix_Multiply, "M1", ABuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_Multiply, "M2", BBuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_Multiply, "M3", CBuf);


            puppetWarpCS.SetInt("M1row", Arow);
            puppetWarpCS.SetInt("M1col", Acol);
            puppetWarpCS.SetInt("M2row", Brow);
            puppetWarpCS.SetInt("M2col", Bcol);
            puppetWarpCS.SetInt("M3row", Crow);
            puppetWarpCS.SetInt("M3col", Ccol);

            // Dispatch
            puppetWarpCS.Dispatch(kernel_Matrix_Multiply, Mathf.FloorToInt((Ccol - 1) / 16) + 1, Mathf.FloorToInt((Crow - 1) / 16) + 1, 1);


            float[,] C = new float[Crow, Ccol];
            CBuf.GetData(C);

            Res = new Matrix(C);

            //Debug.LogWarning("B size " + Crow + ", " + Ccol + " , val " + C[0, 0]);

            ABuf.Release();
            BBuf.Release();
            CBuf.Release();
        }


        /// <summary>
        /// GPU method: The matrix is calculated as A.T*B = Res, but the input is matrix A instead of A.T
        /// Since the CPU calculation of A.T can cause high overhead, this method can be passed directly to A, 
        /// and the transpose operation is automatically performed within the method
        /// </summary>
        /// <param name="A">m*n matrix </param>
        /// <param name="B">m*k matrix </param>
        /// <param name="Res">n*k matrix, note that the result is A.T*B</param>
        public void Matrix_TransposeMultiplyGPU(ref Matrix A, ref Matrix B, ref Matrix Res)
        {
            int Arow = A.mat.GetLength(0);
            int Acol = A.mat.GetLength(1);
            int Brow = B.mat.GetLength(0);
            int Bcol = B.mat.GetLength(1);

            if (Arow != Brow)
            {
                Debug.LogError("Matirx Multiply: Dimension not match.");
                Res = null;
                return;
            }

            int Crow = Acol, Ccol = Bcol;

            //Debug.LogWarning("A size " + Arow + ", " + Acol + " , val " + A[0, 0]);
            //Debug.LogWarning("B size " + Brow + ", " + Bcol + " , val " + B[0, 0]);

            var ABuf = new ComputeBuffer(Arow * Acol, Marshal.SizeOf(typeof(float)));
            var BBuf = new ComputeBuffer(Brow * Bcol, Marshal.SizeOf(typeof(float)));
            var CBuf = new ComputeBuffer(Crow * Ccol, Marshal.SizeOf(typeof(float)));

            ABuf.SetData(A.mat);
            BBuf.SetData(B.mat);


            puppetWarpCS.SetBuffer(kernel_Matrix_TransposeMultiply, "M1", ABuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_TransposeMultiply, "M2", BBuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_TransposeMultiply, "M3", CBuf);


            puppetWarpCS.SetInt("M1row", Arow);
            puppetWarpCS.SetInt("M1col", Acol);
            puppetWarpCS.SetInt("M2row", Brow);
            puppetWarpCS.SetInt("M2col", Bcol);
            puppetWarpCS.SetInt("M3row", Crow);
            puppetWarpCS.SetInt("M3col", Ccol);

            puppetWarpCS.Dispatch(kernel_Matrix_TransposeMultiply, Mathf.FloorToInt((Ccol - 1) / 16) + 1, Mathf.FloorToInt((Crow - 1) / 16) + 1, 1);


            float[,] C = new float[Crow, Ccol];
            CBuf.GetData(C);

            Res = new Matrix(C);

            //Debug.LogWarning("B size " + Crow + ", " + Ccol + " , val " + C[0, 0]);

            ABuf.Release();
            BBuf.Release();
            CBuf.Release();
        }


        /// <summary>
        /// GPU method: Gaussian elimination method to solve the matrix equation A*x = b solution x
        /// </summary>
        /// <param name="A"> Matrix A</param>
        /// <param name="x"> Solution vector x</param>
        /// <param name="b"> Vector b</param>
        public void Matrix_GaussianTriangleSolver(ref Matrix A, ref Matrix x, ref Matrix b)
        {
            //sw.Restart();

            int Arow = A.mat.GetLength(0);
            int Acol = A.mat.GetLength(1);
            int Brow = b.mat.GetLength(0);
            int Bcol = b.mat.GetLength(1);

            if (Arow != Acol || Bcol != 1)
            {
                Debug.LogError("Matirx Linear Equation Solver.");
                return;
            }

            //Debug.LogWarning("A size " + Arow + ", " + Acol + " , val " + A[0, 0]);
            //Debug.LogWarning("B size " + Brow + ", " + Bcol + " , val " + B[0, 0]);

            var ABuf = new ComputeBuffer(Arow * Acol, Marshal.SizeOf(typeof(float)));
            var bBuf = new ComputeBuffer(Brow * Bcol, Marshal.SizeOf(typeof(float)));
            var DivTmpBuf = new ComputeBuffer(Arow * 1, Marshal.SizeOf(typeof(float)));
            var xBuf = new ComputeBuffer(Acol * 1, Marshal.SizeOf(typeof(float)));

            ABuf.SetData(A.mat);
            bBuf.SetData(b.mat);


            puppetWarpCS.SetBuffer(kernel_Matrix_GaussianElimination_CalEachRowDiv, "A", ABuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_GaussianElimination_CalEachRowDiv, "divTmp", DivTmpBuf);

            puppetWarpCS.SetBuffer(kernel_Matrix_GaussianElimination_Elimination, "A", ABuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_GaussianElimination_Elimination, "b", bBuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_GaussianElimination_Elimination, "divTmp", DivTmpBuf);

            puppetWarpCS.SetBuffer(kernel_Matrix_UpperTriangleMatrix_Solver, "A", ABuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_UpperTriangleMatrix_Solver, "X", xBuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_UpperTriangleMatrix_Solver, "b", bBuf);



            puppetWarpCS.SetInt("Arow", Arow);
            puppetWarpCS.SetInt("Acol", Acol);
            puppetWarpCS.SetInt("brow", Brow);
            puppetWarpCS.SetInt("bcol", Bcol);
            puppetWarpCS.SetInt("xrow", Acol);
            puppetWarpCS.SetInt("xcol", 1);
            puppetWarpCS.SetInt("divSize", Arow);


            for (int i = 0; i < Arow; ++i)
            {
                puppetWarpCS.SetInt("currentOperatingRow", i);

                // Dispatch
                puppetWarpCS.Dispatch(kernel_Matrix_GaussianElimination_CalEachRowDiv, 1, Mathf.FloorToInt((Arow - 1) / 16) + 1, 1);
                puppetWarpCS.Dispatch(kernel_Matrix_GaussianElimination_Elimination, Mathf.FloorToInt((Acol - 1) / 16) + 1, Mathf.FloorToInt((Arow - 1) / 16) + 1, 1);
            }


            /*
            float[,] bb = new float[Brow, Bcol];
            bBuf.GetData(bb);
            Matrix Mbb = new Matrix(bb);
            Debug.Log("Mat b value: \n" + Mbb.ToString());
            */

            for (int i = Arow - 1; i >= 0; --i)
            {
                puppetWarpCS.SetInt("currentOperatingRow", i);
                puppetWarpCS.Dispatch(kernel_Matrix_UpperTriangleMatrix_Solver, 1, Mathf.FloorToInt((Arow - 1) / 16) + 1, 1);
            }


            //float[,] divVal = new float[Arow, 1];
            //DivTmpBuf.GetData(divVal);

            /*
            float[,] AA = new float[Arow, Acol];
            ABuf.GetData(AA);

            Matrix MAT = new Matrix(divVal);
            Debug.Log("MAT div value: \n" + MAT.ToString());

            Matrix MAA = new Matrix(AA);
            Debug.Log("Mat A value: \n" + MAA.ToString());

            float[,] xx = new float[Acol, 1];
            xBuf.GetData(xx);
            Matrix Mx = new Matrix(xx);
            Debug.Log("answer X value: \n" + Mx.ToString());
            */

            float[,] xx = new float[Acol, 1];
            xBuf.GetData(xx);
            x = new Matrix(xx);

            //Debug.LogWarning("B size " + Crow + ", " + Ccol + " , val " + C[0, 0]);

            ABuf.Release();
            bBuf.Release();
            DivTmpBuf.Release();
            xBuf.Release();

            //sw.Stop();
            //var time = sw.Elapsed.TotalMilliseconds;
            //Debug.Log("[GPU] Matrix up-Triangled || total mill seconds: " + time);
        }


        /// <summary>
        /// GPU method: Solve x of the upper triangular matrix equation ATA*x=b
        /// </summary>
        /// <param name="ATA"> Upper triangular matrix ATA</param>
        /// <param name="x"> Solution vector x</param>
        /// <param name="b"> Vector b</param>
        public void Matrix_UpperTriangleSolver(ref Matrix ATA, ref Matrix x, ref Matrix b)
        {
            //sw.Restart();

            int Arow = ATA.mat.GetLength(0);
            int Acol = ATA.mat.GetLength(1);
            int Brow = b.mat.GetLength(0);
            int Bcol = b.mat.GetLength(1);

            if (Arow != Acol || Bcol != 1)
            {
                Debug.LogError("Matirx Linear Equation Solver.");
                return;
            }

            //Debug.LogWarning("A size " + Arow + ", " + Acol + " , val " + A[0, 0]);
            //Debug.LogWarning("B size " + Brow + ", " + Bcol + " , val " + B[0, 0]);

            var ABuf = new ComputeBuffer(Arow * Acol, Marshal.SizeOf(typeof(float)));
            var bBuf = new ComputeBuffer(Brow * Bcol, Marshal.SizeOf(typeof(float)));
            var xBuf = new ComputeBuffer(Acol * 1, Marshal.SizeOf(typeof(float)));

            ABuf.SetData(ATA.mat);
            bBuf.SetData(b.mat);


            puppetWarpCS.SetBuffer(kernel_Matrix_UpperTriangleMatrix_Solver, "A", ABuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_UpperTriangleMatrix_Solver, "X", xBuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_UpperTriangleMatrix_Solver, "b", bBuf);


            puppetWarpCS.SetInt("Arow", Arow);
            puppetWarpCS.SetInt("Acol", Acol);
            puppetWarpCS.SetInt("brow", Brow);
            puppetWarpCS.SetInt("bcol", Bcol);
            puppetWarpCS.SetInt("xrow", Acol);
            puppetWarpCS.SetInt("xcol", 1);

            for (int i = Arow - 1; i >= 0; --i)
            {
                puppetWarpCS.SetInt("currentOperatingRow", i);
                puppetWarpCS.Dispatch(kernel_Matrix_UpperTriangleMatrix_Solver, 1, Mathf.FloorToInt((Arow - 1) / 16) + 1, 1);
            }

            float[,] xx = new float[Acol, 1];
            xBuf.GetData(xx);
            x = new Matrix(xx);

            ABuf.Release();
            bBuf.Release();
            xBuf.Release();

            //sw.Stop();
            //var time = sw.Elapsed.TotalMilliseconds;
            //Debug.Log("[GPU] Upper-Triangle Solver || total mill seconds: " + time);
        }


        /// <summary>
        /// GPU method: Solve the solution x of the lower triangular matrix equation ATA*x=b
        /// </summary>
        /// <param name="ATA"> Lower triangular matrix ATA</param>
        /// <param name="x"> Solution vector x</param>
        /// <param name="b"> Vector b</param>
        public void Matrix_LowerTriangleSolver(ref Matrix ATA, ref Matrix x, ref Matrix b)
        {
            //sw.Restart();

            int Arow = ATA.mat.GetLength(0);
            int Acol = ATA.mat.GetLength(1);
            int Brow = b.mat.GetLength(0);
            int Bcol = b.mat.GetLength(1);

            if (Arow != Acol || Bcol != 1)
            {
                Debug.LogError("Matirx Linear Equation Solver.");
                return;
            }

            //Debug.LogWarning("A size " + Arow + ", " + Acol + " , val " + A[0, 0]);
            //Debug.LogWarning("B size " + Brow + ", " + Bcol + " , val " + B[0, 0]);

            var ABuf = new ComputeBuffer(Arow * Acol, Marshal.SizeOf(typeof(float)));
            var bBuf = new ComputeBuffer(Brow * Bcol, Marshal.SizeOf(typeof(float)));
            var xBuf = new ComputeBuffer(Acol * 1, Marshal.SizeOf(typeof(float)));

            ABuf.SetData(ATA.mat);
            bBuf.SetData(b.mat);


            puppetWarpCS.SetBuffer(kernel_Matrix_LowerTriangleMatrix_Solver, "A", ABuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_LowerTriangleMatrix_Solver, "X", xBuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_LowerTriangleMatrix_Solver, "b", bBuf);

            // puppetWarpCS.SetBuffer(kernel_Matrix_Solver_GetXValue, "A", ABuf);
            // puppetWarpCS.SetBuffer(kernel_Matrix_Solver_GetXValue, "X", xBuf);
            // puppetWarpCS.SetBuffer(kernel_Matrix_Solver_GetXValue, "b", bBuf);


            puppetWarpCS.SetInt("Arow", Arow);
            puppetWarpCS.SetInt("Acol", Acol);
            puppetWarpCS.SetInt("brow", Brow);
            puppetWarpCS.SetInt("bcol", Bcol);
            puppetWarpCS.SetInt("xrow", Acol);
            puppetWarpCS.SetInt("xcol", 1);

            for (int i = 0; i < Arow; ++i)
            {
                puppetWarpCS.SetInt("currentOperatingRow", i);

                puppetWarpCS.Dispatch(kernel_Matrix_LowerTriangleMatrix_Solver, 1, Mathf.FloorToInt((Arow - 1) / 16) + 1, 1);
            }


            float[,] xx = new float[Acol, 1];
            xBuf.GetData(xx);
            x = new Matrix(xx);

            ABuf.Release();
            bBuf.Release();
            xBuf.Release();

            //sw.Stop();
            //var time = sw.Elapsed.TotalMilliseconds;
            //Debug.Log("[GPU] Lower-Triangle Solver || total mill seconds: " + time);
        }


        /// <summary>
        /// GPU method: Parallelization implementation of LDLT decomposition
        /// </summary>
        /// <param name="ATA"> Symmetric matrix AT*A</param>
        /// <param name="x"> Vector x</param>
        /// <param name="b"> Coefficient vector b</param>
        public void GPU_EquationSolver_LDLT(ref Matrix ATA, ref Matrix x, ref Matrix b)
        {
            int m = ATA.rows;
            if (m <= 0 || ATA.cols != m || b.rows != m)
            {
                x = null;
                return;
            }

            string time = null;
            Matrix L = null, D = null;

            //sw.Restart();

            GPU_LDLT(ref ATA, ref L, ref D);

            // Debug.LogError(L*D);
            //sw.Stop();
            //var time = sw.Elapsed.TotalMilliseconds.ToString();
            //Debug.Log("[GPU] LDLT_Solver: LDLT || Total mill seconds: " + time);


            //sw.Restart();
            Matrix lower = null;
            Matrix upper = L.T;
            Matrix_MultiplyGPU(ref L, ref D, ref lower);


            //sw.Stop();
            //time = sw.Elapsed.TotalMilliseconds.ToString();
            //Debug.Log("[GPU] LDLT_Solver: Multiply L*D || Total mill seconds: " + time);

            Matrix Y = null;

            /*
            for (int i = 0; i < m; ++i)
            {
                float B = b[i, 0];
                for (int k = 0; k < i; ++k)
                {
                    B -= lower[i, k] * y[k, 0];
                }
                y[i, 0] = B / lower[i, i];
            }
            */

            //sw.Restart();
            Matrix_LowerTriangleSolver(ref lower, ref Y, ref b);

            //sw.Stop();
            //time = sw.Elapsed.TotalMilliseconds.ToString();
            //Debug.Log("[GPU] LDLT_Solver: Lower Triangle solver || Total mill seconds: " + time);
            //string str = "";
            //for (int i = 0; i < m; ++i) str += y[i].ToString() + ", ";
            //Debug.Log(str);


            /*
            for (int i = m - 1; i >= 0; --i)
            {
                float Y = y[i];
                for (int k = m - 1; k > i; --k)
                {
                    Y -= upper[i, k] * ans[k];
                }
                ans[i] = Y / upper[i, i];
            }
            */

            //sw.Restart();

            Matrix_UpperTriangleSolver(ref upper, ref x, ref Y);

            //sw.Stop();
            //time = sw.Elapsed.TotalMilliseconds.ToString();
            //Debug.Log("[GPU] LDLT_Solver: Upper Triangle solver || Total mill seconds: " + time);
        }


        /// <summary>
        /// GPU method: Parallelization implementation of LDLT decomposition
        /// </summary>
        /// <param name="A"> Symmetric positive definite matrix </param>
        /// <param name="L"> Triangular matrix </param>
        /// <param name="D"> Diagonal matrix </param>
        public void GPU_LDLT(ref Matrix A, ref Matrix L, ref Matrix D)
        {
            int m = A.rows, n = A.cols;
            if (m != n)
            {
                L = null;
                D = null;
                return;
            }

            //sw.Restart();

            float[] Dvec = new float[n];

            //Debug.LogWarning("A size " + Arow + ", " + Acol + " , val " + A[0, 0]);
            //Debug.LogWarning("B size " + Brow + ", " + Bcol + " , val " + B[0, 0]);

            var LBuf = new ComputeBuffer(m * n, Marshal.SizeOf(typeof(float)));
            var DBuf = new ComputeBuffer(n * 1, Marshal.SizeOf(typeof(float)));

            LBuf.SetData(A.mat);
            DBuf.SetData(Dvec);


            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT, "L", LBuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT, "D", DBuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT_CalDii, "L", LBuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT_CalDii, "D", DBuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT_ClearLUpperTriValue, "L", LBuf);


            puppetWarpCS.SetInt("Llen", n);

            puppetWarpCS.Dispatch(kernel_Matrix_LDLT_ClearLUpperTriValue, Mathf.FloorToInt((n - 1) / 16) + 1, Mathf.FloorToInt((n - 1) / 16) + 1, 1);

            for (int i = 0; i < n; ++i)
            {
                puppetWarpCS.SetInt("LDLTcurrentCol", i);

                puppetWarpCS.Dispatch(kernel_Matrix_LDLT_CalDii, 1, 1, 1);

                puppetWarpCS.SetBool("CalculateCurCol", true);

                puppetWarpCS.Dispatch(kernel_Matrix_LDLT, Mathf.FloorToInt((n - 1) / 16) + 1, Mathf.FloorToInt((n - 1) / 16) + 1, 1);

                puppetWarpCS.SetBool("CalculateCurCol", false);

                puppetWarpCS.Dispatch(kernel_Matrix_LDLT, Mathf.FloorToInt((n - 1) / 16) + 1, Mathf.FloorToInt((n - 1) / 16) + 1, 1);
            }

            puppetWarpCS.Dispatch(kernel_Matrix_LDLT_ClearLUpperTriValue, Mathf.FloorToInt((n - 1) / 16) + 1, Mathf.FloorToInt((n - 1) / 16) + 1, 1);


            float[,] Lmat = new float[m, n];
            LBuf.GetData(Lmat);
            L = new Matrix(Lmat);

            DBuf.GetData(Dvec);

            float[,] Dmat = new float[m, n];
            for (int i = 0; i < n; ++i)
            {
                Dmat[i, i] = Dvec[i];
            }
            D = new Matrix(Dmat);


            LBuf.Release();
            DBuf.Release();

            //Debug.LogWarning("A mat: \n" + A.ToString());
            //Debug.LogWarning("L mat: \n" + L.ToString());
            //Debug.LogWarning("D mat: \n" + D.ToString());

            //sw.Stop();
            //var time = sw.Elapsed.TotalMilliseconds;
            //Debug.Log("[GPU] LDLT || total mill seconds: " + time);


            /*
            L = new Matrix(n, n);
            D = new Matrix(n, n);

            for (int i = 0; i < n; ++i)
            {
                // type (1) : L_ij = {a_ij - SIGMA [k = 1 ~ (j - 1)] L_jk L_ik * * d_kk} / d_jj
                // Explanation: To calculate the L_ij (j<i) of column j in row i, you need to use all the L_ik to the 
                // left of column j in row i, all the L_jk to the left of column j in row j, and d_jj and all the d_kk above and to the left of column J
                // In short, to get the value of L_ij, you need to use all the elements in rows i and j to the left of column j, 
                // and d_jj and all the elements in its upper left corner

                // type (2) : d_ii = a_ii - SIGMA [k = 1 ~ (I - 1)] L_ik ^ 2 * d_kk
                // Explanation: To calculate the d_ii of row i and column i, you need to use all the d_kk elements
                // on the upper left of the diagonal and all the L_ik values on the left of the current row i and column i
                // In simple terms, get the values of d_kk at the top of the slope, and the values of L_ik at the left


                // The main diagonal elements of L are all 1
                L[i, i] = 1;

                // Formula (2): Let the current d_ii = a_ii
                D[i, i] = A[i, i];

                // Formula (2): SIGMA[k = 1~(i-1)] L_ik^2 * d_kk
                for (int j = 0;  j < i;  ++j)
                {
                    // To calculate D[i, i], you need to use all the L_ij * v before column i [j]
                    D[i, i] -= L[i, j] * L[i, j] * D[j, j];
                }


                // Solve the elements below the diagonal in column i from top to bottom, row by row
                for (int j = i + 1;  j < n;  ++j)
                {
                    // Formula (1): Let the current L_ji = a_ij
                    L[j, i] = A[j, i];

                    // type (1) : a_ij - SIGMA [k = 1 ~ (j - 1)] L_jk L_ik * * d_kk
                    for (int k = 0;  k < i;  ++k)
                    {
                        // Formula (1): a_ji-L_jk * L_ik * d_kk
                        L[j, i] -= L[j, k] * L[i, k] * D[k, k];
                    }

                    // Formula (1): Last/d_jj
                    L[j, i] /= D[i, i];
                }

            }*/

            //Debug.LogWarning("A mat: \n" + A.ToString());
            //Debug.LogWarning("L mat: \n" + L.ToString());
            //Debug.LogWarning("D mat: \n" + D.ToString());

        }


        /// <summary>
        /// GPU method: Performance optimization -- using LDLT decomposition to solve the equations ATA*x = b directly in one time
        /// </summary>
        /// <param name="ATA"> Symmetric positive definite matrix </param>
        /// <param name="x">ATA*x = b</param>
        /// <param name="b">ATA*x = b</param>
        public void GPU_EquationSolver_LDLT_InOne(ref Matrix ATA, ref Matrix x, ref Matrix b)
        {
            int n = ATA.rows;
            if (n <= 0 || ATA.cols != n || b.rows != n || b.cols != 1)
            {
                x = null;
                return;
            }

            float[,] Dvec = new float[n, 1];

            //Debug.LogWarning("A size " + Arow + ", " + Acol + " , val " + A[0, 0]);
            //Debug.LogWarning("B size " + Brow + ", " + Bcol + " , val " + B[0, 0]);

            var LBuf = new ComputeBuffer(n * n, Marshal.SizeOf(typeof(float)));
            var DBuf = new ComputeBuffer(n * 1, Marshal.SizeOf(typeof(float)));
            var bBuf = new ComputeBuffer(n * 1, Marshal.SizeOf(typeof(float)));

            LBuf.SetData(ATA.mat);
            DBuf.SetData(Dvec);
            bBuf.SetData(b.mat);


            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT, "L", LBuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT, "D", DBuf);

            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT_CalDii, "L", LBuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT_CalDii, "D", DBuf);

            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT_ClearLUpperTriValue, "L", LBuf);

            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT_MergeLDandLT, "L", LBuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT_MergeLDandLT, "D", DBuf);

            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT_ResetDiag, "L", LBuf);

            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT_LowerTriangle_Solver, "L", LBuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT_LowerTriangle_Solver, "D", DBuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT_LowerTriangle_Solver, "B", bBuf);

            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT_UpperTriangle_Solver, "L", LBuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT_UpperTriangle_Solver, "D", DBuf);
            puppetWarpCS.SetBuffer(kernel_Matrix_LDLT_UpperTriangle_Solver, "B", bBuf);

            puppetWarpCS.SetInt("Llen", n);


            puppetWarpCS.Dispatch(kernel_Matrix_LDLT_ClearLUpperTriValue, Mathf.FloorToInt((n - 1) / 16) + 1, Mathf.FloorToInt((n - 1) / 16) + 1, 1);


            for (int i = 0; i < n; ++i)
            {
                puppetWarpCS.SetInt("LDLTcurrentCol", i);
                puppetWarpCS.Dispatch(kernel_Matrix_LDLT_CalDii, 1, 1, 1);

                puppetWarpCS.SetBool("CalculateCurCol", true);
                puppetWarpCS.Dispatch(kernel_Matrix_LDLT, Mathf.FloorToInt((n - 1) / 16) + 1, Mathf.FloorToInt((n - 1) / 16) + 1, 1);

                puppetWarpCS.SetBool("CalculateCurCol", false);
                puppetWarpCS.Dispatch(kernel_Matrix_LDLT, Mathf.FloorToInt((n - 1) / 16) + 1, Mathf.FloorToInt((n - 1) / 16) + 1, 1);
            }


            puppetWarpCS.Dispatch(kernel_Matrix_LDLT_MergeLDandLT, Mathf.FloorToInt((n - 1) / 16) + 1, Mathf.FloorToInt((n - 1) / 16) + 1, 1);


            for (int i = 0; i < n; ++i)
            {
                puppetWarpCS.SetInt("currentOperatingRow", i);

                puppetWarpCS.Dispatch(kernel_Matrix_LDLT_LowerTriangle_Solver, 1, Mathf.FloorToInt((n - 1) / 16) + 1, 1);
            }

            puppetWarpCS.Dispatch(kernel_Matrix_LDLT_ResetDiag, 1, Mathf.FloorToInt((n - 1) / 16) + 1, 1);

            for (int i = n - 1; i >= 0; --i)
            {
                puppetWarpCS.SetInt("currentOperatingRow", i);

                puppetWarpCS.Dispatch(kernel_Matrix_LDLT_UpperTriangle_Solver, 1, Mathf.FloorToInt((n - 1) / 16) + 1, 1);
            }


            bBuf.GetData(Dvec);
            x = new Matrix(Dvec);
            // Debug.LogWarning("{In One} x Value: \n" + x.ToString());

            LBuf.Release();
            DBuf.Release();
            bBuf.Release();

        }


        /// <summary>
        /// Draw the Mesh to the screen using Unity Gizmos.
        /// </summary>
        public void GizmosDrawMesh()
        {
            if (vertList == null || triangleToVertList == null || handleAtTriangle == null) return;

            // Draw Mesh
            Gizmos.color = Color.red;
            for (int i = 0; i < triangleToVertList.Count; ++i)
            {
                Vector3Int t = triangleToVertList[i];
                Gizmos.DrawLine(meshCenter + vertList[t[0]], meshCenter + vertList[t[1]]);
                Gizmos.DrawLine(meshCenter + vertList[t[1]], meshCenter + vertList[t[2]]);
                Gizmos.DrawLine(meshCenter + vertList[t[2]], meshCenter + vertList[t[0]]);
            }


            // Draw lines from Center to all control points
            Gizmos.color = Color.blue;
            for (int i = 0; i < handleAtTriangle.Count; ++i)
            {
                Gizmos.DrawLine(meshCenter, (Vector3)meshCenter + handleAtTriangle[i]);
            }


            // Draw Handle
            Gizmos.color = Color.green;
            for (int i = 0; i < handleAtTriangle.Count; ++i)
            {
                Gizmos.DrawSphere(new Vector3(meshCenter.x + handleAtTriangle[i].x, meshCenter.y + handleAtTriangle[i].y, 0), 0.1f);
            }


            // Draw Mesh Center
            Gizmos.color = Color.gray;
            Gizmos.DrawSphere(new Vector3(meshCenter.x, meshCenter.y, 0), 0.12f);
        }

    }

}

