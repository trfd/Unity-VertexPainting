//
// VertexPainterEditor.cs
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
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(VertexPainter))]
public class VertexPainterEditor : Editor
{
	#region Static Members

	private string[] s_channelLabels = { "Red", "Green", "Blue", "Alpha" };

	#endregion

	#region Private Members

	/// <summary>
	/// Editor's target painter
	/// </summary>
	private VertexPainter m_painter;

	private bool m_advancedFoldout = false;

	private RaycastHit m_lastRaycastHit;

	private bool m_isMouseDown;

	#endregion

	#region SceneGUI stuff

	public void OnSceneGUI()
	{
		if(m_painter == null)
			m_painter = (VertexPainter) target;

        if(Event.current.isMouse)
        {
            m_painter.SelectedObject = HandleUtility.PickGameObject(Event.current.mousePosition, false);
        }

		if(m_painter.SelectedObject == null)
		{
			m_lastRaycastHit = new RaycastHit();
			return;
		}
	
		if(Event.current.isMouse && Event.current.button == 0)
		{	
			Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out m_lastRaycastHit);

			if(Event.current.type == EventType.MouseDown)
			{
				m_isMouseDown = true;
				Event.current.Use();
			}
			else if(Event.current.type == EventType.MouseUp)
			{
				m_isMouseDown = false;
				Event.current.Use();
			}
		}
		else if(Event.current.isKey && Event.current.keyCode == KeyCode.Escape)
		{
			m_painter.ResetMeshColors();
		}

		if(m_isMouseDown)
			PaintVertices(m_lastRaycastHit.point);

		DrawBrush(m_lastRaycastHit.point,m_lastRaycastHit.normal);

		HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
	}

	/// <summary>
	/// Paints the vertices around brush position according to painter's current color and size.
	/// </summary>
	/// <param name="penPoint">brushPosition.</param>
	private void PaintVertices(Vector3 brushPosition)
	{
		Mesh mesh = m_painter.SelectedObject.GetComponent<MeshFilter>().sharedMesh;

		if(mesh.colors == null || mesh.colors.Length != mesh.vertices.Length)
		{
			Debug.Log("VertexPainter: Setup black vertices for current selected object");
			mesh.colors = new Color[mesh.vertices.Length];
		}

		Vector3 localPenPoint = m_painter.SelectedObject.transform.InverseTransformPoint(brushPosition);

		Color[] colors = mesh.colors;

		float dist;

		for(int i=0 ; i<mesh.vertices.Length ; i++)
		{
			Vector3 worldPos = m_painter.SelectedObject.transform.TransformPoint(mesh.vertices[i]);
	
			if((dist=Vector3.Distance(worldPos,brushPosition)) < m_painter._brushRadius )
			{
				colors[i] = ApplyBrush(colors[i],dist);
			}
		}

		mesh.colors = colors;
	}

	/// <summary>
	/// Applies the brush channel to the color.
	/// </summary>
	/// <returns>The brush.</returns>
	/// <param name="inColor">In color.</param>
	Color ApplyBrush(Color inColor,float dist)
	{
		float intensity = m_painter._brushFalloff.Evaluate(dist / m_painter._brushRadius) * m_painter._brushIntensity;

		switch(m_painter._brushChannel)
		{
		case ColorChannel.RED:
			inColor.r = Mathf.Clamp01(inColor.r + intensity);
			break;
		case ColorChannel.GREEN:
			inColor.g = Mathf.Clamp01(inColor.g + intensity);
			break;
		case ColorChannel.BLUE:
			inColor.b = Mathf.Clamp01(inColor.b + intensity);
			break;
		case ColorChannel.ALPHA:
			inColor.a = Mathf.Clamp01(inColor.a + intensity);
			break;
		}

		return inColor;
	}

	private void DrawBrush(Vector3 brush, Vector3 normal)
	{
		Handles.color = Color.white;
		Handles.DrawWireDisc(brush, normal, m_painter._brushRadius);
	}

	#endregion

	#region Inspector stuff

	public override void OnInspectorGUI()
	{
		if(m_painter == null)
			m_painter = (VertexPainter) target;

		m_painter._brushRadius = EditorGUILayout.Slider("Size",m_painter._brushRadius,0f,10f);

		m_painter._brushIntensity = EditorGUILayout.Slider("Intensity",m_painter._brushIntensity,-0.1f,0.1f);

		m_painter._brushChannel = (ColorChannel) GUILayout.Toolbar(
			(int)m_painter._brushChannel,s_channelLabels);

		EditorGUILayout.Space();

		string previewButtonLabel;

		if(!m_painter.IsPreviewingRaw)
			previewButtonLabel = "Show Raw Painting";
		else
			previewButtonLabel = "Hide Raw Painting";

		if(GUILayout.Button(previewButtonLabel))
		{
			m_painter.IsPreviewingRaw = !m_painter.IsPreviewingRaw;
		}

		EditorGUILayout.Space();

		EditorGUILayout.HelpBox("Press Esc. to reset vertices colors",MessageType.Info);

		EditorGUILayout.Space();

		if((m_advancedFoldout = EditorGUILayout.Foldout(m_advancedFoldout,"Advanced")))
		{
			m_painter._previewMaterial = (Material) EditorGUILayout.ObjectField("Preview Material",m_painter._previewMaterial,typeof(Material),false);

			m_painter._brushFalloff = EditorGUILayout.CurveField("Falloff Curve", m_painter._brushFalloff);
		}

		EditorUtility.SetDirty(m_painter);
	}

	#endregion
}
