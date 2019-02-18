using Kokoro.Physics.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Physics
{
    public class PhysicsWorld
    {
        internal Dictionary<int, PhysicsObject> PhysicsObjects;
        private Dictionary<string, int> PhysicsObjectIDs;
        private int ObjectIDCounter = 1;

        private BroadphaseCollisionDetector Broadphase;

        public PhysicsWorld()
        {
            PhysicsObjects = new Dictionary<int, PhysicsObject>();
            PhysicsObjectIDs = new Dictionary<string, int>();

            Broadphase = new BroadphaseCollisionDetector(this);
        }

        #region Object Access Management
        public void AddObject(PhysicsObject obj)
        {
            PhysicsObjectIDs[obj.Name] = ObjectIDCounter;
            PhysicsObjects[ObjectIDCounter] = obj;

            Broadphase.AddObject(ObjectIDCounter);

            ObjectIDCounter++;
        }

        public void RemoveObject(string name)
        {
            PhysicsObjects.Remove(PhysicsObjectIDs[name]);
            PhysicsObjectIDs.Remove(name);
        }

        public bool ContainsObject(string name)
        {
            return PhysicsObjectIDs.ContainsKey(name);
        }

        public PhysicsObject this[string name]
        {
            get
            {
                return PhysicsObjects[PhysicsObjectIDs[name]];
            }
            set
            {
                AddObject(value);
            }
        }
        #endregion

        //TODO: Build a graphics list from this, submit frustums to cull to all available threads, then perform occlusion culling via cpu rasterization

        public void Update(double interval)
        {

        }
    }
}
