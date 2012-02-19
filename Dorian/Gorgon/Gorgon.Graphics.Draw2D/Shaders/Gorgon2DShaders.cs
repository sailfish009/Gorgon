﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMath;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics.Renderers
{
	/// <summary>
	/// An interface for shader functionality.
	/// </summary>
	public class Gorgon2DShaders
	{
		#region Variables.
		private Gorgon2D _gorgon2D = null;						// 2D interface that owns this object.
		private Gorgon2DVertexShader _vertexShader = null;		// Current vertex shader.
		private Gorgon2DPixelShader _pixelShader = null;		// Current pixel shader.
		private GorgonConstantBuffer _viewProjection = null;	// Constant buffer for handling the view/projection matrix upload to the video device.
		#endregion

		#region Properties.

		/// <summary>
		/// Property to return the default verte shader.
		/// </summary>
		internal GorgonDefaultVertexShader DefaultVertexShader
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Property to return the default diffuse pixel shader.
		/// </summary>
		internal GorgonDefaultPixelShaderDiffuse DefaultPixelShaderDiffuse
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the default textured pixel shader.
		/// </summary>
		internal GorgonDefaultPixelShaderTextured DefaultPixelShaderTextured
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the current vertex shader.
		/// </summary>
		public Gorgon2DVertexShader VertexShader
		{
			get
			{
				return _vertexShader;
			}
			set
			{
				if ((_vertexShader != value) || ((value == null) && (_vertexShader != DefaultVertexShader)))
				{
					if (value == null)
						_vertexShader = DefaultVertexShader;
					else
						_vertexShader = value;

					if (value != null)
						_gorgon2D.Graphics.Shaders.VertexShader.Current = _vertexShader.Shader;
					else
						_gorgon2D.Graphics.Shaders.VertexShader.Current = DefaultVertexShader.Shader;

					_gorgon2D.Graphics.Shaders.VertexShader.ConstantBuffers[0] = _viewProjection;
				}
			}
		}

		/// <summary>
		/// Property to set or return the current pixel shader.
		/// </summary>
		public Gorgon2DPixelShader PixelShader
		{
			get
			{
				return _pixelShader;
			}
			set
			{
				if ((_pixelShader != value) || ((value == null) && (_pixelShader != DefaultPixelShaderTextured) && (_pixelShader != DefaultPixelShaderDiffuse)))
				{
					if (value == null)
					{
						// If we have a texture in the first slot, then set the proper shader.
						if (_gorgon2D.Graphics.Shaders.PixelShader.Textures[0] == null)
							_pixelShader = DefaultPixelShaderDiffuse;
						else
							_pixelShader = DefaultPixelShaderTextured;
					}
					else
						_pixelShader = value;
					
					_gorgon2D.Graphics.Shaders.PixelShader.Current = _pixelShader.Shader;
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to gorgon's transformation matrix.
		/// </summary>
		internal void UpdateGorgonTransformation()
		{
			using (GorgonDataStream streamBuffer = _viewProjection.Lock(BufferLockFlags.Discard | BufferLockFlags.Write))
			{
				Matrix viewProjection = Matrix.Multiply(_gorgon2D.ViewMatrix.Value, _gorgon2D.ProjectionMatrix.Value);
				streamBuffer.Write(viewProjection);
				_viewProjection.Unlock();
			}
		}

		/// <summary>
		/// Function to clean up the shader interface.
		/// </summary>
		internal void CleanUp()
		{
			if (_viewProjection != null)
				_viewProjection.Dispose();

			if (DefaultVertexShader != null)
				DefaultVertexShader.Dispose();

			if (DefaultPixelShaderDiffuse != null)
				DefaultPixelShaderDiffuse.Dispose();

			if (DefaultPixelShaderTextured != null)
				DefaultPixelShaderTextured.Dispose();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DShaders"/> class.
		/// </summary>
		/// <param name="gorgon2D">The gorgon 2D interface that owns this object.</param>
		internal Gorgon2DShaders(Gorgon2D gorgon2D)
		{
			_gorgon2D = gorgon2D;
			
			DefaultVertexShader = new GorgonDefaultVertexShader(gorgon2D);
			DefaultPixelShaderDiffuse = new GorgonDefaultPixelShaderDiffuse(gorgon2D);
			DefaultPixelShaderTextured = new GorgonDefaultPixelShaderTextured(gorgon2D);

			_viewProjection = gorgon2D.Graphics.Shaders.CreateConstantBuffer(DirectAccess.SizeOf<Matrix>(), true);
		}
		#endregion
	}
}