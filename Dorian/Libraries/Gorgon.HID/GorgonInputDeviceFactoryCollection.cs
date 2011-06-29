﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Monday, June 27, 2011 10:01:20 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Collections;

namespace GorgonLibrary.HID
{
	/// <summary>
	/// A collection of HID factories.
	/// </summary>
	/// <remarks>This collection is not case sensitive.</remarks>
	public class GorgonInputDeviceFactoryCollection
		: GorgonBaseNamedObjectCollection<GorgonInputDeviceFactory>
	{		
		#region Properties.
		/// <summary>
		/// Property to return a input device factory from the collection by index.
		/// </summary>
		public GorgonInputDeviceFactory this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return a input device factory from the collection by its name.
		/// </summary>
		public GorgonInputDeviceFactory this[string name]
		{
			get
			{
				return GetItem(name);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a input device factory manager to the collection.
		/// </summary>
		/// <param name="deviceManager">input device factory manager to add.</param>
		internal void Add(GorgonInputDeviceFactory deviceManager)
		{
			if (deviceManager == null)
				throw new ArgumentNullException("deviceManager");

			AddItem(deviceManager);
		}

		/// <summary>
		/// Function to remove a input device factory from the collection.
		/// </summary>
		/// <param name="deviceManager">input device factory to remove.</param>
		internal void Remove(GorgonInputDeviceFactory deviceManager)
		{
			if (deviceManager == null)
				throw new ArgumentNullException("deviceManager");

			RemoveItem(deviceManager);
		}

		/// <summary>
		/// Function to remove all HID factories from the collection.
		/// </summary>
		internal void Clear()
		{
			ClearItems();
		}		
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputDeviceFactoryCollection"/> class.
		/// </summary>
		internal GorgonInputDeviceFactoryCollection()
			: base(false)
		{
		}
		#endregion
	}
}