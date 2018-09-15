using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing
{
    class RotationMatrix
    {
        private readonly double[,] _xRotation;
        private readonly double[,] _yRotation;
        private readonly double[,] _zRotation;

        public RotationMatrix(double xGrad, double yGrad, double zGrad)
        {
            _xRotation = GetXRotation(xGrad);
            _yRotation = GetYRotation(yGrad);
            _zRotation = GetZRotation(zGrad);
        }

        public double[,] X => _xRotation;
        public double[,] Y => _yRotation;
        public double[,] Z => _zRotation;

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
