using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [BoxGroup("Settings"), LabelWidth(150)]
    public float ViewRadius;

    [BoxGroup("Settings"), LabelWidth(150)] [Range(0, 360)]
    public float ViewAngle;


    [FoldoutGroup("Settings/Advanced Settings")] [BoxGroup("Settings/Advanced Settings/Calculation"), LabelWidth(150)]
    public LayerMask ObstacleMask;

    [BoxGroup("Settings/Advanced Settings/Calculation"), LabelWidth(150)]
    public Vector3 CenterPoint;

    [BoxGroup("Settings/Advanced Settings/Calculation"), LabelWidth(150), Range(0, 1)]
    public float MeshResolution;

    [BoxGroup("Settings/Advanced Settings/Calculation"), LabelWidth(150)]
    public int EdgeResolveIterations;

    [BoxGroup("Settings/Advanced Settings/Calculation"), LabelWidth(150)]
    public float EdgeDistanceThreshold;

    [BoxGroup("Settings/Advanced Settings/Visualisation"), LabelWidth(150)]
    public Vector3 ViewMeshOffset;

    [BoxGroup("Settings/Advanced Settings/Visualisation"), LabelWidth(150)]
    public MeshFilter ViewMeshFilter;

    [BoxGroup("Settings/Advanced Settings/Visualisation"), LabelWidth(150)]
    private Mesh _viewMesh;


    private void Start() {
        _viewMesh = new Mesh();
        _viewMesh.name = "View Mesh";
        ViewMeshFilter.mesh = _viewMesh;
    }

    private void LateUpdate() {
        DrawFieldOfView();
    }

    private void Update() {
        ViewMeshFilter.transform.position = transform.position + ViewMeshOffset;
    }

    public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal) {
        if (!angleIsGlobal) {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    void DrawFieldOfView() {
        int stepCount = Mathf.RoundToInt(ViewAngle * MeshResolution);
        float stepAngleSize = ViewAngle / stepCount;

        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= stepCount; i++) {
            float angle = transform.eulerAngles.y - ViewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0) {
                bool edgeDistanceThresholdExceeded =
                    Mathf.Abs(oldViewCast.Distance - newViewCast.Distance) > EdgeDistanceThreshold;
                if (oldViewCast.Hit != newViewCast.Hit ||
                    (oldViewCast.Hit && newViewCast.Hit && edgeDistanceThresholdExceeded)) {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.PointA != Vector3.zero) viewPoints.Add(edge.PointA);
                    if (edge.PointB != Vector3.zero) viewPoints.Add(edge.PointB);
                }
            }

            viewPoints.Add(newViewCast.Point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = CenterPoint;
        for (int i = 0; i < vertexCount - 1; i++) {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2) {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        _viewMesh.Clear();
        _viewMesh.vertices = vertices;
        _viewMesh.triangles = triangles;
        _viewMesh.RecalculateNormals();
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast) {
        float minAngle = minViewCast.Angle;
        float maxAngle = maxViewCast.Angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < EdgeResolveIterations; i++) {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDistanceThresholdExceeded =
                Mathf.Abs(minViewCast.Distance - newViewCast.Distance) > EdgeDistanceThreshold;
            if (newViewCast.Hit == minViewCast.Hit && !edgeDistanceThresholdExceeded) {
                minAngle = angle;
                minPoint = newViewCast.Point;
            }
            else {
                maxAngle = angle;
                maxPoint = newViewCast.Point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    ViewCastInfo ViewCast(float globalAngle) {
        Vector3 dir = DirectionFromAngle(globalAngle, true);
        RaycastHit hit;


        if (Physics.Raycast((transform.position + CenterPoint), dir, out hit, ViewRadius, ObstacleMask)) {
//            Debug.DrawLine(transform.position, hit.point, Color.red);
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else {
//            Debug.DrawLine(transform.position, hit.point, Color.red);
            return new ViewCastInfo(false, (transform.position + CenterPoint) + dir * ViewRadius, ViewRadius, globalAngle);
        }
    }

    public struct EdgeInfo
    {
        public Vector3 PointA;
        public Vector3 PointB;

        public EdgeInfo(Vector3 pointA, Vector3 pointB) {
            PointA = pointA;
            PointB = pointB;
        }
    }

    public struct ViewCastInfo
    {
        public bool Hit;
        public Vector3 Point;
        public float Distance;
        public float Angle;

        public ViewCastInfo(bool hit, Vector3 point, float distance, float angle) {
            Hit = hit;
            Point = point;
            Distance = distance;
            Angle = angle;
        }
    }
}