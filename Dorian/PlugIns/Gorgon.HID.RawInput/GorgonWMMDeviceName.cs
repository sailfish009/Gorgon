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
// Created: Thursday, July 14, 2011 2:16:33 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Win32;

namespace GorgonLibrary.HID.RawInput
{
	/// <summary>
	/// The WMM joystick implementation of a device name.
	/// </summary>
	internal class GorgonWMMDeviceName
		: GorgonInputDeviceName
	{
		#region Variables.
		private int _joyCapsSize = 0;			// Structure size of the JOYCAPS value type.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the ID of the device.
		/// </summary>
		public int JoystickID
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the device is connected or not.
		/// </summary>
		public override bool IsConnected
		{
			get
			{				
				JOYCAPS caps = new JOYCAPS();
				return Win32API.joyGetDevCaps(JoystickID, ref caps, _joyCapsSize) == 0;
			}
			protected set
			{				
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRawInputDeviceName"/> class.
		/// </summary>
		/// <param name="name">The device name.</param>
		/// <param name="className">Class name of the device.</param>
		/// <param name="hidPath">Human interface device path.</param>
		/// <param name="joystickID">The joystick ID.</param>
		/// <exception cref="System.ArgumentException">The handle is set to 0.</exception>
		/// <exception cref="System.ArgumentNullException">Either the name, className or hidPath are NULL or empty.</exception>
		public GorgonWMMDeviceName(string name, string className, string hidPath, int joystickID)
			: base(name, className, hidPath, false)
		{
			JoystickID = joystickID;
			_joyCapsSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(JOYCAPS));
		}
		#endregion
	}
}
