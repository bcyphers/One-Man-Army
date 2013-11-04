using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace One_Man_Army
{
    /// <summary>
    /// A static class used for dealing with SAT collisions.
    /// </summary>
    public static class Collisions
    {
        #region Public Methods

        /// <summary>
        /// Checks the bounding boxes of two polys, returning true if they intersect.
        /// </summary>
        public static bool BoundingBoxCollide(CollisionObject obj1, CollisionObject obj2)
        {
            obj1.GetBoundingBox();
            obj2.GetBoundingBox();

            if (obj1.boundingBox.Intersects(obj2.boundingBox))
                return true;

            return false;
        }

        /// <summary>
        /// Check if polygon A is going to collide with polygon B, using 
        /// Separating Axis Theorum.
        /// </summary>
        public static bool DoCollision(Polygon polygonA,
                              Polygon polygonB)
        {
            if (!BoundingBoxCollide(polygonA, polygonB))
                return false;

            bool Intersect = true;

            int edgeCountA = polygonA.Edges.Count;
            int edgeCountB = polygonB.Edges.Count;
            Vector2 edge;

            // Loop through all the edges of each polygon, checking to see if
            // any are NOT intersecting.
            for (int edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++)
            {
                if (edgeIndex < edgeCountA)
                    edge = polygonA.Edges[edgeIndex];
                else
                    edge = polygonB.Edges[edgeIndex - edgeCountA];

                edge.Normalize();

                // Find the axis perpendicular to the current edge
                Vector2 axis = new Vector2(-edge.Y, edge.X);

                // Find the projection of the polygon on the current axis
                float minA = 0; float minB = 0; float maxA = 0; float maxB = 0;
                ProjectPolygon(axis, polygonA, ref minA, ref maxA);
                ProjectPolygon(axis, polygonB, ref minB, ref maxB);

                // Check if the polygon projections are currentlty intersecting. If not, break the loop.
                if (IntervalDistance(minA, maxA, minB, maxB) > 0)
                {
                    Intersect = false;
                    break;
                }
            }

            return Intersect;
        }

        /// <summary>
        /// Check if a polygon is colliding with a circle.
        /// </summary>
        public static bool DoCollision(Polygon polygon,
                                Circle circle)
        {
            polygon.GetBoundingBox();
            circle.GetBoundingBox();

            if (!BoundingBoxCollide(polygon, circle))
                return false;

            bool Intersect = true;

            int edgeCount = polygon.Edges.Count;
            Vector2 edge;

            // Loop through all the edges of each polygon, checking to see if
            // any are NOT intersecting.
            for (int edgeIndex = 0; edgeIndex < edgeCount; edgeIndex++)
            {
                edge = polygon.Edges[edgeIndex];

                edge.Normalize();

                // Find the axis perpendicular to the current edge
                Vector2 axis = new Vector2(-edge.Y, edge.X);

                // Find the projection of the polygon on the current axis
                float minA = 0; float minB = 0; float maxA = 0; float maxB = 0;
                ProjectPolygon(axis, polygon, ref minA, ref maxA);
                ProjectCircle(axis, circle, ref minB, ref maxB);

                // Check if the polygon projections are currentlty intersecting. If not, break the loop.
                if (IntervalDistance(minA, maxA, minB, maxB) > 0)
                {
                    Intersect = false;
                    break;
                }
            }

            return Intersect;
        }

        /// <summary>
        /// Check if a polygon is colliding with a rectangle.
        /// </summary>
        public static bool DoCollision(Polygon polygon,
                              Rectangle rectangle)
        {
            Polygon rectPoly = Polygon.MakeRectanglePolygon(rectangle);

            return DoCollision(polygon, rectPoly);
        }

        /// <summary>
        /// Check if a circle is colliding with a rectangle.
        /// </summary>
        public static bool DoCollision(Circle circle,
                              Rectangle rectangle)
        {
            BoundingSphere sphere = new BoundingSphere(new Vector3(circle.Position, 0), circle.radius);
            BoundingBox box = new BoundingBox(new Vector3(rectangle.X, rectangle.Y, 0),
                new Vector3(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height, 0));

            return sphere.Intersects(box);
        }


        #endregion

        #region Private Methods

        /// <summary>
        /// Calculate the projection of a polygon on an axis
        /// and returns it as a [min, max] interval
        /// </summary>
        private static void ProjectPolygon(Vector2 axis, Polygon polygon,
                                   ref float min, ref float max)
        {
            // To project a point on an axis, use the dot product
            float dotProduct = Vector2.Dot(axis, polygon.TrueVertices[0]);
            min = dotProduct;
            max = dotProduct;
            for (int i = 0; i < polygon.TrueVertices.Count; i++)
            {
                dotProduct = Vector2.Dot(polygon.TrueVertices[i], axis);
                if (dotProduct < min)
                {
                    min = dotProduct;
                }
                else
                {
                    if (dotProduct > max)
                    {
                        max = dotProduct;
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the projection of a circle on an axis
        /// and returns it as a [min, max] interval
        /// </summary>
        private static void ProjectCircle(Vector2 axis, Circle circle,
                                   ref float min, ref float max)
        {
            // To project a point on an axis, use the dot product
            min = Vector2.Dot(axis, circle.Position - axis * circle.radius);
            max = Vector2.Dot(axis, circle.Position + axis * circle.radius);
        }

        /// <summary>
        /// Calculate the distance between [minA, maxA] and [minB, maxB]
        /// The distance will be negative if the intervals overlap
        /// </summary>
        private static float IntervalDistance(float minA, float maxA, float minB, float maxB)
        {
            if (minA < minB)
                return minB - maxA;
            else
                return minA - maxB;
        }
 
        /// <summary>
        /// Method used to find if a point is inside a specified polygon.
        /// </summary>
        internal static bool PointIntersects(Vector2 point, List<Vector2> polygon)
        {

            int counter = 0;
            int i;

            Vector2 p1 = polygon[0];
            for (i = 1; i <= polygon.Count; i++)
            {
                Vector2 p2 = polygon[i % polygon.Count];
                if (point.Y > Math.Min(p1.Y, p2.Y))
                {
                    if (point.Y <= Math.Max(p1.Y, p2.Y))
                    {
                        if (point.X <= Math.Max(p1.X, p2.X))
                        {
                            if (p1.Y != p2.Y)
                            {
                                double xinters = (point.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;
                                if (p1.X == p2.X || point.X <= xinters)
                                    counter++;
                            }
                        }
                    }
                }
                p1 = p2;
            }

            if (counter % 2 == 0)
                return false;

            return true;
        }

        #endregion
    }
}
