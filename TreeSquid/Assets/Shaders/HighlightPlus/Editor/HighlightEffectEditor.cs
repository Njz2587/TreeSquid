using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HighlightPlus {
	
	[CustomEditor (typeof(HighlightEffect))]
	[CanEditMultipleObjects]
	public class HighlightEffectEditor : Editor {

		SerializedProperty profile, profileSync, ignoreObjectVisibility, ignore, previewInEditor;
		SerializedProperty highlighted, fadeInDuration, fadeOutDuration;
		SerializedProperty overlay, overlayColor, overlayAnimationSpeed, overlayMinIntensity, overlayBlending;
		SerializedProperty outline, outlineColor, outlineWidth, outlineHQ, outlineAlwaysOnTop, outlineCullBackFaces;
		SerializedProperty glow, glowWidth, glowHQ, glowDithering, glowMagicNumber1, glowMagicNumber2, glowAnimationSpeed, glowPasses, glowAlwaysOnTop, glowCullBackFaces;
		SerializedProperty innerGlow, innerGlowWidth, innerGlowColor, innerGlowAlwaysOnTop;
		SerializedProperty seeThrough, seeThroughIntensity, seeThroughTintAlpha, seeThroughTintColor;
		SerializedProperty targetFX, targetFXTexture, targetFXColor, targetFXCenter, targetFXRotationSpeed, targetFXInitialScale, targetFXEndScale, targetFXTransitionDuration, targetFXStayDuration;
		HighlightEffect thisEffect;
		bool profileChanged, enableProfileApply, presetChanged;

		void OnEnable () {
			profile = serializedObject.FindProperty ("profile");
			profileSync = serializedObject.FindProperty ("profileSync");
			ignoreObjectVisibility = serializedObject.FindProperty ("ignoreObjectVisibility");
			ignore = serializedObject.FindProperty ("ignore");
			previewInEditor = serializedObject.FindProperty ("previewInEditor");
			highlighted = serializedObject.FindProperty ("_highlighted");
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
			glowPasses = serializedObject.FindProperty ("glowPasses");
			glowAlwaysOnTop = serializedObject.FindProperty ("glowAlwaysOnTop");
			glowCullBackFaces = serializedObject.FindProperty ("glowCullBackFaces");
			innerGlow = serializedObject.FindProperty ("innerGlow");
			innerGlowColor = serializedObject.FindProperty ("innerGlowColor");
			innerGlowWidth = serializedObject.FindProperty ("innerGlowWidth");
			innerGlowAlwaysOnTop = serializedObject.FindProperty ("innerGlowAlwaysOnTop");
			seeThrough = serializedObject.FindProperty ("seeThrough");
			seeThroughIntensity = serializedObject.FindProperty ("seeThroughIntensity");
			seeThroughTintAlpha = serializedObject.FindProperty ("seeThroughTintAlpha");
			seeThroughTintColor = serializedObject.FindProperty ("seeThroughTintColor");
			targetFX = serializedObject.FindProperty ("targetFX");
			targetFXTexture = serializedObject.FindProperty ("targetFXTexture");
			targetFXRotationSpeed = serializedObject.FindProperty ("targetFXRotationSpeed");
			targetFXInitialScale = serializedObject.FindProperty ("targetFXInitialScale");
			targetFXEndScale = serializedObject.FindProperty ("targetFXEndScale");
			targetFXColor = serializedObject.FindProperty ("targetFXColor");
			targetFXCenter = serializedObject.FindProperty ("targetFXCenter");
			targetFXTransitionDuration = serializedObject.FindProperty ("targetFXTransitionDuration");
			targetFXStayDuration = serializedObject.FindProperty ("targetFXStayDuration");
			thisEffect = (HighlightEffect)target;
			thisEffect.Refresh ();
		}

		public override void OnInspectorGUI () {
			bool isManager = thisEffect.GetComponent<HighlightManager> () != null;
			EditorGUILayout.Separator ();
			serializedObject.Update ();


			EditorGUILayout.BeginHorizontal ();
			HighlightProfile prevProfile = (HighlightProfile)profile.objectReferenceValue;
			EditorGUILayout.PropertyField (profile, new GUIContent ("Profile", "Create or load stored presets."));
			if (profile.objectReferenceValue != null) {

				if (prevProfile != profile.objectReferenceValue) {
					profileChanged = true;
				}

				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.BeginHorizontal ();
				GUILayout.Label ("", GUILayout.Width (EditorGUIUtility.labelWidth));
				if (GUILayout.Button (new GUIContent ("Create", "Creates a new profile which is a copy of the current settings."), GUILayout.Width (60))) {
					CreateProfile ();
					profileChanged = false;
					enableProfileApply = false;
					GUIUtility.ExitGUI ();
					return;
				}
				if (GUILayout.Button (new GUIContent ("Load", "Updates fog settings with the profile configuration."), GUILayout.Width (60))) {
					profileChanged = true;
				}
				if (!enableProfileApply)
					GUI.enabled = false;
				if (GUILayout.Button (new GUIContent ("Save", "Updates profile configuration with changes in this inspector."), GUILayout.Width (60))) {
					enableProfileApply = false;
					profileChanged = false;
					thisEffect.profile.Save (thisEffect);
					EditorUtility.SetDirty (thisEffect.profile);
					GUIUtility.ExitGUI ();
					return;
				}
				GUI.enabled = true;
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.PropertyField (profileSync, new GUIContent ("Sync With Profile", "If disabled, profile settings will only be loaded when clicking 'Load' which allows you to customize settings after loading a profile and keep those changes."));
				EditorGUILayout.BeginHorizontal ();
			} else {
				if (GUILayout.Button (new GUIContent ("Create", "Creates a new profile which is a copy of the current settings."), GUILayout.Width (60))) {
					CreateProfile ();
					GUIUtility.ExitGUI ();
					return;
				}
			}
			EditorGUILayout.EndHorizontal ();


			if (isManager) {
				EditorGUILayout.HelpBox ("These are default settings for highlighted objects. If the highlighted object already has a Highlight Effect component, those properties will be used.", MessageType.Info);
			} else {
				EditorGUILayout.PropertyField (previewInEditor);
			}

			EditorGUILayout.PropertyField (ignoreObjectVisibility);
			if (thisEffect.staticChildren) {
				EditorGUILayout.HelpBox ("This GameObject or one of its children is marked as static. If highlight is not visible, add a MeshCollider to them.", MessageType.Warning);
			}

			EditorGUILayout.Separator ();
			EditorGUILayout.LabelField ("Highlight Options", EditorStyles.boldLabel);
			EditorGUI.BeginChangeCheck ();
			if (!isManager) {
				EditorGUILayout.PropertyField (ignore, new GUIContent ("Ignore", "This object won't be highlighted."));
				if (!ignore.boolValue) {
					EditorGUI.BeginChangeCheck ();
					EditorGUILayout.PropertyField (highlighted);
					if (EditorGUI.EndChangeCheck ()) {
						foreach (HighlightEffect effect in targets) {
							effect.SetHighlighted (highlighted.boolValue);
						}
					}
				}
			}
			if (!ignore.boolValue) {
				EditorGUILayout.PropertyField (fadeInDuration);
				EditorGUILayout.PropertyField (fadeOutDuration);
				DrawSectionField (overlay, "Overlay", overlay.floatValue > 0);
				if (overlay.floatValue < overlayMinIntensity.floatValue) {
					overlayMinIntensity.floatValue = overlay.floatValue;
				}
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField (overlayColor, new GUIContent ("Color"));
				EditorGUILayout.PropertyField (overlayBlending, new GUIContent ("Blending"));
				EditorGUILayout.PropertyField (overlayMinIntensity, new GUIContent ("Min Intensity"));
				if (overlayMinIntensity.floatValue > overlay.floatValue) {
					overlay.floatValue = overlayMinIntensity.floatValue;
				}
				EditorGUILayout.PropertyField (overlayAnimationSpeed, new GUIContent ("Animation Speed"));
				EditorGUI.indentLevel--;
				DrawSectionField (outline, "Outline", outline.floatValue > 0);
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField (outlineWidth, new GUIContent ("Width"));
				EditorGUILayout.PropertyField (outlineColor, new GUIContent ("Color"));
				EditorGUILayout.PropertyField (outlineHQ, new GUIContent ("HQ", "Additional passes to create a better outline on certain shapes."));
				EditorGUILayout.PropertyField (outlineAlwaysOnTop, new GUIContent ("Always On Top", "Shows outline on top of any occluding objects."));
				EditorGUILayout.PropertyField (outlineCullBackFaces, new GUIContent ("Cull Back Faces", "Do not render back facing triangles."));
				EditorGUI.indentLevel--;
				DrawSectionField (innerGlow, "Inner Glow", innerGlow.floatValue > 0);
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField (innerGlowColor, new GUIContent ("Color"));
				EditorGUILayout.PropertyField (innerGlowWidth, new GUIContent ("Width"));
				EditorGUILayout.PropertyField (innerGlowAlwaysOnTop, new GUIContent ("Always On Top", "Shows inner glow on top of any occluding objects."));
				EditorGUI.indentLevel--;
				DrawSectionField (glow, "Outer Glow", glow.floatValue > 0);
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
				DrawSectionField (targetFX, "Target", targetFX.boolValue);
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField (targetFXTexture, new GUIContent ("Texture"));
				EditorGUILayout.PropertyField (targetFXColor, new GUIContent ("Color"));
				EditorGUILayout.PropertyField (targetFXCenter, new GUIContent ("Center", "Optionally assign a transform. Target will follow transform. If the object is skinned, you can also assign a bone to reflect currenct animation position."));
				EditorGUILayout.PropertyField (targetFXRotationSpeed, new GUIContent ("Rotation Speed"));
				EditorGUILayout.PropertyField (targetFXInitialScale, new GUIContent ("Initial Scale"));
				EditorGUILayout.PropertyField (targetFXEndScale, new GUIContent ("End Scale"));
				EditorGUILayout.PropertyField (targetFXTransitionDuration, new GUIContent ("Transition Duration"));
				EditorGUILayout.PropertyField (targetFXStayDuration, new GUIContent ("Stay Duration"));
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.Separator ();
			EditorGUILayout.LabelField ("See-Through Options", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField (seeThrough);
			if (isManager && seeThrough.intValue == (int)SeeThroughMode.AlwaysWhenOccluded) {
				EditorGUILayout.HelpBox ("This option is not valid in Manager.\nTo make an object always visible add a Highlight Effect component to the gameobject and enable this option on the component.", MessageType.Error);
			}
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField (seeThroughIntensity, new GUIContent ("Intensity"));
			EditorGUILayout.PropertyField (seeThroughTintAlpha, new GUIContent ("Alpha"));
			EditorGUILayout.PropertyField (seeThroughTintColor, new GUIContent ("Color"));
			EditorGUI.indentLevel--;

			if (serializedObject.ApplyModifiedProperties () || profileChanged) {
				if (thisEffect.profile != null) {
					if (profileChanged) {
						thisEffect.profile.Load (thisEffect);
						profileChanged = false;
						enableProfileApply = false;
					} else {
						enableProfileApply = true;
					}
				}

				foreach (HighlightEffect effect in targets) {
					effect.Refresh ();
				}
			}
			HighlightEffect _effect = (HighlightEffect)target;
			if (_effect != null && _effect.previewInEditor) {
				EditorUtility.SetDirty (_effect);
			}
		}

		void DrawSectionField (SerializedProperty property, string label, bool active) {
			EditorGUILayout.PropertyField (property, new GUIContent (active ? label + " •" : label));
		}

		#region Profile handling

		void CreateProfile () {

			HighlightProfile newProfile = ScriptableObject.CreateInstance<HighlightProfile> ();
			newProfile.Save (thisEffect);

			AssetDatabase.CreateAsset (newProfile, "Assets/Highlight Plus Profile.asset");
			AssetDatabase.SaveAssets ();

			EditorUtility.FocusProjectWindow ();
			Selection.activeObject = newProfile;

			thisEffect.profile = newProfile;
		}


		#endregion

	}

}