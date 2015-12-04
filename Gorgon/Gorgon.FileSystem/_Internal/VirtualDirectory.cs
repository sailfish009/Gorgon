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
// Created: Monday, June 27, 2011 8:58:26 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Collections;
using Gorgon.IO.Properties;

namespace Gorgon.IO
{
	/// <inheritdoc/>
	class VirtualDirectory
		: IGorgonVirtualDirectory
	{
		#region Properties.
		/// <summary>
		/// Property to return the name of the directory.
		/// </summary>
		public string Name
		{
			get;
		}

		/// <inheritdoc/>
		IGorgonFileSystem IGorgonVirtualDirectory.FileSystem => FileSystem;

		/// <inheritdoc cref="IGorgonVirtualDirectory.FileSystem"/>
		public GorgonFileSystem FileSystem
		{
			get;
		}

		/// <inheritdoc/>
		public GorgonFileSystemMountPoint MountPoint
		{
			get;
			set;
		}
		
		/// <inheritdoc/>
		IGorgonNamedObjectReadOnlyDictionary<IGorgonVirtualDirectory> IGorgonVirtualDirectory.Directories => Directories;

		/// <inheritdoc cref="IGorgonVirtualDirectory.Directories"/>
		public VirtualDirectoryCollection Directories
		{
			get;
		}

		/// <inheritdoc/>
		IGorgonNamedObjectReadOnlyDictionary<IGorgonVirtualFile> IGorgonVirtualDirectory.Files => Files;

		/// <inheritdoc cref="IGorgonVirtualDirectory.Files"/>
		public VirtualFileCollection Files
		{
			get;
		}

		/// <inheritdoc/>
		IGorgonVirtualDirectory IGorgonVirtualDirectory.Parent => Parent;

		/// <inheritdoc cref="IGorgonVirtualDirectory.Parent"/>
		public VirtualDirectory Parent
		{
			get;
		}

		/// <inheritdoc/>
		public string FullPath
		{
			get
			{
				if (Parent == null)
				{
					return "/";
				}

				return Parent.FullPath + Name + "/";
			}
		}
		#endregion

        #region Methods.
		/// <inheritdoc cref="IGorgonVirtualDirectory.GetParents"/>
		public IEnumerable<IGorgonVirtualDirectory> GetParents()
		{
			IGorgonVirtualDirectory parent = Parent;

			while (parent != null)
			{
				yield return parent;

				parent = parent.Parent;
			}
		}

		/// <inheritdoc/>
		public int GetDirectoryCount()
		{
			return Directories.Count == 0 ? 0 : GorgonFileSystem.FlattenDirectoryHierarchy(this, "*").Count();
		}

		/// <inheritdoc/>
		public int GetFileCount()
		{
			return GorgonFileSystem.FlattenDirectoryHierarchy(this, "*").Sum(item => item.Files.Count) + Files.Count;
		}

		/// <inheritdoc/>
		public bool ContainsFile(IGorgonVirtualFile file)
		{
			if (file == null)
			{
				throw new ArgumentNullException(nameof(file));
			}

			return ContainsFile(file.Name);
		}

		/// <inheritdoc/>
		public bool ContainsFile(string fileName)
		{
			if (fileName == null)
			{
				throw new ArgumentNullException(nameof(fileName));
			}

			if (string.IsNullOrWhiteSpace(fileName))
			{
				throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(fileName));
			}

			if (Files.Contains(fileName))
			{
				return true;
			}

			IEnumerable<VirtualDirectory> directories = GorgonFileSystem.FlattenDirectoryHierarchy(this, "*");

			return directories.Any(item => item.Files.Contains(fileName));
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="VirtualDirectory" /> class.
		/// </summary>
		/// <param name="mountPoint">The mount point that supplied this directory.</param>
		/// <param name="fileSystem">The file system that contains the directory.</param>
		/// <param name="parentDirectory">The parent of this directory.</param>
		/// <param name="name">The name of the directory.</param>
		public VirtualDirectory(GorgonFileSystemMountPoint mountPoint, GorgonFileSystem fileSystem, VirtualDirectory parentDirectory, string name)
		{
			MountPoint = mountPoint;
			FileSystem = fileSystem;
			Name = name != "/" ? name.FormatPathPart() : name;
			Parent = parentDirectory;
			Directories = new VirtualDirectoryCollection(this);
			Files = new VirtualFileCollection(this);
		}
		#endregion
	}
}