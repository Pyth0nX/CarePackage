using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BeamBetweenPoints : MonoBehaviour
{
	public Transform PhysGrabPointOrigin;
	public Transform PhysGrabPoint;
	public Transform PhysGrabPointPuller;
	public Material greenScreenMaterial;
	
	private Material originalMaterial;
	
	[HideInInspector]
	public Vector3 physGrabPointPullerSmoothPosition;
	
	public float CurveStrength = 1f;
	
	public int CurveResolution = 20;
	
	[Header("Texture Scrolling")]
	public Vector2 scrollSpeed = new(5f, 0f);
	
	private LineRenderer lineRenderer;
	
	[HideInInspector]
	public Material lineMaterial;
	
	[SerializeField] private bool useGreenScreen = false;
	
    private void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		originalMaterial = lineRenderer.material;
		lineMaterial = lineRenderer.material;
	}
    
	private void LateUpdate()
	{
		DrawCurve();
		ScrollTexture();
	}
	
	private void OnEnable()
	{
		physGrabPointPullerSmoothPosition = PhysGrabPointPuller.position;
		if (useGreenScreen)
		{
			lineMaterial = greenScreenMaterial;
			GetComponent<LineRenderer>().material = greenScreenMaterial;
		}
	}
	
	private void OnDisable()
	{
		lineMaterial = originalMaterial;
		if (lineRenderer)
		{
			lineRenderer.material = originalMaterial;
		}
	}
	
	private void DrawCurve()
	{
		if (!PhysGrabPointPuller)
		{
			return;
		}
		Vector3[] array = new Vector3[CurveResolution];
		Vector3 position = PhysGrabPointPuller.position;
		physGrabPointPullerSmoothPosition = Vector3.Lerp(physGrabPointPullerSmoothPosition, position, Time.deltaTime * 10f);
		Vector3 p = physGrabPointPullerSmoothPosition * CurveStrength;
		for (int i = 0; i < CurveResolution; i++)
		{
			var t = i / (CurveResolution - 1f);
			array[i] = CalculateBezierPoint(t, PhysGrabPointOrigin.position, p, PhysGrabPoint.position);
		}
		lineRenderer.positionCount = CurveResolution;
		lineRenderer.SetPositions(array);
	}
	
	private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
	{
		return Mathf.Pow(1f - t, 2f) * p0 + 2f * (1f - t) * t * p1 + Mathf.Pow(t, 2f) * p2;
	}
	
	private void ScrollTexture()
	{
		if (lineMaterial)
		{
			lineMaterial.mainTextureScale = new Vector2(1f, 1f);
			Vector2 mainTextureOffset = Time.time * scrollSpeed;
			lineMaterial.mainTextureOffset = mainTextureOffset;
		}
	}
}