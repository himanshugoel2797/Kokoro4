using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Math
{
    /// 

    /// Represents an octree spatial partioning system.
    /// 

    public class Octree<T>
    {
        private const int ChildNum = 8;
        private int depth = 0;
        private Vector3 center = Vector3.Zero;
        private float length = 0f;
        private BoundingBox bounds = default(BoundingBox);
        private List<T> objects = new List<T>();
        private Octree<T>[] children = null;
        private float worldSize = 0f;


        /// <summary>
        /// Create a new Octree object
        /// </summary>
        /// <param name="worldSize">The size of the world</param>
        /// <param name="depth">The maximum depth the tree may go to</param>
        public Octree(float worldSize, int depth)
            : this(worldSize, depth, Vector3.Zero)
        {
        }

        /// <summary>
        /// Create a new Octree object
        /// </summary>
        /// <param name="worldSize">The size of the world</param>
        /// <param name="depth">The maximum deoth the tree may go to</param>
        /// <param name="center">The location of the tree</param>
        public Octree(float worldSize, int depth, Vector3 center)
        {
            this.worldSize = worldSize;
            this.depth = depth;
            this.center = center;

            // Create the bounding box.
            Vector3 min = this.center - new Vector3(worldSize / 2f);
            Vector3 max = this.center + new Vector3(worldSize / 2f);
            this.bounds = new BoundingBox(min, max);
        }

        /// <summary>
        /// Remove the object
        /// </summary>
        /// <param name="obj">The object to remove</param>
        public void Remove(T obj)
        {
            objects.Remove(obj);
        }

        /// <summary>
        /// Add an object to the octree
        /// </summary>
        /// <param name="o">The object to add</param>
        /// <param name="center">The object's position</param>
        /// <param name="radius">The object's radius</param>
        /// <returns>The octree node the object was added to</returns>
        public Octree<T> Add(T o, Vector3 center, float radius)
        {
            Vector3 min = center - new Vector3(radius);
            Vector3 max = center + new Vector3(radius);
            BoundingBox bounds = new BoundingBox(min, max);

            if (this.bounds.Contains(bounds) == ContainmentType.Contains)
            {
                return this.Add(o, bounds, center, radius);
            }
            return null;
        }

        public void BalanceOctree()
        {
            //TODO Get all the objects in the tree and then clear the tree and place each object in the tree again
            throw new NotImplementedException();
        }

        /// 

        /// Adds the given object to the octree.
        /// 

        public Octree<T> Add(T o, BoundingBox transformebbox)
        {
            float radius = (transformebbox.Max - transformebbox.Min).Length / 2;
            Vector3 center = (transformebbox.Max + transformebbox.Min) / 2;

            if (this.bounds.Contains(transformebbox) == ContainmentType.Contains)
            {
                return this.Add(o, transformebbox, center, radius);
            }
            return null;
        }


        /// 

        /// Adds the given object to the octree.
        /// 

        /// The object to add.
        /// The object's bounds.
        /// The object's center coordinates.
        /// The object's radius.
        private Octree<T> Add(T o, BoundingBox bounds, Vector3 center, float radius)
        {
            if (this.children != null)
            {
                // Find which child the object is closest to based on where the
                // object's center is located in relation to the octree's center.
                int index = (center.X <= this.center.X ? 0 : 1) +
                    (center.Y >= this.center.Y ? 0 : 4) +
                    (center.Z <= this.center.Z ? 0 : 2);

                // Add the object to the child if it is fully contained within
                // it.
                if (this.children[index].bounds.Contains(bounds) == ContainmentType.Contains)
                {
                    return this.children[index].Add(o, bounds, center, radius);
                }
            }
            this.objects.Add(o);
            return this;
        }

        /// <summary>
        /// Get a list of all visible objects in the octree
        /// </summary>
        /// <param name="view">The View Matrix</param>
        /// <param name="projection">The Projection Matrix</param>
        /// <param name="objects">The list of objects</param>
        /// <returns></returns>
        public List<T> GetVisibleObjects(Matrix4 view, Matrix4 projection)
        {
            BoundingFrustum frustum = new BoundingFrustum(projection * view);
            ContainmentType containment = frustum.Contains(this.bounds);

            return GetVisibleObjects(frustum, view, projection, containment);
        }

        public List<T> GetVisibleObjects(BoundingFrustum frustum, Matrix4 view, Matrix4 projection,
            ContainmentType containment)
        {
            List<T> objects = new List<T>();
            int count = 0;

            if (containment != ContainmentType.Contains)
            {
                containment = frustum.Contains(this.bounds);
            }

            // Draw the octree only if it is atleast partially in view.
            if (containment != ContainmentType.Disjoint)
            {
                // Draw the octree's bounds if there are objects in the octree.
                if (this.objects.Count > 0)
                {
                    objects.AddRange(this.objects);
                    count++;
                }

                // Draw the octree's children.
                if (this.children != null)
                {
                    foreach (Octree<T> child in this.children)
                    {
                        objects.AddRange(child.GetVisibleObjects(frustum, view, projection, containment));
                    }
                }
            }

            return objects;
        }


        /// <summary>
        /// Create targetDepth levels of the octree
        /// </summary>
        /// <param name="targetDepth">The target depth relative to the current octree depth</param>
        public void Split(int targetDepth)
        {
            this.children = new Octree<T>[ChildNum];
            int depth = this.depth + 1;
            float quarter = 0;

            this.children[0] = new Octree<T>(this.worldSize,
                depth, this.center + new Vector3(-quarter, quarter, -quarter));
            this.children[1] = new Octree<T>(this.worldSize,
                depth, this.center + new Vector3(quarter, quarter, -quarter));
            this.children[2] = new Octree<T>(this.worldSize,
                depth, this.center + new Vector3(-quarter, quarter, quarter));
            this.children[3] = new Octree<T>(this.worldSize,
                depth, this.center + new Vector3(quarter, quarter, quarter));
            this.children[4] = new Octree<T>(this.worldSize,
                depth, this.center + new Vector3(-quarter, -quarter, -quarter));
            this.children[5] = new Octree<T>(this.worldSize,
                depth, this.center + new Vector3(quarter, -quarter, -quarter));
            this.children[6] = new Octree<T>(this.worldSize,
                depth, this.center + new Vector3(-quarter, -quarter, quarter));
            this.children[7] = new Octree<T>(this.worldSize,
                depth, this.center + new Vector3(quarter, -quarter, quarter));

            if (targetDepth != 0)
            {
                targetDepth--;
                for (int i = 0; i < ChildNum; i++)
                {
                    this.children[i].Split(targetDepth);
                }
            }
        }

    }
}
