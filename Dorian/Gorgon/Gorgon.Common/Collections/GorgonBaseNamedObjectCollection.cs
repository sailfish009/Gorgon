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
// Created: Tuesday, June 14, 2011 10:11:18 PM
// 
#endregion

using System;
using System.Collections.Generic;

namespace GorgonLibrary.Collections
{
	/// <summary>
	/// Base collection for Gorgon library named objects.
	/// </summary>
	/// <typeparam name="T">Type of object, must implement <see cref="GorgonLibrary.INamedObject">INamedObject</see>.</typeparam>
	public abstract class GorgonBaseNamedObjectCollection<T>
		: IList<T>
		where T : INamedObject
	{
		#region Variables.
		private SortedList<string, T> _list = null;				// Internal collection to hold our objects.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of items in the underlying collection.
		/// </summary>
		protected IDictionary<string, T> Items
		{
			get
			{
				return _list;
			}
		}

		/// <summary>
		/// Property to return whether the keys are case sensitive.
		/// </summary>
		public bool KeysAreCaseSensitive
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the item at the specified index.
		/// </summary>
		/// <param name="index">Index of the item to retrieve.</param>
		/// <returns>The item at the specified index.</returns>
		protected virtual T GetItem(int index)
		{			
			return _list.Values[index];
		}

		/// <summary>
		/// Function to add an item to the collection.
		/// </summary>
		/// <param name="value">Value to add.</param>
		protected virtual void AddItem(T value)
		{			
			if (Contains(value.Name))
				throw new ArgumentException("The item with the name '" + value.Name + "' already exists in this collection.");

			if (string.IsNullOrEmpty(value.Name))
				throw new ArgumentException("The name for this item is empty.");

			if (!KeysAreCaseSensitive)
				_list.Add(value.Name.ToLower(), value);
			else
				_list.Add(value.Name, value);
		}

		/// <summary>
		/// Function to add several items to the list.
		/// </summary>
		/// <param name="items">IEnumerable containing the items to copy.</param>
		protected virtual void AddItems(IEnumerable<T> items)
		{
			foreach (T item in items)
				AddItem(item);
		}

		/// <summary>
		/// Function to retrieve an item with the specified name.
		/// </summary>
		/// <param name="name">Name of the item to retrieve.</param>
		/// <returns>Item with the specified key.</returns>
		protected virtual T GetItem(string name)
		{
			if (!KeysAreCaseSensitive)
				return _list[name.ToLower()];
			else
				return _list[name];
		}

		/// <summary>
		/// Function to set an item with the specified name.
		/// </summary>
		/// <param name="name">Name of the item to set.</param>
		/// <param name="value">Value to set to the item.</param>
		protected virtual void SetItem(string name, T value)
		{
			RemoveItem(name);
			AddItem(value);
		}

		/// <summary>
		/// Function to remove an item from the collection.
		/// </summary>
		/// <param name="index">The index of the item to remove.</param>
		protected virtual void RemoveItem(int index)
		{
			_list.RemoveAt(index);
		}

		/// <summary>
		/// Function to remove an item from the collection.
		/// </summary>
		/// <param name="name">Name of the item to remove.</param>
		protected virtual void RemoveItem(string name)
		{
			if (!KeysAreCaseSensitive)
				_list.Remove(name.ToLower());
			else
				_list.Remove(name);
		}

		/// <summary>
		/// Function to remove an item from the collection.
		/// </summary>
		/// <param name="item">Item to remove.</param>
		protected virtual void RemoveItem(T item)
		{
			RemoveItem(item.Name);
		}

		/// <summary>
		/// Function to remove all the items from the collection.
		/// </summary>
		protected virtual void ClearItems()
		{
			_list.Clear();
		}

		/// <summary>
		/// Function to return whether an item with the specified name exists in this collection.
		/// </summary>
		/// <param name="name">Name of the item to find.</param>
		/// <returns>TRUE if found, FALSE if not.</returns>
		public virtual bool Contains(string name)
		{
			if (!KeysAreCaseSensitive)
				return _list.ContainsKey(name.ToLower());
			else
				return _list.ContainsKey(name);
		}

		/// <summary>
		/// Function to copy the contents of the collection to an array.
		/// </summary>
		/// <returns>Array containing the contents of this collection.</returns>
		public T[] ToArray()
		{
			T[] array = new T[Count];
			CopyTo(array, 0);
			
			return array;
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBaseNamedObjectCollection&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="caseSensitive">TRUE if the key names are case sensitive, FALSE if not.</param>
		protected GorgonBaseNamedObjectCollection(bool caseSensitive)
		{
			_list = new SortedList<string, T>();
			KeysAreCaseSensitive = caseSensitive;
		}
		#endregion

		#region IList<T> Members
		/// <summary>
		/// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
		/// <returns>
		/// The index of <paramref name="item"/> if found in the list; otherwise, -1.
		/// </returns>
		public virtual int IndexOf(T item)
		{
			return _list.IndexOfValue(item);
		}

		/// <summary>
		/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
		/// </exception>
		void IList<T>.Insert(int index, T item)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
		/// </exception>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
		/// </exception>
		void IList<T>.RemoveAt(int index)
		{
			if (IsReadOnly)
				throw new NotSupportedException("List is read-only.");
			RemoveItem(index);
		}

		/// <summary>
		/// Property to set or return the item at the specified index.
		/// </summary>
		T IList<T>.this[int index]
		{
			get
			{
				return GetItem(index);
			}
			set
			{
				if (IsReadOnly)
					throw new NotSupportedException("List is read-only.");

				SetItem(GetItem(index).Name, value);
			}
		}
		#endregion

		#region ICollection<T> Members
		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </exception>
		void ICollection<T>.Add(T item)
		{
			if (IsReadOnly)
				throw new NotSupportedException("List is read-only.");
			AddItem(item);
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </exception>
		void ICollection<T>.Clear()
		{
			if (IsReadOnly)
				throw new NotSupportedException("List is read-only.");
			ClearItems();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
		/// </returns>
		public bool Contains(T item)
		{
			return _list.ContainsValue(item);
		}

		/// <summary>
		/// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="array"/> is null.
		/// </exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="arrayIndex"/> is less than 0.
		/// </exception>
		/// <exception cref="T:System.ArgumentException">
		/// 	<paramref name="array"/> is multidimensional.
		/// -or-
		/// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
		/// -or-
		/// The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
		/// -or-
		/// Type <paramref name="array"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.
		/// </exception>
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (Count == 0)
				return;

			_list.Values.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </returns>
		public int Count
		{
			get 
			{
				return _list.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </summary>
		/// <value></value>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
		/// </returns>
		public virtual bool IsReadOnly
		{
			get 
			{
				return false;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </exception>
		bool ICollection<T>.Remove(T item)
		{
			if (IsReadOnly)
				throw new NotSupportedException("List is read-only.");

			RemoveItem(item);
			return true;
		}
		#endregion

		#region IEnumerable<T> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public virtual IEnumerator<T> GetEnumerator()
		{
			foreach (KeyValuePair<string, T> item in _list)
				yield return item.Value;
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion
	}
}