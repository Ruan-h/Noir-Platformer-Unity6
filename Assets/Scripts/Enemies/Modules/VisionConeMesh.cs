using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VisionConeMesh : MonoBehaviour
{
    [Header("Configurações da Malha")]
    [SerializeField] private int rayCount = 50;
    [SerializeField] private float updateDelay = 0.05f;

    [Header("Cores da Visão")]
    [SerializeField] private Color patrolColor = new Color(1f, 1f, 0f, 0.4f);
    [SerializeField] private Color chaseColor = new Color(1f, 0f, 0f, 0.4f);

    [Header("Referências")]
    [SerializeField] private IEnemyVision visionSource;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock matPropertyBlock;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        matPropertyBlock = new MaterialPropertyBlock();
        
        visionSource = GetComponentInParent<IEnemyVision>();

        mesh = new Mesh();
        mesh.name = "VisionConeMesh";
        meshFilter.mesh = mesh;
    }

    private void OnEnable()
    {
        if (visionSource == null) visionSource = GetComponentInParent<IEnemyVision>();

        if (mesh != null) mesh.Clear();

        Physics2D.SyncTransforms();

        GenerateConeMesh();

        StartCoroutine(UpdateConeRoutine());
    }

    private void OnDisable()
    {

        StopAllCoroutines();
        if (mesh != null) mesh.Clear();
    }


    private IEnumerator UpdateConeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateDelay);
            
            if (visionSource == null)
            {
                if(mesh != null) mesh.Clear();
                yield break;
            }
            
            GenerateConeMesh();
        }
    }

    private void GenerateConeMesh()
    {
        if (visionSource == null || mesh == null) return;

        float range = visionSource.VisionRange;
        float angle = visionSource.VisionAngle;
        Transform eyePos = visionSource.EyePosition;
        LayerMask obstacles = visionSource.ObstacleLayer;
        bool facingRight = visionSource.IsFacingRight;
        bool isAlert = visionSource.IsAlert; 

        Color targetColor = isAlert ? chaseColor : patrolColor;
        matPropertyBlock.SetColor("_BaseColor", targetColor);
        meshRenderer.SetPropertyBlock(matPropertyBlock);

        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        Vector3 rayOrigin = eyePos.position;
        vertices[0] = transform.InverseTransformPoint(rayOrigin);

        float angleStep = angle / rayCount;
        float currentRelativeAngle = -angle / 2; 
        
        float baseWorldAngle = eyePos.eulerAngles.z;

        if (!facingRight && Mathf.Abs(Mathf.DeltaAngle(baseWorldAngle, 0f)) < 1f)
        {
            baseWorldAngle += 180f;
        }

        for (int i = 0; i <= rayCount; i++)
        {
            float worldAngle = baseWorldAngle + currentRelativeAngle;

            Vector3 rayDirection = new Vector3(
                Mathf.Cos(worldAngle * Mathf.Deg2Rad),
                Mathf.Sin(worldAngle * Mathf.Deg2Rad),
                0
            );

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, range, obstacles);

            Vector3 vertexPosition;

            if (hit.collider == null)
            {
                vertexPosition = rayOrigin + rayDirection * range;
            }
            else
            {
                vertexPosition = hit.point;
            }

            vertices[i + 1] = transform.InverseTransformPoint(vertexPosition);
            currentRelativeAngle += angleStep;
        }

        for (int i = 0; i < rayCount; i++)
        {
            int triangleIndex = i * 3;
            triangles[triangleIndex + 0] = 0;
            
            if (facingRight)
            {
                triangles[triangleIndex + 1] = i + 2;
                triangles[triangleIndex + 2] = i + 1;
            }
            else
            {
                triangles[triangleIndex + 1] = i + 1;
                triangles[triangleIndex + 2] = i + 2;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
}
