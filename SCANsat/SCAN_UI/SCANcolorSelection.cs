﻿#region license
/* 
 *  [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 *
 * SCANsat - Color Selection and Settings Menu
 * 
 * Copyright (c)2013 damny;
 * Copyright (c)2014 David Grandy <david.grandy@gmail.com>;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
 *
 */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using SCANsat.Platform;
using SCANsat.Platform.Palettes;
using palette = SCANsat.SCAN_UI.SCANpalette;
using UnityEngine;

namespace SCANsat.SCAN_UI
{
	class SCANcolorSelection: SCAN_MBW
	{
		private bool paletteBox, reversePalette, oldReverseState, discretePalette, oldDiscreteState;
		private bool spaceCenterLock, trackingStationLock, clampHeight, oldClampState;
		private Rect paletteRect;
		private Palette dataPalette;
		private string paletteSize = "6";
		private int paletteSizeInt, oldPaletteSizeInt = 6;
		private int paletteIndex;
		private Texture2D currentLegend, previewLegend;
		private string minHeightS = "-500";
		private string maxHeightS = "8000";
		private string clampHeightS = "0";
		private float minHeightF = -500;
		private float maxHeightF = 8000;
		private float clampHeightF = 0;
		private const string lockID = "colorLockID";
		internal static Rect defaultRect = new Rect(100, 400, 600, 300);

		private SCANdata data;

		protected override void Awake()
		{
			WindowCaption = "S.C.A.N. Color Management";
			WindowRect = defaultRect;
			WindowStyle = SCANskins.SCAN_window;
			WindowOptions = new GUILayoutOption[2] { GUILayout.Width(600), GUILayout.Height(300) };
			Visible = false;
			DragEnabled = true;
			ClampToScreenOffset = new RectOffset(-400, -400, -250, -250);

			SCAN_SkinsLibrary.SetCurrent("SCAN_Unity");

			InputLockManager.RemoveControlLock(lockID);
		}

		internal override void Start()
		{
			paletteSizeInt = palette.CurrentPalettes.size;
			paletteSize = paletteSizeInt.ToString();
		}

		internal override void OnDestroy()
		{
			InputLockManager.RemoveControlLock(lockID);
		}

		protected override void DrawWindowPre(int id)
		{
			if (HighLogic.LoadedSceneIsFlight)
			{
				if (data == null)
				{
					data = SCANUtil.getData(FlightGlobals.currentMainBody);
					if (data == null)
					{
						data = new SCANdata(FlightGlobals.currentMainBody);
						SCANcontroller.controller.addToBodyData(FlightGlobals.currentMainBody, data);
					}
				}
				else if (data.Body != FlightGlobals.currentMainBody)
				{
					data = SCANUtil.getData(FlightGlobals.currentMainBody);
					if (data == null)
					{
						data = new SCANdata(FlightGlobals.currentMainBody);
						SCANcontroller.controller.addToBodyData(FlightGlobals.currentMainBody, data);
					}
				}
			}

			//Lock space center click through - Sync SCANdata
			else if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				data = ((SCANkscMap)SCANcontroller.controller.kscMap).data;
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && !spaceCenterLock)
				{
					InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS | ControlTypes.KSC_FACILITIES | ControlTypes.KSC_UI, lockID);
					spaceCenterLock = true;
				}
				else if (!WindowRect.Contains(mousePos) && spaceCenterLock)
				{
					InputLockManager.RemoveControlLock(lockID);
					spaceCenterLock = false;
				}
			}

			//Lock tracking scene click through - Sync SCANdata
			else if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				data = ((SCANkscMap)SCANcontroller.controller.kscMap).data;
				Vector2 mousePos = Input.mousePosition;
				mousePos.y = Screen.height - mousePos.y;
				if (WindowRect.Contains(mousePos) && !trackingStationLock)
				{
					InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS | ControlTypes.TRACKINGSTATION_ALL, lockID);
					trackingStationLock = true;
				}
				else if (!WindowRect.Contains(mousePos) && trackingStationLock)
				{
					InputLockManager.RemoveControlLock(lockID);
					trackingStationLock = false;
				}
			}

			if (currentLegend == null || data.ColorPalette != dataPalette)
			{
				dataPalette = data.ColorPalette;
				minHeightF = data.MinHeight;
				minHeightS = minHeightF.ToString();
				maxHeightF = data.MaxHeight;
				maxHeightS = maxHeightF.ToString();
				oldPaletteSizeInt= paletteSizeInt = data.PaletteSize;
				paletteSize = paletteSizeInt.ToString();
				oldReverseState = reversePalette = data.PaletteReverse;
				oldDiscreteState = discretePalette = data.PaletteDiscrete;
				oldClampState = clampHeight = data.ClampHeight != null;
				if (clampHeight)
				{
					clampHeightF = (float)data.ClampHeight;
					clampHeightS = clampHeightF.ToString();
				}
				palette.CurrentPalettes = palette.setCurrentPalettesType(dataPalette.kind);
				palette.CurrentPalette = palette.CurrentPalettes.availablePalettes[0];
				regenPaletteSets();
				drawCurrentLegend();
			}
			if (previewLegend == null)
			{
				drawPreviewLegend();
			}
		}

		protected override void DrawWindow(int id)
		{
			versionLabel(id);
			closeBox(id);

			growS();
				growE();
					paletteTextures(id);
					paletteOptions(id);
				stopE();
				fillS(8);
				growE();
					palettePreview(id);
					fillS(20);
					paletteConfirmation(id);
				stopE();
			stopS();

			paletteSelectionBox(id);
		}

		protected override void DrawWindowPost(int id)
		{
			if (paletteBox && Event.current.type == EventType.mouseDown && !paletteRect.Contains(Event.current.mousePosition))
				paletteBox = false;

			if (reversePalette != oldReverseState)
			{
				oldReverseState = reversePalette;
				drawPreviewLegend();
			}

			if (discretePalette != oldDiscreteState)
			{
				oldDiscreteState = discretePalette;
				drawPreviewLegend();
			}

			if (clampHeight != oldClampState)
			{
				oldClampState = clampHeight;
				drawPreviewLegend();
			}

			if (paletteSizeInt != oldPaletteSizeInt)
			{
				if (paletteSizeInt > 2)
				{
					oldPaletteSizeInt = paletteSizeInt;
					regenPaletteSets();
					palette.CurrentPalette = palette.CurrentPalettes.availablePalettes[paletteIndex];
					drawPreviewLegend();
				}
			}
		}

		//Draw the version label in the upper left corner
		private void versionLabel(int id)
		{
			Rect r = new Rect(6, 0, 40, 18);
			GUI.Label(r, SCANversions.SCANsatVersion, SCANskins.SCAN_whiteReadoutLabel);
		}

		//Draw the close button in the upper right corner
		private void closeBox(int id)
		{
			Rect r = new Rect(WindowRect.width - 20, 0, 18, 18);
			if (GUI.Button(r, SCANcontroller.controller.closeBox, SCANskins.SCAN_closeButton))
			{
				InputLockManager.RemoveControlLock(lockID);
				spaceCenterLock = false;
				trackingStationLock = false;
				Visible = false;
			}
		}

		//Draw the palette selection field
		private void paletteTextures(int id)
		{
			growS();
				GUILayout.Label("Palette Selection", SCANskins.SCAN_headline);
				fillS(8);
				growE();
					if (GUILayout.Button("Palette Style:", SCANskins.SCAN_buttonFixed, GUILayout.MaxWidth(120)))
					{
						paletteBox = !paletteBox;
					}
					fillS(10);
					GUILayout.Label(palette.getPaletteTypeName, SCANskins.SCAN_whiteReadoutLabel);
				stopE();
				growE();
					int j = 9;
					if (palette.CurrentPalettes.paletteType == Palette.Kind.Sequential)
						j = 12;
					else if (palette.CurrentPalettes.paletteType == Palette.Kind.Invertable || palette.CurrentPalettes.paletteType == Palette.Kind.Unknown)
						j = 0;
					for (int i = 0; i < j; i++)
					{
						if (i % 3 == 0)
						{
							stopE();
							fillS(8);
							growE();
						}
						Texture2D t = palette.CurrentPalettes.paletteSwatch[i];
						if (paletteBox)
						{
							GUILayout.Label("", GUILayout.Width(110), GUILayout.Height(25));
						}
						else
						{
							if (GUILayout.Button("", SCANskins.SCAN_texButton, GUILayout.Width(110), GUILayout.Height(25)))
							{
								palette.CurrentPalette = palette.CurrentPalettes.availablePalettes[i];
								paletteIndex = palette.CurrentPalette.index;
								drawPreviewLegend();
							}
						}
						Rect r = GUILayoutUtility.GetLastRect();
						r.width -= 10;
						GUI.DrawTexture(r, t);
					}
				stopE();
			stopS();
		}

		//Main palette option settings
		private void paletteOptions(int id)
		{
			growS();
				GUILayout.Label("Palette Options", SCANskins.SCAN_headline);
				fillS(8);
				GUILayout.Label("Terrain Height Range", SCANskins.SCAN_headlineSmall);

				growE();
					fillS();
					minHeightF = drawInputBox(ref minHeightS, SCANskins.SCAN_textBox, 40, 40, "Min:", SCANskins.SCAN_whiteReadoutLabel);
					fillS(10);
					maxHeightF = drawInputBox(ref maxHeightS, SCANskins.SCAN_textBox, 40, 40, "Max:", SCANskins.SCAN_whiteReadoutLabel);
					fillS();
				stopE();

				growE();
					fillS();
					clampHeight = GUILayout.Toggle(clampHeight, "Clamp Terrain", GUILayout.Width(100));
					if (clampHeight)
					{
						fillS(5);
						clampHeightF = drawInputBox(ref clampHeightS, SCANskins.SCAN_textBox, 40);
					}
					fillS();
				stopE();

				GUILayout.Label("Palette Size", SCANskins.SCAN_headlineSmall);
				paletteSizeInt = (int)drawInputBox(ref paletteSize, SCANskins.SCAN_textBox, 35, 40, "Size:", SCANskins.SCAN_whiteReadoutLabel);

				growE();
					reversePalette = GUILayout.Toggle(reversePalette, "Reverse Order");
					fillS(10);
					discretePalette = GUILayout.Toggle(discretePalette, "Discrete Gradient");
				stopE();

			stopS();
		}

		//Two boxes to show the current and new palettes as they appear on the legend
		private void palettePreview(int id)
		{
			growS();
				GUILayout.Label("Current Palette", SCANskins.SCAN_headlineSmall);
				fillS(8);
				GUILayout.Label("", SCANskins.SCAN_legendTex, GUILayout.Width(180), GUILayout.Height(25));
				Rect r = GUILayoutUtility.GetLastRect();
				GUI.DrawTexture(r, currentLegend);
			stopS();
			fillS(8);
			growS();
				GUILayout.Label("New Palette", SCANskins.SCAN_headlineSmall);
				fillS(8);
				GUILayout.Label("", SCANskins.SCAN_legendTex, GUILayout.Width(180), GUILayout.Height(25));
				r = GUILayoutUtility.GetLastRect();
				GUI.DrawTexture(r, previewLegend);
			stopS();
		}

		//Buttons to apply the new palette or cancel and return to the original
		private void paletteConfirmation(int id)
		{
			growS();
			fillS(16);
			if (GUILayout.Button("Apply", GUILayout.Width(80)))
			{
				if (minHeightF < maxHeightF)
				{
					data.MinHeight = minHeightF;
					data.MaxHeight = maxHeightF;
				}
				if (clampHeight)
				{
					if (clampHeightF > minHeightF && clampHeightF < maxHeightF)
						data.ClampHeight = (float?)clampHeightF;
				}
				else
					data.ClampHeight = null;

				data.ColorPalette = palette.CurrentPalette;
				data.PaletteName = palette.CurrentPalette.name;
				data.PaletteSize = palette.CurrentPalette.size;
				data.PaletteDiscrete = discretePalette;
				data.PaletteReverse = reversePalette;
				drawCurrentLegend();
			}
			fillS(8);
			if (GUILayout.Button("Cancel", GUILayout.Width(80)))
			{
				palette.CurrentPalette = data.ColorPalette;
				InputLockManager.RemoveControlLock(lockID);
				spaceCenterLock = false;
				trackingStationLock = false;
				Visible = false;
			}
			stopS();
		}

		//Drop down menu for palette selection
		private void paletteSelectionBox(int id)
		{
			if (paletteBox)
			{
				paletteRect = new Rect(40, 90, 100, 80);
				GUI.Box(paletteRect, "", SCANskins.SCAN_dropDownBox);
				for (int i = 0; i < Palette.kindNames.Length; i++)
				{
					Rect r = new Rect(paletteRect.x + 10, paletteRect.y + 5 + (i * 23), 80, 22);
					if (GUI.Button(r, Palette.kindNames[i], SCANskins.SCAN_dropDownButton))
					{
						paletteBox = false;
						palette.CurrentPalettes = palette.setCurrentPalettesType((Palette.Kind)i);
					}
				}
			}
		}

		private float drawInputBox(ref string oldVal, GUIStyle boxStyle, float boxWidth, float labelWidth = 0, string title = "", GUIStyle labelStyle = null)
		{
			float newVal = 0;
			float.TryParse(oldVal, out newVal);
			growE();
				if (!string.IsNullOrEmpty(title))
					GUILayout.Label(title, labelStyle, GUILayout.Width(labelWidth));
				oldVal = GUILayout.TextField(oldVal, boxStyle, GUILayout.Width(boxWidth));
			stopE();

			float.TryParse(oldVal, out newVal);
			return newVal;
		}

		private void drawCurrentLegend()
		{
			currentLegend = SCANmap.getLegend(0, data);
		}

		private void drawPreviewLegend()
		{
			float? clamp = null;
			Color32[] c = palette.CurrentPalette.colors;
			if (clampHeight)
				clamp = (float?)clampHeightF;
			if (reversePalette)
				c = palette.CurrentPalette.colorsReverse;
			previewLegend = SCANmap.getLegend(maxHeightF, minHeightF, clamp, discretePalette, c);
		}

		private void regenPaletteSets()
		{
			palette.DivPaletteSet = palette.generatePaletteSet(paletteSizeInt, Palette.Kind.Diverging);
			palette.QualPaletteSet = palette.generatePaletteSet(paletteSizeInt, Palette.Kind.Qualitative);
			palette.SeqPaletteSet = palette.generatePaletteSet(paletteSizeInt, Palette.Kind.Sequential);
			palette.CurrentPalettes = palette.setCurrentPalettesType(palette.getPaletteType);
		}

	}
}