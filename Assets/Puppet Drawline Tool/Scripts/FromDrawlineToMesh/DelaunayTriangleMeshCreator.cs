using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SpRiseMChen.DelaunayMeshCreator
{
    /// <summary>
    /// Delaunay triangle info
    /// </summary>
    public class MyDelaunayTriangle
    {
        // Delaunay vertices' indexs.
        public int vertId0;
        public int vertId1;
        public int vertId2;

        /// <summary>
        /// Whether the current triangle violates the Delaunay Rules.
        /// </summary>
        public bool illegalTriangle;

        /// <summary>
        /// The center of the circumcircle of the Delaunay triangle
        /// </summary>
        public Vector2 circumCenter;

        /// <summary>
        /// The radius of the circumcircle of the Delaunay triangle
        /// </summary>
        public float sqrCircumRadius;

        /// <summary>
        /// The area of the Delaunay triangle.
        /// </summary>
        public float triangleArea;


        /// <summary>
        /// Construction
        /// </summary>
        /// <param name="id0">vert id 0</param>
        /// <param name="id1">vert id 1</param>
        /// <param name="id2">vert id 2</param>
        /// <param name="vertices">vertices list</param>
        public MyDelaunayTriangle(int id0, int id1, int id2, ref List<Vector2> vertices)
        {
            vertId0 = id0;
            vertId1 = id1;
            vertId2 = id2;

            illegalTriangle = false;

            CalCircum(ref vertices);
            TriangleCCW(ref vertices);
        }


        /// <summary>
        /// Calculate circumcircle.
        /// </summary>
        /// <param name="vertices">global vertices list</param>
        public void CalCircum(ref List<Vector2> vertices)
        {
            Vector2 p = vertices[vertId0];
            Vector2 q = vertices[vertId1];
            Vector2 r = vertices[vertId2];

            Vector2 pq = q - p;
            Vector2 qr = r - q;

            Vector2 a = 0.5f * (p + q);
            Vector2 b = 0.5f * (q + r);
            Vector2 u = new Vector2(-pq.y, pq.x);

            float d = Vector2.Dot(u, qr);
            float t = Vector2.Dot(b - a, qr) / d;

            // calculate the center & sqrRadius of circumcircle.
            circumCenter = a + t * u;
            sqrCircumRadius = (circumCenter - p).sqrMagnitude;

            // Debug.Log("Radius : r1:" + r1 + ", r2:" + r2 + ", r3:" + r3);
        }


        /// <summary>
        /// Check the vertex order of the triangle,
        /// if it does not meet the requirements,
        /// it will be processed counterclockwise
        /// </summary>
        /// <param name="vertices"></param>
        public void TriangleCCW(ref List<Vector2> vertices)
        {
            // If the cross product of the current triangle vectors is negative,
            // then the order of the triangle vertices need to be reversed.
            if (CountTriangleArea(ref vertices) < 0.0f)
            {
                int tmp = vertId0;
                vertId0 = vertId2;
                vertId2 = tmp;
            }
        }


        /// <summary>
        /// Calculated triangle area.
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public float CountTriangleArea(ref List<Vector2> vertices)
        {
            Vector3 a = vertices[vertId0];
            Vector3 b = vertices[vertId1];
            Vector3 c = vertices[vertId2];

            // Area = ab * ac * sinθ / 2 = cross(ab, ac) / 2, where θ = ∠bac
            float area = 0.5f * Vector3.Cross(b - a, c - a).z;

            triangleArea = area;

            return area;
        }

        /// <summary>
        /// Index, to get the ith vert's id.
        /// </summary>
        /// <param name="i">i = 0, 1 or 2</param>
        /// <returns></returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public int this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return vertId0;
                    case 1: return vertId1;
                    case 2: return vertId2;
                    default: throw new IndexOutOfRangeException("Index out of range: " + i);
                }
            }
            set
            {
                switch (i)
                {
                    case 0: vertId0 = value; break;
                    case 1: vertId1 = value; break;
                    case 2: vertId2 = value; break;
                    default: throw new IndexOutOfRangeException("Index out of range: " + i);
                }
            }
        }

    }


    /// <summary>
    /// Generate a Delaunay Triangle mesh from the given drawline data.
    /// </summary>
    [Serializable]
    public class DelaunayTriangleMeshCreator
    {
        /// <summary>
        /// Drawline list.
        /// </summary>
        [SerializeField] public List<LineRenderer> linePrefabs;

        /// <summary>
        /// The number of sampling points for all drawlines used to construct the Delaunay mesh
        /// </summary>
        [SerializeField] public int lineSampleCount;

        /// <summary>
        /// The number of grid Rows m (to obtain the outer vertices of the Delaunay triangle)
        /// </summary>
        [SerializeField] int meshRow;

        /// <summary>
        /// The number of grid Columns n (to obtain the outer vertices of the Delaunay triangle)
        /// </summary>
        [SerializeField] int meshCol;

        /// <summary>
        /// Build the amount of slack in the drawlines' AABB box
        /// </summary>
        [SerializeField] float slack = 0.1f;

        /// <summary>
        /// The width and height of a divided individual cell
        /// </summary>
        public Vector2 leftUp;

        /// <summary>
        /// Record the width and height of a single small box of the entire AABB box
        /// </summary>
        public Vector2 deltaXY;

        /// <summary>
        /// Record which cells under the AABB subdivision are active
        /// </summary>
        public bool[] boxActivated;

        /// <summary>
        /// Record which grids under the AABB subdivision are marked as having strokes present
        /// </summary>
        public bool[] boxContainPoints;

        /// <summary>
        /// Record all the data that will be the vertices of the Delaunay triangle
        /// </summary>
        public List<Vector2> delaunayVertices;

        /// <summary>
        /// Record the final triangulation data after delaunay triangulation
        /// </summary>
        public List<Vector3Int> delaunayTriangles;


        /// <summary>
        /// Construction
        /// </summary>
        public DelaunayTriangleMeshCreator(GameObject lineParent, int meshRow, int meshCol, int insideRow = 0,
                                           int insideCol = 0, float slack = 0.15f)
        {
            linePrefabs = new List<LineRenderer>();
            delaunayVertices = new List<Vector2>();
            delaunayTriangles = new List<Vector3Int>();

            this.meshRow = meshRow;
            this.meshCol = meshCol;
            this.slack = slack;

            var LineList = lineParent.GetComponentsInChildren<LineRenderer>();
            foreach (var line in LineList)
            {
                AddDrawline(line);
            }

            GenerateDelaunayVertices();
            GenerateDelaunayTriangles();
        }


        /// <summary>
        /// add a drawline
        /// </summary>
        /// <param name="line"></param>
        public void AddDrawline(LineRenderer line)
        {
            linePrefabs.Add(line);
        }


        /// <summary>
        /// Remove a certain drawline
        /// </summary>
        /// <param name="line"></param>
        public void RemoveDrawline(LineRenderer line)
        {
            linePrefabs.Remove(line);
        }


        /// <summary>
        /// Clear Drawlines.
        /// </summary>
        public void ClearDrawline()
        {
            linePrefabs.Clear();
        }



        /// <summary>
        /// According to the current stroke set, the reference vertex sequence in Delaunay triangulation is obtained
        /// </summary>
        public void GenerateDelaunayVertices()
        {
            if (linePrefabs == null || linePrefabs.Count <= 0) return;

            if (delaunayVertices == null) delaunayVertices = new List<Vector2>();
            delaunayVertices.Clear();

            if (delaunayTriangles != null) delaunayTriangles.Clear();


            // Gets the outer surround AABB box properties for all strokes
            Vector3 p0 = linePrefabs[0].GetPosition(0);
            float left = p0.x - slack;
            float right = p0.x + slack;
            float up = p0.y + slack;
            float bottom = p0.y - slack;

            for (int j = 0; j < linePrefabs.Count; ++j)
            {
                for (int i = 0; i < linePrefabs[j].positionCount; ++i)
                {
                    Vector3 v = linePrefabs[j].GetPosition(i);

                    left = Mathf.Min(left, v.x - slack);
                    right = Mathf.Max(right, v.x + slack);
                    up = Mathf.Max(up, v.y + slack);
                    bottom = Mathf.Min(bottom, v.y - slack);

                    lineSampleCount++;
                }
            }

            Debug.Log("Line Sampler Count: " + lineSampleCount);

            // According to the AABB box position and width and height, add the vertices of the supertriangle first
            leftUp = new Vector2(left, up);
            Vector2 rightBottom = new Vector2(right, bottom);
            float wid = right - left;
            float hei = up - bottom;
            delaunayVertices.Add(new Vector2(leftUp.x - wid, rightBottom.y - hei / 2f));
            delaunayVertices.Add(new Vector2(rightBottom.x + wid, rightBottom.y - hei / 2f));
            delaunayVertices.Add(new Vector2(leftUp.x + wid / 2f, leftUp.y + 2 * hei));


            // Calculate the width and height of a single lattice based on the AABB box and the given number of segments
            float width = (right - left) / meshCol;
            float height = (up - bottom) / meshRow;
            deltaXY = new Vector2(width, height);


            // Initializes the relevant data structure based on the number of segments
            boxActivated = new bool[meshCol * meshRow];
            boxContainPoints = new bool[meshCol * meshRow];


            // Go through all the strokes and record all the grids where the strokes are present
            for (int j = 0; j < linePrefabs.Count; ++j)
            {
                // Check which cells the current stroke has passed through
                for (int i = 0; i < linePrefabs[j].positionCount; ++i)
                {
                    Vector3 pos = linePrefabs[j].GetPosition(i);
                    int boxx = (int)((pos.x - left) / width);
                    int boxy = (int)((up - pos.y) / height);

                    // Calculate the boxID for this box
                    int boxId = meshCol * boxy + boxx;


                    // The current sampling point falls in this cell and is marked activated
                    if (!boxActivated[boxId])
                    {
                        // Record the box info.
                        boxActivated[boxId] = true;
                        boxContainPoints[boxId] = true;
                    }


                    // For adjacent sampling points in the stroke, a connected grid path should be guaranteed
                    if (i != linePrefabs[j].positionCount - 1)
                    {
                        Vector3 nextPos = linePrefabs[j].GetPosition(i + 1);

                        // Divide a smaller AABB box according to pos[i] and pos[i+1]
                        int nextX = (int)((nextPos.x - left) / width);
                        int nextY = (int)((up - nextPos.y) / height);

                        int L = Mathf.Min(boxx, nextX);
                        int R = Mathf.Max(boxx, nextX);
                        int U = Mathf.Max(boxy, nextY);
                        int B = Mathf.Min(boxy, nextY);

                        // For this small range AABB box, check if each cell is connected by line segments pos[i] and pos[i+1]
                        for (int xid = L; xid <= R; ++xid)
                        {
                            for (int yid = B; yid <= U; ++yid)
                            {
                                int curBoxId = meshCol * yid + xid;

                                if (!boxActivated[curBoxId] && CheckLineSectionInBox(pos, nextPos, left, up, width, height, xid, yid))
                                {
                                    boxActivated[curBoxId] = true;
                                    boxContainPoints[curBoxId] = true;
                                }
                            }
                        }

                    }

                }
            }


            // Scan the grid in all four directions. When an active box is encountered, all the boxes following it will count +1
            // After scanning in four directions, top to bottom, left to right, bottom to top, right to left,
            // all the cells with a count of 4 are recorded in the lattice
            // Notice that the outermost layer cannot satisfy this condition,
            // so the outermost circle can be discarded when adding box, and the array overflow can be prevented
            int[] coverCount = new int[meshRow * meshCol];

            for (int i = 0; i < meshRow; ++i)
            {
                bool findActivated = false;
                for (int j = 0; j < meshCol; ++j)
                {
                    int boxid = meshCol * i + j;
                    if (findActivated == true) coverCount[boxid]++;
                    if (boxActivated[boxid] == true) findActivated = true;
                }
            }
            for (int i = 0; i < meshRow; ++i)
            {
                bool findActivated = false;
                for (int j = meshCol - 1; j >= 0; --j)
                {
                    int boxid = meshCol * i + j;
                    if (findActivated == true) coverCount[boxid]++;
                    if (boxActivated[boxid] == true) findActivated = true;
                }
            }
            for (int j = 0; j < meshCol; ++j)
            {
                bool findActivated = false;
                for (int i = 0; i < meshRow; ++i)
                {
                    int boxid = meshCol * i + j;
                    if (findActivated == true) coverCount[boxid]++;
                    if (boxActivated[boxid] == true) findActivated = true;
                }
            }
            for (int j = 0; j < meshCol; ++j)
            {
                bool findActivated = false;
                for (int i = meshRow - 1; i >= 0; --i)
                {
                    int boxid = meshCol * i + j;
                    if (findActivated == true) coverCount[boxid]++;
                    if (boxActivated[boxid] == true) findActivated = true;
                }
            }

            // Set all cells that count 4 to active
            for (int i = 1; i < meshRow - 1; ++i)
            {
                for (int j = 1; j < meshCol - 1; ++j)
                {
                    int boxid = meshCol * i + j;
                    if (coverCount[boxid] >= 4)
                    {
                        boxActivated[boxid] = true;
                    }
                }
            }


            // using Dfs the edges and remove all the surrounding blank squares
            for (int i = 0; i < meshRow; ++i)
            {
                for (int j = 0; j < meshCol; ++j)
                {
                    int boxid = meshCol * i + j;

                    // This grid isn't even on. Skip it
                    if (boxActivated[boxid] == false) continue;

                    //Debug.Log("Box ID: " + boxid + " , Contain Point? " + boxContainPoints[boxid]);

                    // The current cell is started, and there is no stroke data in this cell.
                    // And this lattice is an AABB edge, or the current lattice is not surrounded by other grids
                    // (that is, the lattice is at the edge of the lattice)
                    if (!boxContainPoints[boxid] && ((i == 0 || i == meshRow - 1 || j == 0 || j == meshCol - 1)
                       || !(boxActivated[boxid + 1] && boxActivated[boxid - 1] && boxActivated[boxid + meshCol] && boxActivated[boxid - meshCol])))
                    {
                        Stack<int> st = new Stack<int>();
                        st.Push(boxid);
                        while (st.Count != 0)
                        {
                            int curID = st.Pop();
                            boxActivated[curID] = false;  // Delete this box that is on the edge and has no stroke record

                            // For the four directions, if the grid is enabled and no stroke data exists in the grid, record the grid to be deleted
                            if (curID - 1 >= 0 && boxActivated[curID - 1] && !boxContainPoints[curID - 1])
                                st.Push(curID - 1);
                            if (curID + 1 < meshCol * meshRow && boxActivated[curID + 1] && !boxContainPoints[curID + 1])
                                st.Push(curID + 1);
                            if (curID - meshCol >= 0 && boxActivated[curID - meshCol] && !boxContainPoints[curID - meshCol])
                                st.Push(curID - meshCol);
                            if (curID + meshCol < meshCol * meshRow && boxActivated[curID + meshCol] && !boxContainPoints[curID + meshCol])
                                st.Push(curID + meshCol);
                        }
                    }

                }
            }


            // An anonymous method that detects whether a location around the box is activated
            Func<int, int, bool> checkBoxActivated = delegate (int i, int j)
            {
                if (i < 0 || i >= meshRow || j < 0 || j >= meshCol) return false;

                int boxid = meshCol * i + j;
                return boxActivated[boxid];
            };


            HashSet<Vector2> verts = new HashSet<Vector2>();

            // Are there vertices in verts that are very close to the input point p
            Func<Vector2, bool> ApproximatePos = delegate (Vector2 p)
            {
                foreach (Vector2 v in verts)
                {
                    if ((v - p).magnitude <= 0.001f) return true;
                }
                return false;
            };

            // Find all active grids that are located at the boundary and obtain their vertices
            // as the vertices sequence used in the generation of the Delaunay triangle
            for (int i = 0; i < meshRow; ++i)
            {
                // i:row   j:col
                for (int j = 0; j < meshCol; ++j)
                {
                    int boxid = meshCol * i + j;
                    Vector2 boxLeftUp = new Vector2(left + j * width, up - i * height);
                    // Debug.Log("BOX ID: " + boxid + ", box left up: " + boxLeftUp);

                    // check current box
                    // --- Pos Definition ---
                    //       7  8  9
                    //       4     6
                    //       1  2  3
                    if (boxActivated[boxid])
                    {
                        // Add the upper left vertex of the box whenever a position in the 4/7/8 position of the box is not active
                        if (!checkBoxActivated(i, j - 1) || !checkBoxActivated(i - 1, j - 1) || !checkBoxActivated(i - 1, j))
                        {
                            if (!ApproximatePos(boxLeftUp))
                            {
                                verts.Add(boxLeftUp);
                                //delaunayVertices.Add(boxLeftUp);
                            }
                        }
                        // Add the lower left vertex of the box whenever a position in the 4/1/2 position of the box is not active
                        if (!checkBoxActivated(i, j - 1) || !checkBoxActivated(i + 1, j - 1) || !checkBoxActivated(i + 1, j))
                        {
                            Vector2 boxLeftBottom = new Vector2(boxLeftUp.x, boxLeftUp.y - height);
                            if (!ApproximatePos(boxLeftBottom))
                            {
                                verts.Add(boxLeftBottom);
                                //delaunayVertices.Add(boxLeftBottom);
                            }
                        }
                        // The upper right vertex of the box is added whenever a position in the 6/9/8 position of this box is not active
                        if (!checkBoxActivated(i, j + 1) || !checkBoxActivated(i - 1, j + 1) || !checkBoxActivated(i - 1, j))
                        {
                            Vector2 boxRightUp = new Vector2(boxLeftUp.x + width, boxLeftUp.y);
                            if (!ApproximatePos(boxRightUp))
                            {
                                verts.Add(boxRightUp);
                                //delaunayVertices.Add(boxRightUp);
                            }
                        }
                        // Add the bottom right vertex of the box whenever a position in the 6/3/2 position of this box is not active
                        if (!checkBoxActivated(i, j + 1) || !checkBoxActivated(i + 1, j + 1) || !checkBoxActivated(i + 1, j))
                        {
                            Vector2 boxRightBottom = new Vector2(boxLeftUp.x + width, boxLeftUp.y - height);
                            if (!ApproximatePos(boxRightBottom))
                            {
                                verts.Add(boxRightBottom);
                                //delaunayVertices.Add(boxRightBottom);
                            }
                        }
                    }

                }
            }

            foreach (Vector2 v in verts)
            {
                delaunayVertices.Add(v);
            }

            Debug.Log("delaunay triangle vert num : " + delaunayVertices.Count);
        }




        public void GenerateDelaunayTriangles()
        {
            if (delaunayVertices == null || delaunayVertices.Count == 0) return;

            Debug.Log("Vertices Count: " + delaunayVertices.Count);

            // Structural supertriangle
            MyDelaunayTriangle mdt = new MyDelaunayTriangle(0, 1, 2, ref delaunayVertices);

            // Temporarily stores a list of Delaunay triangles
            List<MyDelaunayTriangle> delaunayList = new List<MyDelaunayTriangle> { mdt };


            // Determine whether two edges represent the same edge
            Func<Vector3Int, Vector3Int, bool> isSameEdge = delegate (Vector3Int p1, Vector3Int p2)
            {
                return ((p1.x == p2.x && p1.y == p2.y) || (p1.x == p2.y && p1.y == p2.x));
            };

            // original delaunay vertices count
            int delaunayVertOriginNum = delaunayVertices.Count;


            for (int i = 3; i < delaunayVertices.Count; i++)
            {
                Vector2 p = delaunayVertices[i];

                List<Vector3Int> edges = new List<Vector3Int>();

                // For each triangle of the current record
                for (int j = 0; j < delaunayList.Count; j++)
                {
                    MyDelaunayTriangle dt = delaunayList[j];

                    float sqrDis = (dt.circumCenter - p).sqrMagnitude;

                    // If the vertex p is in the outer circle of the current triangle
                    if (sqrDis <= dt.sqrCircumRadius)
                    {

                        // this triangle need to be removed
                        dt.illegalTriangle = true;

                        // Adds the three sides of a triangle to the edges list
                        edges.Add(new Vector3Int(dt.vertId0, dt.vertId1, 0));
                        edges.Add(new Vector3Int(dt.vertId1, dt.vertId2, 0));
                        edges.Add(new Vector3Int(dt.vertId2, dt.vertId0, 0));
                    }
                }


                // Remove all triangles that violate Delaunay's rules
                delaunayList.RemoveAll((dt) => dt.illegalTriangle == true);

                // 接下来处理各个边
                for (int j = 0; j < edges.Count; j++)
                {
                    Vector3Int e1 = edges[j];

                    // 检查到有相邻三角之间的重复边，标记为违反规则边
                    for (int k = j + 1; k < edges.Count; k++)
                    {
                        Vector3Int e2 = edges[k];

                        if (isSameEdge(e1, e2))
                        {
                            // edge.z为1时，表示此边是待删除的边
                            edges[j] = new Vector3Int(e1.x, e1.y, 1);
                            edges[k] = new Vector3Int(e2.x, e2.y, 1);
                        }
                    }

                }

                // Remove sides that are no longer used to construct triangles.
                edges.RemoveAll((e) => e.z == 1);

                // Constructing a new triangles.
                foreach (Vector3Int e in edges)
                {
                    MyDelaunayTriangle deTri = new MyDelaunayTriangle(i, e.x, e.y, ref delaunayVertices);
                    delaunayList.Add(deTri);
                }

            }

            // Calculate the average area of most triangles.
            HashSet<Vector2> addedVertices = new HashSet<Vector2>();
            float avgArea = 0;
            int triNum = 0;
            foreach (MyDelaunayTriangle dt in delaunayList)
            {
                if ((dt.vertId0 < 3) || (dt.vertId1 < 3) || (dt.vertId2 < 3)) continue;
                if (dt.triangleArea <= (deltaXY.x * deltaXY.y / 2f)) continue;
                avgArea += dt.triangleArea;
                triNum++;
            }
            avgArea /= triNum;

            // Add new vertices for all triangles that exceed the average area.
            foreach (MyDelaunayTriangle dt in delaunayList)
            {
                if ((dt.vertId0 < 3) || (dt.vertId1 < 3) || (dt.vertId2 < 3)) continue;

                if (dt.triangleArea > 1f * avgArea)
                {
                    //addedVertices.Add((delaunayVertices[dt.vertId0] + delaunayVertices[dt.vertId1]) / 2f);
                    //addedVertices.Add((delaunayVertices[dt.vertId1] + delaunayVertices[dt.vertId2]) / 2f);
                    //addedVertices.Add((delaunayVertices[dt.vertId2] + delaunayVertices[dt.vertId0]) / 2f);

                    // Add a new vertex to the triangle's center of gravity.
                    addedVertices.Add((delaunayVertices[dt.vertId1] + delaunayVertices[dt.vertId2] + delaunayVertices[dt.vertId0]) / 3f);
                }
            }

            // Add new vertices.
            foreach (Vector2 p in addedVertices)
            {
                delaunayVertices.Add(p);
            }

            // Generate Delaunay Triangle again.
            for (int i = delaunayVertOriginNum; i < delaunayVertices.Count; i++)
            {
                Vector2 p = delaunayVertices[i];

                List<Vector3Int> edges = new List<Vector3Int>();

                // For each triangle of the current record
                for (int j = 0; j < delaunayList.Count; j++)
                {
                    MyDelaunayTriangle dt = delaunayList[j];

                    float sqrDis = (dt.circumCenter - p).sqrMagnitude;

                    // Debug.Log("Radius: " + dt.sqrCircumRadius);

                    // If the vertex p is inside the circumcircle of the current triangle
                    if (sqrDis <= dt.sqrCircumRadius)
                    {
                        // current triangle need to be deleted.
                        dt.illegalTriangle = true;

                        // Adds the three sides of a triangle to the edges list.
                        edges.Add(new Vector3Int(dt.vertId0, dt.vertId1, 0));
                        edges.Add(new Vector3Int(dt.vertId1, dt.vertId2, 0));
                        edges.Add(new Vector3Int(dt.vertId2, dt.vertId0, 0));
                    }
                }


                // Remove all triangles that violate Delaunay's rules.
                delaunayList.RemoveAll((dt) => dt.illegalTriangle == true);

                // Next we deal with the sides.
                for (int j = 0; j < edges.Count; j++)
                {
                    Vector3Int e1 = edges[j];

                    // A duplicate edge between adjacent triangles is detected and marked as a violation edge.
                    for (int k = j + 1; k < edges.Count; k++)
                    {
                        Vector3Int e2 = edges[k];

                        if (isSameEdge(e1, e2))
                        {
                            // If edge.z = 1, it indicates that this edge is the edge to be deleted
                            edges[j] = new Vector3Int(e1.x, e1.y, 1);
                            edges[k] = new Vector3Int(e2.x, e2.y, 1);
                        }
                    }

                }

                // Remove edges that are no longer used to construct triangles.
                edges.RemoveAll((e) => e.z == 1);

                // Finally each edge builds a new triangle with the vertex p.
                foreach (Vector3Int e in edges)
                {
                    MyDelaunayTriangle deTri = new MyDelaunayTriangle(i, e.x, e.y, ref delaunayVertices);
                    delaunayList.Add(deTri);
                }

            }


            // Delete all triangles connected to the vertices of supertriangles.
            delaunayList.RemoveAll((t) => (t.vertId0 < 3) || (t.vertId1 < 3) || (t.vertId2 < 3));

            // Delete the vertices of the supertriangle.
            delaunayVertices.RemoveAt(0);
            delaunayVertices.RemoveAt(0);
            delaunayVertices.RemoveAt(0);

            // Reset the list of triangles used for recording.
            if (delaunayTriangles == null) delaunayTriangles = new List<Vector3Int>();
            delaunayTriangles.Clear();


            // Use vector3 to record the three-vertex subscript of the final triangle,
            // and consider whether you need to change to Vector4 to record additional triangle areas
            foreach (MyDelaunayTriangle dt in delaunayList)
            {
                // The supertriangle vertex at the head of the list is removed,
                // so the other vertices are numbered 3 digits forward
                Vector3Int tri = new Vector3Int(dt.vertId0 - 3, dt.vertId1 - 3, dt.vertId2 - 3);

                Vector2 center = (delaunayVertices[tri[0]] + delaunayVertices[tri[1]] + delaunayVertices[tri[2]]) / 3f;
                int x = (int)((center.x - leftUp.x) / deltaXY.x);
                int y = (int)((leftUp.y - center.y) / deltaXY.y);
                int boxId = meshCol * y + x;

                Debug.Log("Box ID: " + boxId);
                if (x < 0 || y < 0 || x >= meshCol || y >= meshRow || boxActivated[boxId] == false) continue;

                delaunayTriangles.Add(tri);
            }

        }


        /// <summary>
        /// Check if the from-to line is crossed with current box.
        /// </summary>
        /// <param name="from">The start position of a line segment</param>
        /// <param name="to">The end position of a line segment</param>
        /// <param name="left">The left boundary coordinates of the entire lattice net</param>
        /// <param name="up">The up boundary coordinates of the entire lattice net</param>
        /// <param name="width">The width of a single lattice</param>
        /// <param name="height">The height of a single lattice</param>
        /// <param name="x">The x-th column of the lattice, left to right</param>
        /// <param name="y">The y-th row of the lattice, up to bottom</param>
        /// <returns>If the from-to line is crossed with current box</returns>
        private bool CheckLineSectionInBox(Vector2 from, Vector2 to, float left, float up, float width, float height, int x, int y)
        {
            float k = (to.y - from.y) / (to.x - from.x + 1e-9f);  // add 1e-9 to avoid divide-by-zero issue.
            float b = from.y - k * from.x;

            int count = 0;

            Vector2[] v4 = new Vector2[4];
            v4[0] = new Vector2(left + width * x, up - height * y);
            v4[1] = new Vector2(left + width * x, up - height * (y + 1));
            v4[2] = new Vector2(left + width * (x + 1), up - height * (y + 1));
            v4[3] = new Vector2(left + width * (x + 1), up - height * y);

            for (int i = 0; i < 4; ++i)
            {
                if (v4[i].x * k + b < v4[i].y)
                    count++;
                else
                    count--;
            }

            return !(count == 4 || count == -4);
        }


        /// <summary>
        /// Unity Gizmos Visualization method.
        /// </summary>
        public void Unity_DrawGizmos()
        {
            if (delaunayVertices == null || delaunayVertices.Count <= 0) return;
            Gizmos.color = Color.black;
            for (int i = 0; i < delaunayVertices.Count; ++i)
            {
                Gizmos.DrawCube(delaunayVertices[i], new Vector3(0.09f, 0.09f, 0.09f));
            }

            if (delaunayTriangles == null || delaunayTriangles.Count <= 0) return;
            foreach (Vector3Int tri in delaunayTriangles)
            {
                Gizmos.DrawLine(delaunayVertices[tri[0]], delaunayVertices[tri[1]]);
                Gizmos.DrawLine(delaunayVertices[tri[1]], delaunayVertices[tri[2]]);
                Gizmos.DrawLine(delaunayVertices[tri[2]], delaunayVertices[tri[0]]);
            }
        }

    }

}

