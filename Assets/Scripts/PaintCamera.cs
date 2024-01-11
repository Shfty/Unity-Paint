using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* PaintCamera
 * 
 * Accompanying class to PaintCanvas, handles the actual canvas rendering process.
 */
public class PaintCamera : MonoBehaviour {
	
	public Mesh Brush;
	public Color ClearColor;
	public RenderTexture RenderTarget;
	public Material BrushMaterial;
	public Vector3 BrushRotation;
	public Vector3 BrushScale;
	public Vector3 OrthoBounds;

	Vector3 coordOffset = new Vector3( 0.5f, 0.5f, 0.0f );
	Matrix4x4 ortho;
	
	List<Vector3> paintBuffer = new List<Vector3>();

	public void Init() {
		// Clear the camera's render texture to the specified colour
		Graphics.SetRenderTarget( RenderTarget );
		GL.Clear( false, true, ClearColor );
		Graphics.SetRenderTarget( null );

		// Build a custom orthographic projection matrix from the canvas' local scale,
		// in order to prevent stretching in non-square aspect ratios
		ortho = Matrix4x4.Ortho( 0f, OrthoBounds.x, 0f, OrthoBounds.y, -10f, 10f );
	}
	
	public void ApplyPaint( Vector3 pos ) {
		// Offset the input position by 0.5 in the XY axes to account for object space vs canvas space,
		// then scale it according to the canvas' orthographic projection size
		pos += coordOffset;
		pos.Scale( OrthoBounds );

		// Store this position in the paint buffer.
		// This is done because Graphics.DrawMeshNow has to be called in OnPostRender to prevent unexpected behaviour.
		paintBuffer.Add( pos );
	}
	
	public void OnPostRender() {
		// Only render if there are entries in the paint buffer
		if( paintBuffer.Count > 0 ) {
			// Switch the Graphics engine's render target to this instance's RenderTexture
			Graphics.SetRenderTarget( RenderTarget );
		
			// Store the current camera matrix
			GL.PushMatrix();
		
			// Use the first pass of our brush material for rendering
			BrushMaterial.SetPass( 0 );
		
			// Load the custom ortho matrix we constructed in Init()
			GL.LoadProjectionMatrix( ortho );

			// Loop over all the entries in the paint buffer
			foreach( Vector3 p in paintBuffer ) {
		
				// Compose a Transform/Rotate/Scale matrix from the paint buffer entry and this instance's brush rotation/scale
				Matrix4x4 brushMatrix = Matrix4x4.TRS(
					p,
					Quaternion.Euler( BrushRotation ),
					BrushScale
				);
		
				// Draw the brush mesh
				Graphics.DrawMeshNow( Brush, brushMatrix );
			}

			// Clear the paint buffer now that we're done with it
			paintBuffer.Clear ();
		
			// Restore the camera matrix we pushed onto the stack earlier
			GL.PopMatrix();
		
			// Unset the Graphics engine's render target,
			// in order to prevent unexpected geometry from being rendered to our target texture
			Graphics.SetRenderTarget( null );
		}
	}
}
