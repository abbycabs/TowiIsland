﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CoreDraw
{
		public enum Samples
		{
				None,
				Samples2,
				Samples4,
				Samples8,
				Samples16,
				Samples32,
				RotatedDisc
		}
		[System.Serializable]
		public class BezierPoint
		{
				public Vector2 main;
				public Vector2 control1;//Think of as left
				public Vector2 control2;//Right
				//var rect : Rect;
				public BezierCurve curve1;//Left
				public BezierCurve curve2;//Right
		
				public BezierPoint (Vector2 m, Vector2 l, Vector2 r)
				{
						main = m;
						control1 = l;
						control2 = r;
				}
		}
	
		public class BezierCurve
		{
				public Vector2[] points;
				public float aproxLength;
				public Rect rect;

				public Vector2 Get (float t)
				{
						int t2 =(int)Mathf.Round (t * (points.Length - 1));
						return points [t2];
				}
		
				public void Init (Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
				{
						Vector2 topleft = new Vector2 (Mathf.Infinity, Mathf.Infinity);
						Vector2 bottomright = new Vector2 (Mathf.NegativeInfinity, Mathf.NegativeInfinity);
			
						topleft.x = Mathf.Min (topleft.x, p0.x);
						topleft.x = Mathf.Min (topleft.x, p1.x);
						topleft.x = Mathf.Min (topleft.x, p2.x);
						topleft.x = Mathf.Min (topleft.x, p3.x);
			
						topleft.y = Mathf.Min (topleft.y, p0.y);
						topleft.y = Mathf.Min (topleft.y, p1.y);
						topleft.y = Mathf.Min (topleft.y, p2.y);
						topleft.y = Mathf.Min (topleft.y, p3.y);
			
						bottomright.x = Mathf.Max (bottomright.x, p0.x);
						bottomright.x = Mathf.Max (bottomright.x, p1.x);
						bottomright.x = Mathf.Max (bottomright.x, p2.x);
						bottomright.x = Mathf.Max (bottomright.x, p3.x);
			
						bottomright.y = Mathf.Max (bottomright.y, p0.y);
						bottomright.y = Mathf.Max (bottomright.y, p1.y);
						bottomright.y = Mathf.Max (bottomright.y, p2.y);
						bottomright.y = Mathf.Max (bottomright.y, p3.y);
			
						Rect rect = new Rect (topleft.x, topleft.y, bottomright.x - topleft.x, bottomright.y - topleft.y);
			
			
						List<Vector2> ps = new List<Vector2> ();
			
						Vector2 point1 = Mathfx.CubicBezier (0f, p0, p1, p2, p3);
						Vector2 point2 = Mathfx.CubicBezier (0.05f, p0, p1, p2, p3);
						Vector2 point3 = Mathfx.CubicBezier (0.1f, p0, p1, p2, p3);
						Vector2 point4 = Mathfx.CubicBezier (0.15f, p0, p1, p2, p3);
			
						Vector2 point5 = Mathfx.CubicBezier (0.5f, p0, p1, p2, p3);
						Vector2 point6 = Mathfx.CubicBezier (0.55f, p0, p1, p2, p3);
						Vector2 point7 = Mathfx.CubicBezier (0.6f, p0, p1, p2, p3);
			
						float aproxLength = Vector2.Distance (point1, point2) + Vector2.Distance (point2, point3) + Vector2.Distance (point3, point4) + Vector2.Distance (point5, point6) + Vector2.Distance (point6, point7);
			
						Debug.Log (Vector2.Distance (point1, point2) + "     " + Vector2.Distance (point3, point4) + "   " + Vector2.Distance (point6, point7));
						aproxLength *= 4;
			
						float a2 = 0.5f / aproxLength;//Double the amount of points since the aproximation is quite bad
						for (float i = 0; i<1; i+=a2) {
								ps.Add (Mathfx.CubicBezier (i, p0, p1, p2, p3));
						}
						points = ps.ToArray ();
				}
		
				public BezierCurve (Vector2 main, Vector2 control1, Vector2 control2, Vector2 end)
				{
						Init (main, control1, control2, end);
				}
		}

		public class Drawing : MonoBehaviour
		{

				public static Samples NumSamples = Samples.Samples4;
	
				public static Texture2D DrawLine (Vector2 from, Vector2 to, float w, Color col, Texture2D tex)
				{
						return DrawLine (from, to, w, col, tex, false, Color.black, 0);
				}
	
				public static Texture2D DrawLine (Vector2 from, Vector2 to, float w, Color col, Texture2D tex, bool stroke, Color strokeCol, float strokeWidth)
				{
						w = Mathf.Round (w);//It is important to round the numbers otherwise it will mess up with the texture width
						strokeWidth = Mathf.Round (strokeWidth);
		
						float extent = w + strokeWidth;
						int stY =(int) Mathf.Clamp (Mathf.Min (from.y, to.y) - extent, 0, tex.height);//This is the topmost Y value
						int stX =(int) Mathf.Clamp (Mathf.Min (from.x, to.x) - extent, 0, tex.width);
						int endY =(int) Mathf.Clamp (Mathf.Max (from.y, to.y) + extent, 0, tex.height);
						int endX =(int) Mathf.Clamp (Mathf.Max (from.x, to.x) + extent, 0, tex.width);//This is the rightmost Y value
		
						strokeWidth = strokeWidth / 2;
						float strokeInner = (w - strokeWidth) * (w - strokeWidth);
						float strokeOuter = (w + strokeWidth) * (w + strokeWidth);
						float strokeOuter2 = (w + strokeWidth + 1) * (w + strokeWidth + 1);
						float sqrW = w * w;//It is much faster to calculate with squared values
		
						int lengthX = endX - stX;
						int lengthY = endY - stY;
						Vector2 start = new Vector2 (stX, stY);
						Color[] pixels = tex.GetPixels (stX, stY, lengthX, lengthY, 0);//Get all pixels
		
						for (int y=0; y<lengthY; y++) {
								for (int x=0; x<lengthX; x++) {//Loop through the pixels
										Vector2 p = new  Vector2 (x, y) + start;
										Vector2 center = p + new Vector2 (0.5f, 0.5f);
										float dist = (center - Mathfx.NearestPointStrict (from, to, center)).sqrMagnitude;//The squared distance from the center of the pixels to the nearest point on the line
										if (dist <= strokeOuter2) {
												Vector2[] samples = Sample (p);
												Color c = Color.black;
												Color pc = pixels [y * lengthX + x];
												for (int i=0; i<samples.Length; i++) {//Loop through the samples
														dist = (samples [i] - Mathfx.NearestPointStrict (from, to, samples [i])).sqrMagnitude;//The squared distance from the sample to the line
														if (stroke) {
																if (dist <= strokeOuter && dist >= strokeInner) {
																		c += strokeCol;
																} else if (dist < sqrW) {
																		c += col;
																} else {
																		c += pc;
																}
														} else {
																if (dist < sqrW) {//Is the distance smaller than the width of the line
																		c += col;
																} else {
																		c += pc;//No it wasn't, set it to be the original colour
																}
														}
												}
												c /= samples.Length;//Get the avarage colour
												pixels [y * lengthX + x] = c;
										}
								}
						}
						tex.SetPixels (stX, stY, lengthX, lengthY, pixels, 0);
						tex.Apply ();
						return tex;
				}
	
				public static Texture2D Paint (Vector2 pos, float rad, Color col, float hardness, Texture2D tex)
				{
						Vector2 start = new Vector2 (Mathf.Clamp (pos.x - rad, 0, tex.width), Mathf.Clamp (pos.y - rad, 0, tex.height));
						float width = rad * 2;
						Vector2 end = new Vector2 (Mathf.Clamp (pos.x + rad, 0, tex.width), Mathf.Clamp (pos.y + rad, 0, tex.height));
						int widthX =(int) Mathf.Round (end.x - start.x);
						int widthY =(int) Mathf.Round (end.y - start.y);
						float sqrRad = rad * rad;
						float sqrRad2 = (rad + 1) * (rad + 1);
						Color[] pixels = tex.GetPixels ((int)start.x, (int)start.y, widthX, widthY, 0);
		
						for (int y=0; y<widthY; y++) {
								for (int x=0; x<widthX; x++) {
										Vector2 p = new Vector2 (x, y) + start;
										Vector2 center = p + new Vector2 (0.5f, 0.5f);
										float dist = (center - pos).sqrMagnitude;
										if (dist < sqrRad2) {
												Vector2[] samples = Sample (p);
												Color c = Color.black;
												for (int i=0; i<samples.Length; i++) {
														dist = Mathfx.GaussFalloff (Vector2.Distance (samples [i], pos), rad) * hardness;
														if (dist > 0) {
																c += Color.Lerp (pixels [y * widthX + x], col, dist);
														} else {
																c += pixels [y * widthX + x];
														}
												}
												c /= samples.Length;
					
												pixels [y * widthX + x] = c;
										}
								}
						}
		
						tex.SetPixels ((int)start.x, (int)start.y, widthX, widthY, pixels, 0);
						return tex;
				}
	
				public static Texture2D PaintLine (Vector2 from, Vector2 to, float rad, Color col, float hardness, Texture2D tex)
				{
						float width = rad * 2;
						float extent = rad;
						int stY =(int) Mathf.Clamp (Mathf.Min (from.y, to.y) - extent, 0, tex.height);
						int stX =(int) Mathf.Clamp (Mathf.Min (from.x, to.x) - extent, 0, tex.width);
						int endY =(int) Mathf.Clamp (Mathf.Max (from.y, to.y) + extent, 0, tex.height);
						int endX =(int) Mathf.Clamp (Mathf.Max (from.x, to.x) + extent, 0, tex.width);

						int lengthX = endX - stX;
						int lengthY = endY - stY;

						float sqrRad = rad * rad;
						float sqrRad2 = (rad + 1) * (rad + 1);
						Color[] pixels = tex.GetPixels (stX, stY, lengthX, lengthY, 0);
						Vector2 start = new Vector2 (stX, stY);

						for (int y=0; y<lengthY; y++) {
								for (int x=0; x<lengthX; x++) {
										Vector2 p = new Vector2 (x, y) + start;
										Vector2 center = p + new Vector2 (0.5f, 0.5f);
										float dist = (center -Mathfx.NearestPointStrict (from, to, center)).sqrMagnitude;
										Color c;
										if (dist < sqrRad2) {
												dist = Mathfx.GaussFalloff (Mathf.Sqrt (dist), rad) * hardness;
												//dist = (samples[i]-pos).sqrMagnitude;
												if (dist > 0) {
														c = Color.Lerp (pixels [y * lengthX + x], col, dist);
												} else {
														c = pixels [y * lengthX + x];
												}
					
												pixels [y * lengthX + x] = c;
										}
								}
						}
						tex.SetPixels ((int)start.x, (int)start.y, lengthX, lengthY, pixels, 0);
						return tex;
				}

				public static void DrawBezier (BezierPoint[] points, float rad, Color col, Texture2D tex)
				{
						rad = Mathf.Round (rad);//It is important to round the numbers otherwise it will mess up with the texture width
		
						if (points.Length <= 1) {
								return;
						}
		
						Vector2 topleft = new Vector2 (Mathf.Infinity, Mathf.Infinity);
						Vector2 bottomright = new Vector2 (0, 0);
		
						for (int i=0; i<points.Length-1; i++) {
								BezierCurve curve = new BezierCurve (points [i].main, points [i].control2, points [i + 1].control1, points [i + 1].main);
								points [i].curve2 = curve;
								points [i + 1].curve1 = curve;
			
								topleft.x = Mathf.Min (topleft.x, curve.rect.x);
			
								topleft.y = Mathf.Min (topleft.y, curve.rect.y);
			
								bottomright.x = Mathf.Max (bottomright.x, curve.rect.x + curve.rect.width);
			
								bottomright.y = Mathf.Max (bottomright.y, curve.rect.y + curve.rect.height);
						}
		
						topleft -= new Vector2 (rad, rad);
						bottomright += new Vector2 (rad, rad);
		
						Vector2 start = new Vector2 (Mathf.Clamp (topleft.x, 0, tex.width), Mathf.Clamp (topleft.y, 0, tex.height));
						Vector2 width = new Vector2 (Mathf.Clamp (bottomright.x - topleft.x, 0, tex.width - start.x), Mathf.Clamp (bottomright.y - topleft.y, 0, tex.height - start.y));
		
						Color[] pixels = tex.GetPixels ((int)start.x, (int)start.y, (int)width.x, (int)width.y, 0);
		
						for (int y=0; y<width.y; y++) {
								for (int x=0; x<width.x; x++) {
										Vector2 p = new Vector2 (x + start.x, y + start.y);
										if (Mathfx.IsNearBeziers (p, points, rad + 2)) {
												Vector2[] samples = Sample (p);
												Color c = Color.black;
												Color pc = pixels [y * (int)width.x + x];//Previous pixel color
												for (int i=0; i<samples.Length; i++) {
														if (Mathfx.IsNearBeziers (samples [i], points, rad)) {
																c += col;
														} else {
																c += pc;
														}
												}
												c /= samples.Length;
												pixels [y * (int)width.x + x] = c;
										}	
								}
						}
		
						tex.SetPixels ((int)start.x, (int)start.y, (int)width.x, (int)width.y, pixels, 0);
						tex.Apply ();
				}
	
				public static Vector2[] Sample (Vector2 p)
				{
						switch (NumSamples) {
						case Samples.None:
								return new Vector2[] {p + new Vector2 (0.5f, 0.5f)};
						case Samples.Samples2:
								return new Vector2[]{p + new Vector2 (0.25f, 0.5f),p + new Vector2 (0.75f, 0.5f)};
						case Samples.Samples4: 
								return new Vector2[]{
								        p + new Vector2 (0.25f, 0.5f),
								        p + new Vector2 (0.75f, 0.5f),
								        p + new Vector2 (0.5f, 0.25f),
								        p + new Vector2 (0.5f, 0.75f)
								};
						case Samples.Samples8: 
								return new Vector2[]{
								        p + new Vector2 (0.25f, 0.5f),
								        p + new Vector2 (0.75f, 0.5f),
								        p + new Vector2 (0.5f, 0.25f),
								        p + new Vector2 (0.5f, 0.75f),
								        p + new Vector2 (0.25f, 0.25f),
								        p + new Vector2 (0.75f, 0.25f),
								        p + new Vector2 (0.25f, 0.75f),
								        p + new Vector2 (0.75f, 0.75f)
								};
						case Samples.Samples16: 
								return new Vector2[]{
							        p + new Vector2 (0, 0),
							        p + new Vector2 (0.3f, 0),
							        p + new Vector2 (0.7f, 0),
							        p + new Vector2 (1, 0),
							        p + new Vector2 (0, 0.3f),
							        p + new Vector2 (0.3f, 0.3f),
							        p + new Vector2 (0.7f, 0.3f),
							        p + new Vector2 (1, 0.3f),
							        p + new Vector2 (0, 0.7f),
							        p + new Vector2 (0.3f, 0.7f),
							        p + new Vector2 (0.7f, 0.7f),
							        p + new Vector2 (1, 0.7f),
							        p + new Vector2 (0, 1),
							        p + new Vector2 (0.3f, 1),
							        p + new Vector2 (0.7f, 1),
							        p + new Vector2 (1, 1)
								};
						case Samples.Samples32:
								return new Vector2[]{
								        p + new Vector2 (0, 0),
								        p + new Vector2 (1, 0),
								        p + new Vector2 (0, 1),
								        p + new Vector2 (1, 1), 
								        p + new Vector2 (0.2f, 0.2f),
								        p + new Vector2 (0.4f, 0.2f),
								        p + new Vector2 (0.6f, 0.2f),
								        p + new Vector2 (0.8f, 0.2f),
								        p + new Vector2 (0.2f, 0.4f),
								        p + new Vector2 (0.4f, 0.4f),
								        p + new Vector2 (0.6f, 0.4f),
								        p + new Vector2 (0.8f, 0.4f),
								        p + new Vector2 (0.2f, 0.6f),
								        p + new Vector2 (0.4f, 0.6f),
								        p + new Vector2 (0.6f, 0.6f),
								        p + new Vector2 (0.8f, 0.6f),
								        p + new Vector2 (0.2f, 0.8f),
								        p + new Vector2 (0.4f, 0.8f),
								        p + new Vector2 (0.6f, 0.8f),
								        p + new Vector2 (0.8f, 0.8f),
								        p + new Vector2 (0.5f, 0),
								        p + new Vector2 (0.5f, 1),
								        p + new Vector2 (0, 0.5f),
								        p + new Vector2 (1, 0.5f),
								        p + new Vector2 (0.5f, 0.5f)
								};
						case Samples.RotatedDisc:
								return new Vector2[]{
								        p + new Vector2 (0, 0),
								        p + new Vector2 (1, 0),
								        p + new Vector2 (0, 1),
								        p + new Vector2 (1, 1),
								        p + new Vector2 (0.5f, 0.5f) + new Vector2 (0.258f, 0.965f),//Sin (75°) && Cos (75°)
								        p + new Vector2 (0.5f, 0.5f) + new Vector2 (-0.965f, -0.258f),
								        p + new Vector2 (0.5f, 0.5f) + new Vector2 (0.965f, 0.258f),
								        p + new Vector2 (0.5f, 0.5f) + new Vector2 (0.258f, -0.965f)
								};
						default:
							return null;
						}
				}
		}
}