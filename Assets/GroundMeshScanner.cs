using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GroundMeshScanner : MonoBehaviour
{
    [Header("Scan Settings")]
    public float scanRadius = 2f;
    public float scanResolution = 0.5f;
    public float scanHeight = 2f;
    public float scanDepth = 5f;
    public LayerMask groundLayer;

    private Vector3[,] hitPoints;
    private MeshFilter meshFilter;
    private Mesh scannedMesh;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        InvokeRepeating(nameof(ScanAndBuildMesh), 0.5f, 1f);
    }

    void ScanAndBuildMesh()
    {
        PerformScan3();
        BuildMesh();
    }

void PerformScan()
{
    int gridSize = Mathf.RoundToInt((scanRadius * 2) / scanResolution) + 1;
    hitPoints = new Vector3[gridSize, gridSize];

    Vector3 origin = transform.position+Vector3.up*0.5f;

    for (int x = 0; x < gridSize; x++)
    {
        for (int z = 0; z < gridSize; z++)
        {
            float offsetX = (x * scanResolution) - scanRadius;
            float offsetZ = (z * scanResolution) - scanRadius;

            Vector3 rayOrigin = origin + new Vector3(offsetX, scanHeight, offsetZ);
            Ray ray = new Ray(rayOrigin, Vector3.down);

            float radius = 0.1f; // Tweak to match ladder rung size
            if (Physics.SphereCast(ray, radius, out RaycastHit hit, scanHeight + scanDepth, groundLayer))
            {
                hitPoints[x, z] = transform.InverseTransformPoint(hit.point);
            }
            else
            {
                hitPoints[x, z] = Vector3.positiveInfinity;
            }
        }
    }
}
void PerformScan2()
{
    int gridSize = Mathf.RoundToInt((scanRadius * 2) / scanResolution) + 1;
    hitPoints = new Vector3[gridSize, gridSize];

    Vector3 origin = transform.position + Vector3.up * 0.5f;

    for (int x = 0; x < gridSize; x++)
    {
        for (int z = 0; z < gridSize; z++)
        {
            float offsetX = (x * scanResolution) - scanRadius;
            float offsetZ = (z * scanResolution) - scanRadius;

            Vector3 boxOrigin = origin + new Vector3(offsetX, scanHeight, offsetZ);
            Vector3 halfExtents = new Vector3(scanResolution * 0.5f, 0.01f, scanResolution * 0.5f); // thin box

            // Cast a thin box downward
            RaycastHit[] hits = Physics.BoxCastAll(
                boxOrigin,
                halfExtents,
                Vector3.down,
                Quaternion.identity,
                scanHeight + scanDepth,
                groundLayer
            );

            // Pick the lowest hit point
            if (hits.Length > 0)
            {
                Vector3 bestPoint = Vector3.positiveInfinity;
                float shortestDistance = float.MaxValue;
                
                foreach (var hit in hits)
                {
                    if (hit.distance < shortestDistance)
                    {
                        shortestDistance = hit.distance;
                        bestPoint = hit.point;
                    }
                }

                if (bestPoint != Vector3.positiveInfinity)
                    hitPoints[x, z] = transform.InverseTransformPoint(bestPoint);
                else
                    hitPoints[x, z] = Vector3.positiveInfinity;
            }
            else
            {
                hitPoints[x, z] = Vector3.positiveInfinity;
            }
        }
    }
}

void PerformScan3()
{
    int gridSize = Mathf.RoundToInt((scanRadius * 2) / scanResolution) + 1;
    hitPoints = new Vector3[gridSize, gridSize];

    Vector3 origin = transform.position + Vector3.up * 0.5f;

    for (int x = 0; x < gridSize; x++)
    {
        for (int z = 0; z < gridSize; z++)
        {
            float offsetX = (x * scanResolution) - scanRadius;
            float offsetZ = (z * scanResolution) - scanRadius;

            Vector3 boxOrigin = origin + new Vector3(offsetX, scanHeight, offsetZ);
            Vector3 halfExtents = new Vector3(scanResolution * 0.5f, 0.01f, scanResolution * 0.5f); // thin box

            // Cast a thin box downward
            RaycastHit[] hits = Physics.SphereCastAll(
                boxOrigin,
                scanResolution*scanResolution,
                Vector3.down,
                scanHeight + scanDepth,
                groundLayer
            );

            // Pick the lowest hit point
            if (hits.Length > 0)
            {
                Vector3 bestPoint = Vector3.positiveInfinity;
                float shortestDistance = float.MaxValue;
                
                foreach (var hit in hits)
                {
                    if (hit.distance < shortestDistance)
                    {
                        shortestDistance = hit.distance;
                        bestPoint = hit.point;
                    }
                }

                if (bestPoint != Vector3.positiveInfinity)
                    hitPoints[x, z] = transform.InverseTransformPoint(bestPoint);
                else
                    hitPoints[x, z] = Vector3.positiveInfinity;
            }
            else
            {
                hitPoints[x, z] = Vector3.positiveInfinity;
            }
        }
    }
}


void BuildMesh()
{
    if (hitPoints == null) return;

    int gridSize = hitPoints.GetLength(0);
    Vector3[] vertices = new Vector3[gridSize * gridSize];
    // We'll collect only valid triangles dynamically
    var trianglesList = new System.Collections.Generic.List<int>();

    int vi = 0;
    for (int x = 0; x < gridSize; x++)
    {
        for (int z = 0; z < gridSize; z++)
        {
            vertices[vi++] = hitPoints[x, z];
        }
    }

    // Build triangles, skip invalid ones
    for (int x = 0; x < gridSize - 1; x++)
    {
        for (int z = 0; z < gridSize - 1; z++)
        {
            int topLeft = x * gridSize + z;
            int topRight = (x + 1) * gridSize + z;
            int bottomLeft = x * gridSize + (z + 1);
            int bottomRight = (x + 1) * gridSize + (z + 1);

            // Check if any vertex is invalid (positive infinity)
            if (IsValidVertex(vertices[topLeft]) && IsValidVertex(vertices[bottomRight]) && IsValidVertex(vertices[bottomLeft]))
            {
                trianglesList.Add(topLeft);
                trianglesList.Add(bottomRight);
                trianglesList.Add(bottomLeft);
            }

            if (IsValidVertex(vertices[topLeft]) && IsValidVertex(vertices[topRight]) && IsValidVertex(vertices[bottomRight]))
            {
                trianglesList.Add(topLeft);
                trianglesList.Add(topRight);
                trianglesList.Add(bottomRight);
            }
        }
    }

    scannedMesh = new Mesh();
    scannedMesh.name = "ScannedWireMesh";
    scannedMesh.vertices = vertices;
    scannedMesh.triangles = trianglesList.ToArray();
    scannedMesh.RecalculateNormals();

    meshFilter.sharedMesh = scannedMesh;
}

bool IsValidVertex(Vector3 v)
{
    return v != Vector3.positiveInfinity;
}


    void OnDrawGizmosSelected()
    {
        if (scannedMesh != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireMesh(scannedMesh, transform.position, transform.rotation, transform.localScale);
        }
    }
}

public class ConnectedPlanes : MonoBehaviour
{
    public Camera mainCamera;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                MeshCollider meshCollider = hit.collider as MeshCollider;
                if (meshCollider == null || meshCollider.sharedMesh == null)
                    return;

                Mesh mesh = meshCollider.sharedMesh;
                Transform meshTransform = meshCollider.transform;

                List<Plane> connectedPlanes = GetConnectedPlanes(mesh, hit.triangleIndex, meshTransform);
                Debug.Log($"Connected planes count: {connectedPlanes.Count}");
            }
        }
    }

    List<Plane> GetConnectedPlanes(Mesh mesh, int triangleIndex, Transform transform)
    {
        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;

        // Get the main triangle
        int i0 = triangles[triangleIndex * 3];
        int i1 = triangles[triangleIndex * 3 + 1];
        int i2 = triangles[triangleIndex * 3 + 2];

        Vector3 v0 = transform.TransformPoint(vertices[i0]);
        Vector3 v1 = transform.TransformPoint(vertices[i1]);
        Vector3 v2 = transform.TransformPoint(vertices[i2]);

        Plane basePlane = new Plane(v0, v1, v2);

        // Represent the triangle's edges as unordered pairs
        var baseEdges = new HashSet<Edge>
        {
            new Edge(i0, i1),
            new Edge(i1, i2),
            new Edge(i2, i0)
        };

        List<Plane> connectedPlanes = new List<Plane>();

        int triangleCount = triangles.Length / 3;
        for (int t = 0; t < triangleCount; t++)
        {
            if (t == triangleIndex)
                continue;

            int j0 = triangles[t * 3];
            int j1 = triangles[t * 3 + 1];
            int j2 = triangles[t * 3 + 2];

            var edges = new HashSet<Edge>
            {
                new Edge(j0, j1),
                new Edge(j1, j2),
                new Edge(j2, j0)
            };

            // Check if it shares an edge
            foreach (var edge in edges)
            {
                if (baseEdges.Contains(edge))
                {
                    Vector3 w0 = transform.TransformPoint(vertices[j0]);
                    Vector3 w1 = transform.TransformPoint(vertices[j1]);
                    Vector3 w2 = transform.TransformPoint(vertices[j2]);
                    connectedPlanes.Add(new Plane(w0, w1, w2));
                    break;
                }
            }
        }

        return connectedPlanes;
    }

    // Helper struct for edge comparison
    struct Edge
    {
        public int a, b;

        public Edge(int a, int b)
        {
            if (a < b)
            {
                this.a = a;
                this.b = b;
            }
            else
            {
                this.a = b;
                this.b = a;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Edge edge && a == edge.a && b == edge.b;
        }

        public override int GetHashCode()
        {
            return (a * 397) ^ b;
        }
    }
}