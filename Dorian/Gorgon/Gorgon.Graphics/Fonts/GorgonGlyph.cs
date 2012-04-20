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
// Created: Friday, April 13, 2012 7:12:15 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimMath;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A glyph used to define a character in the font.
	/// </summary>
	public class GorgonGlyph
		: INamedObject
	{
		#region Properties.
		/// <summary>
		/// Property to return the character that this glyph represents.
		/// </summary>
		public char Character
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the texture that the glyph can be found on.
		/// </summary>
		public GorgonTexture2D Texture
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the coordinates, in pixel space, of the glyph.
		/// </summary>
		public Rectangle GlyphCoordinates
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the texture coordinates for the glyph.
		/// </summary>
		public RectangleF TextureCoordinates
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the ABC kerning advance for the glyph.
		/// </summary>
		/// <remarks>The A part is the distance added to the current position before placing the glyph, the B part is the width of the glyph and the C part is the distance added to the current position (this is white space on the right of the glyph).</remarks>
		public Vector3 Advance
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the horizontal and vertical offset of the glyph.
		/// </summary>
		public Vector2 Offset
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return Character.GetHashCode();
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return "Gorgon Font Glyph: " + Character;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGlyph"/> class.
		/// </summary>
		/// <param name="character">The character that the glyph represents.</param>
		/// <param name="texture">The texture that the glyph can be found on.</param>
		/// <param name="glyphCoordinates">Coordinates on the texture to indicate where the glyph is stored.</param>
		/// <param name="glyphOffset">Vertical offset of the glyph.</param>
		/// <param name="glyphAdvancing">Advancement kerning data for the glyph.</param>
		/// <remarks>The <paramref name="glyphCoordinates"/> parameter is in pixel coordinates (i.e. 0 .. Width/Height).</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="texture"/> parameter is NULL (Nothing in VB.Net).
		/// </exception>
		public GorgonGlyph(char character, GorgonTexture2D texture, Rectangle glyphCoordinates, Vector2 glyphOffset, Vector3 glyphAdvancing)
		{
			GorgonDebug.AssertNull<GorgonTexture2D>(texture, "texture");

			Character = character;
			GlyphCoordinates = glyphCoordinates;
			TextureCoordinates = RectangleF.FromLTRB((float)glyphCoordinates.Left / (float)texture.Settings.Width,
												(float)glyphCoordinates.Bottom / (float)texture.Settings.Height,
												(float)glyphCoordinates.Right / (float)texture.Settings.Width,
												(float)glyphCoordinates.Bottom / (float)texture.Settings.Height);
			Texture = texture;
			Offset = glyphOffset;
			Advance = glyphAdvancing;
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		/// <value></value>
		string INamedObject.Name
		{
			get 
			{
				return Character.ToString();
			}
		}
		#endregion
	}
}