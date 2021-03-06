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
// Created: Monday, March 05, 2012 10:34:16 AM
// 
#endregion

using System;
using System.Linq;
using System.Drawing;
using GorgonLibrary.Graphics;
using GorgonLibrary.Math;
using GorgonLibrary.Renderers.Properties;
using SlimMath;

namespace GorgonLibrary.Renderers
{
	/// <summary>
    /// A camera that performs orthographic (2D) projection.
	/// </summary>
	/// <remarks>The orthographic camera is the default camera used in Gorgon.  This camera performs 2D projection of sprites and other renderables on to a target. 
	/// By default, the camera will use absolute screen space coordinates e.g. 160x120 will be the center of a 320x240 render target.  The user may define their own  
	/// coordinate system to apply to the projection.</remarks>
	public class Gorgon2DOrthoCamera
		: GorgonNamedObject, I2DCamera
	{
		#region Variables.
		private Matrix _viewProjecton = Matrix.Identity;				// Projection view matrix.
		private Matrix _projection = Matrix.Identity;					// Projection matrix.
		private Matrix _view = Matrix.Identity;							// View matrix.
		private RectangleF _viewDimensions = RectangleF.Empty;			// View projection dimensions.
		private float _maxDepth;										// Maximum depth.
		private readonly GorgonSprite _cameraIcon;						// Camera icon.
		private float _angle;											// Angle of rotation.
		private float _minDepth;										// Minimum depth value.
		private Vector2 _zoom = new Vector2(1.0f);						// Scale.
		private Vector2 _position = Vector2.Zero;						// Position.
		private Vector2 _anchor = Vector2.Zero;							// Target position.
		private bool _needsProjectionUpdate = true;						// Flag to indicate that the projection matrix needs updating.
		private bool _needsViewUpdate = true;							// Flag to indicate that the view matrix needs updating.
		private bool _needsUpload = true;								// Flag to indicate that the view/projection matrix needs uploading to the GPU.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the interface that created this camera.
		/// </summary>
		public Gorgon2D Gorgon2D
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the angle of rotation in degrees.
		/// </summary>
		/// <remarks>An orthographic camera can only rotate around a Z-Axis.</remarks>
		public float Angle
		{
			get
			{
				return _angle;
			}
			set
			{
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				if (_angle == value)
				{
					return;
				}

				_angle = value;
				_needsViewUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the camera position.
		/// </summary>
		public Vector2 Position
		{
			get
			{
				return _position;
			}
			set
			{
				if (value == _position)
				{
					return;
				}

				_position = value;
				_needsViewUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the zoom for the camera.
		/// </summary>
		public Vector2 Zoom
		{
			get
			{
				return _zoom;
			}
			set
			{
				if (value == _zoom)
				{
					return;
				}

				_zoom = value;
				_needsViewUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return an anchor for rotation, scaling and positioning.
		/// </summary>
		public Vector2 Anchor
		{
			get
			{
				return _anchor;
			}
			set
			{
				if (_anchor == value)
				{
					return;
				}

				_anchor = value;
				_needsProjectionUpdate = true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the matrices used for the camera.
		/// </summary>
		private void UpdateMatrices()
		{
			if (_needsProjectionUpdate)
			{
				Matrix.OrthoOffCenterLH(_viewDimensions.Left - _anchor.X,
					_viewDimensions.Right - _anchor.X,
					_viewDimensions.Bottom - _anchor.Y,
					_viewDimensions.Top - _anchor.Y,
					_minDepth,
					_maxDepth,
					out _projection);
			}

			if (_needsViewUpdate)
			{
				UpdateViewMatrix();
			}

			if ((_needsProjectionUpdate) || (_needsViewUpdate))
			{
				_needsUpload = true;
				Matrix.Multiply(ref _view, ref _projection, out _viewProjecton);
			}

			_needsProjectionUpdate = false;
			_needsViewUpdate = false;
		}

		/// <summary>
		/// Function to update the view matrix.
		/// </summary>
		private void UpdateViewMatrix()
		{
			Matrix center = Matrix.Identity;	// Centering matrix.
			Matrix translation;					// Translation matrix.

			// Scale it.
			// ReSharper disable CompareOfFloatsByEqualityOperator
			if ((_zoom.X != 1.0f) || (_zoom.Y != 1.0f))
			{
				center.M11 = _zoom.X;
				center.M22 = _zoom.Y;
				center.M33 = 1.0f;
			}

			if (_angle != 0.0f)
			{
				Matrix rotation;						// Rotation matrix.

				Matrix.RotationZ(_angle.Radians(), out rotation);
				Matrix.Multiply(ref rotation, ref center, out center);
			}
			// ReSharper restore CompareOfFloatsByEqualityOperator

			Matrix.Translation(_position.X, _position.Y, 0.0f, out translation);
			Matrix.Multiply(ref translation, ref center, out center);

			_view = center;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DOrthoCamera"/> class.
		/// </summary>
		/// <param name="gorgon2D">The 2D interface that created this object.</param>
		/// <param name="name">The name.</param>
		/// <param name="viewDimensions">The view dimensions.</param>
		/// <param name="minDepth">The minimum depth value.</param>
		/// <param name="maximumDepth">The maximum depth value.</param>
		internal Gorgon2DOrthoCamera(Gorgon2D gorgon2D, string name, RectangleF viewDimensions,float minDepth, float maximumDepth)
			: base(name)
		{
			Gorgon2D = gorgon2D;
			_minDepth = minDepth;
			_maxDepth = maximumDepth;
			_viewDimensions = viewDimensions;

			_cameraIcon = new GorgonSprite(gorgon2D, "GorgonCamera.OrthoIcon")
			              {
				              Size = new Vector2(64, 50),
				              Texture =
					              gorgon2D.Graphics.GetTrackedObjectsOfType<GorgonTexture2D>()
					                      .FirstOrDefault(item =>
					                                      item.Name.Equals("Gorgon2D.Icons", StringComparison.OrdinalIgnoreCase))
					              ??
					              gorgon2D.Graphics.Textures.CreateTexture<GorgonTexture2D>("Gorgon2D.Icons",
					                                                                        Resources.Icons),
				              TextureRegion = new RectangleF(0.253906f, 0, 0.25f, 0.195313f),
				              Anchor = new Vector2(32f, 25),
				              Scale = new Vector2(1.0f),
				              Color = Color.White
			              };
		}
		#endregion

		#region ICamera Members
		#region Properties.
		/// <summary>
		/// Property to return the horizontal and vertical aspect ratio for the camera view area.
		/// </summary>
		public Vector2 AspectRatio
		{
			get
			{
				return new Vector2(TargetWidth / (float)TargetHeight, TargetHeight / (float)TargetWidth);
			}
		}

		/// <summary>
		/// Property to set or return the projection view dimensions for the camera.
		/// </summary>
		public RectangleF ViewDimensions
		{
			get
			{
				return _viewDimensions;
			}
			set
			{
				if (_viewDimensions == value)
				{
					return;
				}

				_viewDimensions = value;
				_needsProjectionUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the minimum depth for the camera.
		/// </summary>
		public float MinimumDepth
		{
			get
			{
				return _minDepth;
			}
			set
			{
				_minDepth = value;
				_needsProjectionUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the maximum depth for the camera.
		/// </summary>
		public float MaximumDepth
		{
			get
			{
				return _maxDepth;
			}
			set
			{
				if (value < 1.0f)
				{
					value = 1.0f;
				}

				_maxDepth = value;
				_needsProjectionUpdate = true;
			}
		}

		/// <summary>
		/// Property to return whether the camera needs updating.
		/// </summary>
		public bool NeedsUpdate
		{
			get
			{
				return _needsProjectionUpdate || _needsViewUpdate || _needsUpload;
			}
		}

        /// <summary>
        /// Property to return the 
        /// </summary>
        public Matrix ViewProjection
        {
            get
            {
                return _viewProjecton;
            }
        }

		/// <summary>
		/// Property to return the projection matrix for the camera.
		/// </summary>
		public Matrix Projection
		{
			get
			{
				return _projection;
			}
		}

		/// <summary>
		/// Property to return the view matrix for the camera.
		/// </summary>
		public Matrix View
		{
			get
			{
				return _view;
			}
		}

		/// <summary>
		/// Property to set or return whether the dimensions of the camera should be automatically adjusted to match the current render target.
		/// </summary>
		public bool AutoUpdate
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the width of the current target.
		/// </summary>
		public int TargetWidth
		{
			get
			{
				switch (Gorgon2D.Target.Resource.ResourceType)
				{
					case ResourceType.Buffer:
						return Gorgon2D.Target.Resource.SizeInBytes / Gorgon2D.Target.FormatInformation.SizeInBytes;
					case ResourceType.Texture1D:
						return ((GorgonRenderTarget1D)Gorgon2D.Target.Resource).Settings.Width;
					case ResourceType.Texture2D:
						return ((GorgonRenderTarget2D)Gorgon2D.Target.Resource).Settings.Width;
					case ResourceType.Texture3D:
						return ((GorgonRenderTarget3D)Gorgon2D.Target.Resource).Settings.Width;
					default:
						return 0;
				}
			}
		}

		/// <summary>
		/// Property to return the height of the current target.
		/// </summary>
		public int TargetHeight
		{
			get
			{
				switch (Gorgon2D.Target.Resource.ResourceType)
				{
					case ResourceType.Buffer:
						return 1;
					case ResourceType.Texture1D:
						return 1;
					case ResourceType.Texture2D:
						return ((GorgonRenderTarget2D)Gorgon2D.Target.Resource).Settings.Height;
					case ResourceType.Texture3D:
						return ((GorgonRenderTarget3D)Gorgon2D.Target.Resource).Settings.Height;
					default:
						return 0;
				}
			}
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function to project a screen position into camera space.
        /// </summary>
        /// <param name="screenPosition">3D Position on the screen.</param>
        /// <param name="result">The resulting projected position.</param>
        /// <param name="includeViewTransform">[Optional] TRUE to include the view transformation in the projection calculations, FALSE to only use the projection.</param>
        /// <remarks>Use this to convert a position in screen space into the camera view/projection space.  If the <paramref name="includeViewTransform"/> is set to 
        /// TRUE, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to FALSE only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        public void Project(ref Vector3 screenPosition, out Vector3 result, bool includeViewTransform = true)
        {
            Matrix transformMatrix;

            UpdateMatrices();

            if (includeViewTransform)
            {
                Matrix.Invert(ref _viewProjecton, out transformMatrix);
            }
            else
            {
                Matrix.Invert(ref _projection, out transformMatrix);
            }

            // Calculate relative position of our screen position.
            var relativePosition = new Vector2(2.0f * screenPosition.X / TargetWidth - 1.0f,
                                               1.0f - screenPosition.Y / TargetHeight * 2.0f);

            // Transform our screen position by our inverse matrix.
            Vector4 transformed;
            Vector2.Transform(ref relativePosition, ref transformMatrix, out transformed);

            result = (Vector3)transformed;

	        // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (transformed.W != 1.0f)
            {
                Vector3.Divide(ref result, transformed.W, out result);
            }
        }

        /// <summary>
        /// Function to unproject a world space position into screen space.
        /// </summary>
        /// <param name="worldSpacePosition">A position in world space.</param>
        /// <param name="result">The resulting projected position.</param>
        /// <param name="includeViewTransform">[Optional] TRUE to include the view transformation in the projection calculations, FALSE to only use the projection.</param>
        /// <returns>The unprojected world space coordinates.</returns>
        /// <remarks>Use this to convert a position in world space into the screen space.  If the <paramref name="includeViewTransform"/> is set to 
        /// TRUE, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to FALSE only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        public void Unproject(ref Vector3 worldSpacePosition, out Vector3 result, bool includeViewTransform = true)
        {
            UpdateMatrices();

            Matrix transformMatrix = includeViewTransform ? _viewProjecton : _projection;

            Vector4 transform;
            Vector3.Transform(ref worldSpacePosition, ref transformMatrix, out transform);

	        // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (transform.W != 1.0f)
            {
                Vector4.Divide(ref transform, transform.W, out transform);
            }

            result = new Vector3((transform.X + 1.0f) * 0.5f * TargetWidth,
                (1.0f - transform.Y) * 0.5f * TargetHeight, 0);
        }

        /// <summary>
        /// Function to project a screen position into camera space.
        /// </summary>
        /// <param name="screenPosition">3D Position on the screen.</param>
        /// <param name="includeViewTransform">[Optional] TRUE to include the view transformation in the projection calculations, FALSE to only use the projection.</param>
        /// <returns>
        /// The projected 3D position of the screen.
        /// </returns>
        /// <remarks>
        /// Use this to convert a position in screen space into the camera view/projection space.  If the <paramref name="includeViewTransform" /> is set to
        /// TRUE, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to FALSE only the projection will
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.
        /// </remarks>
        public Vector3 Project(Vector3 screenPosition, bool includeViewTransform = true)
        {
            Vector3 result;

            Project(ref screenPosition, out result, includeViewTransform);

            return result;
        }

        /// <summary>
        /// Function to unproject a world space position into screen space.
        /// </summary>
        /// <param name="worldSpacePosition">A position in world space.</param>
        /// <param name="includeViewTransform">[Optional] TRUE to include the view transformation in the projection calculations, FALSE to only use the projection.</param>
        /// <returns>The unprojected world space coordinates.</returns>
        /// <remarks>Use this to convert a position in world space into the screen space.  If the <paramref name="includeViewTransform"/> is set to 
        /// TRUE, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to FALSE only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        public Vector3 Unproject(Vector3 worldSpacePosition, bool includeViewTransform = true)
        {
            Vector3 result;

            Unproject(ref worldSpacePosition, out result, includeViewTransform);

            return result;
        }
        
        /// <summary>
		/// Function to update the view projection matrix for the camera and populate a view/projection constant buffer.
		/// </summary>
		public void Update()
		{
			UpdateMatrices();

			// Update the projection view matrix on the vertex shader.
			Gorgon2D.VertexShader.TransformBuffer.Update(ref _viewProjecton);

			_needsUpload = false;
		}

		/// <summary>
		/// Function to draw the camera icon.
		/// </summary>
		public void Draw()
		{
            var position = new Vector3(-_position.X, -_position.Y, 0);
		    Vector3 iconPosition;

            Unproject(ref position, out iconPosition);      // Convert to screen space.

            // Update the position to be projected into the current camera space.
		    if (Gorgon2D.Camera != this)
		    {
			    Vector3.Negate(ref iconPosition, out iconPosition);
		        iconPosition.Z = (Gorgon2D.Camera.MaximumDepth - Gorgon2D.Camera.MinimumDepth) / Gorgon2D.Camera.MinimumDepth;
		        Gorgon2D.Camera.Project(ref iconPosition, out iconPosition, false);

		        // Now update that position to reflect in screen space relative to our current camera.
                // We do this without the view transform because we only want to undo the projection and
                // not the camera transformation.
				Gorgon2D.Camera.Unproject(ref iconPosition, out iconPosition);
		    }

            // Project back to the default camera.
            iconPosition = Gorgon2D.DefaultCamera.Project(iconPosition);

			// ReSharper disable CompareOfFloatsByEqualityOperator
		    if ((Gorgon2D.DefaultCamera.Zoom.X != 1.0f)
                || (Gorgon2D.DefaultCamera.Zoom.Y != 1.0f))
            {
                _cameraIcon.Scale = new Vector2(1.0f / Gorgon2D.DefaultCamera.Zoom.X,
                    1.0f / Gorgon2D.DefaultCamera.Zoom.Y);
            }
			// ReSharper restore CompareOfFloatsByEqualityOperator

            // Highlight current camera.
		    _cameraIcon.Color = Gorgon2D.Camera == this ? Color.FromArgb(204, Color.Green) : Color.FromArgb(204, Color.White);

            _cameraIcon.Position = (Vector2)iconPosition;
            _cameraIcon.Angle = -Gorgon2D.DefaultCamera.Angle;

            // Draw the icon in our camera space, otherwise it won't look right.
            var prevCamera = Gorgon2D.Camera;

            if (prevCamera != Gorgon2D.DefaultCamera)
            {
                Gorgon2D.Camera = null;
            }

            _cameraIcon.Draw();

            if (prevCamera == Gorgon2D.DefaultCamera)
            {
                return;
            }

            Gorgon2D.Camera = prevCamera;
        }
		#endregion
		#endregion
	}
}