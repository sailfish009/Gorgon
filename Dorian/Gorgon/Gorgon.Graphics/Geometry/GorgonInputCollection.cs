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
// Created: Thursday, November 24, 2011 3:38:16 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Collections;
using GorgonLibrary.Math;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Defines the layout of an item in a buffer.
	/// </summary>
	/// <remarks>This is a collection of input elements used to describe the layout of an input object.  The user can create this by hand using explicit element types, or 
	/// by passing the type of the input object to the <see cref="GorgonLibrary.Graphics.GorgonInputLayout.GetLayoutFromType">GetLayoutFromType</see> method.
	/// </remarks>
	public class GorgonInputLayout
		: GorgonBaseNamedObjectList<GorgonInputElement>, INotifier
	{
		#region Variables.
		private IDictionary<int, int> _slotSizes = null;		// List of slot sizes.
		private bool _isUpdated = false;						// Flag to indicate that the input was updated.
		private Type[] _allowedTypes = null;					// Types allowed when pulling information from an object.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether the list has been updated.
		/// </summary>
		public bool HasChanged
		{
			get
			{
				return _isUpdated;
			}
			set
			{
				_isUpdated = value;

				// Recalculate the size of the element.
				if (value)
					UpdateVertexSize();
			}
		}

		/// <summary>
		/// Property to return the input object size in bytes.
		/// </summary>
		public int Size
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return an element by index.
		/// </summary>
		public GorgonInputElement this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function used to determine the type of a field/property from its type.
		/// </summary>
		/// <param name="type">Type to use when evaluating.</param>
		/// <returns>The element format.</returns>
		private GorgonBufferFormat GetElementType(Type type)
		{
			if (type == typeof(byte))
				return GorgonBufferFormat.R8_UInt;
			if (type == typeof(SByte))
				return GorgonBufferFormat.R8_Int;

			if (type == typeof(Int32))
				return GorgonBufferFormat.R32_Int;
			if (type == typeof(UInt32))
				return GorgonBufferFormat.R32_UInt;

			if (type == typeof(Int16))
				return GorgonBufferFormat.R16_Int;
			if (type == typeof(UInt16))
				return GorgonBufferFormat.R16_UInt;

			if (type == typeof(Int64))
				return GorgonBufferFormat.R32G32_Int;
			if (type == typeof(UInt64))
				return GorgonBufferFormat.R32G32_UInt;

			if (type == typeof(float))
				return GorgonBufferFormat.R32_Float;
			if (type == typeof(Vector2D))
				return GorgonBufferFormat.R32G32_Float;
			if (type == typeof(Vector3D))
				return GorgonBufferFormat.R32G32B32_Float;				
			if (type == typeof(Vector4D))
				return GorgonBufferFormat.R32G32B32A32_Float;

			return GorgonBufferFormat.Unknown;
		}		

		/// <summary>
		/// Function to retrieve the size, in bytes, of the input object.
		/// </summary>
		private void UpdateVertexSize()
		{
			Size = this.Sum(item => item.Size);

			_slotSizes = new Dictionary<int, int>();

			var slotSizing = from slot in this
							   group slot by slot.Slot;

			foreach (var slotGroup in slotSizing)
				_slotSizes[slotGroup.Key] = slotGroup.Sum(item => item.Size);
		}

		/// <summary>
		/// Function to normalize the offsets in the element list.
		/// </summary>
		public void NormalizeOffsets()
		{
			int lastOffset = 0;

			foreach (var element in this)
			{
				element.Offset = lastOffset;
				lastOffset += element.Size;
			}

			HasChanged = true;
		}

		/// <summary>
		/// Property to return the size of the elements for a given slot in an input element.
		/// </summary>
		/// <param name="slot">Slot to count.</param>
		/// <returns>The size of the elements in the slot, in bytes.</returns>
		public int GetSlotSize(int slot)
		{
			if (HasChanged)
				UpdateVertexSize();

			return _slotSizes[slot];
		}

		/// <summary>
		/// Function to add an input element to the list.
		/// </summary>
		/// <param name="element">Element to add.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="element"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown if the element format is not supported.
		/// <para>-or-</para>
		/// <para>Thrown if the element offset or element context is in use and the element index is the same.</para>
		/// <para>-or-</para>
		/// <para>Thrown is the element slot is less than 0 or greater than 15.</para>
		/// </exception>
		/// <remarks>See the <see cref="GorgonLibrary.Graphics.GorgonInputElement">GorgonInputElement</see> type for details on the various parameters.</remarks>
		public void Add(GorgonInputElement element)
		{
			GorgonDebug.AssertNull<GorgonInputElement>(element, "element");

			if (GorgonBufferFormatInfo.GetInfo(element.Format).BitDepth == 0)
				throw new ArgumentException("'" + element.Format.ToString() + "' is not a supported format.", "format");

			if ((element.Slot < 0) || (element.Slot > 15))
				throw new ArgumentException("The value must be from 0 to 15.", "slot");

			if (this.Count(elementItem => ((element.Offset == elementItem.Offset) || (string.Compare(element.Context, elementItem.Context, true) == 0)) && element.Index == elementItem.Index && element.Slot == elementItem.Slot) > 0)
				throw new ArgumentException("The offset '" + element.Offset + "' or context '" + element.Context + "' is in use by another item with the same index or slot.", "context, offset");

			AddItem(element);

			HasChanged = true;
		}

		/// <summary>
		/// Function to add an input element to the list.
		/// </summary>
		/// <param name="context">Context of the element.</param>
		/// <param name="format">Format of the element.</param>
		/// <param name="offset">Offset in bytes of the element.</param>
		/// <param name="index">Index of the element.</param>
		/// <param name="slot">Vertex buffer slot for the element.</param>
		/// <param name="instanced">TRUE if this element is instanced, FALSE if not.</param>
		/// <param name="instanceCount">If <paramref name="instanced"/> is TRUE, then the number of instances.  If FALSE, this parameter must be 0.</param>
		/// <returns>A new input element.</returns>
		/// <remarks>See the <see cref="GorgonLibrary.Graphics.GorgonInputElement">GorgonInputElement</see> type for details on the various parameters.</remarks>
		/// <exception cref="System.ArgumentException">Thrown if the <paramref name="format"/> is not supported.
		/// <para>-or-</para>
		/// <para>Thrown if the <paramref name="offset"/> or <paramref name="context"/> is in use and the <paramref name="index"/> is the same.</para>
		/// <para>-or-</para>
		/// <para>Thrown is the <paramref name="slot"/> parameter is less than 0 or greater than 15.</para>
		/// </exception>
		public GorgonInputElement Add(string context, GorgonBufferFormat format, int? offset, int index, int slot, bool instanced, int instanceCount)
		{
			// Find the highest offset and increment by the last element size.
			if (offset == null) 
			{
				if (this.Count > 0)
				{
					var lastElement = (from elementItem in this
									   orderby elementItem.Offset descending
									   select elementItem).Single();

					offset = lastElement.Offset + lastElement.Size;
				}
				else
					offset = 0;		
			}

			// Check for existing context at the same index.
			GorgonInputElement element = new GorgonInputElement(context, format, offset.Value, index, slot, instanced, (instanced ? instanceCount : 0));

			Add(element);

			return element;
		}

		/// <summary>
		/// Function to add an input element to the list.
		/// </summary>
		/// <param name="context">Context of the element.</param>
		/// <param name="format">Format of the element.</param>
		/// <param name="offset">Offset in bytes of the element.</param>
		/// <param name="index">Index of the element.</param>
		/// <param name="slot">Vertex buffer slot for the element.</param>
		/// <returns>A new input element.</returns>
		/// <remarks>See the <see cref="GorgonLibrary.Graphics.GorgonInputElement">GorgonInputElement</see> type for details on the various parameters.</remarks>
		/// <exception cref="System.ArgumentException">Thrown if the <paramref name="format"/> is not supported.
		/// <para>-or-</para>
		/// <para>Thrown if the <paramref name="offset"/> is in use and the <paramref name="index"/> is the same.</para>
		/// <para>-or-</para>
		/// <para>Thrown is the <paramref name="slot"/> parameter is less than 0 or greater than 15.</para>
		/// </exception>
		public GorgonInputElement Add(string context, GorgonBufferFormat format, int? offset, int index, int slot)
		{
			return Add(context, format, offset, index, slot, false, 0);
		}

		/// <summary>
		/// Function to add an input element to the list.
		/// </summary>
		/// <param name="context">Context of the element.</param>
		/// <param name="format">Format of the element.</param>
		/// <param name="offset">Offset in bytes of the element.</param>
		/// <param name="index">Index of the element.</param>
		/// <returns>A new input element.</returns>
		/// <remarks>See the <see cref="GorgonLibrary.Graphics.GorgonInputElement">GorgonInputElement</see> type for details on the various parameters.</remarks>
		/// <exception cref="System.ArgumentException">Thrown if the <paramref name="format"/> is not supported.
		/// <para>-or-</para>
		/// <para>Thrown if the <paramref name="offset"/> is in use and the <paramref name="index"/> is the same.</para>
		/// </exception>
		public GorgonInputElement Add(string context, GorgonBufferFormat format, int? offset, int index)
		{
			return Add(context, format, offset, index, 0, false, 0);
		}

		/// <summary>
		/// Function to add an input element to the list.
		/// </summary>
		/// <param name="context">Context of the element.</param>
		/// <param name="format">Format of the element.</param>
		/// <param name="offset">Offset in bytes of the element.</param>
		/// <returns>A new input element.</returns>
		/// <remarks>See the <see cref="GorgonLibrary.Graphics.GorgonInputElement">GorgonInputElement</see> type for details on the various parameters.</remarks>
		/// <exception cref="System.ArgumentException">Thrown if the <paramref name="format"/> is not supported.
		/// </exception>
		public GorgonInputElement Add(string context, GorgonBufferFormat format, int? offset)
		{
			return Add(context, format, offset, 0, 0, false, 0);
		}

		/// <summary>
		/// Function to remove an input element by index.
		/// </summary>
		/// <param name="index">Index of the element to remove.</param>
		public void Remove(int index)
		{
			RemoveItem(index);
			HasChanged = true;
		}

		/// <summary>
		/// Function to clear the layout.
		/// </summary>
		public void Clear()
		{
			ClearItems();
			HasChanged = true;
		}

		/// <summary>
		/// Function to retrieve the input layout from a specific type.
		/// </summary>
		/// <param name="type">Type of retrieve layout info from.</param>
		/// <remarks>Use this to create an input element layout from a type.  Properties and fields in this type must be marked with the <see cref="GorgonLibrary.Graphics.VertexElementAttribute">GorgonInputElementAttribute</see> in order for the element list to consider it and those fields or properties must be public.
		/// <para>Fields/properties marked with the attribute must be either a (u)byte, (u)short, (u)int, (u)long, float or one of the Vector2/3/4D types.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="type"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown if a field/property type cannot be mapped to a <see cref="E:GorgonLibrary.Graphics.GorgonBufferFormat">GorgonBufferFormat</see>.</exception>
		public void GetLayoutFromType(Type type)
		{
			GorgonInputElement element = null;
			IList<MemberInfo> members = type.GetMembers();
			int byteOffset = 0;

			if (type == null)
				throw new ArgumentNullException("type");

			// Get fields.
			Clear();

			// Get only properties and fields, and sort by explicit ordering (then by offset).
			var propertiesAndFields = from member in members
									  let memberAttribute = member.GetCustomAttributes(typeof(InputElementAttribute), true) as IList<InputElementAttribute>
									  where ((member.MemberType == MemberTypes.Property) || (member.MemberType == MemberTypes.Field)) && ((memberAttribute != null) && (memberAttribute.Count > 0))
									  orderby memberAttribute[0].ExplicitOrder, memberAttribute[0].Offset
									  select new { Name = member.Name, ReturnType = (member is FieldInfo ? ((FieldInfo)member).FieldType : ((PropertyInfo)member).PropertyType), Attribute = memberAttribute[0] };

			foreach (var item in propertiesAndFields)
			{
				GorgonBufferFormat format = item.Attribute.Format;
				string contextName = item.Attribute.Context;

				if (!_allowedTypes.Contains(item.ReturnType))
					throw new GorgonException(GorgonResult.CannotCreate, "The type '" + item.ReturnType.FullName.ToString() + "' is not a valid type for an input element.");

				// Try to determine the format from the type.
				if (format == GorgonBufferFormat.Unknown)
				{
					format = GetElementType(item.ReturnType);
					if (format == GorgonBufferFormat.Unknown)
						throw new GorgonException(GorgonResult.CannotCreate, "The type '" + item.ReturnType.FullName.ToString() + "' for the property/field '" + item.Name + "' cannot be mapped to a GorgonBufferFormat.");
				}

				// Determine the context name from the field name.
				if (string.IsNullOrEmpty(contextName))
					contextName = item.Name;

				element = Add(contextName, format, (item.Attribute.AutoOffset ? byteOffset : item.Attribute.Offset), item.Attribute.Index, item.Attribute.Slot, item.Attribute.Instanced, item.Attribute.InstanceCount);
				byteOffset += element.Size;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputLayout"/> class.
		/// </summary>
		public GorgonInputLayout()			
		{
			_allowedTypes = new [] {		
									typeof(SByte),
									typeof(byte),
									typeof(ushort),
									typeof(short),
									typeof(int),
									typeof(uint),
									typeof(long),
									typeof(ulong),
									typeof(float),
									typeof(Vector2D),
									typeof(Vector3D),
									typeof(Vector4D)
								};
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputLayout"/> class.
		/// </summary>
		/// <param name="type">The type to parse for the layout.</param>
		public GorgonInputLayout(Type type)
			: this()
		{
			GetLayoutFromType(type);
		}
		#endregion
	}
}