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
// Created: Saturday, February 25, 2012 4:11:13 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimMath;
using GorgonLibrary.Math;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics.Renderers
{
	/// <summary>
	/// A renderable object for drawing a point on the screen.
	/// </summary>
	public class GorgonPoint
		: GorgonNamedObject, IRenderable
	{
		#region Variables.
		private GorgonRenderable.BlendState _blendState = null;								// Blending state.
		private GorgonRenderable.DepthStencilStates _depthState = null;						// Depth/stencil state.
		private GorgonRenderable.TextureSamplerState _samplerState = null;					// Sampler state.
		private Gorgon2D.Vertex[] _vertices = null;											// List of vertices.
		private Vector2 _penSize = new Vector2(1);											// Pen size.
		#endregion

		#region Properties.	
		/// <summary>
		/// Property to set or return the depth buffer depth for the point.
		/// </summary>
		public float Depth
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the position of the point.
		/// </summary>
		public Vector2 Position
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the interface that created this point.
		/// </summary>
		public Gorgon2D Gorgon2D
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the size of the point.
		/// </summary>
		public Vector2 PenSize
		{
			get
			{
				return _penSize;
			}
			set
			{
				if (_penSize != value)
				{
					if (value.X < 1)
						value.X = 1;
					if (value.Y < 1)
						value.Y = 1;

					_penSize = value;
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to transform the vertices.
		/// </summary>
		private void TransformVertices()
		{
			_vertices[0].Position.X = Position.X;
			_vertices[0].Position.Y = Position.Y;
			_vertices[1].Position.X = Position.X + _penSize.X;
			_vertices[1].Position.Y = Position.Y;
			_vertices[2].Position.X = Position.X;
			_vertices[2].Position.Y = Position.Y + _penSize.Y;
			_vertices[3].Position.X = Position.X + _penSize.X;
			_vertices[3].Position.Y = Position.Y + _penSize.Y;

			// Apply depth to the sprite.
			if (Depth != 0.0f)
				_vertices[3].Position.Z = _vertices[2].Position.Z = _vertices[1].Position.Z = _vertices[0].Position.Z = Depth;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPoint"/> class.
		/// </summary>
		/// <param name="gorgon2D">Gorgon interface that owns this renderable.</param>
		/// <param name="name">The name of the point.</param>
		/// <param name="position">Position of the point.</param>
		internal GorgonPoint(Gorgon2D gorgon2D, string name, Vector2 position)
			: base(name)
		{
			CullingMode = Graphics.CullingMode.Back;
			Gorgon2D = gorgon2D;
			Position = position;
			_depthState = new GorgonRenderable.DepthStencilStates();
			_blendState = new GorgonRenderable.BlendState();
			_samplerState = new GorgonRenderable.TextureSamplerState();
			_vertices = new []
			{
				new Gorgon2D.Vertex() 
				{
					Position = new Vector4(0, 0, 0, 1.0f),
					UV = Vector2.Zero,
					Color = new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f)
				},
				new Gorgon2D.Vertex() 
				{
					Position = new Vector4(0, 0, 0, 1.0f),
					UV = Vector2.Zero,
					Color = new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f)
				},
				new Gorgon2D.Vertex() 
				{
					Position = new Vector4(0, 0, 0, 1.0f),
					UV = Vector2.Zero,
					Color = new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f)
				},
				new Gorgon2D.Vertex() 
				{
					Position = new Vector4(0, 0, 0, 1.0f),
					UV = Vector2.Zero,
					Color = new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f)
				}
			};
		}
		#endregion

		#region IRenderable Members
		/// <summary>
		/// Property to set or return the vertex buffer binding for this renderable.
		/// </summary>
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
		GorgonIndexBuffer IRenderable.IndexBuffer
		{
			get 
			{
				if ((_penSize.X == 1.0f) && (_penSize.Y == 1.0f))
					return null;
				else
					return Gorgon2D.DefaultIndexBuffer;
			}
		}

		/// <summary>
		/// Property to return the type of primitive for the renderable.
		/// </summary>
		PrimitiveType IRenderable.PrimitiveType
		{
			get 
			{
				if ((_penSize.X == 1.0f) && (_penSize.Y == 1.0f))
					return PrimitiveType.PointList;
				else
					return PrimitiveType.TriangleList;
			}
		}

		/// <summary>
		/// Property to return a list of vertices to render.
		/// </summary>
		Gorgon2D.Vertex[] IRenderable.Vertices
		{
			get 
			{
				return _vertices;
			}
		}

		/// <summary>
		/// Property to return the number of indices that make up this renderable.
		/// </summary>
		int IRenderable.IndexCount
		{
			get 
			{
				if ((_penSize.X == 1.0f) && (_penSize.Y == 1.0f))
					return 0;
				else
					return 6;
			}
		}

		/// <summary>
		/// Property to set or return the number of vertices to add to the base starting index.
		/// </summary>
		int IRenderable.BaseVertexCount
		{
			get 
			{
				if ((_penSize.X == 1.0f) && (_penSize.Y == 1.0f))
					return 1;
				else
					return 0;
			}
		}

		/// <summary>
		/// Property to return the number of vertices for the renderable.
		/// </summary>
		int IRenderable.VertexCount
		{
			get 
			{
				if ((_penSize.X == 1.0f) && (_penSize.Y == 1.0f))
					return 1;
				else
					return 4;
			}
		}

		/// <summary>
		/// Property to set or return depth/stencil buffer states for this renderable.
		/// </summary>
		public GorgonRenderable.DepthStencilStates DepthStencil
		{
			get 
			{
				return _depthState;
			}
			set
			{
				if (value == null)
					return;

				_depthState = value;
			}
		}

		/// <summary>
		/// Property to set or return advanced blending states for this renderable.
		/// </summary>
		public GorgonRenderable.BlendState Blending
		{
			get 
			{
				return _blendState;
			}
			set
			{
				if (value == null)
					return;

				_blendState = value;
			}
		}

		/// <summary>
		/// Property to set or return a pre-defined blending states for the renderable.
		/// </summary>
		public BlendingMode BlendingMode
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
		public GorgonMinMaxF AlphaTestValues
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the opacity (Alpha channel) of the renderable object.
		/// </summary>
		public float Opacity
		{
			get
			{
				return _vertices[0].Color.Alpha;
			}
			set
			{
				_vertices[3].Color.Alpha = _vertices[2].Color.Alpha = _vertices[1].Color.Alpha = _vertices[0].Color.Alpha = value;
			}
		}

		/// <summary>
		/// Property to set or return the color for a renderable object.
		/// </summary>
		public GorgonColor Color
		{
			get
			{
				return _vertices[0].Color;
			}
			set
			{
				_vertices[3].Color = _vertices[2].Color = _vertices[1].Color = _vertices[0].Color = value;
			}
		}

		/// <summary>
		/// Property to set or return advanded texture sampler states for this renderable.
		/// </summary>
		GorgonRenderable.TextureSamplerState IRenderable.TextureSampler
		{
			get
			{
				return _samplerState;
			}
			set
			{				
			}
		}

		/// <summary>
		/// Property to set or return pre-defined smoothing states for the renderable.
		/// </summary>
		SmoothingMode IRenderable.SmoothingMode
		{
			get
			{
				return SmoothingMode.None;
			}
			set
			{				
			}
		}

		/// <summary>
		/// Property to set or return a texture for the renderable.
		/// </summary>
		GorgonTexture2D IRenderable.Texture
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <remarks>Please note that this doesn't draw the object to the target right away, but queues it up to be 
		/// drawn when <see cref="M:GorgonLibrary.Graphics.Renderers.Gorgon2D.Render">Render</see> is called.
		/// </remarks>
		public void Draw()
		{
			TransformVertices();
			Gorgon2D.AddRenderable(this);
		}
		#endregion
	}
}