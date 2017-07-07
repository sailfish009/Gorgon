﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 29, 2016 7:31:42 PM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Information used to create the stencil portion of a <see cref="GorgonDepthStencilStateInfo"/>.
	/// </summary>
	public class GorgonStencilOperationInfo 
		: IGorgonStencilOperationInfo
	{
		#region Properties.

		/// <summary>
		/// Property to set or return the comparison function to use for stencil operations.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This specifies the function to evaluate with stencil data being read/written and existing stencil data.
		/// </para>
		/// <para>
		/// The default value is <c>Keep</c>.
		/// </para>
		/// </remarks>
		public D3D11.Comparison Comparison
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the operation to perform when the depth testing function fails, but stencil testing passes.
		/// </summary>
		/// <remarks>
		/// The default value is <c>Keep</c>.
		/// </remarks>
		public D3D11.StencilOperation DepthFailOperation
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the operation to perform when the stencil testing fails.
		/// </summary>
		/// <remarks>
		/// The default value is <c>Keep</c>.
		/// </remarks>
		public D3D11.StencilOperation FailOperation
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the operation to perform when the stencil testing passes.
		/// </summary>
		/// <remarks>
		/// The default value is <c>Keep</c>.
		/// </remarks>
		public D3D11.StencilOperation PassOperation
		{
			get;
			set;
		}
        #endregion

        #region Methods.
	    /// <summary>
	    /// Function to copy another <see cref="IGorgonStencilOperationInfo"/> into this one.
	    /// </summary>
	    /// <param name="info">The info to copy from.</param>
	    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
	    internal void CopyFrom(IGorgonStencilOperationInfo info)
	    {
	        if (info == null)
	        {
	            throw new ArgumentNullException(nameof(info));
	        }

	        Comparison = info.Comparison;
	        DepthFailOperation = info.DepthFailOperation;
	        FailOperation = info.FailOperation;
	        PassOperation = info.PassOperation;
	    }
        #endregion

        #region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonStencilOperationInfo"/> class.
		/// </summary>
		internal GorgonStencilOperationInfo()
		{
			Comparison = D3D11.Comparison.Always;
			FailOperation= PassOperation = DepthFailOperation = D3D11.StencilOperation.Keep;
		}
		#endregion
	}
}