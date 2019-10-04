using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HighlightPlus {

	public delegate bool OnObjectHighlightStartEvent (GameObject obj);
	public delegate void OnObjectHighlightEndEvent (GameObject obj);
	public delegate bool OnRendererHighlightEvent (Renderer renderer);


	public enum SeeThroughMode {
		WhenHighlighted = 0,
		AlwaysWhenOccluded = 1,
		Never = 2
	}

	[Serializable]
	public struct GlowPassData {
		public float offset;
		public float alpha;
		public Color color;
	}

	[ExecuteInEditMode]
	[HelpURL ("https://kronnect.freshdesk.com/support/solutions/42000065090")]
	public partial class HighlightEffect : MonoBehaviour {

		public HighlightProfile profile;
		[Tooltip ("If enabled, settings will be synced with profile.")]
		public bool profileSync = true;
		public bool previewInEditor;

		[Tooltip ("Show highlight effects even if the object is not visible. If this object or its children use GPU Instancing tools, the MeshRenderer can be disabled although the object is visible. In this case, this option is useful to enable highlighting.")]
		public bool ignoreObjectVisibility;

		[Tooltip ("Ignore highlighting on this object.")]
		public bool ignore;

		[SerializeField]
		bool _highlighted;

		public bool highlighted { get { return _highlighted; } set { SetHighlighted (value); } }

		public float fadeInDuration;
		public float fadeOutDuration;

		[Range (0, 1)]
		public float overlay = 0.5f;
		public Color overlayColor = Color.yellow;
		public float overlayAnimationSpeed = 1f;
		[Range (0, 1)]
		public float overlayMinIntensity = 0.5f;
		[Range (0, 1)]
		public float overlayBlending = 1.0f;

		[Range (0, 1)]
		public float outline = 1f;
		public Color outlineColor = Color.black;
		public float outlineWidth = 0.45f;
		public bool outlineHQ = false;
		public bool outlineAlwaysOnTop;
		public bool outlineCullBackFaces = true;

		[Range (0, 5)]
		public float glow = 1f;
		public float glowWidth = 0.4f;
		public bool glowHQ = false;
		public bool glowDithering = true;
		public float glowMagicNumber1 = 0.75f;
		public float glowMagicNumber2 = 0.5f;
		public float glowAnimationSpeed = 1f;
		public bool glowAlwaysOnTop;
		public bool glowCullBackFaces = true;
		public GlowPassData[] glowPasses;

		[Range (0, 5f)]
		public float innerGlow = 0f;
		[Range (0, 2)]
		public float innerGlowWidth = 1f;
		public Color innerGlowColor = Color.white;
		public bool innerGlowAlwaysOnTop;

		public bool targetFX;
		public Texture2D targetFXTexture;
		public Color targetFXColor = Color.white;
		public Transform targetFXCenter;
		public float targetFXRotationSpeed = 50f;
		public float targetFXInitialScale = 4f;
		public float targetFXEndScale = 1.5f;
		public float targetFXTransitionDuration = 0.5f;
		public float targetFXStayDuration = 1.5f;

		public event OnObjectHighlightStartEvent OnObjectHighlightStart;
		public event OnObjectHighlightEndEvent OnObjectHighlightEnd;
		public event OnRendererHighlightEvent OnRendererHighlightStart;

		public SeeThroughMode seeThrough;
		[Range (0, 5f)]
		public float seeThroughIntensity = 0.8f;
		[Range (0, 1)]
		public float seeThroughTintAlpha = 0.5f;
		public Color seeThroughTintColor = Color.red;


		struct ModelMaterials {
			public Transform transform;
			public bool bakedTransform;
			public Vector3 currentPosition, currentRotation, currentScale;
			public bool renderWasVisibleDuringSetup;
			public Mesh mesh, originalMesh;
			public Renderer renderer;
			public bool skinnedMesh;
			public Material material;
			public Material fxMatTarget, fxMatGlow, fxMatInnerGlow, fxMatOutline;
			public Material[] fxMatSeeThrough, fxMatOverlay;
			public Matrix4x4 renderingMatrix;
		}

		enum FadingState {
			FadingOut = -1,
			NoFading = 0,
			FadingIn = 1
		}

		[SerializeField, HideInInspector]
		ModelMaterials[] rms;
		[SerializeField, HideInInspector]
		int rmsCount = 0;

		#if UNITY_EDITOR
		/// <summary>
		/// True if there's some static children
		/// </summary>
		[NonSerialized]
		public bool staticChildren;
		#endif

		[NonSerialized]
		public Transform target;

		// Time in which the highlight started
		[NonSerialized]
		public float highlightStartTime;

		const string SKW_ALPHACLIP = "HP_ALPHACLIP";
		const string UNIFORM_CUTOFF = "_CutOff";
		const String UNIFORM_ALPHA_TEX = "_AlphaTex";
		const string PIXELSNAP_ON = "PIXELSNAP_ON";
		const string ETC1_EXTERNAL_ALPHA = "ETC1_EXTERNAL_ALPHA";
		const float TAU = 0.70711f;

		static Material fxMatMask, fxMatSeeThrough, fxMatGlow, fxMatInnerGlow, fxMatOutline, fxMatOverlay, fxMatOccluder, fxMatTarget;
		static Vector3[] offsets;

		float fadeStartTime;
		FadingState fading = FadingState.NoFading;
		CommandBuffer cbOccluder, cbMask, cbSeeThrough, cbGlow, cbOutline, cbOverlay, cbInnerGlow;
		static Mesh quadMesh;

		void OnEnable () {
			if (offsets == null || offsets.Length < 8) {
				offsets = new Vector3[] {
					Vector3.up,
					Vector3.right,
					Vector3.down,
					Vector3.left,
					new Vector3 (-TAU, TAU, 0),
					new Vector3 (TAU, TAU, 0),
					new Vector3 (TAU, -TAU, 0),
					new Vector3 (-TAU, -TAU, 0)
				};
			}

			if (quadMesh == null) {
				BuildQuad ();
			}
			if (target == null) {
				target = transform;
			}
			if (profileSync && profile != null) {
				profile.Load (this);
			}
			if (glowPasses == null || glowPasses.Length == 0) {
				glowPasses = new GlowPassData[4];
				glowPasses [0] = new GlowPassData () { offset = 4, alpha = 0.1f, color = new Color (0.64f, 1f, 0f, 1f) };
				glowPasses [1] = new GlowPassData () { offset = 3, alpha = 0.2f, color = new Color (0.64f, 1f, 0f, 1f) };
				glowPasses [2] = new GlowPassData () { offset = 2, alpha = 0.3f, color = new Color (0.64f, 1f, 0f, 1f) };
				glowPasses [3] = new GlowPassData () { offset = 1, alpha = 0.4f, color = new Color (0.64f, 1f, 0f, 1f) };
			}
			CheckGeometrySupportDependencies ();
			SetupMaterial ();
		}

		void OnDisable () {
			UpdateMaterialProperties ();
		}


		void Reset () {
			Refresh ();
		}


		public void Refresh () {
			if (!enabled) {
				enabled = true;
			} else {
				SetupMaterial ();
			}
		}

		public void RenderOccluder () {

			if (rms == null || fxMatOccluder == null)
				return;
			
			for (int k = 0; k < rms.Length; k++) {
				Transform t = rms [k].transform;
				if (t == null)
					continue;
				Mesh mesh = rms [k].mesh;
				if (mesh == null)
					continue;
				if (!rms [k].renderer.isVisible)
					continue;

				if (rms [k].skinnedMesh) {
					cbOccluder.Clear ();
					cbOccluder.DrawRenderer (rms [k].renderer, fxMatOccluder);
					Graphics.ExecuteCommandBuffer (cbOccluder);
				} else {
					Vector3 position = t.position;
					Vector3 lossyScale = t.lossyScale;
					if (rms [k].bakedTransform) {
						if (rms [k].currentPosition != t.position || rms [k].currentRotation != t.eulerAngles || rms [k].currentScale != t.lossyScale) {
							BakeTransform (k, true);
						}
						rms [k].renderingMatrix = Matrix4x4.identity;
					} else {
						rms [k].renderingMatrix = Matrix4x4.TRS (position, t.rotation, lossyScale);
					}
					Graphics.DrawMesh (mesh, rms [k].renderingMatrix, fxMatOccluder, gameObject.layer);
				}
			}
		}


		void OnRenderObject () {
			
#if UNITY_EDITOR
			if (!previewInEditor && !Application.isPlaying)
				return;
#endif

			bool seeThroughReal = seeThroughIntensity > 0 && (this.seeThrough == SeeThroughMode.AlwaysWhenOccluded || (this.seeThrough == SeeThroughMode.WhenHighlighted && _highlighted));
			if (!_highlighted && !seeThroughReal) {
				return;
			}

			// Check camera culling mask
			Camera cam = Camera.current;
			int cullingMask = cam.cullingMask;

			// Ensure renderers are valid and visible (in case LODgroup has changed active renderer)
			if (!ignoreObjectVisibility) {
				for (int k = 0; k < rmsCount; k++) {
					if (rms [k].renderer != null && rms [k].renderer.isVisible != rms [k].renderWasVisibleDuringSetup) {
						SetupMaterial ();
						break;
					}
				}
			}

			// Apply effect
			float glowReal = _highlighted ? this.glow : 0;
			int layer = gameObject.layer;

			if (fxMatMask == null)
				return;
			
			// First create masks
			for (int k = 0; k < rmsCount; k++) {
				Transform t = rms [k].transform;
				if (t == null)
					continue;
				Mesh mesh = rms [k].mesh;
				if (mesh == null)
					continue;
				if (((1 << t.gameObject.layer) & cullingMask) == 0)
					continue;
				if (!rms [k].renderer.isVisible)
					continue;
				
				if (rms [k].skinnedMesh) {
					cbMask.Clear ();
					cbMask.DrawRenderer(rms[k].renderer, fxMatMask);
					Graphics.ExecuteCommandBuffer (cbMask);
				} else {
					Vector3 lossyScale = t.lossyScale;
					Vector3 position = t.position;
					if (rms [k].bakedTransform) {
						if (rms [k].currentPosition != t.position || rms [k].currentRotation != t.eulerAngles || rms [k].currentScale != t.lossyScale) {
							BakeTransform (k, true);
						}
						rms [k].renderingMatrix = Matrix4x4.identity;
					} else {
						rms [k].renderingMatrix = Matrix4x4.TRS (position, t.rotation, lossyScale);
					}

					fxMatMask.SetPass (0);
					Graphics.DrawMeshNow (mesh, rms [k].renderingMatrix);
				}
			}

			// Compute tweening
			float fade = 1f;
			if (fading != FadingState.NoFading) {
				if (fading == FadingState.FadingIn) {
					if (fadeInDuration > 0) {
						fade = (Time.time - fadeStartTime) / fadeInDuration;
						if (fade > 1f) {
							fade = 1f;
							fading = FadingState.NoFading;
						}
					}
				} else if (fadeOutDuration > 0) {
					fade = 1f - (Time.time - fadeStartTime) / fadeOutDuration;
					if (fade < 0f) {
						fade = 0f;
						fading = FadingState.NoFading;
						_highlighted = false;
						if (OnObjectHighlightEnd != null) {
							OnObjectHighlightEnd (gameObject);
						}
						SendMessage ("HighlightEnd", null, SendMessageOptions.DontRequireReceiver);
					}
				}
			} 

			if (glowHQ) {
				glowReal *= 0.25f;
			}

			// Add effects
			for (int k = 0; k < rmsCount; k++) {
				Transform t = rms [k].transform;
				if (t == null)
					continue;
				Mesh mesh = rms [k].mesh;
				if (mesh == null)
					continue;
				if (((1 << t.gameObject.layer) & cullingMask) == 0)
					continue;
				if (!rms [k].renderer.isVisible)
					continue;
				
				// See-Through
				if (seeThroughReal) {
					if (rms [k].skinnedMesh) {
						cbSeeThrough.Clear ();
						for (int l = 0; l < mesh.subMeshCount; l++) {
							if (l < rms [k].fxMatSeeThrough.Length && rms [k].fxMatSeeThrough [l] != null) {
								cbSeeThrough.DrawRenderer (rms [k].renderer, rms [k].fxMatSeeThrough [l], l);
							}
						}
						Graphics.ExecuteCommandBuffer (cbSeeThrough);
					} else {
						for (int l = 0; l < mesh.subMeshCount; l++) {
							if (l < rms [k].fxMatSeeThrough.Length && rms [k].fxMatSeeThrough [l] != null) {
								rms [k].fxMatSeeThrough [l].SetPass (0);
								Graphics.DrawMeshNow (mesh, rms [k].renderingMatrix, l);
							}
						}
					}
				}

				if (!_highlighted)
					return;
					
				// Glow
				for (int l = 0; l < mesh.subMeshCount; l++) {
					if (glow > 0) {
						rms [k].fxMatGlow.SetVector ("_GlowDirection", Vector3.zero);
						for (int j = 0; j < glowPasses.Length; j++) {
							rms [k].fxMatGlow.SetColor ("_GlowColor", glowPasses [j].color);
							rms [k].fxMatGlow.SetVector ("_Glow", new Vector4 (fade * glowReal * glowPasses [j].alpha, glowPasses [j].offset * glowWidth / 100f, glowMagicNumber1, glowMagicNumber2));
							if (glowHQ) {
								for (int o = 0; o < offsets.Length; o++) {
									Vector3 direction = offsets [o];
									direction.y *= cam.aspect;
									rms [k].fxMatGlow.SetVector ("_GlowDirection", direction);

									if (rms [k].skinnedMesh) {
										cbGlow.Clear ();
										cbGlow.DrawRenderer (rms [k].renderer, rms [k].fxMatGlow, l);
										Graphics.ExecuteCommandBuffer (cbGlow);
									} else {
										rms [k].fxMatGlow.SetPass (0);
										Graphics.DrawMeshNow (mesh, rms [k].renderingMatrix, l);
									}
								}
							} else {
								if (rms [k].skinnedMesh) {
									cbGlow.Clear ();
									cbGlow.DrawRenderer (rms [k].renderer, rms [k].fxMatGlow, l);
									Graphics.ExecuteCommandBuffer (cbGlow);
								} else {
									rms [k].fxMatGlow.SetPass (0);
									Graphics.DrawMeshNow (mesh, rms [k].renderingMatrix, l);
								}
							}
						}
					}
					if (outline > 0) {
						Color outlineColor = this.outlineColor;
						outlineColor.a = outline * fade;
						rms [k].fxMatOutline.SetColor ("_OutlineColor", outlineColor);
						if (outlineHQ) {
							for (int o = 0; o < offsets.Length; o++) {
								Vector3 direction = offsets [o] * (outlineWidth / 100f);
								direction.y *= cam.aspect;
								rms [k].fxMatOutline.SetVector ("_OutlineDirection", direction);
								if (rms [k].skinnedMesh) {
									cbOutline.Clear ();
									cbOutline.DrawRenderer (rms [k].renderer, rms [k].fxMatOutline, l);
									Graphics.ExecuteCommandBuffer (cbOutline);
								} else {
									rms [k].fxMatOutline.SetPass (0);
									Graphics.DrawMeshNow (mesh, rms [k].renderingMatrix, l);
								}
							}
						} else {
							if (rms [k].skinnedMesh) {
								cbOutline.Clear ();
								cbOutline.DrawRenderer (rms [k].renderer, rms [k].fxMatOutline, l);
								Graphics.ExecuteCommandBuffer (cbOutline);
							} else {
								rms [k].fxMatOutline.SetPass (0);
								Graphics.DrawMeshNow (mesh, rms [k].renderingMatrix, l);
							}
						}
					}
					if (overlay > 0) {
						if (l < rms [k].fxMatOverlay.Length && rms [k].fxMatOverlay [l] != null) {
							Color overlayColor = this.overlayColor;
							overlayColor.a = overlay * fade;
							rms [k].fxMatOverlay [l].color = overlayColor;
							rms [k].fxMatOverlay [l].SetVector ("_OverlayData", new Vector3 (overlayAnimationSpeed, overlayMinIntensity, overlayBlending));

							if (rms [k].skinnedMesh) {
								cbOverlay.Clear ();
								cbOverlay.DrawRenderer (rms [k].renderer, rms [k].fxMatOverlay [l], l);
								Graphics.ExecuteCommandBuffer (cbOverlay);
							} else {
								rms [k].fxMatOverlay [l].SetPass (0);
								Graphics.DrawMeshNow (mesh, rms [k].renderingMatrix, l);
							}
						}
					}
					if (innerGlow > 0 && innerGlowWidth > 0) {
						Color innerGlowColorA = innerGlowColor;
						innerGlowColorA.a = innerGlow * fade;
						rms [k].fxMatInnerGlow.SetColor ("_Color", innerGlowColorA);

						if (rms [k].skinnedMesh) {
							cbInnerGlow.Clear ();
							cbInnerGlow.DrawRenderer (rms [k].renderer, rms [k].fxMatInnerGlow, l);
							Graphics.ExecuteCommandBuffer (cbInnerGlow);
						} else {
							rms [k].fxMatInnerGlow.SetPass (0);
							Graphics.DrawMeshNow (mesh, rms [k].renderingMatrix, l);
						}
					}
				}
				if (targetFX) {
					// Stay?
					float fadeOut = 1f;
					if (Application.isPlaying) {
						fadeOut = (Time.time - highlightStartTime);
						if (fadeOut >= targetFXStayDuration) {
							fadeOut -= targetFXStayDuration;
							fadeOut = 1f - fadeOut;
						}
						if (fadeOut > 1f) {
							fadeOut = 1f;
						}
					} 
					if (fadeOut > 0) {
						// Get scale
						float scaleT = 1f;
						float time;
						if (Application.isPlaying) {
							scaleT = (Time.time - highlightStartTime) / targetFXTransitionDuration;
							if (scaleT > 1f) {
								scaleT = 1f;
							}
							scaleT = Mathf.Sin (scaleT * Mathf.PI * 0.5f);
							time = Time.time;
						} else {
							time = (float)DateTime.Now.Subtract (DateTime.Today).TotalSeconds;
						}
						Bounds bounds = rms [k].renderer.bounds;
						Vector3 size = bounds.size;
						float minSize = size.x;
						if (size.y < minSize) {
							minSize = size.y;
						}
						if (size.z < minSize) {
							minSize = size.z;
						}
						size.x = size.y = size.z = minSize;
						size = Vector3.Lerp (size * targetFXInitialScale, size * targetFXEndScale, scaleT);
						Quaternion camRot = Quaternion.LookRotation (cam.transform.position - rms [k].transform.position); 
						Quaternion rotation = Quaternion.Euler (0, 0, time * targetFXRotationSpeed);
						camRot *= rotation;
						Vector3 center = targetFXCenter != null ? targetFXCenter.transform.position : bounds.center;
						Matrix4x4 m = Matrix4x4.TRS (center, camRot, size);
						Color color = targetFXColor;
						color.a *= fade * fadeOut;
						rms [k].fxMatTarget.color = color;
						rms [k].fxMatTarget.SetPass (0);
						Graphics.DrawMeshNow (quadMesh, m);
					}
				}
			}
		}

		void InitMaterial (ref Material material, string shaderName) {
			if (material == null) {
				Shader shaderFX = Shader.Find (shaderName);
				if (shaderFX == null) {
					Debug.LogError ("Shader " + shaderName + " not found.");
					enabled = false;
					return;
				}
				material = new Material (shaderFX);
			}
		}

		public void SetTarget (Transform transform) {
			if (transform == target || transform == null)
				return;

			if (_highlighted) {
				SetHighlighted (false);
			}

			target = transform;
			SetupMaterial ();
		}

		/// <summary>
		/// Start or finish highlight on the object
		/// </summary>
		public void SetHighlighted (bool state) {

			if (!Application.isPlaying) {
				_highlighted = state;
				return;
			}

			if (fading == FadingState.NoFading) {
				fadeStartTime = Time.time;
			}

			if (state && !ignore) {
				if (_highlighted && fading == FadingState.NoFading) {
					return;
				}
				if (OnObjectHighlightStart != null) {
					if (!OnObjectHighlightStart (gameObject)) {
						return; 
					}
				}
				SendMessage ("HighlightStart", null, SendMessageOptions.DontRequireReceiver);
				highlightStartTime = Time.time;
				if (fadeInDuration > 0) {
					if (fading == FadingState.FadingOut) {
						float remaining = fadeOutDuration - (Time.time - fadeStartTime);
						fadeStartTime = Time.time - remaining;
					}
					fading = FadingState.FadingIn;
				} else {
					fading = FadingState.NoFading;
				}
				_highlighted = true;
				Refresh ();
			} else if (_highlighted) {
				if (fadeOutDuration > 0) {
					if (fading == FadingState.FadingIn) {
						float elapsed = Time.time - fadeStartTime;
						fadeStartTime = Time.time + elapsed - fadeInDuration;
					}
					fading = FadingState.FadingOut; // when fade out ends, highlighted will be set to false in OnRenderObject
				} else {
					fading = FadingState.NoFading;
					_highlighted = false;
					if (OnObjectHighlightEnd != null) {
						OnObjectHighlightEnd (gameObject);
					}
					SendMessage ("HighlightEnd", null, SendMessageOptions.DontRequireReceiver);
				}
			}
		}

		void SetupMaterial () {

			#if UNITY_EDITOR
			staticChildren = false;
			#endif

			if (target == null || fxMatMask == null)
				return;

			Renderer[] rr = target.GetComponentsInChildren<Renderer> ();
			if (rms == null || rms.Length < rr.Length) {
				rms = new ModelMaterials[rr.Length];
			}

			rmsCount = 0;
			for (int k = 0; k < rr.Length; k++) {
				rms [rmsCount] = new ModelMaterials ();
				Renderer renderer = rr [k];
				rms [rmsCount].renderer = renderer;
				rms [rmsCount].renderWasVisibleDuringSetup = renderer.isVisible;

				if (renderer.transform != target && renderer.GetComponent<HighlightEffect> () != null)
					continue; // independent subobject

				if (OnRendererHighlightStart != null) {
					if (!OnRendererHighlightStart (renderer)) {
						rmsCount++;
						continue;
					}
				}

				if (renderer is SkinnedMeshRenderer) {
					// ignore cloth skinned renderers
					rms [rmsCount].skinnedMesh = true;
					rms [rmsCount].mesh = ((SkinnedMeshRenderer)renderer).sharedMesh;
					CheckCommandBuffers ();
				} else if (Application.isPlaying && renderer.isPartOfStaticBatch) {
					// static batched objects need to have a mesh collider in order to use its original mesh
					MeshCollider mc = renderer.GetComponent<MeshCollider> ();
					if (mc != null) {
						rms [rmsCount].mesh = mc.sharedMesh;
					}
				} else {
					MeshFilter mf = renderer.GetComponent<MeshFilter> ();
					if (mf != null) {
						rms [rmsCount].mesh = mf.sharedMesh;

						#if UNITY_EDITOR
						if (renderer.gameObject.isStatic && renderer.GetComponent<MeshCollider> () == null) {
							staticChildren = true;
						}
						#endif
							
					}
				}

				if (rms [rmsCount].mesh == null) {
					continue;
				}

				rms [rmsCount].transform = renderer.transform;
				rms [rmsCount].material = renderer.sharedMaterial;
				rms [rmsCount].fxMatGlow = Instantiate<Material> (fxMatGlow);
				rms [rmsCount].fxMatInnerGlow = Instantiate<Material> (fxMatInnerGlow);
				rms [rmsCount].fxMatOutline = Instantiate<Material> (fxMatOutline);
				rms [rmsCount].fxMatSeeThrough = Fork (fxMatSeeThrough, rms [rmsCount].mesh);
				rms [rmsCount].fxMatOverlay = Fork (fxMatOverlay, rms [rmsCount].mesh);
				rms [rmsCount].fxMatTarget = Instantiate<Material> (fxMatTarget);
				rms [rmsCount].originalMesh = rms [rmsCount].mesh;
				if (!rms [rmsCount].skinnedMesh) {
					// check if scale is negative
					BakeTransform (rmsCount, true);
				}
				rmsCount++;
			}

			UpdateMaterialProperties ();
		}

		void CheckGeometrySupportDependencies () {
			InitMaterial (ref fxMatMask, "HighlightPlus/Geometry/Mask");
			InitMaterial (ref fxMatGlow, "HighlightPlus/Geometry/Glow");
			InitMaterial (ref fxMatInnerGlow, "HighlightPlus/Geometry/InnerGlow");
			InitMaterial (ref fxMatOutline, "HighlightPlus/Geometry/Outline");
			InitMaterial (ref fxMatOverlay, "HighlightPlus/Geometry/Overlay");
			InitMaterial (ref fxMatSeeThrough, "HighlightPlus/Geometry/SeeThrough");
			InitMaterial (ref fxMatOccluder, "HighlightPlus/Geometry/SeeThroughOccluder");
			InitMaterial (ref fxMatTarget, "HighlightPlus/Geometry/Target");
		}

		void CheckCommandBuffers () {
			if (cbOccluder == null) {
				cbOccluder = new CommandBuffer ();
				cbOccluder.name = "Occluder";
			}
			if (cbMask == null) {
				cbMask = new CommandBuffer ();
				cbMask.name = "Mask";
			}
			if (cbSeeThrough == null) {
				cbSeeThrough = new CommandBuffer ();
				cbSeeThrough.name = "See Through";
			}
			if (cbGlow == null) {
				cbGlow = new CommandBuffer ();
				cbGlow.name = "Outer Glow";
			}
			if (cbOutline == null) {
				cbOutline = new CommandBuffer ();
				cbOutline.name = "Outline";
			}
			if (cbOverlay == null) {
				cbOverlay = new CommandBuffer ();
				cbOverlay.name = "Overlay";
			}
			if (cbInnerGlow == null) {
				cbInnerGlow = new CommandBuffer ();
				cbInnerGlow.name = "Inner Glow";
			}
		}

		Material[] Fork (Material mat, Mesh mesh) {
			if (mesh == null)
				return null;
			int count = mesh.subMeshCount;
			Material[] mm = new Material[count];
			for (int k = 0; k < count; k++) {
				mm [k] = Instantiate<Material> (mat);
			}
			return mm;
		}

		void BakeTransform (int i, bool duplicateMesh) {
			if (rms [i].mesh == null)
				return;
			Transform t = rms [i].transform;
			Vector3 scale = t.localScale;
			if (scale.x >= 0 && scale.y >= 0 && scale.z >= 0) {
				rms [i].bakedTransform = false;
				return;
			}
			// Duplicates mesh and bake rotation
			Mesh fixedMesh = duplicateMesh ? Instantiate<Mesh> (rms [i].originalMesh) : rms [i].mesh;
			Vector3[] vertices = fixedMesh.vertices;
			for (int k = 0; k < vertices.Length; k++) {
				vertices [k] = t.TransformPoint (vertices [k]);
			}
			fixedMesh.vertices = vertices;
			Vector3[] normals = fixedMesh.normals;
			if (normals != null) {
				for (int k = 0; k < normals.Length; k++) {
					normals [k] = t.TransformVector (normals [k]).normalized;
				}
				fixedMesh.normals = normals;
			}
			fixedMesh.RecalculateBounds ();
			rms [i].mesh = fixedMesh;
			rms [i].bakedTransform = true;
			rms [i].currentPosition = t.position;
			rms [i].currentRotation = t.eulerAngles;
			rms [i].currentScale = t.lossyScale;
		}


		void UpdateMaterialProperties () {

			if (rms == null)
				return;

			if (ignore) {
				_highlighted = false;
			}

			Color seeThroughTintColor = this.seeThroughTintColor;
			seeThroughTintColor.a = this.seeThroughTintAlpha;


			if (outlineWidth < 0) {
				outlineWidth = 0;
			}
			if (glowWidth < 0) {
				glowWidth = 0;
			}
			if (overlay < overlayMinIntensity) {
				overlay = overlayMinIntensity;
			}
			if (targetFXTransitionDuration <= 0) {
				targetFXTransitionDuration = 0.0001f;
			}
			if (targetFXStayDuration <= 0) {
				targetFXStayDuration = 0.0001f;
			}

			for (int k = 0; k < rmsCount; k++) {
				if (rms [k].mesh != null) {
					// Setup materials
					Material fxMat;

					// Outline
					fxMat = rms [k].fxMatOutline;
					float scaledOutlineWidth = outlineHQ ? 0f : outlineWidth / 100f;
					fxMat.SetFloat ("_OutlineWidth", scaledOutlineWidth);
					fxMat.SetVector ("_OutlineDirection", Vector3.zero);
					fxMat.SetInt ("_OutlineZTest", outlineAlwaysOnTop ? (int)UnityEngine.Rendering.CompareFunction.Always : (int)UnityEngine.Rendering.CompareFunction.LessEqual);
					fxMat.SetInt ("_OutlineCull", outlineCullBackFaces ? (int)UnityEngine.Rendering.CullMode.Back : (int)UnityEngine.Rendering.CullMode.Off);

					// Glow
					fxMat = rms [k].fxMatGlow;
					fxMat.SetVector ("_Glow2", new Vector3 (outlineWidth / 100f, glowAnimationSpeed, glowDithering ? 0 : 1));
					fxMat.SetInt ("_GlowZTest", glowAlwaysOnTop ? (int)UnityEngine.Rendering.CompareFunction.Always : (int)UnityEngine.Rendering.CompareFunction.LessEqual);
					fxMat.SetInt ("_GlowCull", glowCullBackFaces ? (int)UnityEngine.Rendering.CullMode.Back : (int)UnityEngine.Rendering.CullMode.Off);

					// Inner Glow
					fxMat = rms [k].fxMatInnerGlow;
					fxMat.SetFloat ("_Width", innerGlowWidth);
					fxMat.SetInt ("_InnerGlowZTest", innerGlowAlwaysOnTop ? (int)UnityEngine.Rendering.CompareFunction.Always : (int)UnityEngine.Rendering.CompareFunction.LessEqual);

					// Target
					fxMat = rms [k].fxMatTarget;
					if (targetFX && targetFXTexture == null) {
						targetFXTexture = Resources.Load<Texture2D> ("HighlightPlus/target");
					}
					fxMat.mainTexture = targetFXTexture;

					// Mask, See-through & Overlay per submesh
					for (int l = 0; l < rms [k].mesh.subMeshCount; l++) {
						Renderer renderer = rms [k].renderer;
						if (renderer == null)
							continue;

						Material mat = null;
						if (renderer.sharedMaterials != null && l < renderer.sharedMaterials.Length) {
							mat = renderer.sharedMaterials [l];
						}

						// See-through
						fxMat = rms [k].fxMatSeeThrough [l];
						if (fxMat != null) {
							fxMat.SetFloat ("_SeeThrough", seeThroughIntensity);
							fxMat.SetColor ("_SeeThroughTintColor", seeThroughTintColor);
							if (mat != null && mat.HasProperty ("_MainTex")) {
								Texture texture = mat.mainTexture;
								fxMat.mainTexture = texture;
								fxMat.mainTextureOffset = mat.mainTextureOffset;
								fxMat.mainTextureScale = mat.mainTextureScale;
							}
						}

						// Overlay
						fxMat = rms [k].fxMatOverlay [l];
						if (fxMat != null) {
							if (mat != null) {
								if (mat.HasProperty ("_MainTex")) {
									Texture texture = mat.mainTexture;
									fxMat.mainTexture = texture;
									fxMat.mainTextureOffset = mat.mainTextureOffset;
									fxMat.mainTextureScale = mat.mainTextureScale;
								}
								if (mat.HasProperty ("_Color")) {
									fxMat.SetColor ("_OverlayBackColor", mat.GetColor ("_Color"));
								}
							}
						}
					}
				}
			}
		}


		void BuildQuad () {
			quadMesh = new Mesh ();

			// Setup vertices
			Vector3[] newVertices = new Vector3[4];
			float halfHeight = 0.5f;
			float halfWidth = 0.5f;
			newVertices [0] = new Vector3 (-halfWidth, -halfHeight, 0);
			newVertices [1] = new Vector3 (-halfWidth, halfHeight, 0);
			newVertices [2] = new Vector3 (halfWidth, -halfHeight, 0);
			newVertices [3] = new Vector3 (halfWidth, halfHeight, 0);

			// Setup UVs
			Vector2[] newUVs = new Vector2[newVertices.Length];
			newUVs [0] = new Vector2 (0, 0);
			newUVs [1] = new Vector2 (0, 1);
			newUVs [2] = new Vector2 (1, 0);
			newUVs [3] = new Vector2 (1, 1);

			// Setup triangles
			int[] newTriangles = new int[] { 0, 1, 2, 3, 2, 1 };

			// Setup normals
			Vector3[] newNormals = new Vector3[newVertices.Length];
			for (int i = 0; i < newNormals.Length; i++) {
				newNormals [i] = Vector3.forward;
			}

			// Create quad
			quadMesh.vertices = newVertices;
			quadMesh.uv = newUVs;
			quadMesh.triangles = newTriangles;
			quadMesh.normals = newNormals;

			quadMesh.RecalculateBounds ();
		}

	}
}


