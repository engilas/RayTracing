namespace RayTracing
{
    public class Helpers
    {
        public static double[,] TransponMatrix(double[,] matrix)
        {
            var rows = matrix.GetLength(0);
            var cols = matrix.GetLength(1);

            var res = new double[cols, rows];
            for (var i = 0; i < rows; i++)
            for (var j = 0; j < cols; j++)
                res[j, i] = matrix[i, j];
            return res;
        }
    }
}