using UnityEngine;
using System.Collections;

/* PaintCanvas
 * 
 * Uses Unity's low-level geometry rendering engine to implement a MS Paint-style canvas.
 * This is the manager class that handles object creation and material assignment,
 * actual rendering is handled by a local instance of PaintCamera.
 * 
 * To use, add an instance of PaintCanvas to a GameObject containing a Quad mesh filter and Quad mesh collider.
 */
public class PaintCanvas : MonoBehaviour {
	public int BaseResolution = 1024;		// RenderTexture resolution assuming a 1x1x1 scale Quad, will scale up accordingly
	public Color ClearColor = Color.white;	// Canvas background colour
	public Color BrushColor = Color.red;	// Brush colour
	public Mesh Brush;						// Brush mesh
	public Vector3 BrushRotation;			// Brush rotation
	public Vector3 BrushScale;				// Brush scale

	GameObject cameraObject;
	PaintCamera paintCamera;
	RenderTexture renderTarget;
	Material canvasMaterial;
	Material brushMaterial;

	void Start() {
		/* Ensure that the GameObject this script instance is assigned to has everything necessary for it to function
		 * 
		 * It must have a Quad mesh filter, and a Quad MeshColldier
		 */
		if( GetComponent<MeshFilter> ().mesh.name != "Quad Instance" ) {
			Debug.LogWarning( "PaintCanvas must be assigned to a Quad" );
			this.enabled = false;
			return;
		}

		if( GetComponent<MeshCollider> () == null ) {
			Debug.LogWarning( "PaintCanvas requires a MeshCollider of type Quad" );
			this.enabled = false;
			return;
		} else {
			if( GetComponent<MeshCollider>().sharedMesh.name != "Quad" )
			{
				Debug.LogWarning( "PaintCanvas requires a MeshCollider of type Quad" );
				this.enabled = false;
				return;
			}
		}

		/* Create a RenderTexture for this canvas
		 * 
		 * BaseResolution is multiplied by the canvas' scale in order to maintain square pixel mapping
		 */
		renderTarget = new RenderTexture(
			(int)( BaseResolution * transform.localScale.x ),
			(int)( BaseResolution * transform.localScale.y ),
			0
		);
		
		// Create a Material to display the RenderTexture on and assign it to the canvas' renderer
		canvasMaterial = new Material( Shader.Find( "Standard" ) );
		canvasMaterial.SetTexture( "_MainTex", renderTarget );
		GetComponent<Renderer>().material = canvasMaterial;

		// Create a Material for rendering this canvas' brush and assign its colour
		brushMaterial = new Material( Shader.Find( "Custom/FlatColor" ) );
		brushMaterial.SetColor( "_Color", BrushColor );

		// Create a new GameObject to hold a Camera component and an instance of the PaintCamera script (for rendering brush geometry)
		cameraObject = new GameObject ();
		Camera camComponent = cameraObject.AddComponent<Camera>();
		paintCamera = cameraObject.AddComponent<PaintCamera>();

		// Set the camera component's depth lower than the main camera to ensure it doesn't interfere with the main display
		camComponent.depth = -1;

		// Assign the variables required for PaintCamera to function
		paintCamera.ClearColor = ClearColor;			// Canvas background colour
		paintCamera.RenderTarget = renderTarget;		// Target render texture
		paintCamera.Brush = Brush;						// Brush mesh
		paintCamera.BrushMaterial = brushMaterial;		// Brush material
		paintCamera.BrushRotation = BrushRotation;		// Brush rotation in euler angles
		paintCamera.BrushScale = BrushScale;			// Brush scale
		paintCamera.OrthoBounds = transform.localScale;	// Orthographic projection size, based on the Quad's scale

		// Initialise the PaintCamera instance
		paintCamera.Init();
	}

	public void Update() {
		// Check to see if the left mouse button is down, and apply paint if so
		if( Input.GetMouseButton( 0 ) ) {
			ApplyPaint ();
		}
	}
	
	void ApplyPaint()
	{
		/* Cast a ray into the screen in the direction of the mouse pointer
		 * 
		 * Uses a LayerMask to test only against the PaintCanvas layer
		 */
		Ray clickRay = Camera.main.ScreenPointToRay( Input.mousePosition );
		RaycastHit clickHitInfo = new RaycastHit();
		int canvasMask = 1 << LayerMask.NameToLayer( "PaintCanvas" );
		
		if( Physics.Raycast( clickRay, out clickHitInfo, Mathf.Infinity, canvasMask ) )
		{
			// If the ray hits a canvas, and that canvas is being managed by this script instance,
			// have the associated PaintCamera draw a brush into its render target
			if( clickHitInfo.collider.gameObject == gameObject )
			{
				Vector3 worldPos = clickHitInfo.point;
				Vector3 localPos = clickHitInfo.collider.transform.InverseTransformPoint( worldPos );
				paintCamera.ApplyPaint( localPos );
			}
		}
	}
}
