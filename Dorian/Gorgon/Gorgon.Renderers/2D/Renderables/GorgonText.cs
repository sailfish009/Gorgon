﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Friday, April 20, 2012 8:12:15 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using GorgonLibrary.Animation;
using GorgonLibrary.Graphics;
using GorgonLibrary.Math;
using GorgonLibrary.UI;
using SlimMath;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// A renderable object that renders a block of text.
	/// </summary>
	public class GorgonText
		: GorgonNamedObject, IRenderable, IMoveable, I2DCollisionObject
	{
		#region Variables.
		private readonly StringBuilder _tabText;								// Tab spacing text.
		private Gorgon2DCollider _collider;                                     // Collision object.
		private int _colliderVertexCount;                                       // Collider vertex count.
		private RectangleF? _textRect;											// Text rectangle.
		private int _vertexCount;												// Number of vertices.
		private Gorgon2DVertex[] _vertices;										// Vertices.
		private GorgonFont _font;												// Font to apply to the text.
		private string _text;													// Original text.
		private readonly GorgonColor[] _colors = new GorgonColor[4];			// Vertex colors.
		private GorgonRenderable.DepthStencilStates _depthStencil;				// Depth/stencil states.
		private GorgonRenderable.BlendState _blend;								// Blending states.
		private GorgonRenderable.TextureSamplerState _sampler;					// Texture sampler states.
		private GorgonTexture2D _currentTexture;								// Currently active texture.
		private bool _needsVertexUpdate = true;									// Flag to indicate that we need a vertex update.
		private bool _needsColorUpdate = true;									// Flag to indicate that the color needs updating.
		private bool _needsShadowUpdate = true;									// Flag to indicate that the shadow alpha needs updating.
		private readonly StringBuilder _formattedText;							// Formatted text.
		private readonly List<string> _lines;									// Lines in the text.
		private Vector2 _size = Vector2.Zero;									// Size of the text block.
		private Vector2 _position = Vector2.Zero;								// Position of the text.
		private Vector2 _scale = new Vector2(1);								// Scale for the text.
		private float _angle;													// Angle of rotation for the text.
		private Vector2 _anchor = Vector2.Zero;									// Anchor point for the text.
		private float _depth;													// Depth value.
		private int _tabSpace = 3;												// Tab spaces.
		private bool _wordWrap;													// Flag to indicate that the text should word wrap.
		private Alignment _alignment = Alignment.UpperLeft;						// Text alignment.
		private float _lineSpace = 1.0f;										// Line spacing.
		private readonly float[] _shadowAlpha = new float[4];					// Shadow vertex opacity.
		private Vector2 _shadowOffset = new Vector2(1);							// Shadow offset.
		private bool _shadowEnabled;											// Flag to indicate whether shadowing is enabled or not.
		private bool _useKerning = true;										// Flag to indicate that kerning should be used.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the clipping region.
		/// </summary>
		private RectangleF ClipRegion
		{
			get
			{
				return _textRect == null ? (RectangleF)Gorgon2D.Target.Viewport : _textRect.Value;
			}
		}

		/// <summary>
		/// Property to set or return whether kerning should be used.
		/// </summary>
		public bool UseKerning
		{
			get
			{
				return _useKerning;
			}
			set
			{
				if (value == _useKerning)
				{
					return;
				}

				_useKerning = value;
				_needsVertexUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the offset for the shadow.
		/// </summary>
		/// <remarks>This value only applies when <see cref="P:GorgonLibrary.Renderers.GorgonText.ShadowEnabled">ShadowEnabled</see> is TRUE.</remarks>
		[AnimatedProperty()]
		public Vector2 ShadowOffset
		{
			get
			{
				return _shadowOffset;
			}
			set
			{
				if (_shadowOffset != value)
				{
					_shadowOffset = value;
					_needsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return whether shadowing is enabled or not.
		/// </summary>
		public bool ShadowEnabled
		{
			get
			{
				return _shadowEnabled;
			}
			set
			{
				if (_shadowEnabled != value)
				{
					_shadowEnabled = value;
					_needsShadowUpdate = value;
					_needsVertexUpdate = true;
					_needsColorUpdate = true;
					UpdateText();
				}
			}
		}

		/// <summary>
		/// Property to set or return the shadow opacity.
		/// </summary>
		/// <remarks>This value only applies when <see cref="P:GorgonLibrary.Renderers.GorgonText.ShadowEnabled">ShadowEnabled</see> is TRUE.</remarks>
		[AnimatedProperty()]
		public float ShadowOpacity
		{
			get
			{
				return _shadowAlpha[0];
			}
			set
			{
				for (int i = 0; i < 4; i++)
					_shadowAlpha[i] = value;
				_needsShadowUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the spacing for the lines.
		/// </summary>
		[AnimatedProperty()]
		public float LineSpacing
		{
			get
			{
				return _lineSpace;
			}
			set
			{
				if (_lineSpace != value)
				{
					_lineSpace = value;
					_size = GetTextSize();
					_needsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the alignment of the text within the <see cref="P:GorgonLibrary.Renderers.GorgonText.TextRectangle">TextRectangle.</see>/.
		/// </summary>
		/// <remarks>If the TextRectangle property is NULL (Nothing in VB.Net), then this value has no effect.</remarks>
		public Alignment Alignment
		{
			get
			{
				return _alignment;
			}
			set
			{
				if (_alignment != value)
				{
					_alignment = value;
					FormatText();
					_needsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return whether word wrapping is enabled.
		/// </summary>
		/// <remarks>
		/// This uses the <see cref="P:GorgonLibrary.Renderers.GorgonText.TextRectangle">TextRectangle</see> to determine where the cutoff boundary is for word wrapping.  
		/// If a character is positioned outside of the region, then the previous space character is located and the region is broken at that point.  Note that the left position 
		/// of the rectangle is not taken into consideration when performing a word wrap, and consequently the user will be responsible for calculating the word wrap boundary.
		/// <para>Only the space character is considered when performing word wrapping.  Other whitespace or control characters are not considered break points in the string.</para>
		/// <para>If the TextRectangle property is NULL (Nothing in VB.Net), then this value has no effect.</para></remarks>
		public bool WordWrap
		{
			get
			{
				return _wordWrap;
			}
			set
			{
				if (value != _wordWrap)
				{
					_wordWrap = value;
					FormatText();
					_needsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the number of spaces to use for a tab character.
		/// </summary>
		/// <remarks>The default value is 3.</remarks>
		public int TabSpaces
		{
			get
			{
				return _tabSpace;
			}
			set
			{
				if (value < 1)
					value = 1;

				_tabSpace = value;
				_tabText.Length = 0;
				_tabText.Append(' ', value);
			}
		}

		/// <summary>
		/// Property to set or return the rectangle used for clipping and/or alignment for the text.
		/// </summary>
		/// <remarks>
		/// This defines the range used when aligning text horizontally and/or vertically.  For example, if the alignment is set to center horizontally, then the width of this rectangle is 
		/// used to determine the horizontal center point.  Likewise for vertically aligned text.
		/// <para>If <see cref="P:GorgonLibrary.Renderers.GorgonText.WordWrap">WordWrap</see> is set to TRUE, then this determines how far, in pixels, a line of text can go before it will be wrapped.</para>
		/// <para>This property will clip the text to the rectangle if the <see cref="P:GorgonLibrary.Renderers.GorgonText.ClipToRectangle">ClipToRectangle</see> property is set to TRUE.</para>
		/// <para>Setting this value to NULL (Nothing in VB.Net) will disable alignment, word wrapping and clipping.</para>
		/// </remarks>
		public RectangleF? TextRectangle
		{
			get
			{
				return _textRect;
			}
			set
			{
				if (_textRect != value)
				{
					_textRect = value;
					_needsVertexUpdate = true;
					FormatText();
				}
			}
		}

		/// <summary>
		/// Property to set or return whether to clip the text to the <see cref="P:GorgonLibrary.Renderers.GorgonText.TextRectangle">TextRectangle</see>.
		/// </summary>
		/// <remarks>If the TextRectangle property is NULL (Nothing in VB.Net), this value will have no effect.</remarks>
		public bool ClipToRectangle
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the 2D interface that created this object.
		/// </summary>
		public Gorgon2D Gorgon2D
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the text to display.
		/// </summary>
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				int prevLength = _text.Length;

				if (value == null)
					value = string.Empty;

				if (string.Compare(value, _text, false) != 0)
				{
					_text = value;
					UpdateText();
				}

				// Update the colors for the rest of the string if the text length is longer.
				if (prevLength < _text.Length)
				{
					_needsColorUpdate = true;
					if (ShadowEnabled)
						_needsShadowUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the font to be applied to the text.
		/// </summary>
		/// <remarks>This property will ignore requests to set it to NULL (Nothing in VB.Net).</remarks>
		public GorgonFont Font
		{
			get
			{
				return _font;
			}
			set
			{
				if (value == null)
					return;

				if (value != _font)
				{
					_font = value;
					_font.HasChanged = false;
					UpdateText();
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform an update on the text object.
		/// </summary>
		private void UpdateText()
		{
			if ((_text.Length == 0) || (_font == null) || (_font.Glyphs.Count == 0) || (_font.Textures.Count == 0))
			{
				_vertexCount = 0;
				if (_vertices == null)
					_vertices = new Gorgon2DVertex[1024];
				for (int i = 0; i < _vertices.Length - 1; i++)
				{
					_vertices[i].Color = new GorgonColor(1, 1, 1, 1);
					_vertices[i].Position = new Vector4(0, 0, 0, 1);
					_vertices[i].UV = new Vector2(0, 0);
				}

				_currentTexture = null;
				_needsShadowUpdate = true;
				_needsVertexUpdate = true;
				_needsColorUpdate = true;
				return;
			}

			// Find out how many vertices we have.
			_vertexCount = _text.Length * (_shadowEnabled ? 8 : 4);

			// Don't do less than 512 vertices.
			if (_vertexCount < 512)
			{
				_vertexCount = 512;
			}

			// Allocate the number of vertices if the vertex count is less than the required amount.
			if (_vertexCount > _vertices.Length)
			{
				_vertices = new Gorgon2DVertex[_vertexCount * 2];
				for (int i = 0; i < _vertices.Length - 1; i++)
				{
					_vertices[i].Color = new GorgonColor(1, 1, 1, 1);
					_vertices[i].Position = new Vector4(0, 0, 0, 1);
					_vertices[i].UV = new Vector2(0, 0);
				}

				_needsShadowUpdate = true;
				_needsColorUpdate = true;
			}

			// Get the first texture.
			for (int i = 0; i < _text.Length; i++)
			{
				if (_font.Glyphs.Contains(_text[i]))
				{
					_currentTexture = _font.Glyphs[_text[i]].Texture;
					break;
				}
			}

			_needsVertexUpdate = true;
			FormatText();
		}

		/// <summary>
		/// Function to update the transform coordinates by the alignment settings.
		/// </summary>
		/// <param name="leftTop">Left and top coordinates.</param>
		/// <param name="rightBottom">Right and bottom coordinates.</param>
		/// <param name="lineLength">Length of the line, in pixels.</param>
		private void GetAlignmentExtents(ref Vector2 leftTop, ref Vector2 rightBottom, float lineLength)
		{
			int outlineOffset = 0;
			int calc = 0;

			if (_font.Settings.OutlineColor.Alpha > 0)
				outlineOffset = _font.Settings.OutlineSize;

			switch (_alignment)
			{
				case UI.Alignment.UpperCenter:
					calc = (int)((ClipRegion.Width / 2.0f) - (lineLength / 2.0f));
					leftTop.X += calc;
					rightBottom.X += calc;
					break;
				case UI.Alignment.UpperRight:
					calc = (int)(ClipRegion.Width - lineLength);
					leftTop.X += calc;
					rightBottom.X += calc;
					break;
				case UI.Alignment.CenterLeft:
					calc = (int)(ClipRegion.Height / 2.0f - _size.Y / 2.0f);
					leftTop.Y += calc;
					rightBottom.Y += calc;
					break;
				case UI.Alignment.Center:
					calc = (int)((ClipRegion.Width / 2.0f) - (lineLength / 2.0f));
					leftTop.X += calc;
					rightBottom.X += calc;
					calc = (int)(ClipRegion.Height / 2.0f - _size.Y / 2.0f);
					leftTop.Y += calc;
					rightBottom.Y += calc;
					break;
				case UI.Alignment.CenterRight:
					calc = (int)(ClipRegion.Width - lineLength);
					leftTop.X += calc;
					rightBottom.X += calc;
					calc = (int)(ClipRegion.Height / 2.0f - _size.Y / 2.0f);
					leftTop.Y += calc;
					rightBottom.Y += calc;
					break;
				case UI.Alignment.LowerLeft:
					calc = (int)(ClipRegion.Height - _size.Y);
					leftTop.Y += calc;
					rightBottom.Y += calc;
					break;
				case UI.Alignment.LowerCenter:
					calc = (int)((ClipRegion.Width / 2.0f) - (lineLength / 2.0f));
					leftTop.X += calc;
					rightBottom.X += calc;
					calc = (int)(ClipRegion.Height - _size.Y);
					leftTop.Y += calc;
					rightBottom.Y += calc;
					break;
				case UI.Alignment.LowerRight:
					calc = (int)(ClipRegion.Width - lineLength);
					leftTop.X += calc;
					rightBottom.X += calc;
					calc = (int)(ClipRegion.Height - _size.Y);
					leftTop.Y += calc;
					rightBottom.Y += calc;
					break;
			}
		}

		/// <summary>
		/// Function to update the transformation for the text.
		/// </summary>
		/// <param name="glyph">Glyph to transform.</param>
		/// <param name="vertexIndex">Index of the vertex to modify.</param>
		/// <param name="textOffset">Offset of the glyph within the text.</param>
		/// <param name="lineLength">The length of the line of text when alignment is active.</param>
		/// <param name="cosValue">Cosine value for rotation.</param>
		/// <param name="sinValue">Sin value for rotation.</param>
		private void UpdateTransform(GorgonGlyph glyph, int vertexIndex, Vector2 textOffset, float lineLength, float cosValue, float sinValue)
		{
			Vector2 ltCorner = Vector2.Subtract(textOffset, Anchor);
			Vector2 rbCorner = Vector2.Add(ltCorner, glyph.GlyphCoordinates.Size);

			if ((_alignment != Alignment.UpperLeft) && (_textRect != null))
				GetAlignmentExtents(ref ltCorner, ref rbCorner, lineLength);

			// Scale.
			if (_scale.X != 1.0f)
			{
				ltCorner.X *= Scale.X;
				rbCorner.X *= Scale.X;
			}

			if (_scale.Y != 1.0f)
			{
				ltCorner.Y *= Scale.Y;
				rbCorner.Y *= Scale.Y;
			}

			if (Angle != 0.0f)
			{
				// Rotate the vertices.
				_vertices[vertexIndex].Position.X = (ltCorner.X * cosValue - ltCorner.Y * sinValue);
				_vertices[vertexIndex].Position.Y = (ltCorner.X * sinValue + ltCorner.Y * cosValue);
				_vertices[vertexIndex + 1].Position.X = (rbCorner.X * cosValue - ltCorner.Y * sinValue);
				_vertices[vertexIndex + 1].Position.Y = (rbCorner.X * sinValue + ltCorner.Y * cosValue);
				_vertices[vertexIndex + 2].Position.X = (ltCorner.X * cosValue - rbCorner.Y * sinValue);
				_vertices[vertexIndex + 2].Position.Y = (ltCorner.X * sinValue + rbCorner.Y * cosValue);
				_vertices[vertexIndex + 3].Position.X = (rbCorner.X * cosValue - rbCorner.Y * sinValue);
				_vertices[vertexIndex + 3].Position.Y = (rbCorner.X * sinValue + rbCorner.Y * cosValue);
			}
			else
			{
				// Else just keep the positions.
				_vertices[vertexIndex].Position = new Vector4(ltCorner, 0, 1);
				_vertices[vertexIndex + 1].Position = new Vector4(rbCorner.X, ltCorner.Y, 0, 1);
				_vertices[vertexIndex + 2].Position = new Vector4(ltCorner.X, rbCorner.Y, 0, 1);
				_vertices[vertexIndex + 3].Position = new Vector4(rbCorner, 0, 1);
			}

			// Translate.
			_vertices[vertexIndex].Position.X += _position.X;
			_vertices[vertexIndex + 1].Position.X += _position.X;
			_vertices[vertexIndex + 2].Position.X += _position.X;
			_vertices[vertexIndex + 3].Position.X += _position.X;

			_vertices[vertexIndex].Position.Y += _position.Y;
			_vertices[vertexIndex + 1].Position.Y += _position.Y;
			_vertices[vertexIndex + 2].Position.Y += _position.Y;
			_vertices[vertexIndex + 3].Position.Y += _position.Y;

			// Update texture coordinates.
			_vertices[vertexIndex].UV = glyph.TextureCoordinates.Location;
			_vertices[vertexIndex + 1].UV = new Vector2(glyph.TextureCoordinates.Right, glyph.TextureCoordinates.Top);
			_vertices[vertexIndex + 2].UV = new Vector2(glyph.TextureCoordinates.Left, glyph.TextureCoordinates.Bottom);
			_vertices[vertexIndex + 3].UV = new Vector2(glyph.TextureCoordinates.Right, glyph.TextureCoordinates.Bottom);

			// Set depth.
			if (_depth != 0.0f)
				_vertices[vertexIndex + 3].Position.Z = _vertices[vertexIndex + 2].Position.Z = _vertices[vertexIndex + 1].Position.Z = _vertices[vertexIndex].Position.Z = -Depth;
		}

		/// <summary>
		/// Function to perform a vertex update for the text.
		/// </summary>
		private void UpdateVertices()
		{
			int vertexIndex = 0;
			Vector2 pos = Vector2.Zero;
			Vector2 outlineOffset = Vector2.Zero;
			GorgonGlyph glyph = null;
			float angle = _angle.Radians();						// Angle in radians.
			float cosVal = angle.Cos();							// Cached cosine.
			float sinVal = angle.Sin();							// Cached sine.

			if ((_font.Settings.OutlineColor.Alpha > 0) && (_font.Settings.OutlineSize > 0))
				outlineOffset = new Vector2(_font.Settings.OutlineSize, _font.Settings.OutlineSize);

			_colliderVertexCount = 0;
			for (int line = 0; line < _lines.Count; line++)
			{
				float lineLength = 0;
				string currentLine = _lines[line];

				if ((_alignment != UI.Alignment.UpperLeft) && (_textRect.HasValue))
					lineLength = LineMeasure(_lines[line], outlineOffset.X);

				for (int i = 0; i < currentLine.Length; i++)
				{
					char c = currentLine[i];									

					if (!_font.Glyphs.Contains(c))
						c = _font.Settings.DefaultCharacter;

					glyph = _font.Glyphs[c];

					if (c == ' ')
					{
						pos.X += glyph.GlyphCoordinates.Width - 1;
						continue;
					}

					// Add shadow character.
					if (_shadowEnabled)
					{
						UpdateTransform(glyph, vertexIndex, Vector2.Add(Vector2.Add(pos, glyph.Offset), _shadowOffset), lineLength, cosVal, sinVal);
						vertexIndex += 4;
						_colliderVertexCount += 4;
					}

					UpdateTransform(glyph, vertexIndex, Vector2.Add(pos, glyph.Offset), lineLength, cosVal, sinVal);
					vertexIndex += 4;
					_colliderVertexCount += 4;

					// Apply kerning pairs.
					if (_useKerning)
					{
						pos.X += glyph.Advance.X + glyph.Advance.Y + outlineOffset.X;

						if ((i < currentLine.Length - 1) && (_font.KerningPairs.Count > 0))
						{
							var kerning = new GorgonKerningPair(c, currentLine[i + 1]);
							if (_font.KerningPairs.ContainsKey(kerning))
								pos.X += _font.KerningPairs[kerning];
							else
								pos.X += glyph.Advance.Z;
						}
						else
							pos.X += glyph.Advance.Z;
					}
					else
						pos.X += glyph.GlyphCoordinates.Width + outlineOffset.X;
				}

				// TODO: Ensure that this is documented somewhere.				
				// If we have texture filtering on, this is going to look weird because it's a sub-pixel.
				// In order to get the font to look right on a second line, we need to use point filtering.
				// We -could- use a Floor here, but then movement becomes very jerky.  We're better off turning
				// off texture filtering even just for an increase in speed.
				pos.Y += (_font.FontHeight + outlineOffset.Y) * _lineSpace;
				pos.X = 0;
			}

			if (_collider != null)
				_collider.UpdateFromCollisionObject();
		}

		/// <summary>
		/// Function to update shadow alpha.
		/// </summary>
		private void UpdateShadowAlpha()
		{
			for (int i = 0; i < _vertexCount; i += 8)
			{
				_vertices[i].Color = new GorgonColor(0, 0, 0, _shadowAlpha[0]);
				_vertices[i + 1].Color = new GorgonColor(0, 0, 0, _shadowAlpha[1]);
				_vertices[i + 2].Color = new GorgonColor(0, 0, 0, _shadowAlpha[2]);
				_vertices[i + 3].Color = new GorgonColor(0, 0, 0, _shadowAlpha[3]);
			}
		}

		/// <summary>
		/// Function to update the colors.
		/// </summary>
		private void UpdateColors()
		{
			if (!_shadowEnabled)
			{
				for (int i = 0; i < _vertexCount; i += 4)
				{
					_vertices[i].Color = _colors[0];
					_vertices[i + 1].Color = _colors[1];
					_vertices[i + 2].Color = _colors[2];
					_vertices[i + 3].Color = _colors[3];
				}
			}
			else
			{
				for (int i = 4; i < _vertexCount; i += 8)
				{
					_vertices[i].Color = _colors[0];
					_vertices[i + 1].Color = _colors[1];
					_vertices[i + 2].Color = _colors[2];
					_vertices[i + 3].Color = _colors[3];
				}
			}
		}

		/// <summary>
		/// Function to word wrap at the text region border.
		/// </summary>
		private void WordWrapText()
		{
			int i = 0;
			char character = ' ';
			GorgonGlyph glyph = null;
			float pos = 0;
			int outlineSize = 0;
			RectangleF region = ClipRegion;

			// If we don't have a font, then we have nothing to wrap.  Yet.
			if (_font == null)
			{
				return;
			}

			if (_font.Settings.OutlineColor.Alpha > 0)
				outlineSize = _font.Settings.OutlineSize;

			if (pos >= region.Width)
				return;

			_formattedText.Append(_text);
			_formattedText.Replace("\n\r", "\n");
			_formattedText.Replace("\r\n", "\n");
			_formattedText.Replace("\r", "\n");
			_formattedText.Replace("\t", _tabText.ToString());

			while (i < _formattedText.Length)
			{
				character = _formattedText[i];

				if (character == '\n')
				{
					pos = 0;
					i++;
					continue;
				}

				if (!_font.Glyphs.Contains(character))
				{
					character = _font.Settings.DefaultCharacter;
				}

				glyph = _font.Glyphs[character];

				// If we can't fit a single glyph into the boundaries, then just leave.  Else we'll have an infinite loop on our hands.
				if (glyph.GlyphCoordinates.Width > region.Width)
					return;

				switch (character)
				{
					case ' ':
						pos += glyph.GlyphCoordinates.Width - 1;
						break;
					default:
						float kernValue = glyph.Advance.Z;

						if (_useKerning)
						{
							if ((i < _formattedText.Length - 1) && (_font.KerningPairs.Count > 0))
							{
								var kernPair = new GorgonKerningPair(character, _formattedText[i + 1]);
								if (_font.KerningPairs.ContainsKey(kernPair))
									kernValue = _font.KerningPairs[kernPair];
							}

							pos += glyph.Advance.X + glyph.Advance.Y + kernValue + outlineSize;
						}
						else
							pos += glyph.GlyphCoordinates.Width + outlineSize;

						break;
				}

				if (pos > region.Width)
				{
					int j = i;

					// Try to find the previous space and replace it with a new line.
					// Otherwise just insert it at the previous character.
					while (j >= 0)
					{
						if ((_formattedText[j] == '\n') || (_formattedText[j] == '\r'))
						{
							j = -1;
							break;
						}

						if ((_formattedText[j] == ' ') || (_formattedText[j] == '\t'))
						{
							_formattedText[j] = '\n';
							i = j;
							break;
						}

						j--;
					}

					// If we reach end of line without finding a line break, add a line break.
					if ((i > 0) && (j < 0))
						_formattedText.Insert(i, "\n");

					pos = 0;
				}

				i++;
			}
		}

		/// <summary>
		/// Function to format the text for measuring and clipping.
		/// </summary>
		private void FormatText()
		{
			_formattedText.Length = 0;
			if ((!_textRect.HasValue) || (!_wordWrap))
			{
				_formattedText.Append(_text);
				_formattedText.Replace("\n\r", "\n");
				_formattedText.Replace("\r\n", "\n");
				_formattedText.Replace("\r", "\n");
				_formattedText.Replace("\t", _tabText.ToString());
			}
			else
			{
				WordWrapText();
			}

			_lines.Clear();
			_lines.AddRange(_formattedText.ToString().Split('\n'));
			_size = GetTextSize();
		}

		/// <summary>
		/// Function to measure the width of a single line of text 
		/// </summary>
		/// <param name="line">Line to measure.</param>
		/// <param name="outlineOffset">Outline offset.</param>
		/// <returns>The width, pixels, of a single line of text.</returns>
		private float LineMeasure(string line, float outlineOffset)
		{
			bool offsetApplied = false;
			float size = 0;

			for (int i = 0; i < line.Length; i++)
			{				
				GorgonGlyph glyph = null;
				char c = line[i];

				if (!_font.Glyphs.Contains(c))
					c = _font.Settings.DefaultCharacter;

				glyph = _font.Glyphs[c];

				switch (c)
				{
					case ' ':
						size += glyph.GlyphCoordinates.Width - 1;
						continue;
					case '\r':
					case '\n':
						continue;
				}

				// Include the initial offset.
				if (!offsetApplied)
				{
					size += glyph.Offset.X + outlineOffset;
					offsetApplied = true;
				}

				// Apply kerning pairs.
				if (_useKerning)
				{
					size += glyph.Advance.X + glyph.Advance.Y + outlineOffset;

					if ((i < line.Length - 1) && (_font.KerningPairs.Count > 0))
					{
						var kerning = new GorgonKerningPair(c, line[i + 1]);
						if (_font.KerningPairs.ContainsKey(kerning))
							size += _font.KerningPairs[kerning];
						else
							size += glyph.Advance.Z;
					}
					else
						size += glyph.Advance.Z;
				}
				else
					size += glyph.GlyphCoordinates.Width + outlineOffset;
			}
			return size;
		}

		/// <summary>
		/// Function to measure the bounds of the text in the text object.
		/// </summary>
		/// <returns>The bounding rectangle for the text.</returns>
		private Vector2 GetTextSize()
		{
			Vector2 result = Vector2.Zero;
			float outlineSize = 0;

			if ((_text.Length == 0) || (_formattedText.Length == 0) || (_lines.Count == 0) || (_font == null))
				return result;

			if (_font.Settings.OutlineColor.Alpha > 0)
				outlineSize = _font.Settings.OutlineSize;

			if (_lineSpace != 1.0)
				result.Y = (_lines.Count - 1) * (((_font.FontHeight + (outlineSize * 2.0f)) * _lineSpace)) + (_font.FontHeight + (outlineSize * 2.0f));
			else
				result.Y = (_lines.Count * (_font.FontHeight + (outlineSize * 2.0f)));

			for (int i = 0; i < _lines.Count; i++)
				result.X = result.X.Max(LineMeasure(_lines[i], outlineSize));

			return result;
		}

		/// <summary>
		/// Function to set the color for a specific corner on all characters in the string.
		/// </summary>
		/// <param name="corner">Corner of the characters to set.</param>
		/// <param name="color">Color to set.</param>
		public void SetCornerColor(RectangleCorner corner, GorgonColor color)
		{
			var index = (int)corner;

			if (color != _colors[index])
			{
				_colors[index] = color;
				_needsColorUpdate = true;
			}
		}

		/// <summary>
		/// Function to retrieve the color for a specific corner on all characters in the string.
		/// </summary>
		/// <param name="corner">Corner of the characters to retrieve the color from.</param>
		/// <returns>The color on the specified corner of the characters.</returns>
		public GorgonColor GetCornerColor(RectangleCorner corner)
		{
			return _colors[(int)corner];
		}

		/// <summary>
		/// Function to return the size, in pixels of the string assigned to the text object.
		/// </summary>
		/// <remarks>Unlike the overloaded MeasureText function, this one determines whether to use word wrapping based on the settings for the text object and uses the text that's already assigned to the text object.</remarks>
		public Vector2 MeasureText()
		{
			return MeasureText(Text);
		}

		/// <summary>
		/// Function to return the size, in pixels, of a string.
		/// </summary>
		/// <param name="text">Text to measure.</param>
		/// <returns>The size of the text, in pixels.</returns>
		/// <remarks>Unlike the overloaded MeasureText function, this one determines whether to use word wrapping based on the settings for the text object.</remarks>
		public Vector2 MeasureText(string text)
		{
			float size = float.MaxValue - 1;

			if (this.TextRectangle != null)
				size = this.TextRectangle.Value.Width;

			return MeasureText(text, WordWrap, size);
		}

		/// <summary>
		/// Function to return the size, in pixels, of a string.
		/// </summary>
		/// <param name="text">Text to measure.</param>
		/// <param name="useWordWrap">TRUE to use word wrapping.</param>
		/// <param name="wrapBoundaryWidth">The boundary to word wrap on.</param>
		/// <returns>The size of the text, in pixels.</returns>
		/// <remarks>If the <paramref name="useWordWrap"/> is TRUE and the <paramref name="wrapBoundaryWidth"/> is 0 or less, then word wrapping is disabled.</remarks>
		public Vector2 MeasureText(string text, bool useWordWrap, float wrapBoundaryWidth)
		{
			string previousString = Text;
			bool lastWordWrap = _wordWrap;
			RectangleF? lastRect = _textRect;

			try
			{
				if (string.IsNullOrEmpty(text))
					return Vector2.Zero;

				if (wrapBoundaryWidth <= 0.0f)
					useWordWrap = false;

				WordWrap = useWordWrap;
				if (WordWrap)
					TextRectangle = new RectangleF(0, 0, wrapBoundaryWidth, 0);

				// If the string is not the same as what we've stored, then temporarily
				// replace that string with the one being passed in and use that to
				// gauge size.  Otherwise, use the original size.
				if (string.Compare(text, Text, false) != 0)
					Text = text;

				return Size;
			}
			finally
			{
				_wordWrap = lastWordWrap;
				_textRect = lastRect;
				Text = previousString;
			}
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <remarks>Please note that this doesn't draw the object to the target right away, but queues it up to be
		/// drawn when <see cref="M:GorgonLibrary.Renderers.Gorgon2D.Render">Render</see> is called.
		/// </remarks>
		public void Draw()
		{
			Rectangle clipRegion = Rectangle.Round(ClipRegion);
			Rectangle? lastClip = Gorgon2D.ClipRegion;
			var states = StateChange.None;
			int vertexIndex = 0;

			// We don't need to draw anything if we don't have a font or text to draw.
			if ((_text.Length == 0) || (_font == null))
				return;

			// If we've updated the font, then we need to update the text display.
			if (_font.HasChanged)
			{
				UpdateText();

				_needsColorUpdate = true;
				_needsVertexUpdate = true;
				_font.HasChanged = false;
			}

			if ((ClipToRectangle) && (((lastClip == null) && (_textRect != null)) || ((lastClip != null) && (lastClip.Value != ClipRegion))))
				Gorgon2D.ClipRegion = clipRegion;

			states = Gorgon2D.StateManager.CheckState(this);
			if (states != StateChange.None)
			{
				Gorgon2D.RenderObjects();
				Gorgon2D.StateManager.ApplyState(this, states);
			}

			if (_needsVertexUpdate)
			{
				UpdateVertices();
				_needsVertexUpdate = false;
			}

			if (_needsColorUpdate)
			{
				UpdateColors();
				_needsColorUpdate = false;
			}

			if ((_shadowEnabled) && (_needsShadowUpdate))
			{
				UpdateShadowAlpha();
				_needsShadowUpdate = false;
			}

			if (_shadowEnabled)
			{
				for (int i = 0; i < _text.Length; i++)
				{
					char c = _text[i];

					if ((c != '\n') && (c != '\t') && (c != ' '))
					{
						GorgonGlyph glyph = null;

						if (!_font.Glyphs.Contains(c))
							c = _font.Settings.DefaultCharacter;

						glyph = _font.Glyphs[c];

						// Change to the current texture.
						if (Gorgon2D.PixelShader.Resources[0] != GorgonTexture.ToShaderView(glyph.Texture))
						{
							Gorgon2D.RenderObjects();
							Gorgon2D.PixelShader.Resources[0] = glyph.Texture;
						}

						// Add shadowed characters.
						Gorgon2D.AddVertices(_vertices, 0, 6, vertexIndex, 4);
						vertexIndex += 8;
					}
				}

				vertexIndex = 4;
			}

			for (int i = 0; i < _text.Length; i++)
			{
				char c = _text[i];

				if ((c != '\n') && (c != '\t') && (c != ' ') && (c != '\r'))
				{
					GorgonGlyph glyph = null;

					if (!_font.Glyphs.Contains(c))
						c = _font.Settings.DefaultCharacter;

					glyph = _font.Glyphs[c];

					// Change to the current texture.
                    if (Gorgon2D.PixelShader.Resources[0] != GorgonTexture.ToShaderView(glyph.Texture))
					{
						Gorgon2D.RenderObjects();
						Gorgon2D.PixelShader.Resources[0] = glyph.Texture;
					}

					Gorgon2D.AddVertices(_vertices, 0, 6, vertexIndex, 4);
					vertexIndex += _shadowEnabled ? 8 : 4;
				}
			}

			if (ClipToRectangle)
			{
				Gorgon2D.ClipRegion = lastClip;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonText"/> class.
		/// </summary>
		/// <param name="gorgon2D">2D interface that created this object.</param>
		/// <param name="name">Name of the text object.</param>
		/// <param name="font">The font to use.</param>
		internal GorgonText(Gorgon2D gorgon2D, string name, GorgonFont font)
			: base(name)
		{
			Gorgon2D = gorgon2D;

			_text = string.Empty;
			_formattedText = new StringBuilder(256);
			_lines = new List<string>();
			if (font != null)
			{
				_font = font;
				_font.HasChanged = false;
			}
			_shadowAlpha[3] = _shadowAlpha[2] = _shadowAlpha[1] = _shadowAlpha[0] = 0.25f;

			_depthStencil = new GorgonRenderable.DepthStencilStates();
			_blend = new GorgonRenderable.BlendState();
			_sampler = new GorgonRenderable.TextureSamplerState();
			Color = new GorgonColor(1, 1, 1, 1);
			CullingMode = Graphics.CullingMode.Back;
			AlphaTestValues = GorgonRangeF.Empty;
			BlendingMode = BlendingMode.Modulate;
			_tabText = new StringBuilder("   ", 8);

			UpdateText();
		}
		#endregion

		#region IRenderable Members
		/// <summary>
		/// Property to return the number of vertices for the renderable.
		/// </summary>
		int IRenderable.VertexCount
		{
			get
			{
				return _vertexCount;
			}
		}

		/// <summary>
		/// Property to return the type of primitive for the renderable.
		/// </summary>
		/// <value></value>
		PrimitiveType IRenderable.PrimitiveType
		{
			get
			{
				return PrimitiveType.TriangleList;
			}
		}

		/// <summary>
		/// Property to return the number of indices that make up this renderable.
		/// </summary>
		/// <value></value>
		/// <remarks>This is only matters when the renderable uses an index buffer.</remarks>
		int IRenderable.IndexCount
		{
			get
			{
				return 6;
			}
		}

		/// <summary>
		/// Property to set or return the vertex buffer binding for this renderable.
		/// </summary>
		/// <value></value>
		GorgonVertexBufferBinding IRenderable.VertexBufferBinding
		{
			get
			{
				return Gorgon2D.DefaultVertexBufferBinding;
			}
		}

		/// <summary>
		/// Property to set or return the index buffer for this renderable.
		/// </summary>
		/// <value></value>
		GorgonIndexBuffer IRenderable.IndexBuffer
		{
			get
			{
				return Gorgon2D.DefaultIndexBuffer;
			}
		}

		/// <summary>
		/// Property to return a list of vertices to render.
		/// </summary>
		/// <value></value>
		Gorgon2DVertex[] IRenderable.Vertices
		{
			get
			{
				return _vertices;
			}
		}

		/// <summary>
		/// Property to return the number of vertices to add to the base starting index.
		/// </summary>
		/// <value></value>
		int IRenderable.BaseVertexCount
		{
			get
			{
				return 0;
			}
		}

		/// <summary>
		/// Property to set or return depth/stencil buffer states for this renderable.
		/// </summary>
		public virtual GorgonRenderable.DepthStencilStates DepthStencil
		{
			get
			{
				return _depthStencil;
			}
			set
			{
				if (value == null)
					return;

				_depthStencil = value;
			}
		}

		/// <summary>
		/// Property to set or return advanced blending states for this renderable.
		/// </summary>
		public virtual GorgonRenderable.BlendState Blending
		{
			get
			{
				return _blend;
			}
			set
			{
				if (value == null)
					return;

				_blend = value;
			}
		}

		/// <summary>
		/// Property to set or return advanded texture sampler states for this renderable.
		/// </summary>
		public virtual GorgonRenderable.TextureSamplerState TextureSampler
		{
			get
			{
				return _sampler;
			}
			set
			{
				if (value == null)
					return;

				_sampler = value;
			}
		}

		/// <summary>
		/// Property to set or return pre-defined smoothing states for the renderable.
		/// </summary>
		/// <remarks>These modes are pre-defined smoothing states, to get more control over the smoothing, use the <see cref="P:GorgonLibrary.Renderers.GorgonRenderable.TextureSamplerState.TextureFilter.">TextureFilter</see> 
		/// property exposed by the <see cref="P:GorgonLibrary.Renderers.GorgonRenderable.TextureSampler.">TextureSampler</see> property.</remarks>
		public virtual SmoothingMode SmoothingMode
		{
			get
			{
				switch (TextureSampler.TextureFilter)
				{
					case TextureFilter.Point:
						return SmoothingMode.None;
					case TextureFilter.Linear:
						return SmoothingMode.Smooth;
					case TextureFilter.MinLinear | TextureFilter.MipLinear:
						return SmoothingMode.SmoothMinify;
					case TextureFilter.MagLinear | TextureFilter.MipLinear:
						return SmoothingMode.SmoothMagnify;
					default:
						return SmoothingMode.Custom;
				}
			}
			set
			{
				switch (value)
				{
					case SmoothingMode.None:
						TextureSampler.TextureFilter = TextureFilter.Point;
						break;
					case SmoothingMode.Smooth:
						TextureSampler.TextureFilter = TextureFilter.Linear;
						break;
					case SmoothingMode.SmoothMinify:
						TextureSampler.TextureFilter = TextureFilter.MinLinear | TextureFilter.MipLinear;
						break;
					case SmoothingMode.SmoothMagnify:
						TextureSampler.TextureFilter = TextureFilter.MagLinear | TextureFilter.MipLinear;
						break;
				}
			}
		}

		/// <summary>
		/// Property to set or return a pre-defined blending states for the renderable.
		/// </summary>
		/// <remarks>These modes are pre-defined blending states, to get more control over the blending, use the <see cref="P:GorgonLibrary.Renderers.GorgonRenderable.BlendingState.SourceBlend">SourceBlend</see> 
		/// or the <see cref="P:GorgonLibrary.Renderers.GorgonRenderable.Blending.DestinationBlend">DestinationBlend</see> property which are exposed by the 
		/// <see cref="P:GorgonLibrary.Renderers.GorgonRenderable.Blending">Blending</see> property.</remarks>
		public virtual BlendingMode BlendingMode
		{
			get
			{
				if ((Blending.SourceBlend == BlendType.One) && (Blending.DestinationBlend == BlendType.Zero))
					return Renderers.BlendingMode.None;

				if (Blending.SourceBlend == BlendType.SourceAlpha)
				{
					if (Blending.DestinationBlend == BlendType.InverseSourceAlpha)
						return Renderers.BlendingMode.Modulate;
					if (Blending.DestinationBlend == BlendType.One)
						return Renderers.BlendingMode.Additive;
				}

				if ((Blending.SourceBlend == BlendType.One) && (Blending.DestinationBlend == BlendType.InverseSourceAlpha))
					return Renderers.BlendingMode.PreMultiplied;

				if ((Blending.SourceBlend == BlendType.InverseDestinationColor) && (Blending.DestinationBlend == BlendType.InverseSourceColor))
					return Renderers.BlendingMode.Inverted;

				return Renderers.BlendingMode.Custom;
			}
			set
			{
				switch (value)
				{
					case Renderers.BlendingMode.Additive:
						Blending.SourceBlend = BlendType.SourceAlpha;
						Blending.DestinationBlend = BlendType.One;
						break;
					case Renderers.BlendingMode.Inverted:
						Blending.SourceBlend = BlendType.InverseDestinationColor;
						Blending.DestinationBlend = BlendType.InverseSourceColor;
						break;
					case Renderers.BlendingMode.Modulate:
						Blending.SourceBlend = BlendType.SourceAlpha;
						Blending.DestinationBlend = BlendType.InverseSourceAlpha;
						break;
					case Renderers.BlendingMode.PreMultiplied:
						Blending.SourceBlend = BlendType.One;
						Blending.DestinationBlend = BlendType.InverseSourceAlpha;
						break;
					case Renderers.BlendingMode.None:
						Blending.SourceBlend = BlendType.One;
						Blending.DestinationBlend = BlendType.Zero;
						break;
				}
			}
		}

		/// <summary>
		/// Property to set or return the culling mode.
		/// </summary>
		public CullingMode CullingMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the range of alpha values to reject on this renderable.
		/// </summary>
		public GorgonRangeF AlphaTestValues
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the opacity (Alpha channel) of the renderable object.
		/// </summary>
		[AnimatedProperty()]
		public float Opacity
		{
			get
			{
				return Color.Alpha;
			}
			set
			{
				if (value != Color.Alpha)
					Color = new GorgonColor(Color.Red, Color.Green, Color.Blue, value);
			}
		}

		/// <summary>
		/// Property to set or return the color for a renderable object.
		/// </summary>
		[AnimatedProperty()]
		public GorgonColor Color
		{
			get
			{
				return _colors[0];
			}
			set
			{
				if (value != _colors[0])
				{
					_colors[3] = _colors[2] = _colors[1] = _colors[0] = value;
					_needsColorUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to return the texture being used by the renderable.
		/// </summary>
		GorgonTexture2D IRenderable.Texture
		{
			get
			{
				return _currentTexture;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the texture region.
		/// </summary>
		RectangleF IRenderable.TextureRegion
		{
			get
			{
				return _currentTexture == null ? RectangleF.Empty : new RectangleF(Vector2.Zero, _currentTexture.Settings.Size);
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the coordinates in the texture to use as a starting point for drawing.
		/// </summary>
		Vector2 IRenderable.TextureOffset
		{
			get
			{
				return Vector2.Zero;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to set or return the scaling of the texture width and height.
		/// </summary>
		Vector2 IRenderable.TextureSize
		{
			get
			{
				return _currentTexture != null ? _currentTexture.Settings.Size : Vector2.Zero;
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		#endregion

		#region IMoveable Members
		/// <summary>
		/// Property to set or return the position of the renderable.
		/// </summary>
		[AnimatedProperty()]
		public Vector2 Position
		{
			get
			{
				return _position;
			}
			set
			{
				if (_position != value)
				{
					_position = value;
					_needsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the angle of rotation (in degrees) for a renderable.
		/// </summary>
		[AnimatedProperty()]
		public float Angle
		{
			get
			{
				return _angle;
			}
			set
			{
				if (_angle != value)
				{
					_angle = value;
					_needsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the scale of the renderable.
		/// </summary>
		[AnimatedProperty()]
		public Vector2 Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				if (_scale != value)
				{
					_scale = value;
					_needsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the anchor point of the renderable.
		/// </summary>
		[AnimatedProperty()]
		public Vector2 Anchor
		{
			get
			{
				return _anchor;
			}
			set
			{
				if (_anchor != value)
				{
					_anchor = value;
					_needsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the "depth" of the renderable in a depth buffer.
		/// </summary>
		public float Depth
		{
			get
			{
				return _depth;
			}
			set
			{
				if (_depth != value)
				{
					_depth = value;
					_needsVertexUpdate = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the size of the renderable.
		/// </summary>
		public Vector2 Size
		{
			get
			{
				return _size;
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		#endregion

		#region I2DCollisionObject Members
		/// <summary>
		/// Property to set or return the collider that is assigned to the object.
		/// </summary>
		public Gorgon2DCollider Collider
		{
			get
			{
				return _collider;
			}
			set
			{
				if (value == _collider)
					return;

				if (value == null)
				{
					if (_collider != null)
						_collider.CollisionObject = null;
					return;
				}

				// Force a transform to get the latest vertices.
				value.CollisionObject = this;
				_collider = value;
				UpdateText();
				UpdateVertices();
			}
		}

		/// <summary>
		/// Property to return the number of vertices to process.
		/// </summary>
		int I2DCollisionObject.VertexCount
		{
			get
			{
				return _colliderVertexCount;
			}
		}

		/// <summary>
		/// Property to return the list of vertices associated with the object.
		/// </summary>
		Gorgon2DVertex[] I2DCollisionObject.Vertices
		{
			get
			{
				return _vertices;
			}
		}
		#endregion
	}
}