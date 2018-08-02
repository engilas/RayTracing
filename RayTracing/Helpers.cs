using System;

namespace RayTracing
{
    public class Helpers
    {
	    public static double[,] TransponMatrix(double[,] matrix)
	    {
		    var rows = matrix.GetLength(0);
		    var cols = matrix.GetLength(1);

		    var res = new double[cols, rows];
		    for (int i = 0; i < rows; i++)
		    {
			    for (int j = 0; j < cols; j++)
			    {
				    res[j, i] = matrix[i, j];
			    }
		    }
		    return res;
	    }

	    public static double[,] GetXRotation(double grad) {
		    var xRad = Math.PI / 180 * grad;
		    var sin = Math.Sin(xRad);
		    var cos = Math.Cos(xRad);
		    return Helpers.TransponMatrix(new [,] {
			    {1, 0, 0},
			    {0, cos, -sin},
			    {0, sin, cos}
		    });
	    }

	    public static double[,] GetYRotation(double grad) {
		    var xRad = Math.PI / 180 * grad;
		    var sin = Math.Sin(xRad);
		    var cos = Math.Cos(xRad);
		    return Helpers.TransponMatrix(new [,] {
			    {cos, 0, sin},
			    {0, 1, 0},
			    {-sin, 0, cos}
		    });
	    }

	    public static double[,] GetZRotation(double grad) {
		    var xRad = Math.PI / 180 * grad;
		    var sin = Math.Sin(xRad);
		    var cos = Math.Cos(xRad);
		    return Helpers.TransponMatrix(new [,] {
			    {cos, -sin, 0},
			    {sin, cos, 0},
			    {0, 0, 1}
		    });
	    }
    }
}
