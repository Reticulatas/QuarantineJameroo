using UnityEngine;

namespace SCPSim.Util
{
    public static class Curve
    {
        // The normal distribution function.
        private static float NormalDistribution(float x, float one_over_2pi, float mean,
            float stddev, float var)
        {
            return (float)(one_over_2pi *
                           Mathf.Exp(-(x - mean) * (x - mean) / (2 * var)));
        }

        public static Vector2 Bezier(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            float cx = 3 * (p1.x - p0.x);
            float cy = 3 * (p1.y - p0.y);

            float bx = 3 * (p2.x - p1.x) - cx;
            float by = 3 * (p2.y - p1.y) - cy;

            float ax = p3.x - p0.x - cx - bx;
            float ay = p3.y - p0.y - cy - by;

            float Cube = t * t * t;
            float Square = t * t;

            float resX = (ax * Cube) + (bx * Square) + (cx * t) + p0.x;
            float resY = (ay * Cube) + (by * Square) + (cy * t) + p0.y;

            return new Vector2(resX, resY);
        }

        public static Vector3 CubicBezier(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;

            return p;
        }
    }
}
