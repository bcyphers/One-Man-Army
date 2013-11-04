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
    public class Polygon : CollisionObject
    {
        #region Fields

        internal List<Vector2> relativeVertices;
        internal List<Vector2> trueVertices;
        internal List<Vector2> edges;
        internal bool flipped = false;
        
        #endregion

        #region Properties

        public override float Radius
        {
            get 
            { 
                float minX = float.PositiveInfinity;
                float maxX = float.NegativeInfinity;

                float minY = float.PositiveInfinity;
                float maxY = float.NegativeInfinity;

                for (int i = 0; i < RelativeVertices.Count; i++)
                {
                    minX = MathHelper.Min(RelativeVertices[i].X, minX);
                    maxX = MathHelper.Max(RelativeVertices[i].X, maxX);

                    minY = MathHelper.Min(RelativeVertices[i].Y, minY);
                    maxY = MathHelper.Max(RelativeVertices[i].Y, maxY);
                }

                return (maxX - minX + maxY - minY) / 4;
            }
        }

        /// <summary>
        /// Gets or sets the vertices, relative to the center of the poly.
        /// If the "flipped" value is true, the vertices are all flipped 
        /// across the Y axis before being returned.
        /// </summary>
        public List<Vector2> RelativeVertices
        {
            get
            {
                if (flipped)
                {
                    List<Vector2> verts = new List<Vector2>();

                    foreach (Vector2 vert in relativeVertices)
                        verts.Add(new Vector2(-vert.X, vert.Y));

                    return verts;
                }
                else
                {
                    return relativeVertices;
                }
            }

            set { relativeVertices = value; }
        }

        /// <summary>
        /// The set of vertices in their true world positions.
        /// </summary>
        public List<Vector2> TrueVertices
        {
            get { return trueVertices; }
        }

        /// <summary>
        /// The list of vector directions representing the edges of the polygon
        /// (used in collisions).
        /// </summary>
        public List<Vector2> Edges
        {
            get { return edges; }
        }

        #endregion

        #region Initialization

        public Polygon()
            : base()
        {
            relativeVertices = new List<Vector2>();
            trueVertices = new List<Vector2>();
            edges = new List<Vector2>();
        }

        #endregion
        
        #region Static Methods

        /// <summary>
        /// Returns a polygon with the body and list of relative vertices.
        /// </summary>
        public static Polygon MakePolygon(List<Vector2> vertices)
        {
            Polygon poly = new Polygon();

            poly.RelativeVertices = vertices;

            return poly;
        }
        
        /// <summary>
        /// Makes a rectangle polygon with the specified width, height, and body.
        /// </summary>
        public static Polygon MakeRectanglePolygon(float width, float height)
        {
            Polygon poly = new Polygon();

            List<Vector2> vertices = new List<Vector2>();
            vertices.Add(new Vector2(-width / 2, -height / 2));
            vertices.Add(new Vector2(width / 2, -height / 2));
            vertices.Add(new Vector2(width / 2, height / 2));
            vertices.Add(new Vector2(-width / 2, height / 2));

            poly.RelativeVertices = vertices;

            return poly;
        }


        /// <summary>
        /// Makes a rectangle polygon with the specified width, height, and body.
        /// </summary>
        public static Polygon MakeRectanglePolygon(Rectangle rect)
        {
            Polygon poly = new Polygon();

            poly.Position = new Vector2(rect.Center.X, rect.Center.Y);

            List<Vector2> vertices = new List<Vector2>();
            vertices.Add(new Vector2(-rect.Width / 2, -rect.Height / 2));
            vertices.Add(new Vector2(rect.Width / 2, -rect.Height / 2));
            vertices.Add(new Vector2(rect.Width / 2, rect.Height / 2));
            vertices.Add(new Vector2(-rect.Width / 2, rect.Height / 2));

            poly.RelativeVertices = vertices;

            return poly;
        }

        /// <summary>
        /// Returns an equilateral polygon with the specified number of edges.
        /// </summary>
        public static Polygon MakeEquilateralPolygon(float radius, int numberOfEdges)
        {
            Polygon poly = new Polygon();

            List<Vector2> vertices = new List<Vector2>();
            float stepSize = MathHelper.TwoPi / numberOfEdges;
            for (int i = 0; i < numberOfEdges; i++)
            {
                vertices.Add(new Vector2((float)(radius * Math.Cos(stepSize * i)), (float)(-radius * Math.Sin(stepSize * i))));
            }

            poly.RelativeVertices = vertices;

            return poly;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the AABB of the current coordinates in world space.
        /// </summary>
        public override void GetBoundingBox()
        {
            UpdateVerticesPositions();

            float x1 = trueVertices[0].X;
            float y1 = trueVertices[0].Y;
            float x2 = x1;
            float y2 = y1;

            foreach (Vector2 vertex in trueVertices)
            {
                x1 = MathHelper.Min(x1, vertex.X);
                y1 = MathHelper.Min(y1, vertex.Y);
                x2 = MathHelper.Max(x2, vertex.X);
                y2 = MathHelper.Max(y2, vertex.Y);
            }

            boundingBox.X = (int)x1;
            boundingBox.Y = (int)y1;
            boundingBox.Width = (int)(x2 - x1);
            boundingBox.Height = (int)(y2 - y1);
        }

        /// <summary>
        /// Updates the TrueVertices positions with the new position and
        /// orientation of the body; this is called once per frame.
        /// </summary>
        public void UpdateVerticesPositions()
        {
            Matrix bodyMat = Matrix.CreateRotationZ(Orientation) *
                Matrix.CreateTranslation(Position.X, Position.Y, 0);

            trueVertices.Clear();

            for (int i = 0; i < RelativeVertices.Count; i++)
            {
                trueVertices.Add(Vector2.Transform(RelativeVertices[i], bodyMat));
            }

            GetEdges();
        }

        /// <summary>
        /// Updates the list of edges with the new TrueVertices positions.
        /// </summary>
        public void GetEdges()
        {
            edges.Clear();

            for (int i = 0; i < trueVertices.Count; i++)
            {
                Vector2 vert1 = trueVertices[i];
                Vector2 vert2;
                if (i < trueVertices.Count - 1)
                    vert2 = trueVertices[i + 1];
                else
                    vert2 = trueVertices[0];

                edges.Add(vert2 - vert1);
            }
        }

        #endregion
    }
}