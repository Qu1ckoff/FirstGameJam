using UnityEngine;

public class JellyEff : MonoBehaviour
{
    public float Intensity = 1f;
    public float mass = 1f;
    public float stiffness = 1f;
    public float damping = 0.75f;

    private Mesh originalMesh, meshClone;
    private MeshRenderer renderer_;
    private JellyVertex[] jv;
    private Vector3[] vertexArray;
    private 
    void Start()
    {
        originalMesh = GetComponent<MeshFilter>().sharedMesh;
        meshClone = Instantiate(originalMesh);
        GetComponent<MeshFilter>().sharedMesh = meshClone;
        renderer_ = GetComponent<MeshRenderer>();
        jv = new JellyVertex[meshClone.vertices.Length];
        for (int i = 0; i < meshClone.vertices.Length; i++)
        {
            jv[i] = new JellyVertex(i, transform.TransformPoint(meshClone.vertices[i]));
        }
    }

    void FixedUpdate()
    {
        vertexArray = originalMesh.vertices;
        for (int i = 0; i < jv.Length; i++)
        {
            Vector3 target = transform.TransformPoint(vertexArray[jv[i].id]);
            float intensity = (1 - (renderer_.bounds.max.y - target.y) /  renderer_.bounds.size.y) * Intensity;
            jv[i].Shake(target, mass, stiffness, damping);
            target = transform.InverseTransformPoint(jv[i].Position);
            vertexArray[jv[i].id] = Vector3.Lerp(vertexArray[jv[i].id], target, intensity);
        }
        meshClone.vertices = vertexArray;
    }

    public class JellyVertex
    {
        public int id;
        public Vector3 Position;
        public Vector3 velocity, force;

        public JellyVertex(int _id, Vector3 _pos)
        {
            id = _id;
            Position = _pos;
        }

        public void Shake(Vector3 target, float m, float s, float d)
        {
            force = (target - Position) * s;
            velocity = (velocity + force / m) * d;
            Position += velocity;
            if ((velocity + force + force / m).magnitude < 0.001f)
            {
                Position = target;
            }
        }
    }

}
