using DG.Tweening;
using UnityEngine;

namespace HiveVolumeRenderer.XR.Tools
{
    public class XRToolRadial : MonoBehaviour
    {
        public XRTool Tool { get; private set; }

        public float TransitionDuration;

        [Header("Icon")]
        public Vector3 ActiveIconScale;
        public Vector3 InactiveIconScale;
        public Color ActiveIconColor;
        public Color InactiveIconColor;

        [Header("Region")]
        public int RegionResolution;
        public Vector3 ActiveRegionScale;
        public Vector3 InactiveRegionScale;
        public Color ActiveRegionColor;
        public Color InactiveRegionColor;

        [Header("References")]
        [SerializeField] private Renderer _iconRenderer;
        [SerializeField] private MeshFilter _regionFilter;
        [SerializeField] private MeshRenderer _regionRenderer;

        public void Inject(XRTool tool)
        {
            Tool = tool;
            _iconRenderer.material.mainTexture = tool.Icon;
        }

        public void SetPosition(float radius, float depth, float midPointDegrees, float arcDegrees)
        {
            Vector3 origin = new Vector3(0f, radius, 0f);
            Quaternion rotation = Quaternion.Euler(0, 0, -midPointDegrees);
            _iconRenderer.transform.localPosition = rotation * origin;

            _regionFilter.transform.localPosition = Vector3.forward * 1.5f * depth;

            GenerateRegion(
                radius: 1.5f * radius, 
                midpointDegrees: midPointDegrees, 
                arcDegrees: arcDegrees
                );
        }

        public void SetActive(bool isActive)
        {
            _iconRenderer.transform.DOScale(isActive ? ActiveIconScale : InactiveIconScale, TransitionDuration);
            _iconRenderer.material.DOColor(isActive ? ActiveIconColor : InactiveIconColor, TransitionDuration);

            _regionRenderer.transform.DOScale(isActive ? ActiveRegionScale : InactiveRegionScale, TransitionDuration);
            _regionRenderer.material.DOColor(isActive ? ActiveRegionColor : InactiveRegionColor, TransitionDuration);
        }

        private void GenerateRegion(float radius, float midpointDegrees, float arcDegrees)
        {
            // Generate the mesh
            Mesh mesh = new Mesh();
            _regionFilter.mesh = mesh;

            // Convert degrees to radians
            float arcRadians = Mathf.Deg2Rad * arcDegrees;
            float midpointAngleRadians = Mathf.Deg2Rad * -midpointDegrees;

            // Create the vertices
            Vector3[] vertices = new Vector3[RegionResolution + 2];
            vertices[0] = Vector3.zero; // Center point

            float angleIncrement = arcRadians / RegionResolution;
            float startAngle = midpointAngleRadians - arcRadians / 2 + Mathf.PI / 2f; // Adjust to set the midpoint angle

            for (int i = 0; i <= RegionResolution; i++)
            {
                float angle = startAngle + angleIncrement * i;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                vertices[i + 1] = new Vector3(x, y, 0);
            }

            // Create the triangles
            int[] triangles = new int[RegionResolution * 3];

            for (int i = 0; i < RegionResolution; i++)
            {
                triangles[i * 3] = 0; // Center point
                triangles[i * 3 + 1] = i + 2; // Define triangle
                triangles[i * 3 + 2] = i + 1; // Define triangle
            }

            // Assign vertices and triangles to the mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;

            // Optionally, calculate normals and UVs
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }
}
