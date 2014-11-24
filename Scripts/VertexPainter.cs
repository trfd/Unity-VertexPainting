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
	public class Selection
	{
		#region Public Members

		/// <summary>
		/// GameObject over which the mouse was during the last scene update.
		/// </summary>
		public GameObject _selectedObject;

		/// <summary>
		/// Backup of original material current selected object is using.
		/// When showing raw painting material is replaced.
		/// </summary>
		public Material _objectBackupMaterial;

		/// <summary>
		/// Holds whether or not the painter is showing preview.
		/// </summary>
		public bool _isShowingPreview = false;

		/// <summary>
		/// The collider created to allow Physics raycasts.
		/// </summary>
		public MeshCollider _usedCollider;

		/// <summary>
		/// Current Mesh painting on.
		/// </summary>
		public Mesh _mesh;

		#endregion

		#region Interface

		public Selection(GameObject obj)
		{
			_selectedObject = obj;

			MeshFilter filter = _selectedObject.GetComponent<MeshFilter>();
			
			if(filter == null)
			{
				Debug.LogError("Vertex painter can not paint on object that does not contain any mesh");
				return;
			}
			
			_mesh = filter.sharedMesh;
			
			if(_selectedObject.GetComponent<Collider>() == null)
			{
				_usedCollider = _selectedObject.AddComponent<MeshCollider>();
				
				_usedCollider.sharedMesh = _mesh;
			}

			if(_objectBackupMaterial == null)
			{
				MeshRenderer renderer = _selectedObject.GetComponent<MeshRenderer>();

				_objectBackupMaterial = renderer.sharedMaterial;
			}
		}

		public void Restore()
		{
			RestoreCollider();

			RestoreMaterial();
		}

		public void ShowPreview(Material previewMat)
		{
			_isShowingPreview = true;

			MeshRenderer renderer = _selectedObject.GetComponent<MeshRenderer>();

			if(_objectBackupMaterial == null)
				_objectBackupMaterial = renderer.sharedMaterial;

			renderer.material = previewMat;
		}

		public void RestoreMaterial()
		{
			if(_objectBackupMaterial == null)
				return;

			MeshRenderer renderer = _selectedObject.GetComponent<MeshRenderer>();

			renderer.material = _objectBackupMaterial;
		}

		public void ResetColors()
		{
			Color[] colors = new Color[_mesh.colors.Length];

			for(int i=0 ; i<_mesh.vertices.Length ; i++)
			{
				colors[i] = Color.black;
				colors[i].a = 0f;
			}

			_mesh.colors = colors;
		}

		public void RestoreCollider()
		{
			if(_usedCollider == null)
				return;

			DestroyImmediate(_usedCollider);
			_usedCollider = null;
		}

		#endregion
	}

	#region Private Members
	
	private Selection m_currSelection;

	private bool m_previewMode;

	#endregion

	#region Public Members
	
	public Material _previewMaterial;
	
	/// <summary>
	/// Size of the brush, in world coordinates.
	/// </summary>
	/// <value>The pen radius.</value>
	public float _brushRadius;
	
	/// <summary>
	/// Intensity of brush 
	/// </summary>
	/// <value>The brush intensity.</value>
	public float _brushIntensity;
	
	/// <summary>
	/// Brush fall off.
	/// </summary>
	/// <value>The brush falloff.</value>
	public AnimationCurve _brushFalloff;
	
	/// <summary>
	/// Color channel in which brush paints vertices.
	/// </summary>
	/// <value>The color of the brush.</value>
	public ColorChannel _brushChannel;

	#endregion

	#region Properties

	/// <summary>
	/// Object in which lives the current mesh.
	/// </summary>
	/// <value>The selected object.</value>
	public GameObject SelectedObject
	{
		get
		{
			if(m_currSelection != null) 
				return m_currSelection._selectedObject; 
			return null;
		}

		set
		{
			if(m_currSelection == null || m_currSelection._selectedObject != value)
				ChangeSelectedObject(value);
		}
	}

	/// <summary>
	/// Readonly access to mesh, painter is targeting.
	/// </summary>
	/// <value>The current mesh.</value>
	public Mesh CurrentMesh
	{
		get{ return m_currSelection._mesh; }
	}

	/// <summary>
	/// Gets or sets a value indicating whether this instance is previewing raw painting.
	/// </summary>
	/// <value><c>true</c> if this instance is previewing raw; otherwise, <c>false</c>.</value>
	public bool IsPreviewingRaw
	{
		get{ return m_previewMode; }
		set
		{
			if(m_previewMode == value)
				return;

			ChangePreview(value);
		}
	}
	
	#endregion

	#region Constructor

	public VertexPainter()
	{
		_brushFalloff = new AnimationCurve();
	}

	#endregion

	#region Public Methods

	public void ResetMeshColors()
	{
		if(m_currSelection != null)
			m_currSelection.ResetColors();
	}

	#endregion

	#region Private Methods

	private void ChangeSelectedObject(GameObject newObject)
	{
		if(m_currSelection != null)
		{
			m_currSelection.Restore();
		}

		if(newObject == null)
		{
			m_currSelection = null;
			return;
		}

		m_currSelection = new Selection(newObject);

		if(m_previewMode)
			m_currSelection.ShowPreview(_previewMaterial);
	}

	private void ChangePreview(bool isPreviewing)
	{
		m_previewMode = isPreviewing;

		if(m_currSelection == null)
			return;

		if(m_previewMode)
			m_currSelection.ShowPreview(_previewMaterial);
		else
			m_currSelection.RestoreMaterial();
	}

	#endregion
}
