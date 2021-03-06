﻿#region license
/*
 * [Scientific Committee on Advanced Navigation]
 * 			S.C.A.N. Satellite
 * 
 * SCANmapLegend - Object to store data on map legend textures
 *
 * Copyright (c)2013 damny;
 * Copyright (c)2014 technogeeky <technogeeky@gmail.com>;
 * Copyright (c)2014 (Your Name Here) <your email here>; see LICENSE.txt for licensing details.
*/
#endregion

using SCANsat.SCAN_Platform.Palettes;
using SCANsat.SCAN_Data;
using palette = SCANsat.SCAN_UI.UI_Framework.SCANpalette;

using UnityEngine;

namespace SCANsat.SCAN_Map
{
	public class SCANmapLegend
	{
		private Texture2D legend;
		private float legendMin, legendMax;
		private Palette dataPalette;
		private int legendScheme;

		public Texture2D Legend
		{
			get { return legend; }
			set { legend = value; }
		}

		internal Texture2D getLegend(float min, float max, int scheme, SCANterrainConfig terrain)
		{
			if (legend != null && legendMin == min && legendMax == max && legendScheme == scheme && terrain.ColorPal.hash == dataPalette.hash)
				return legend;
			legend = new Texture2D(256, 1, TextureFormat.RGB24, false);
			legendMin = min;
			legendMax = max;
			legendScheme = scheme;
			dataPalette = terrain.ColorPal;
			Color[] pix = legend.GetPixels();
			for (int x = 0; x < 256; ++x)
			{
				float val = (x * (max - min)) / 256f + min;
				pix[x] = palette.heightToColor(val, scheme, terrain);
			}
			legend.SetPixels(pix);
			legend.Apply();
			return legend;
		}

		internal Texture2D getLegend(int scheme, SCANterrainConfig terrain)
		{
			Texture2D t = new Texture2D(256, 1, TextureFormat.RGB24, false);
			Color[] pix = t.GetPixels();
			for (int x = 0; x < 256; ++x)
			{
				float val = (x * (terrain.MaxTerrain - terrain.MinTerrain)) / 256f + terrain.MinTerrain;
				pix[x] = palette.heightToColor(val, scheme, terrain);
			}
			t.SetPixels(pix);
			t.Apply();
			return t;
		}

		internal Texture2D getLegend(float max, float min, float range, float? clamp, bool discrete, Color32[] c)
		{
			Texture2D t = new Texture2D(128, 1, TextureFormat.RGB24, false);
			Color[] pix = t.GetPixels();
			for (int x = 0; x < 128; x++)
			{
				float val = (x * (max - min)) / 128f + min;
				pix[x] = palette.heightToColor(val, max, min, range, clamp, discrete, c);
			}
			t.SetPixels(pix);
			t.Apply();
			return t;
		}
	}
}
