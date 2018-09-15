using System;
using System.Threading;

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
    }
}
