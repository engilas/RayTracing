using System;

namespace RayTracing
{
    internal class RotationMatrix
    {
        //   private readonly double[,] _xRotation;
        //   private readonly double[,] _yRotation;
        //   private readonly double[,] _zRotation;

        //private readonly double[,] _xRotationInv;
        //private readonly double[,] _yRotationInv;
        //private readonly double[,] _zRotationInv;

        public readonly double[,] Rotation;
        public readonly double[,] RotationInv;

        public RotationMatrix(double xGrad, double yGrad, double zGrad)
        {
            Rotation = MultiplyMatrixes(MultiplyMatrixes(GetZRotation(zGrad), GetYRotation(yGrad)),
                GetXRotation(xGrad));
            RotationInv = MultiplyMatrixes(MultiplyMatrixes(GetXRotation(-xGrad), GetYRotation(-yGrad)),
                GetZRotation(-zGrad));

            //      Truncate(Rotation);
            //Truncate(RotationInv);
            //   _xRotation = GetXRotation(xGrad);
            //   _yRotation = GetYRotation(yGrad);
            //   _zRotation = GetZRotation(zGrad);

            //_xRotationInv = GetXRotation(-xGrad);
            //_yRotationInv = GetYRotation(-yGrad);
            //_zRotationInv = GetZRotation(-zGrad);

            //Rotation = Times(Times(_xRotation, _xRotation), _zRotation);
        }

        //private void Truncate(double[,] arr) {
        // for (int i = 0; i < 3; i++) {
        //  for (int j = 0; j < 3; j++) {
        //   if (Math.Abs(arr[i, j]) < 1e-10) {
        //    arr[i, j] = 0;
        //   }
        //  }
        // }
        //}

        private double[,] MultiplyMatrixes(double[,] m1, double[,] m2)
        {
            var copy = new double[3, 3];

            for (var row = 0; row < 3; row++)
            for (var col = 0; col < 3; col++)
            {
                var sum = 0.0;

                for (var j = 0; j < 3; j++) sum += m1[row, j] * m2[j, col];

                copy[row, col] = sum;
            }

            return copy;
        }


        //public double[,] X => _xRotation;
        //public double[,] Y => _yRotation;
        //public double[,] Z => _zRotation;

        //public double[,] XInv => _xRotationInv;
        //public double[,] YInv => _yRotationInv;
        //public double[,] ZInv => _zRotationInv;

        private double[,] GetXRotation(double grad)
        {
            var xRad = Math.PI / 180 * grad;
            var sin = Math.Sin(xRad);
            var cos = Math.Cos(xRad);
            return Helpers.TransponMatrix(new[,]
            {
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
            return Helpers.TransponMatrix(new[,]
            {
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
            return Helpers.TransponMatrix(new[,]
            {
                {cos, -sin, 0},
                {sin, cos, 0},
                {0, 0, 1}
            });
        }
    }
}