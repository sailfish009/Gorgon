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
// Created: Wednesday, February 08, 2012 3:04:49 PM
// 
#endregion

using System;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics.Properties;
using SlimMath;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A 3 dimensional texture object.
	/// </summary>
	/// <remarks>A 3D texture (volumn texture) contains not only width and height, but also depth.  The depth for a volume texture is defined by slices of a 2D texture.  For example 
	/// if the texture is 320x240x160, then there will be 160 320x240 textures stacked.</remarks>
	public class GorgonTexture3D
		: GorgonTexture
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of data in the resource.
		/// </summary>
		public override ResourceType ResourceType
		{
			get
			{
				return ResourceType.Texture3D;
			}
		}

		/// <summary>
		/// Property to return the settings for this texture.
		/// </summary>
		public new GorgonTexture3DSettings Settings
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function to return sub resource data for a lock operation.
        /// </summary>
        /// <param name="dataStream">Stream containing the data.</param>
        /// <param name="rowPitch">The number of bytes per row of the texture.</param>
        /// <param name="slicePitch">The number of bytes per depth slice of the texture.</param>
        /// <param name="context">The graphics context to use when locking the texture.</param>
        /// <returns>
        /// The sub resource data.
        /// </returns>
        /// <remarks>
        /// The <paramref name="context" /> allows a separate thread to access the resource at the same time as another thread.
        /// </remarks>
		protected override ISubResourceData OnGetLockSubResourceData(IO.GorgonDataStream dataStream, int rowPitch, int slicePitch, GorgonGraphics context)
		{
			return new GorgonTexture3DData(dataStream, rowPitch, slicePitch);
		}

        /// <summary>
        /// Function to copy data from the CPU to a texture.
        /// </summary>
        /// <param name="data">Data to copy to the texture.</param>
        /// <param name="subResource">Sub resource index to use.</param>
        /// <param name="context">The graphics context to use when updating the texture.</param>
        /// <remarks>
        /// The <paramref name="context" /> allows a separate thread to access the resource at the same time as another thread.
        /// </remarks>
		protected override void OnUpdateSubResource(ISubResourceData data, int subResource, GorgonGraphics context)
		{
			var box = new SharpDX.DataBox
				{
				DataPointer = data.Data.PositionPointer,
				RowPitch = data.RowPitch,
				SlicePitch = data.SlicePitch
			};

			var region = new D3D.ResourceRegion
			{
				Front = 0,
				Back = Settings.Depth,
				Left = 0,
				Right = Settings.Width,
				Top = 0,
				Bottom = Settings.Height
			};

	        context.Context.UpdateSubresourceSafe(box, D3DResource, data.RowPitch, FormatInformation.SizeInBytes, region, FormatInformation.IsCompressed);
		}

		/// <summary>
		/// Function to copy this texture into a new staging texture.
		/// </summary>
		/// <returns>
		/// The new staging texture.
		/// </returns>
		protected override GorgonTexture OnGetStagingTexture()
		{
			var settings3D = new GorgonTexture3DSettings
				{
				Format = Settings.Format,
				Width = Settings.Width,
				Height = Settings.Height,
				Depth = Settings.Depth,
				MipCount = Settings.MipCount,
				Usage = BufferUsage.Staging
			};

			GorgonTexture staging = Graphics.ImmediateContext.Textures.CreateTexture(Name + ".Staging", settings3D);

			staging.Copy(this);

			return staging;
		}

		/// <summary>
		/// Function to create an image with initial data.
		/// </summary>
		/// <param name="initialData">Data to use when creating the image.</param>
		/// <remarks>
		/// The <paramref name="initialData" /> can be NULL (Nothing in VB.Net) IF the texture is not created with an Immutable usage flag.
		/// <para>To initialize the texture, create a new <see cref="GorgonLibrary.Graphics.GorgonImageData">GorgonImageData</see> object and fill it with image information.</para>
		/// </remarks>
		protected override void OnInitialize(GorgonImageData initialData)
		{
			var desc = new D3D.Texture3DDescription
				{
					Format = (SharpDX.DXGI.Format)Settings.Format,
					Width = Settings.Width,
					Height = Settings.Height,
					Depth = Settings.Depth,
					MipLevels = Settings.MipCount,
					BindFlags = GetBindFlags(false, false),
					Usage = (D3D.ResourceUsage)Settings.Usage,
					OptionFlags = D3D.ResourceOptionFlags.None
				};

			switch (Settings.Usage)
			{
				case BufferUsage.Staging:
					desc.CpuAccessFlags = D3D.CpuAccessFlags.Read | D3D.CpuAccessFlags.Write;
					break;
				case BufferUsage.Dynamic:
					desc.CpuAccessFlags = D3D.CpuAccessFlags.Write;
					break;
				default:
					desc.CpuAccessFlags = D3D.CpuAccessFlags.None;
					break;
			}
			

			if ((initialData != null) && (initialData.Count > 0))
			{
				D3DResource = new D3D.Texture3D(Graphics.D3DDevice, desc, initialData.GetDataBoxes());
			}
			else
			{
				D3DResource = new D3D.Texture3D(Graphics.D3DDevice, desc);
			}
		}

        /// <summary>
        /// Function to retrieve a shader resource view object.
        /// </summary>
        /// <param name="format">The format of the resource view.</param>
        /// <param name="mipStart">[Optional] Starting mip map for the view.</param>
        /// <param name="mipCount">[Optional] Mip map count for the view.</param>
        /// <remarks>Use a shader view to access a texture from a shader.  A shader view can view a select portion of the texture, and the view <paramref name="format"/> can be used to 
        /// cast the format of the texture into another type (as long as the view format is in the same group as the texture format).  For example, a texture with a format of R8G8B8A8 could be cast 
        /// to R8G8B8A8_UInt_Normal, or R8G8B8A8_UInt or any of the other R8G8B8A8 formats.
        /// <para>Multiple views of the texture can be bound to different parts of the shader pipeline.</para>
        /// <para>Textures that have a usage of staging cannot create shader views.</para>
        /// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the view could not created or retrieved from the internal cache.</exception>
        /// <returns>A texture shader view object.</returns>
        public GorgonTextureShaderView GetShaderView(BufferFormat format, int mipStart = 0, int mipCount = 1)
        {
            return OnGetShaderView(format, mipStart, mipCount, 0, 1);
        }

		/// <summary>
		/// Function to retrieve an unordered access view for this texture.
		/// </summary>
		/// <param name="format">Format of the buffer.</param>
		/// <param name="mipStart">[Optiona] First mip map level to map to the view.</param>
		/// <param name="depthStart">[Optional] The first depth level to  map to the view.</param>
		/// <param name="depthCount">[Optional] The number of depth levels to map to the view.</param>
		/// <returns>An unordered access view for the texture.</returns>
		/// <remarks>Use this to create/retrieve an unordered access view that will allow shaders to access the view using multiple threads at the same time.  Unlike a shader view, only one 
		/// unordered access view can be bound to the pipeline at any given time.
		/// <para>Unordered access views require a video device feature level of SM_5 or better.</para>
        /// <para>Textures that have a usage of staging cannot create unordered views.</para>
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the view could not created or retrieved from the internal cache.</exception>
		public GorgonTextureUnorderedAccessView GetUnorderedAccessView(BufferFormat format, int mipStart = 0, int depthStart = 0,
																			 int depthCount = int.MaxValue)
		{
			if (depthCount > Settings.Depth)
			{
				depthCount = Settings.Depth;
			}

			return OnGetUnorderedAccessView(format, mipStart, depthStart, depthCount);
		}
		
		/// <summary>
		/// Function to convert a texel space coordinate into a pixel space coordinate.
		/// </summary>
		/// <param name="texel">The texel coordinate to convert.</param>
		/// <returns>The pixel location of the texel on the texture.</returns>
		public Vector3 ToPixel(Vector3 texel)
		{
			return new Vector3(texel.X * Settings.Width, texel.Y * Settings.Height, texel.Z * Settings.Depth);
		}

		/// <summary>
		/// Function to convert a pixel coordinate into a texel space coordinate.
		/// </summary>
		/// <param name="pixel">The pixel coordinate to convert.</param>
		/// <returns>The texel space location of the pixel on the texture.</returns>
		/// <exception cref="System.DivideByZeroException">Thrown when the texture width, height or depth is equal to 0.</exception>
		public Vector3 ToTexel(Vector3 pixel)
		{
#if DEBUG
			if ((Settings.Width == 0)
			    || (Settings.Height == 0)
			    || (Settings.Depth == 0))
			{
				throw new DivideByZeroException();
			}
#endif

			return new Vector3(pixel.X / Settings.Width, pixel.Y / Settings.Height, pixel.Z / Settings.Depth);
		}

		/// <summary>
		/// Function to convert a texel space coordinate into a pixel space coordinate.
		/// </summary>
		/// <param name="texel">The texel coordinate to convert.</param>
		/// <param name="result">The pixel space location of the texel.</param>
		/// <exception cref="System.DivideByZeroException">Thrown when the texture width, height or depth is equal to 0.</exception>
		public void ToPixel(ref Vector3 texel, out Vector3 result)
		{
			result = new Vector3(texel.X * Settings.Width, texel.Y * Settings.Height, texel.Z * Settings.Depth);
		}

		/// <summary>
		/// Function to convert a pixel coordinate into a texel space coordinate.
		/// </summary>
		/// <param name="pixel">The pixel coordinate to convert.</param>
		/// <param name="result">The pixel space location of the texel.</param>
		/// <exception cref="System.DivideByZeroException">Thrown when the texture width, height or depth is equal to 0.</exception>
		public void ToTexel(ref Vector3 pixel, out Vector3 result)
		{
#if DEBUG
			if ((Settings.Width == 0)
				|| (Settings.Height == 0)
				|| (Settings.Depth == 0))
			{
				throw new DivideByZeroException();
			}
#endif

			result = new Vector3(pixel.X / Settings.Width, pixel.Y / Settings.Height, pixel.Z / Settings.Depth);
		}

		/// <summary>
		/// Function to return the index of a sub resource in a texture.
		/// </summary>
		/// <param name="mipLevel">Mip level to look up.</param>
		/// <param name="mipCount">The number of mip map levels in the texture.</param>
		/// <returns>The sub resource index.</returns>
		/// <remarks>Unlike the GetSubResourceIndex on the <see cref="GorgonLibrary.Graphics.GorgonTexture1D.GetSubResourceIndex">GorgonTexture1D</see> or <see cref="GorgonLibrary.Graphics.GorgonTexture2D.GetSubResourceIndex">GorgonTexture2D</see> objects, this version does not take an array index because 3D textures do not use array resources.</remarks>
		public static int GetSubResourceIndex(int mipLevel, int mipCount)
		{
			// Constrain to settings.
			if (mipLevel < 0)
			{
				mipLevel = 0;
			}
			if (mipLevel >= mipCount)
			{
				mipLevel = mipCount - 1;
			}

			return D3D.Resource.CalculateSubResourceIndex(mipLevel, 0, mipCount);
		}

		/// <summary>
		/// Function to copy a texture subresource from another texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <param name="subResource">Sub resource in the source texture to copy.</param>
		/// <param name="destSubResource">Sub resource in this texture to replace.</param>
		/// <param name="sourceRegion">Region on the source texture to copy.</param>
		/// <param name="destination">Destination point to copy to.</param>
		/// <remarks>This method will -not- perform stretching or filtering and will clip to the size of the destination texture.  
		/// <para>The <paramref name="sourceRegion"/> and ><paramref name="destination"/> must fit within the dimensions of this texture.  If they do not, then the copy will be clipped so that they fit.</para>
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// <para>When copying sub resources (e.g. mip-map levels), the <paramref name="subResource"/> and <paramref name="destSubResource"/> must be different if the source texture is the same as the destination texture.</para>
		/// <para>Sub resource indices can be calculated with the <see cref="GorgonLibrary.Graphics.GorgonTexture3D.GetSubResourceIndex">GetSubResourceIndex</see> static method.</para>
		/// <para>Pass NULL (Nothing in VB.Net) to the sourceRegion parameter to copy the entire sub resource.</para>
		/// <para>SM2_a_b devices using 3D textures can only be copied to textures that are in GPU memory, if either texture is a staging texture, then an exception will be thrown.  Also, if both textures are not staging 
        /// textures, and the video device has a feature level of SM2_a_b, then an exception will be thrown.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the subResource and destSubResource are the same and the source texture is the same as this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture types are not the same.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// <para>-or-</para>
		/// <para>Thrown when this 3D texture is in CPU accessible memory and the video device is a SM2_a_b device.</para>
		/// </exception>
        /// <exception cref="System.NotSupportedException">Thrown when the video device has a feature level of SM2_a_b, and this texture and the source texture are not staging textures.</exception>
		public void CopySubResource(GorgonTexture3D texture, int subResource, int destSubResource, GorgonBox? sourceRegion, Vector3 destination)
		{
			GorgonDebug.AssertNull(texture, "texture");

#if DEBUG
            if ((Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) && (Settings.Usage != BufferUsage.Staging) && (texture.Settings.Usage != BufferUsage.Staging))
            {
				throw new NotSupportedException(string.Format(Resources.GORGFX_REQUIRES_SM, DeviceFeatureLevel.SM4));
            }

			if (Settings.Usage == BufferUsage.Immutable)
			{
				throw new InvalidOperationException(Resources.GORGFX_TEXTURE_IMMUTABLE);
			}

			// If the format is different, then check to see if the format group is the same.
			if ((texture.Settings.Format != Settings.Format) &&
			    ((texture.FormatInformation.Group != FormatInformation.Group) ||
			     (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b) ||
			     (Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM4)))
			{
				throw new ArgumentException(
					string.Format(Resources.GORGFX_TEXTURE_COPY_CANNOT_CONVERT, texture.Settings.Format, Settings.Format), "texture");
			}

			if ((this == texture) && (subResource == destSubResource))
			{
				throw new ArgumentException(Resources.GORGFX_TEXTURE_CANNOT_COPY_SAME_SUBRESOURCE, "destSubResource");
			}

			// Ensure that the SM2_a_b devices don't try and copy between CPU and GPU accessible memory.
			if ((Graphics.VideoDevice.SupportedFeatureLevel == DeviceFeatureLevel.SM2_a_b)
			    && (Settings.Usage == BufferUsage.Staging) && (texture.Settings.Usage != BufferUsage.Staging))
			{
				throw new InvalidOperationException(Resources.GORGFX_TEXTURE_3D_CPU_ACCESSIBLE);
			}
#endif

			// If we have multisampling enabled, then copy the entire sub resource.
			D3D.ResourceRegion? region = null;

			if (sourceRegion != null)
			{
				region = sourceRegion.Value.Convert;
			}

			Graphics.Context.CopySubresourceRegion(texture.D3DResource, subResource, region, D3DResource, destSubResource,
				(int)destination.X, (int)destination.Y, (int)destination.Z);
		}

		/// <summary>
		/// Function to copy a texture subresource from another texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <param name="sourceRegion">Region on the source texture to copy.</param>
		/// <param name="destination">Destination point to copy to.</param>
		/// <remarks>This method will -not- perform stretching or filtering and will clip to the size of the destination texture.  
		/// <para>The <paramref name="sourceRegion"/> and ><paramref name="destination"/> must fit within the dimensions of this texture.  If they do not, then the copy will be clipped so that they fit.</para>
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// <para>SM2_a_b devices using 3D textures can only be copied to textures that are in GPU memory, if either texture is a staging texture, then an exception will be thrown.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the source texture is the same as this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture types are not the same.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// <para>-or-</para>
		/// <para>Thrown when this 3D texture is in CPU accessible memory and the video device is a SM2_a_b device.</para>
		/// </exception>
		public void CopySubResource(GorgonTexture3D texture, GorgonBox sourceRegion, Vector3 destination)
		{
#if DEBUG
			if (texture == this)
			{
				throw new ArgumentException(Resources.GORGFX_TEXTURE_IS_SAME, "texture");
			}
#endif

			CopySubResource(texture, 0, 0, sourceRegion, destination);
		}

		/// <summary>
		/// Function to copy a texture subresource from another texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <remarks>This method will -not- perform stretching or filtering and will clip to the size of the destination texture.  
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// <para>SM2_a_b devices using 3D textures can only be copied to textures that are in GPU memory, if either texture is a staging texture, then an exception will be thrown.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the source texture is the same as this texture.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the texture types are not the same.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// <para>-or-</para>
		/// <para>Thrown when this 3D texture is in CPU accessible memory and the video device is a SM2_a_b device.</para>
		/// </exception>
		public void CopySubResource(GorgonTexture3D texture)
		{
#if DEBUG
			if (texture == this)
			{
				throw new ArgumentException(Resources.GORGFX_TEXTURE_IS_SAME, "texture");
			}
#endif

			CopySubResource(texture, 0, 0, null, Vector3.Zero);
		}

		/// <summary>
		/// Function to copy a texture sub resource from another texture.
		/// </summary>
		/// <param name="texture">Source texture to copy.</param>
		/// <param name="subResource">Sub resource in the source texture to copy.</param>
		/// <param name="destSubResource">Sub resource in this texture to replace.</param>
		/// <remarks>This method will -not- perform stretching or filtering and will clip to the size of the destination texture.  
		/// <para>The source texture must fit within the dimensions of this texture.  If it does not, then the copy will be clipped so that it fits.</para>
		/// <para>For SM_4_1 and SM_5 video devices, texture formats can be converted if they belong to the same format group (e.g. R8G8B8A8, R8G8B8A8_UInt, R8G8B8A8_Int, R8G8B8A8_UIntNormal, etc.. are part of the R8G8B8A8 group).  If the 
		/// video device is a SM_4 or SM_2_a_b device, then no format conversion will be done and an exception will be thrown if format conversion is attempted.</para>
		/// <para>When copying sub resources (e.g. mip-map levels), the <paramref name="subResource"/> and <paramref name="destSubResource"/> must be different if the source texture is the same as the destination texture.</para>
		/// <para>Sub resource indices can be calculated with the <see cref="GorgonLibrary.Graphics.GorgonTexture3D.GetSubResourceIndex">GetSubResourceIndex</see> static method.</para>
		/// <para>SM2_a_b devices using 3D textures can only be copied to textures that are in GPU memory, if either texture is a staging texture, then an exception will be thrown.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the texture parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the formats cannot be converted because they're not of the same group or the current video device is a SM_2_a_b device or a SM_4 device.
		/// <para>-or-</para>
		/// <para>Thrown when the subResource and destSubResource are the same and the source texture is the same as this texture.</para>
		/// </exception>
		/// <exception cref="System.InvalidOperationException">Thrown when this texture is an immutable texture.
		/// <para>-or-</para>
		/// <para>Thrown when this 3D texture is in CPU accessible memory and the video device is a SM2_a_b device.</para>
		/// </exception>
		public void CopySubResource(GorgonTexture3D texture, int subResource, int destSubResource)
		{
			CopySubResource(texture, subResource, destSubResource, null, Vector3.Zero);
		}

        /// <summary>
        /// Function to copy data from the CPU to a texture.
        /// </summary>
        /// <param name="data">Data to copy to the texture.</param>
        /// <param name="subResource">Sub resource index to use.</param>
        /// <param name="destBox">Destination region to copy into.</param>
        /// <param name="deferred">[Optional] The deferred context to use when updating the sub resource.</param>
        /// <exception cref="System.InvalidOperationException">Thrown when this texture has an Immutable, Dynamic or a Staging usage.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="destBox" /> parameter is less than 0 or larger than this texture.</exception>
        /// <remarks>
        /// Use this to copy data to this texture.  If the texture is non CPU accessible texture then an exception is raised.
        /// <para>If <paramref name="deferred"/> is NULL (Nothing in VB.Net), then the immediate context is used to update the texture.  Specify a deferred context when accessing the resource is being accessed by a separate thread.</para>
        /// </remarks>
		public void UpdateSubResource(ISubResourceData data, int subResource, GorgonBox destBox, GorgonGraphics deferred = null)
		{
#if DEBUG
	        if ((Settings.Usage == BufferUsage.Dynamic) || (Settings.Usage == BufferUsage.Immutable))
	        {
		        throw new InvalidOperationException(Resources.GORGFX_TEXTURE_IS_DYNAMIC_IMMUTABLE);
	        }
#endif
	        if (destBox.Z < 0)
	        {
		        destBox.Z = 0;
	        }
	        if (destBox.Depth < 0)
	        {
		        destBox.Depth = 0;
	        }
	        if (destBox.X < 0)
	        {
		        destBox.X = 0;
	        }
	        if (destBox.Y < 0)
	        {
		        destBox.Y = 0;
	        }
	        if (destBox.Width < 0)
	        {
		        destBox.Width = 0;
	        }
	        if (destBox.Height < 0)
	        {
		        destBox.Height = 0;
	        }

	        if (destBox.X >= Settings.Width)
	        {
		        destBox.X = Settings.Width - 1;
	        }
	        if (destBox.Y >= Settings.Height)
	        {
		        destBox.Y = Settings.Height - 1;
	        }
	        if (destBox.Z >= Settings.Depth)
	        {
		        destBox.Z = Settings.Depth - 1;
	        }

	        if (destBox.Right >= Settings.Width)
	        {
		        destBox.Width = Settings.Width - destBox.X - 1;
	        }
	        if (destBox.Bottom >= Settings.Height)
	        {
		        destBox.Height = Settings.Height - destBox.Y - 1;
	        }
	        if (destBox.Back >= Settings.Depth)
	        {
		        destBox.Width = Settings.Depth - destBox.Z - 1;
	        }

	        var box = new SharpDX.DataBox
			{
				DataPointer = data.Data.PositionPointer,
				RowPitch = data.RowPitch,
				SlicePitch = data.SlicePitch
			};

			var region = new D3D.ResourceRegion
			{
				Front = destBox.Front,
				Back = destBox.Back,
				Top = destBox.Top,
				Left = destBox.Left,
				Bottom = destBox.Bottom,
				Right = destBox.Right,
			};

            if (deferred == null)
            {
                deferred = Graphics;
            }

			deferred.Context.UpdateSubresource(box, D3DResource, subResource, region);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture3D"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this texture.</param>
		/// <param name="name">The name of the texture.</param>
		/// <param name="settings">Settings to pass to the texture.</param>
		internal GorgonTexture3D(GorgonGraphics graphics, string name, ITextureSettings settings)
			: base(graphics, name, settings)
		{
			// TODO: This is really kind of a hack.  I need to find a better way to make specific settings show up for their respective types.
			Settings = settings as GorgonTexture3DSettings ?? new GorgonTexture3DSettings
			{
				Width = settings.Width,
				Height = settings.Height,
				Depth =  settings.Depth,
				AllowUnorderedAccessViews = settings.AllowUnorderedAccessViews,
				Format = settings.Format,
				MipCount = settings.MipCount,
				ShaderViewFormat = settings.ShaderViewFormat,
				Usage = settings.Usage
			};
		}
		#endregion
	}
}