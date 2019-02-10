using System;

namespace RayTracing
{
    class RotationMatrix
    {

	    public readonly double[,] Rotation;
	    public readonly double[,] RotationInv;

        public RotationMatrix(double xGrad, double yGrad, double zGrad)
        {
			Rotation = MultiplyMatrixes(MultiplyMatrixes(GetZRotation(zGrad), GetYRotation(yGrad)), GetXRotation(xGrad));
            RotationInv = MultiplyMatrixes(MultiplyMatrixes(GetXRotation(-xGrad), GetYRotation(-yGrad)), GetZRotation(-zGrad));
        }

        public static double[,] FromEuler(double pitch, double yaw, double roll)
        {
            double GetRad(double grad) => Math.PI / 180 * grad;
            
            var s1 = Math.Sin(GetRad(pitch));
            var c1 = Math.Cos(GetRad(pitch));
            var s2 = Math.Sin(GetRad(yaw));
            var c2 = Math.Cos(GetRad(yaw));
            var s3 = Math.Sin(GetRad(roll));
            var c3 = Math.Cos(GetRad(roll));

            var matrix = Helpers.TransponMatrix(new[,] {
                {c2, - c3 * s2,s2 * s3},
                {c1 * s2, c1 * c2 * c3 - s1 * s3, - c3 * s1 - c1 * c2 * s3},
                {s1 * s2, c1 * s3 + c2 * c3 * s1, c1 * c3 - c2 * s1 * s3}
            });

            return matrix;
        }

	    private double[,] MultiplyMatrixes(double[,] m1, double[,] m2) {
		    var copy = new double[3, 3];

		    for (var row = 0; row < 3; row++) {
			    for (var col = 0; col < 3; col++) {
				    var sum = 0.0;

				    for (var j = 0; j < 3; j++) {
					    sum += m1[row, j] * m2[j, col];
				    }

				    copy[row, col] = sum;
			    }
		    }

		    return copy;
	    }

        private double[,] GetXRotation(double grad)
        {
            var xRad = Math.PI / 180 * grad;
            var sin = Math.Sin(xRad);
            var cos = Math.Cos(xRad);
            return Helpers.TransponMatrix(new[,] {
                {1, 0, 0},
                {0, cos, -sin},
                {0, sin, cos}
            });
        }

        private double[,] GetYRotation(double grad)
        {
            var xRad = Math.PI / 180 * grad;
            var sin = Math.Sin(xRad);
            var cos = Math.Cos(xRad);
            return Helpers.TransponMatrix(new[,] {
                {cos, 0, sin},
                {0, 1, 0},
                {-sin, 0, cos}
            });
        }

        private double[,] GetZRotation(double grad)
        {
            var xRad = Math.PI / 180 * grad;
            var sin = Math.Sin(xRad);
            var cos = Math.Cos(xRad);
            return Helpers.TransponMatrix(new[,] {
                {cos, -sin, 0},
                {sin, cos, 0},
                {0, 0, 1}
            });
        }
    }
}
