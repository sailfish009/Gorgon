﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: July 5, 2017 2:44:43 PM
// 
#endregion

using System;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Provides the necessary information required to set up a generic unstructured buffer.
    /// </summary>
    public class GorgonBufferInfo
        : IGorgonBufferInfo
    {
        #region Properties.
        /// <summary>
        /// Property to return the intended usage for binding to the GPU.
        /// </summary>
        public D3D11.ResourceUsage Usage
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the size of the buffer, in bytes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value should be larger than 0, or else an exception will be thrown when the buffer is created.
        /// </para>
        /// <para>
        /// This value should also be a multiple of 16.
        /// </para>
        /// </remarks>
        public int SizeInBytes
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the type of binding for the GPU.
        /// </summary>
        /// <remarks>
        /// The type of binding should be used to determine what type of view to apply to the buffer when accessing it from shaders. This will also help determine how data will be interpreted.
        /// </remarks>
        public BufferBinding Binding
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the format for the default shader view.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Use this to define a default <see cref="GorgonBufferView"/> for the buffer. This default view will allow shaders to access the buffer without needing to create an additional view. If this 
        /// value is set to <c>Unknown</c>, then no default shader view will be created.
        /// </para>
        /// <para>
        /// The default shader view will expose the entire buffer to the shader. To limit view to only a portion of the buffer, call the <see cref="GorgonBuffer.GetShaderResourceView"/> with the appropriate 
        /// element constraints.
        /// </para>
        /// <para>
        /// The format must not be typeless, if it is, an exception will be thrown on buffer creation.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// This property is only used if the <see cref="IGorgonBufferInfo.Binding"/> property has a <see cref="BufferBinding.Shader"/> flag.
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// The default value for this property is <c>Unknown</c>.
        /// </para>
        /// </remarks>
        public DXGI.Format DefaultShaderViewFormat
        {
            get;
            set;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBufferInfo"/> class.
        /// </summary>
        /// <param name="bufferInfo">The buffer information to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="bufferInfo"/> parameter is <b>null</b>.</exception>
        public GorgonBufferInfo(IGorgonBufferInfo bufferInfo)
        {
            if (bufferInfo == null)
            {
                throw new ArgumentNullException(nameof(bufferInfo));
            }

            Usage = bufferInfo.Usage;
            SizeInBytes = bufferInfo.SizeInBytes;
            Binding = bufferInfo.Binding;
            DefaultShaderViewFormat = bufferInfo.DefaultShaderViewFormat;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBufferInfo"/> class.
        /// </summary>
        public GorgonBufferInfo()
        {
            Usage = D3D11.ResourceUsage.Default;
            DefaultShaderViewFormat = DXGI.Format.Unknown;
            Binding = BufferBinding.Shader;
        }
        #endregion
    }
}