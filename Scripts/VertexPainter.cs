//
// VertexPainter.cs
//
// Author(s):
//       Baptiste Dupy <baptiste.dupy@gmail.com>
//
// Copyright (c) 2014
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using UnityEngine;
using System.Collections;

public enum ColorChannel
{
	RED		= 0,
	GREEN	= 1,
	BLUE	= 2,
	ALPHA	= 3
}

public class VertexPainter : MonoBehaviour
{
	#region Private Members

	/// <summary>
	/// Backup of original material current selected object is using.
	/// When showing raw painting material is replaced.
	/// </summary>
	private Material m_objectBackupMaterial;

	/// <summary>
	/// Color channel on which painter is currently painting.
	/// </summary>
	private ColorChannel m_currChannel;

	/// <summary>
	/// GameObject over which the mouse was during the last scene update.
	/// </summary>
	private GameObject m_selectedObject;

	/// <summary>
	/// Current Mesh painting on.
	/// </summary>
	private Mesh m_currMesh;
	
	/// <summary>
	/// The collider created to allow Physics raycasts.
	/// </summary>
	private MeshCollider m_usedCollider;

	/// <summary>
	/// Holds whether or not the painter is showing preview.
	/// </summary>
	private bool m_isShowingPreview = false;

	#endregion

	#region Properties

	/// <summary>
	/// Material used to preview raw painting.
	/// </summary>
	/// <value>The preview material.</value>
	[UnityEngine.SerializeField]
	public Material PreviewMaterial
	{
		get; set;
	}

	/// <summary>
	/// Size of the brush, in world coordinates.
	/// </summary>
	/// <value>The pen radius.</value>
	[UnityEngine.SerializeField]
	public float BrushRadius
	{
		get; set;
	}

	/// <summary>
	/// Intensity of brush 
	/// </summary>
	/// <value>The brush intensity.</value>
	[UnityEngine.SerializeField]
	public float BrushIntensity
	{
		get; set;
	}

	/// <summary>
	/// Brush fall off.
	/// </summary>
	/// <value>The brush falloff.</value>
	[UnityEngine.SerializeField]
	public AnimationCurve BrushFalloff
	{
		get; set;
	}

	/// <summary>
	/// Color channel in which brush paints vertices.
	/// </summary>
	/// <value>The color of the brush.</value>
	[UnityEngine.SerializeField]
	public ColorChannel BrushChannel
	{
		get{ return m_currChannel;  }
		set{ m_currChannel = value; }
	}

	/// <summary>
	/// Object in which lives the current mesh.
	/// </summary>
	/// <value>The selected object.</value>
	public GameObject SelectedObject
	{
		get{ return m_selectedObject; }

		set
		{
			if(m_selectedObject != value)
				ChangeSelectedObject(value);
		}
	}

	/// <summary>
	/// Readonly access to mesh, painter is targeting.
	/// </summary>
	/// <value>The current mesh.</value>
	public Mesh CurrentMesh
	{
		get{ return m_currMesh; }
	}

	/// <summary>
	/// Gets or sets a value indicating whether this instance is previewing raw painting.
	/// </summary>
	/// <value><c>true</c> if this instance is previewing raw; otherwise, <c>false</c>.</value>
	public bool IsPreviewingRaw
	{
		get{ return m_isShowingPreview; }
		set
		{
			if(m_isShowingPreview == value)
				return;

			ChangePreview(value);
		}
	}
	
	#endregion

	#region Constructor

	public VertexPainter()
	{
		BrushFalloff = new AnimationCurve();
	}

	#endregion

	#region Public Methods

	public void ResetMeshColors()
	{
	}

	#endregion

	#region Private Methods

	private void ChangeSelectedObject(GameObject newObject)
	{
		if(m_usedCollider != null)
		{
			DestroyImmediate(m_usedCollider);
			m_usedCollider = null;
		}

		if(m_objectBackupMaterial != null)
		{
			ChangePreview(false);
		}
		
		if(newObject == null)
			return;
		
		m_selectedObject = newObject;

		MeshFilter filter = m_selectedObject.GetComponent<MeshFilter>();

		if(filter == null)
		{
			Debug.LogError("Vertex painter can not paint on object that does not contain any mesh");
			m_selectedObject = null;
			m_currMesh = null;
			m_objectBackupMaterial = null;
			return;
		}

		m_currMesh = filter.sharedMesh;

		if(m_selectedObject.GetComponent<Collider>() == null)
		{
			m_usedCollider = m_selectedObject.AddComponent<MeshCollider>();
			
			m_usedCollider.sharedMesh = m_currMesh;
		}
	}
	                                 
	private void ChangePreview(bool isPreviewing)
	{
		if(m_selectedObject == null)
			return;

		if(PreviewMaterial == null)
		{
			Debug.LogError("Can not preview: Preview material not set");
			return;
		}

		m_isShowingPreview = isPreviewing;

		MeshRenderer renderer = m_selectedObject.GetComponent<MeshRenderer>();

		if(m_isShowingPreview)
		{
			m_objectBackupMaterial = renderer.sharedMaterial;
			renderer.material = PreviewMaterial;
			Debug.Log("Set Preview Material: "+this.PreviewMaterial);
		}
		else
		{
			renderer.material = m_objectBackupMaterial;
			m_objectBackupMaterial = null;
			Debug.Log("Unset Preview Material: "+this.PreviewMaterial);
		}
	}

	#endregion
}
