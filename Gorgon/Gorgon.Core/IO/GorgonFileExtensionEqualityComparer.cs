﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Thursday, June 25, 2015 11:15:24 AM
// 
#endregion

using System.Collections.Generic;

namespace Gorgon.IO
{
	/// <summary>
	/// An equality comparer for the <see cref="GorgonFileExtension"/> type.
	/// </summary>
	public class GorgonFileExtensionEqualityComparer
		: IEqualityComparer<GorgonFileExtension>
	{
		#region IEqualityComparer<GorgonFileExtension> Members
		/// <summary>
		/// Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="x">The first object of type <see cref="GorgonFileExtension"/> to compare.</param>
		/// <param name="y">The second object of type <see cref="GorgonFileExtension"/> to compare.</param>
		/// <returns><b>true</b> if the specified objects are equal; otherwise, <b>false</b> if not.</returns>
		public bool Equals(GorgonFileExtension x, GorgonFileExtension y)
		{
			return GorgonFileExtension.Equals(ref x, ref y);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
		/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
		public int GetHashCode(GorgonFileExtension obj)
		{
			return obj.GetHashCode();
		}
		#endregion
	}
}
