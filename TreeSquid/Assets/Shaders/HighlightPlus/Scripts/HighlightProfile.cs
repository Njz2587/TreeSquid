using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HighlightPlus {

	[CreateAssetMenu (menuName = "Highlight Plus Profile", fileName = "Highlight Plus Profile", order = 100)]
	[HelpURL ("https://kronnect.freshdesk.com/support/solutions/42000065090")]
	public class HighlightProfile : ScriptableObject {

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
		public bool outlineHQ;
		public bool outlineAlwaysOnTop;
		public bool outlineCullBackFaces;

		[Range (0, 5)]
		public float glow = 1f;
		public float glowWidth = 0.4f;
		public bool glowHQ = false;
		public bool glowDithering = true;
		public float glowMagicNumber1 = 0.75f;
		public float glowMagicNumber2 = 0.5f;
		public float glowAnimationSpeed = 1f;
		public bool glowAlwaysOnTop;
		public GlowPassData[] glowPasses;
		public bool glowCullBackFaces;

		[Range (0, 5f)]
		public float innerGlow = 0f;
		[Range (0, 2)]
		public float innerGlowWidth = 1f;
		public Color innerGlowColor = Color.white;
		public bool innerGlowAlwaysOnTop;

		public bool targetFX;
		public Texture2D targetFXTexture;
		public Color targetFXColor = Color.white;
		public float targetFXRotationSpeed = 50f;
		public float targetFXInitialScale = 4f;
		public float targetFXEndScale = 1.5f;
		public float targetFXTransitionDuration = 0.5f;
		public float targetFXStayDuration = 1.5f;

		public SeeThroughMode seeThrough;
		[Range (0, 5f)]
		public float seeThroughIntensity = 0.8f;
		[Range (0, 1)]
		public float seeThroughTintAlpha = 0.5f;
		public Color seeThroughTintColor = Color.red;


		public void Load (HighlightEffect effect) {
			effect.fadeInDuration = fadeInDuration;
			effect.fadeOutDuration = fadeOutDuration;
			effect.overlay = overlay;
			effect.overlayColor = overlayColor;
			effect.overlayAnimationSpeed = overlayAnimationSpeed;
			effect.overlayMinIntensity = overlayMinIntensity;
			effect.overlayBlending = overlayBlending;
			effect.outline = outline;
			effect.outlineColor = outlineColor;
			effect.outlineWidth = outlineWidth;
			effect.outlineHQ = outlineHQ;
			effect.outlineAlwaysOnTop = outlineAlwaysOnTop;
			effect.outlineCullBackFaces = outlineCullBackFaces;
			effect.glow = glow;
			effect.glowWidth = glowWidth;
			effect.glowHQ = glowHQ;
			effect.glowDithering = glowDithering;
			effect.glowMagicNumber1 = glowMagicNumber1;
			effect.glowMagicNumber2 = glowMagicNumber2;
			effect.glowAnimationSpeed = glowAnimationSpeed;
			effect.glowAlwaysOnTop = glowAlwaysOnTop;
			effect.glowCullBackFaces = glowCullBackFaces;
			effect.glowPasses = GetGlowPassesCopy (glowPasses);
			effect.innerGlow = innerGlow;
			effect.innerGlowWidth = innerGlowWidth;
			effect.innerGlowColor = innerGlowColor;
			effect.innerGlowAlwaysOnTop = innerGlowAlwaysOnTop;
			effect.targetFX = targetFX;
			effect.targetFXColor = targetFXColor;
			effect.targetFXEndScale = targetFXEndScale;
			effect.targetFXInitialScale = targetFXInitialScale;
			effect.targetFXRotationSpeed = targetFXRotationSpeed;
			effect.targetFXStayDuration = targetFXStayDuration;
			effect.targetFXTexture = targetFXTexture;
			effect.targetFXTransitionDuration = targetFXTransitionDuration;
			effect.seeThrough = seeThrough;
			effect.seeThroughIntensity = seeThroughIntensity;
			effect.seeThroughTintAlpha = seeThroughTintAlpha;
			effect.seeThroughTintColor = seeThroughTintColor;
		}

		public void Save (HighlightEffect effect) {
			fadeInDuration = effect.fadeInDuration;
			fadeOutDuration = effect.fadeOutDuration;
			overlay = effect.overlay;
			overlayColor = effect.overlayColor;
			overlayAnimationSpeed = effect.overlayAnimationSpeed;
			overlayMinIntensity = effect.overlayMinIntensity;
			overlayBlending = effect.overlayBlending;
			outline = effect.outline;
			outlineColor = effect.outlineColor;
			outlineWidth = effect.outlineWidth;
			outlineHQ = effect.outlineHQ;
			outlineAlwaysOnTop = effect.outlineAlwaysOnTop;
			outlineCullBackFaces = effect.outlineCullBackFaces;
			glow = effect.glow;
			glowWidth = effect.glowWidth;
			glowHQ = effect.glowHQ;
			glowDithering = effect.glowDithering;
			glowMagicNumber1 = effect.glowMagicNumber1;
			glowMagicNumber2 = effect.glowMagicNumber2;
			glowAnimationSpeed = effect.glowAnimationSpeed;
			glowAlwaysOnTop = effect.glowAlwaysOnTop;
			glowCullBackFaces = effect.glowCullBackFaces;
			glowPasses = GetGlowPassesCopy (effect.glowPasses);
			innerGlow = effect.innerGlow;
			innerGlowWidth = effect.innerGlowWidth;
			innerGlowColor = effect.innerGlowColor;
			innerGlowAlwaysOnTop = effect.innerGlowAlwaysOnTop;
			targetFX = effect.targetFX;
			targetFXColor = effect.targetFXColor;
			targetFXEndScale = effect.targetFXEndScale;
			targetFXInitialScale = effect.targetFXInitialScale;
			targetFXRotationSpeed = effect.targetFXRotationSpeed;
			targetFXStayDuration = effect.targetFXStayDuration;
			targetFXTexture = effect.targetFXTexture;
			targetFXTransitionDuration = effect.targetFXTransitionDuration;
			seeThrough = effect.seeThrough;
			seeThroughIntensity = effect.seeThroughIntensity;
			seeThroughTintAlpha = effect.seeThroughTintAlpha;
			seeThroughTintColor = effect.seeThroughTintColor;
		}

		GlowPassData[] GetGlowPassesCopy (GlowPassData[] glowPasses) {
			if (glowPasses == null) {
				return new GlowPassData[0];
			}
			GlowPassData[] pd = new GlowPassData[glowPasses.Length];
			for (int k = 0; k < glowPasses.Length; k++) {
				pd [k].alpha = glowPasses [k].alpha;
				pd [k].color = glowPasses [k].color;
				pd [k].offset = glowPasses [k].offset;
			}
			return pd;
		}
	}
}

