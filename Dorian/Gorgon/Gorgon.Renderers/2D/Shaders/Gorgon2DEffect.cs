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
// Created: Monday, April 02, 2012 1:59:48 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Renderers
{
	/// <summary>
	/// Base effect for 2D effects.
	/// </summary>
	public abstract class Gorgon2DEffect
		: GorgonEffect
	{
		#region Variables.
		private bool _isDisposed = false;							// Flag to indicate that the object was disposed.
		private GorgonPixelShader _lastPixelShader = null;			// Last pixel shader.
		private GorgonVertexShader _lastVertexShader = null;			// Last vertex shader.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Gorgon 2D instance that created this object.
		/// </summary>
		public Gorgon2D Gorgon2D
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function called when a pass is about to start rendering.
		/// </summary>
		/// <param name="passIndex">Index of the pass being rendered.</param>
		protected override void OnBeforeRenderPass(int passIndex)
		{
			base.OnBeforeRenderPass(passIndex);
			_lastPixelShader = Gorgon2D.PixelShader.Current;
			_lastVertexShader = Gorgon2D.VertexShader.Current;
			Gorgon2D.PixelShader.Current = PixelShader;
			Gorgon2D.VertexShader.Current = VertexShader;
		}

		/// <summary>
		/// Function called after a pass has rendered.
		/// </summary>
		/// <param name="passIndex">Index of the pass being rendered.</param>
		protected override void OnAfterRenderPass(int passIndex)
		{
			Gorgon2D.PixelShader.Current = _lastPixelShader;
			Gorgon2D.VertexShader.Current = _lastVertexShader;
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				if (disposing)
				{
					Gorgon2D.TrackedObjects.Remove(this);

					Gorgon2D.PixelShader.Current = _lastPixelShader;
					Gorgon2D.VertexShader.Current = _lastVertexShader;
				}
				
				_isDisposed = true;
			}
			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DEffect"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D instance that created this object.</param>
		/// <param name="name">The name of the effect.</param>
		/// <param name="passCount">The number of passes..</param>
		protected Gorgon2DEffect(Gorgon2D gorgon2D, string name, int passCount)
			: base(gorgon2D.Graphics, name, passCount)
		{
			Gorgon2D = gorgon2D;
		}
		#endregion
	}
}