using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kokoro.Physics
{
    public class PhysicsWorld
    {
        DynamicsWorld world;
        CollisionConfiguration config;
        CollisionDispatcher dispatcher;
        BroadphaseInterface broadphase;

        public PhysicsWorld()
        {
            config = new DefaultCollisionConfiguration();
            dispatcher = new CollisionDispatcher(config);
            broadphase = new DbvtBroadphase();

            world = new DiscreteDynamicsWorld(dispatcher, broadphase, null, config);
        }
    }
}
