using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HighlightPlus {
	
	[CustomEditor (typeof(HighlightProfile))]
	[CanEditMultipleObjects]
	public class HighlightProfileEditor : Editor {

		SerializedProperty overlay, overlayColor, overlayAnimationSpeed, overlayMinIntensity, overlayBlending;
		SerializedProperty fadeInDuration, fadeOutDuration;
		SerializedProperty outline, outlineColor, outlineWidth, outlineHQ, outlineAlwaysOnTop, outlineCullBackFaces;
		SerializedProperty glow, glowWidth, glowHQ, glowDithering, glowMagicNumber1, glowMagicNumber2, glowAnimationSpeed, glowAlwaysOnTop, glowPasses, glowCullBackFaces;
		SerializedProperty innerGlow, innerGlowWidth, innerGlowColor, innerGlowAlwaysOnTop;
		SerializedProperty targetFX, targetFXTexture, targetFXColor, targetFXRotationSpeed, targetFXInitialScale, targetFXEndScale, targetFXTransitionDuration, targetFXStayDuration;
		SerializedProperty seeThrough, seeThroughIntensity, seeThroughTintAlpha, seeThroughTintColor;

		void OnEnable () {
			fadeInDuration = serializedObject.FindProperty ("fadeInDuration");
			fadeOutDuration = serializedObject.FindProperty ("fadeOutDuration");
			overlay = serializedObject.FindProperty ("overlay");
			overlayColor = serializedObject.FindProperty ("overlayColor");
			overlayAnimationSpeed = serializedObject.FindProperty ("overlayAnimationSpeed");
			overlayMinIntensity = serializedObject.FindProperty ("overlayMinIntensity");
			overlayBlending = serializedObject.FindProperty ("overlayBlending");
			outline = serializedObject.FindProperty ("outline");
			outlineColor = serializedObject.FindProperty ("outlineColor");
			outlineWidth = serializedObject.FindProperty ("outlineWidth");
			outlineHQ = serializedObject.FindProperty ("outlineHQ");
			outlineAlwaysOnTop = serializedObject.FindProperty ("outlineAlwaysOnTop");
			outlineCullBackFaces = serializedObject.FindProperty ("outlineCullBackFaces");
			glow = serializedObject.FindProperty ("glow");
			glowWidth = serializedObject.FindProperty ("glowWidth");
			glowHQ = serializedObject.FindProperty ("glowHQ");
			glowAnimationSpeed = serializedObject.FindProperty ("glowAnimationSpeed");
			glowDithering = serializedObject.FindProperty ("glowDithering");
			glowMagicNumber1 = serializedObject.FindProperty ("glowMagicNumber1");
			glowMagicNumber2 = serializedObject.FindProperty ("glowMagicNumber2");
			glowAnimationSpeed = serializedObject.FindProperty ("glowAnimationSpeed");
			glowAlwaysOnTop = serializedObject.FindProperty ("glowAlwaysOnTop");
			glowCullBackFaces = serializedObject.FindProperty ("glowCullBackFaces");
			glowPasses = serializedObject.FindProperty ("glowPasses");
			innerGlow = serializedObject.FindProperty ("innerGlow");
			innerGlowColor = serializedObject.FindProperty ("innerGlowColor");
			innerGlowWidth = serializedObject.FindProperty ("innerGlowWidth");
			innerGlowAlwaysOnTop = serializedObject.FindProperty ("innerGlowAlwaysOnTop");
			targetFX = serializedObject.FindProperty ("targetFX");
			targetFXTexture = serializedObject.FindProperty ("targetFXTexture");
			targetFXRotationSpeed = serializedObject.FindProperty ("targetFXRotationSpeed");
			targetFXInitialScale = serializedObject.FindProperty ("targetFXInitialScale");
			targetFXEndScale = serializedObject.FindProperty ("targetFXEndScale");
			targetFXColor = serializedObject.FindProperty ("targetFXColor");
			targetFXTransitionDuration = serializedObject.FindProperty ("targetFXTransitionDuration");
			targetFXStayDuration = serializedObject.FindProperty ("targetFXStayDuration");
			seeThrough = serializedObject.FindProperty ("seeThrough");
			seeThroughIntensity = serializedObject.FindProperty ("seeThroughIntensity");
			seeThroughTintAlpha = serializedObject.FindProperty ("seeThroughTintAlpha");
			seeThroughTintColor = serializedObject.FindProperty ("seeThroughTintColor");
		}

		public override void OnInspectorGUI () {
           
			EditorGUILayout.Separator ();
			EditorGUILayout.LabelField ("Highlight Options", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (fadeInDuration);
			EditorGUILayout.PropertyField (fadeOutDuration);
			EditorGUILayout.PropertyField (overlay);
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField (overlayColor, new GUIContent ("Color"));
			EditorGUILayout.PropertyField (overlayBlending, new GUIContent ("Blending"));
			EditorGUILayout.PropertyField (overlayMinIntensity, new GUIContent ("Min Intensity"));
			EditorGUILayout.PropertyField (overlayAnimationSpeed, new GUIContent ("Animation Speed"));
			EditorGUI.indentLevel--;
			EditorGUILayout.PropertyField (outline);
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField (outlineWidth, new GUIContent ("Width"));
			EditorGUILayout.PropertyField (outlineColor, new GUIContent ("Color"));
			EditorGUILayout.PropertyField (outlineHQ, new GUIContent ("HQ", "Additional passes to create a better outline on certain shapes."));
			EditorGUILayout.PropertyField (outlineAlwaysOnTop, new GUIContent ("Always On Top", "Shows outline on top of any occluding objects."));
			EditorGUILayout.PropertyField (outlineCullBackFaces, new GUIContent ("Cull Back Faces", "Do not render back facing triangles."));
			EditorGUI.indentLevel--;
			EditorGUILayout.PropertyField (innerGlow, new GUIContent ("Inner Glow"));
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField (innerGlowColor, new GUIContent ("Color"));
			EditorGUILayout.PropertyField (innerGlowWidth, new GUIContent ("Width"));
			EditorGUILayout.PropertyField (innerGlowAlwaysOnTop, new GUIContent ("Always On Top", "Shows inner glow on top of any occluding objects."));
			EditorGUI.indentLevel--;
			EditorGUILayout.PropertyField (glow, new GUIContent ("Outer Glow"));
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField (glowWidth, new GUIContent ("Width"));
			EditorGUILayout.PropertyField (glowHQ, new GUIContent ("HQ", "Additional passes to create a better glow on certain shapes."));
			EditorGUILayout.PropertyField (glowAnimationSpeed, new GUIContent ("Animation Speed"));
			EditorGUILayout.PropertyField (glowDithering, new GUIContent ("Dithering"));
			if (glowDithering.boolValue) {
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField (glowMagicNumber1, new GUIContent ("Magic Number 1"));
				EditorGUILayout.PropertyField (glowMagicNumber2, new GUIContent ("Magic Number 2"));
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.PropertyField (glowAlwaysOnTop, new GUIContent ("Always On Top", "Shows outer glow on top of any occluding objects."));
			EditorGUILayout.PropertyField (glowCullBackFaces, new GUIContent ("Cull Back Faces", "Do not render back facing triangles."));
			EditorGUILayout.PropertyField (glowPasses, true);
			EditorGUI.indentLevel--;
			EditorGUILayout.PropertyField (targetFX, new GUIContent ("Target"));
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField (targetFXTexture, new GUIContent ("Texture"));
			EditorGUILayout.PropertyField (targetFXColor, new GUIContent ("Color"));
			EditorGUILayout.PropertyField (targetFXRotationSpeed, new GUIContent ("Rotation Speed"));
			EditorGUILayout.PropertyField (targetFXInitialScale, new GUIContent ("Initial Scale"));
			EditorGUILayout.PropertyField (targetFXEndScale, new GUIContent ("End Scale"));
			EditorGUILayout.PropertyField (targetFXTransitionDuration, new GUIContent ("Transition Duration"));
			EditorGUILayout.PropertyField (targetFXStayDuration, new GUIContent ("Stay Duration"));
			EditorGUI.indentLevel--;

			EditorGUILayout.Separator ();
			EditorGUILayout.LabelField ("See-Through Options", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField (seeThrough);
			EditorGUILayout.PropertyField (seeThroughIntensity, new GUIContent ("   Intensity"));
			EditorGUILayout.PropertyField (seeThroughTintAlpha, new GUIContent ("   Alpha"));
			EditorGUILayout.PropertyField (seeThroughTintColor, new GUIContent ("   Color"));

			if (serializedObject.ApplyModifiedProperties () || (Event.current.type == EventType.ExecuteCommand &&
			    Event.current.commandName == "UndoRedoPerformed")) {

				// Triggers profile reload on all Volumetric Fog scripts
				HighlightEffect[] effects = FindObjectsOfType<HighlightEffect> ();
				for (int t = 0; t < targets.Length; t++) {
					HighlightProfile profile = (HighlightProfile)targets [t];
					for (int k = 0; k < effects.Length; k++) {
						if (effects [k] != null && effects [k].profile == profile && effects [k].profileSync) {
							profile.Load (effects [k]);
							effects [k].Refresh ();
						}
					}
				}
				EditorUtility.SetDirty (target);
			}

		}
      

	}

}