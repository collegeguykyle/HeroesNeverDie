using UnityEngine;
using SpRiseMChen.DelaunayMeshCreator;
using SpRiseMChen.PuppetDrawline;


namespace SpRiseMChen.PuppetDrawlineDemo
{
    /// <summary>
    /// Puppet Drawline MonoBehaviour.
    /// </summary>
    public class PuppetDrawlineDemo : MonoBehaviour
    {
        /// <summary>
        /// The number of rows of the mesh subdivision.
        /// </summary>
        [Range(1, 35)] public int meshRow;

        /// <summary>
        /// The number of columns of the mesh subdivision.
        /// </summary>
        [Range(1, 35)] public int meshCol;

        /// <summary>
        /// Manipulate the transform LDLT using the Compute Shader.
        /// </summary>
        [Space(10)] public ComputeShader puppetWarpCS;

        /// <summary>
        /// The parent object which stores lineRenderers.
        /// </summary>
        [SerializeField] private GameObject linePrefabObj;

        /// <summary>
        /// Mesh manipulation and deformation tool based on vertex operation with free control points.
        /// </summary>
        private PuppetDrawlineToolGPU puppetWarpFreePos;

        /// <summary>
        /// Delaunay Triangle Mesh Creator Tool.
        /// </summary>
        private DelaunayTriangleMeshCreator DTMC;


        // Start is called before the first frame update
        void Start()
        {
            // DelaunayTriangleMeshCreator instance.
            DTMC = new DelaunayTriangleMeshCreator(linePrefabObj, meshRow, meshCol);

            // PuppetDrawlineTool instance.
            puppetWarpFreePos = new PuppetDrawlineToolGPU(puppetWarpCS, ref DTMC.delaunayTriangles, ref DTMC.delaunayVertices,
                                                              ref DTMC.linePrefabs, DTMC.lineSampleCount);
            // Generate mesh from given drawlines.
            puppetWarpFreePos.GenerateMatrixFromVertices();

            // If the running Frame Rate is too low, try to increase the fixedDeltaTime.
            Time.fixedDeltaTime = 0.06f;
        }


        /// <summary>
        /// selected handle's id
        /// </summary>
        int id = -1;

        /// <summary>
        /// the vector from the mesh's center to the mouse input position
        /// </summary>
        Vector2 centerToInput = Vector2.zero;

        /// <summary>
        /// If the mouse left button is down
        /// </summary>
        bool hitLeftButton = false;


        void Update()
        {
            // Find the vertex closest to the mouse
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Click the right mouse button to generate handle or delete handle on the mouse
            if (Input.GetMouseButtonDown(1))
            {
                // Finds the id of the handle that is the closest to the mouse position
                // and within the effective interaction range
                int id = puppetWarpFreePos.FindNearestHandle(pos);
                Debug.LogWarning("ID: " + id);

                if (id == -1)
                {
                    // If no valid control point is found, the generate control point operation is performed
                    puppetWarpFreePos.AddFreeHandle(pos);
                }
                else
                {
                    // If a valid control point is found, delete the control point
                    puppetWarpFreePos.RemoveHandle(pos);
                }


                // If there is no handle exists, do not update matrix
                if (puppetWarpFreePos.HandleCount() > 0)
                {
                    // Update the constraint matrix according to handle
                    puppetWarpFreePos.UpdateHandleConstraints();

                }

            }


            // mouse left button down.
            if (Input.GetMouseButtonDown(0))
            {

                // find nearest handle id
                id = puppetWarpFreePos.FindNearestHandle(pos);

                // only solve deform equation when left mouse is down
                hitLeftButton = true;

                // get the local position
                centerToInput = pos - puppetWarpFreePos.meshCenter;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                // reset.
                id = -1;
                hitLeftButton = false;
            }

        }


        private void FixedUpdate()
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (hitLeftButton)
            {
                // The handle is found, and the total number of handles exceeds 1.
                if (id != -1 && puppetWarpFreePos.HandleCount() > 1)
                {
                    // move current selected handle.
                    puppetWarpFreePos.MoveHandle(id, pos);

                }
                else
                {
                    // handle not found, move entire mesh.
                    puppetWarpFreePos.MoveEntireMesh(pos, centerToInput);
                }

                // solve the deformed mesh vertices' position.
                puppetWarpFreePos.SolveVerticesPositions();

                // adjust drawline according to the deformed mesh.
                puppetWarpFreePos.AdjustDrawlines();
            }
        }


        private void OnDrawGizmos()
        {
            // Draw the mesh, handle points and mesh's average center.
            if (puppetWarpFreePos == null) return;

            puppetWarpFreePos.GizmosDrawMesh();
        }
    }

}

