﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Tuesday, June 2, 2015 9:56:56 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.IO.Properties;
using Gorgon.Plugins;

namespace Gorgon.IO.Providers
{
	/// <inheritdoc cref="IGorgonFileSystemProviderFactory"/>
	public sealed class GorgonFileSystemProviderFactory 
		: IGorgonFileSystemProviderFactory
	{
		#region Variables.
		// A plugin service where instances of the provider plugins can be found.
		private readonly IGorgonPluginService _pluginService;
		// The application log file.
		private readonly IGorgonLog _log = new GorgonLogDummy();
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve all the file system providers from the plugin service.
		/// </summary>
		/// <returns>A list of file system providers.</returns>
		private IReadOnlyList<GorgonFileSystemProvider> GetAllProviders()
		{
			return _pluginService.GetPlugins<GorgonFileSystemProvider>()
			                     .ToArray();
		}

		/// <inheritdoc/>
		public GorgonFileSystemProvider CreateProvider(string providerPluginName)
		{
			if (providerPluginName == null)
			{
				throw new ArgumentNullException(nameof(providerPluginName));
			}

			if (string.IsNullOrWhiteSpace(providerPluginName))
			{
				throw new ArgumentException(Resources.GORFS_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(providerPluginName));
			}

			_log.Print("Creating file system provider '{0}'.", LoggingLevel.Simple, providerPluginName);

			GorgonFileSystemProvider plugin =  _pluginService.GetPlugin<GorgonFileSystemProvider>(providerPluginName);

			if (plugin == null)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORFS_ERR_NO_PROVIDER_PLUGIN, providerPluginName));
			}

			return plugin;
		}

		/// <inheritdoc/>
		public IReadOnlyList<GorgonFileSystemProvider> CreateProviders(AssemblyName pluginAssembly = null)
		{
			return pluginAssembly == null
				       ? GetAllProviders()
				       : _pluginService.GetPlugins<GorgonFileSystemProvider>(pluginAssembly)
									   .ToArray();
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemProviderFactory"/> class.
		/// </summary>
		/// <param name="pluginService">The plugin service used to retrieve file system provider plugins.</param>
		/// <param name="log">[Optional] The application log file.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="pluginService"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public GorgonFileSystemProviderFactory(IGorgonPluginService pluginService, IGorgonLog log = null)
		{
			if (pluginService == null)
			{
				throw new ArgumentNullException(nameof(pluginService));
			}

			if (log != null)
			{
				_log = log;
			}

			_pluginService = pluginService;
		}
		#endregion
	}
}
