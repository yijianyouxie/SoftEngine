using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SoftEngine
{
    class Mesh
    {
        public string Name { get; set; }

        public Vector3[] Vertices { get; private set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public Mesh(string name, int verticesCount)
        {
            Vertices = new Vector3[verticesCount];
            Name = name;
        }
    }
}
