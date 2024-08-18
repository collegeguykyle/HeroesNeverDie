using System;
using UnityEngine;

namespace SpRiseMChen.Helper
{
    /// <summary>
    /// Used to record the neighbor vertices around each edge
    /// </summary>
    public class EdgeNeighbors
    {
        /// <summary>
        /// Neighbor vertex id
        /// </summary>
        private int[] neighbors;

        /// <summary>
        /// Neighbor vertices count
        /// </summary>
        private int size;


        /// <summary>
        /// Construction
        /// </summary>
        public EdgeNeighbors()
        {
            neighbors = new int[] { -1, -1 };
            size = 0;
        }


        /// <summary>
        /// Records a new neighbor vertex for the current edge
        /// </summary>
        /// <param name="index">neighbor vert's index</param>
        public void AddNeighbor(int index)
        {
            ++size;
            neighbors[size - 1] = index;
        }


        /// <summary>
        /// Indexer
        /// </summary>
        /// <param name="index">index</param>
        /// <returns></returns>
        public int this[int index]
        {
            get
            {
                return neighbors[index];
            }
        }

        /// <summary>
        /// Number of neighbor vertices of the current edge
        /// </summary>
        public int Size { get { return size; } }
    }


    /// <summary>
    /// Matrix Class.
    /// </summary>
    public class Matrix
    {
        /// <summary>
        /// matrix value mat.
        /// </summary>
        public float[,] mat;

        /// <summary>
        /// Rows
        /// </summary>
        public int rows
        {
            get { return mat.GetLength(0); }
        }

        /// <summary>
        /// Columns
        /// </summary>
        public int cols
        {
            get { return mat.GetLength(1); }
        }


        /// <summary>
        /// Matrix construction method
        /// </summary>
        /// <param name="rows">rows</param>
        /// <param name="cols">cols</param>
        public Matrix(int rows, int cols)
        {
            mat = MatrixCreate(rows, cols);
        }


        /// <summary>
        /// Matrix construction method that generates a matrix from a given float two-dimensional array
        /// </summary>
        /// <param name="values"></param>
        public Matrix(float[,] values)
        {
            mat = new float[values.GetLength(0), values.GetLength(1)];
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < cols; ++j)
                    mat[i, j] = values[i, j];
        }


        /// <summary>
        /// Unity 4x4 Matrix into matrix.
        /// </summary>
        /// <param name="m"></param>
        public Matrix(Matrix4x4 m)
        {
            Matrix res = new Matrix(new float[] {
            m.m00, m.m01, m.m02, m.m03,
            m.m10, m.m11, m.m12, m.m13,
            m.m20, m.m21, m.m22, m.m23,
            m.m30, m.m31, m.m32, m.m33
        }, 4, 4);

            mat = res.mat;
        }


        /// <summary>
        /// Return the Transpose of matrix.
        /// </summary>
        public Matrix T
        {
            get
            {
                Matrix transpos = new Matrix(cols, rows);

                for (int i = 0; i < cols; ++i)
                    for (int j = 0; j < rows; ++j)
                        transpos[i, j] = mat[j, i];

                return transpos;
            }
        }


        /// <summary>
        /// Matrix Transpose.
        /// </summary>
        /// <returns></returns>
        public Matrix Transpose()
        {
            Matrix transpos = new Matrix(cols, rows);

            for (int i = 0; i < cols; ++i)
                for (int j = 0; j < rows; ++j)
                    transpos[i, j] = mat[j, i];

            return transpos;
        }


        /// <summary>
        /// Matrix construction method that generates a matrix of the corresponding size with a given float array
        /// </summary>
        /// <param name="values">A float array for converting to a matrix</param>
        /// <param name="rows">row</param>
        /// <param name="cols">col</param>
        /// <exception cref="Exception">The size of the array is different from the size of the matrix to be generated.</exception>
        public Matrix(float[] values, int rows, int cols)
        {
            if (rows * cols != values.Length)
            {
                throw new Exception("rows x cols: " + rows + "x" + cols + " = " + rows * cols + " does not equal given data of size " + values.Length);
            }

            mat = MatrixCreate(rows, cols);

            int pos = 0;
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    mat[i, j] = values[pos];
                    pos++;
                }
            }
        }


        /// <summary>
        /// Matrix construction method, using the given double array to generate the corresponding size of the matrix.
        /// </summary>
        /// <param name="values">double array for conversion to matrix</param>
        /// <param name="rows">row</param>
        /// <param name="cols">col</param>
        /// <exception cref="Exception">The size of the array is different from the size of the matrix to be generated.</exception>
        public Matrix(double[] values, int rows, int cols)
        {
            if (rows * cols != values.Length)
            {
                throw new Exception("rows x cols: " + rows + "x" + cols + " = " + rows * cols + " does not equal given data of size " + values.Length);
            }

            mat = MatrixCreate(rows, cols);

            int pos = 0;
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    mat[i, j] = (float)values[pos];
                    pos++;
                }
            }
        }


        /// <summary>
        /// Concatenate a given series of matrices into a new matrix.
        /// </summary>
        /// <param name="fillByRows">Whether to fill rows from top to bottom when concatenating matrices.</param>
        /// <param name="mats">A series of matrices used to concatenate.</param>
        public Matrix(bool fillByRows, params Matrix[] mats)
        {
            // Fill the matrix in rows from top to bottom using the given matrix.
            if (fillByRows)
            {
                int row = 0, col = mats[0].cols;

                // Count the total number of rows, and check whether the number of columns in each matrix is uniform.
                for (int i = 0; i < mats.Length; ++i)
                {
                    row += mats[i].rows;

                    if (mats[i].cols != col)
                    {
                        Debug.LogError("Given Matrices have different columns. ");
                        return;
                    }
                }

                mat = MatrixCreate(row, col);

                int globalR = 0;
                for (int i = 0; i < mats.Length; ++i)
                {
                    // Adds the current matrix to the merged matrix.
                    for (int matR = 0; matR < mats[i].rows; ++matR)
                    {
                        for (int matC = 0; matC < col; ++matC)
                        {
                            mat[globalR, matC] = mats[i][matR, matC];
                        }
                        ++globalR;
                    }
                }

            }
            else      // Fill the matrix in columns from left to right using the given matrix
            {
                int row = mats[0].rows, col = 0;

                // Count the total number of columns, and check whether the number of rows in each matrix is uniform.
                for (int i = 0; i < mats.Length; ++i)
                {
                    col += mats[i].cols;

                    if (mats[i].rows != row)
                    {
                        Debug.LogError("Given Matrices have different rows. ");
                        return;
                    }
                }

                mat = MatrixCreate(row, col);

                int globalC = 0;
                for (int i = 0; i < mats.Length; ++i)
                {
                    // Adds the current matrix to the merged matrix.
                    for (int matC = 0; matC < mats[i].cols; ++matC)
                    {
                        for (int matR = 0; matR < row; ++matR)
                        {
                            mat[matR, globalC] = mats[i][matR, matC];
                        }
                        ++globalC;
                    }
                }
            }
        }


        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return MatrixAsString(mat);
        }


        /// <summary>
        /// Outputs the current matrix as a one-dimensional float array.
        /// </summary>
        /// <returns></returns>
        public float[] ToArray()
        {
            float[] arr = new float[rows * cols];
            int pos = 0;
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < cols; ++j)
                    arr[pos++] = mat[i, j];
            return arr;
        }


        /// <summary>
        /// Access the elements in row i and column j.
        /// </summary>
        /// <param name="i">ith row</param>
        /// <param name="j">jth col</param>
        /// <returns>mat[i, j]</returns>
        public float this[int i, int j]
        {
            get
            {
                return mat[i, j];
            }
            set
            {
                mat[i, j] = value;
            }
        }


        /// <summary>
        /// For all the entries in the matrix, multiply s.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Matrix operator *(Matrix m, float s)
        {
            Matrix res = new Matrix(m.rows, m.cols);

            for (int i = 0; i < m.rows; ++i)
                for (int j = 0; j < m.cols; ++j)
                    res[i, j] = m[i, j] * s;

            return res;
        }


        /// <summary>
        /// For all the entries in the matrix, multiply s.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Matrix operator *(float s, Matrix m)
        {
            return m * s;
        }


        /// <summary>
        /// For all the entries in the matrix, add s.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Matrix operator +(Matrix m, float s)
        {
            Matrix res = new Matrix(m.rows, m.cols);
            for (int i = 0; i < m.rows; ++i)
                for (int j = 0; j < m.cols; ++j)
                    res[i, j] = m[i, j] + s;
            return res;
        }


        /// <summary>
        /// For all the entries in the matrix, add s.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Matrix operator +(float s, Matrix m)
        {
            return m + s;
        }


        /// <summary>
        /// For all the entries in the matrix, minus s.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Matrix operator -(Matrix m, float s)
        {
            return m + (-s);
        }


        /// <summary>
        /// For all the entries in the matrix, use s to minus them.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Matrix operator -(float s, Matrix m)
        {
            return (-m) + s;
        }


        /// <summary>
        /// Matrix Multiply.
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static Matrix operator *(Matrix c1, Matrix c2)
        {
            return new Matrix(MatrixProduct(c1.mat, c2.mat));
        }


        /// <summary>
        /// Negative matrix.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Matrix operator -(Matrix m)
        {
            Matrix res = new Matrix(m.rows, m.cols);

            for (int i = 0; i < m.rows; ++i)
                for (int j = 0; j < m.cols; ++j)
                    res[i, j] = -m[i, j];

            return res;
        }


        /// <summary>
        /// Matrix Addup.
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">row/column not match.</exception>
        public static Matrix operator +(Matrix c1, Matrix c2)
        {
            if (c1.rows != c2.rows || c1.cols != c2.cols)
            {
                throw new ArgumentException("Left hand side size: (" + c1.rows + ", " + c1.cols + ") != Right hand side size: (" + c2.rows + ", " + c2.cols + ")");
            }

            Matrix res = new Matrix(c1.rows, c1.cols);
            for (int i = 0; i < c1.rows; ++i)
                for (int j = 0; j < c1.cols; ++j)
                    res[i, j] = c1[i, j] + c2[i, j];

            return res;
        }

        /// <summary>
        /// Matrix Sub
        /// </summary>
        /// <param name="c1">matrix</param>
        /// <param name="c2">matrix</param>
        /// <returns></returns>
        public static Matrix operator -(Matrix c1, Matrix c2)
        {
            return c1 + (-c2);
        }


        /// <summary>
        /// Matirx Inverse.
        /// </summary>
        /// <returns></returns>
        public Matrix Inverse()
        {
            return new Matrix(MatrixInverse(mat));
        }


        /// <summary>
        /// Get the inverse of matrix.
        /// </summary>
        /// <param name="matrix">matrix</param>
        /// <returns></returns>
        public static float[,] MatrixInverse(float[,] matrix)
        {
            if (matrix.GetLength(0) == 2)
            {
                float det = matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
                if (det == 0)
                {
                    return new float[,] { { 1, 0 }, { 0, 1 } };
                }
                else
                {
                    return new float[,] { { matrix[1, 1] / det, -matrix[0, 1] / det }, { -matrix[1, 0] / det, matrix[0, 0] / det } };
                }
            }

            // assumes determinant is not 0
            // that is, the matrix does have an inverse
            int n = matrix.GetLength(0);
            float[,] result = MatrixCreate(n, n); // make a copy of matrix
            for (int i = 0; i < n; ++i)
                for (int j = 0; j < n; ++j)
                    result[i, j] = matrix[i, j];

            float[,] lum; // combined lower & upper
            int[] perm;
            int toggle;
            toggle = MatrixDecompose(matrix, out lum, out perm);

            float[] b = new float[n];
            for (int i = 0; i < n; ++i)
            {
                for (int j = 0; j < n; ++j)
                    if (i == perm[j])
                        b[j] = 1.0f;
                    else
                        b[j] = 0.0f;

                float[] x = Helper(lum, b); // 
                for (int j = 0; j < n; ++j)
                    result[j, i] = x[j];
            }
            return result;
        }


        /// <summary>
        /// Crout LU decomposition of matrices.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="lum"></param>
        /// <param name="perm"></param>
        /// <returns></returns>
        public static int MatrixDecompose(float[,] m, out float[,] lum, out int[] perm)
        {
            // Crout's LU decomposition for matrix determinant and inverse
            // stores combined lower & upper in lum[, ]
            // stores row permuations into perm[]
            // returns +1 or -1 according to even or odd number of row permutations
            // lower gets dummy 1.0s on diagonal (0.0s above)
            // upper gets lum values on diagonal (0.0s below)

            int toggle = +1; // even (+1) or odd (-1) row permutatuions
            int n = m.GetLength(0);

            // make a copy of m[, ] into result lu[, ]
            lum = MatrixCreate(n, n);
            for (int i = 0; i < n; ++i)
                for (int j = 0; j < n; ++j)
                    lum[i, j] = m[i, j];


            // make perm[]
            perm = new int[n];
            for (int i = 0; i < n; ++i)
                perm[i] = i;

            for (int j = 0; j < n - 1; ++j) // process by column. note n-1 
            {
                float max = Math.Abs(lum[j, j]);
                int piv = j;

                for (int i = j + 1; i < n; ++i) // find pivot index
                {
                    float xij = Math.Abs(lum[i, j]);
                    if (xij > max)
                    {
                        max = xij;
                        piv = i;
                    }
                } // i

                if (piv != j)
                {
                    for (int i = 0; i < lum.GetLength(1); ++i)
                    {
                        float tmp = lum[piv, i];
                        lum[piv, i] = lum[j, i];
                        lum[j, i] = tmp;
                    }

                    //float[] tmp = lum[piv]; // swap rows j, piv
                    //lum[piv] = lum[j];
                    //lum[j] = tmp;

                    int t = perm[piv]; // swap perm elements
                    perm[piv] = perm[j];
                    perm[j] = t;

                    toggle = -toggle;
                }

                float xjj = lum[j, j];
                if (xjj != 0.0)
                {
                    for (int i = j + 1; i < n; ++i)
                    {
                        float xij = lum[i, j] / xjj;
                        lum[i, j] = xij;
                        for (int k = j + 1; k < n; ++k)
                            lum[i, k] -= xij * lum[j, k];
                    }
                }

            } // j

            return toggle;
        }


        /// <summary>
        /// Matrix inverse helper.
        /// </summary>
        /// <param name="luMatrix"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float[] Helper(float[,] luMatrix, float[] b)
        {
            int n = luMatrix.GetLength(0);
            float[] x = new float[n];
            b.CopyTo(x, 0);

            for (int i = 1; i < n; ++i)
            {
                float sum = x[i];
                for (int j = 0; j < i; ++j)
                    sum -= luMatrix[i, j] * x[j];
                x[i] = sum;
            }

            x[n - 1] /= luMatrix[n - 1, n - 1];
            for (int i = n - 2; i >= 0; --i)
            {
                float sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= luMatrix[i, j] * x[j];
                x[i] = sum / luMatrix[i, i];
            }

            return x;
        }


        /// <summary>
        /// Calculate the determinant of the matrix.
        /// </summary>
        /// <param name="matrix">matrix</param>
        /// <returns>determinant</returns>
        public static float MatrixDeterminant(float[,] matrix)
        {
            float[,] lum;
            int[] perm;
            int toggle = MatrixDecompose(matrix, out lum, out perm);
            float result = toggle;
            for (int i = 0; i < lum.Length; ++i)
                result *= lum[i, i];
            return result;
        }

        // ----------------------------------------------------------------


        /// <summary>
        /// Create matrix with rows and cols
        /// </summary>
        /// <param name="rows">row</param>
        /// <param name="cols">column</param>
        /// <returns></returns>
        public static float[,] MatrixCreate(int rows, int cols)
        {
            float[,] result = new float[rows, cols];
            return result;
        }


        /// <summary>
        /// matrix product A*B
        /// </summary>
        /// <param name="matrixA">A</param>
        /// <param name="matrixB">B</param>
        /// <returns>A*B</returns>
        /// <exception cref="Exception">row/column not match</exception>
        public static float[,] MatrixProduct(float[,] matrixA, float[,] matrixB)
        {
            int aRows = matrixA.GetLength(0);
            int aCols = matrixA.GetLength(1);
            int bRows = matrixB.GetLength(0);
            int bCols = matrixB.GetLength(1);
            if (aCols != bRows)
                throw new Exception("Non-conformable matrices");

            float[,] result = MatrixCreate(aRows, bCols);

            for (int i = 0; i < aRows; ++i) // each row of A
                for (int j = 0; j < bCols; ++j) // each col of B
                    for (int k = 0; k < aCols; ++k) // could use k < bRows
                        result[i, j] += matrixA[i, k] * matrixB[k, j];

            return result;
        }


        /// <summary>
        /// ToString
        /// </summary>
        /// <param name="matrix">matrix</param>
        /// <returns>String of matrix</returns>
        public static string MatrixAsString(float[,] matrix)
        {
            string s = "";
            for (int i = 0; i < matrix.GetLength(0); ++i)
            {
                for (int j = 0; j < matrix.GetLength(1); ++j)
                    s += matrix[i, j].ToString("F3").PadLeft(8) + " ";
                s += Environment.NewLine;
            }
            return s;
        }


        /// <summary>
        /// The Cholesky decomposition is used to decompose the positive definite matrix into the trigonometric matrix A = L*LT
        /// </summary>
        /// <param name="matrix">Positive definite matrix A</param>
        /// <param name="lowerTri">Lower triangle matrix L</param>
        /// <param name="upperTri">Upper triangle matrix LT</param>
        public static void Cholesky_Decomposition(ref float[,] matrix, out float[,] lowerTri, out float[,] upperTri)
        {
            int n = matrix.GetLength(0);
            float[,] lower = new float[n, n];
            float[,] upper = new float[n, n];

            // Decomposing a matrix
            // into Lower Triangular
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    float sum = 0;

                    // summation for diagonals
                    if (j == i)
                    {
                        for (int k = 0; k < j; k++)
                            sum += (float)Math.Pow(lower[j, k], 2);

                        lower[j, j] = (float)Math.Sqrt(matrix[j, j] - sum);
                    }
                    else
                    {
                        // Evaluating L(i, j)
                        // using L(j, j)
                        for (int k = 0; k < j; k++)
                            sum += (lower[i, k] * lower[j, k]);

                        lower[i, j] = (matrix[i, j] - sum) / lower[j, j];
                    }
                    upper[j, i] = lower[i, j];
                }
            }

            // Displaying Lower
            // Triangular and its Transpose
            /*Debug.Log("  Lower Triangular\t   Transpose");
            string str = "";
            for (int i = 0; i < n; i++)
            { 
                // Lower Triangular
                for (int j = 0; j < n; j++)
                    str += (lower[i, j].ToString() + "\t");
                str += ("");

                // Transpose of
                // Lower Triangular
                for (int j = 0; j < n; j++)
                    str += (upper[i, j].ToString() + "\t");
                str += "\n";
            }
            Debug.Log(str);
            */

            lowerTri = lower;
            upperTri = upper;
        }


        /// <summary>
        /// Use LDLT to solve equation AT*A*x = b
        /// </summary>
        /// <param name="ATA">AT*A</param>
        /// <param name="x">x</param>
        /// <param name="b">b</param>
        public static void EquationSolver_LDLT(ref Matrix ATA, out Matrix x, ref Matrix b)
        {
            int m = ATA.rows;
            if (m <= 0 || ATA.cols != m || b.rows != m)
            {
                x = null;
                return;
            }

            float[] ans = new float[m], y = new float[m];
            Matrix L, D;

            LDLT(ref ATA, out L, out D);

            Matrix lower = L * D;
            Matrix upper = L.T;

            for (int i = 0; i < m; ++i)
            {
                float B = b[i, 0];
                for (int k = 0; k < i; ++k)
                {
                    B -= lower[i, k] * y[k];
                }
                y[i] = B / lower[i, i];
            }

            for (int i = m - 1; i >= 0; --i)
            {
                float Y = y[i];
                for (int k = m - 1; k > i; --k)
                {
                    Y -= upper[i, k] * ans[k];
                }
                ans[i] = Y / upper[i, i];
            }

            x = new Matrix(ans, b.rows, 1);
        }


        /// <summary>
        /// LDLT Decomposition: decompose A into L*D*LT.
        /// </summary>
        /// <param name="A">Symmetric positive definite matrix</param>
        /// <param name="L">Lower triangle matrix</param>
        /// <param name="D">Diagonal matrix</param>
        public static void LDLT(ref Matrix A, out Matrix L, out Matrix D)
        {
            int m = A.rows, n = A.cols;
            if (m != n)
            {
                L = null;
                D = null;
                return;
            }

            L = new Matrix(n, n);
            D = new Matrix(n, n);
            float[] v = new float[n];

            for (int i = 0; i < n; ++i)
            {
                L[i, i] = 1;
                for (int j = 0; j < i; ++j)
                {
                    v[j] = L[i, j] * D[j, j];
                }

                D[i, i] = A[i, i];
                for (int j = 0; j < i; ++j)
                {
                    D[i, i] -= L[i, j] * v[j];
                }

                for (int j = i + 1; j < n; ++j)
                {
                    L[j, i] = A[j, i];
                    for (int k = 0; k < i; ++k)
                    {
                        L[j, i] -= L[j, k] * v[k];
                    }
                    L[j, i] /= D[i, i];
                }
            }

            // Debug.LogWarning("L mat: \n" + L.ToString());
        }


        /// <summary>
        /// QR Decomposition of 2x2 matrix.
        /// </summary>
        /// <param name="m">2x2 matrix</param>
        /// <returns>matrix Q</returns>
        public static Matrix QRDecomposition2x2(Matrix m)
        {
            if (m.cols != 2 || m.rows != 2)
            {
                throw new Exception("Matrix not 2x2 matrix.");
            }

            if (m.cols != 2 || m.rows != 2)
            {
                Debug.LogError("Error, Input matrix not 2x2.");
                return m;
            }

            // Gram-Schmidt Orthogonalization method.
            Vector2 a = new Vector2(m[0, 0], m[1, 0]);
            Matrix q = new Matrix(2, 2);

            q[0, 0] = a.x;
            q[1, 0] = a.y;
            q[0, 1] = -a.y;
            q[1, 1] = a.x;

            return q;
        }


        /// <summary>
        /// Polar Decomposition of 2x2 matrix.
        /// </summary>
        /// <param name="m">2x2 matrix</param>
        /// <returns>matrix Q</returns>
        public static Matrix PolarDecomposition(Matrix m)
        {
            if (m.cols != 2 || m.rows != 2)
            {
                throw new Exception("Matrix not 2x2 matrix.");
            }

            Matrix q = m + new Matrix(new float[,] { { m[1, 1], -m[1, 0] }, { -m[0, 1], m[0, 0] } });

            Vector2 c0 = new Vector2(q[0, 0], q[1, 0]);
            Vector2 c1 = new Vector2(q[0, 1], q[1, 1]);

            float s = c0.magnitude;

            q[0, 0] = c0.x / s;
            q[1, 0] = c0.y / s;
            q[0, 1] = c1.x / s;
            q[1, 1] = c1.y / s;

            return q;
        }
    }

}
