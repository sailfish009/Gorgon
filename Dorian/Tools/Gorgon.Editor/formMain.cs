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
// Created: Monday, April 30, 2012 6:28:32 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using KRBTabControl;
using Aga.Controls.Tree;
using GorgonLibrary.FileSystem;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.UI;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Main application object.
	/// </summary>
	public partial class formMain
		: ZuneForm
	{
		#region Classes.
		/// <summary>
		/// Sorter for our file tree nodes.
		/// </summary>
		class FileNodeComparer
			: IComparer
		{
			#region IComparer Members
			/// <summary>
			/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
			/// </summary>
			/// <param name="x">The first object to compare.</param>
			/// <param name="y">The second object to compare.</param>
			/// <returns>
			/// A signed integer that indicates the relative values of <paramref name="x"/> and <paramref name="y"/>, as shown in the following table.Value Meaning Less than zero <paramref name="x"/> is less than <paramref name="y"/>. Zero <paramref name="x"/> equals <paramref name="y"/>. Greater than zero <paramref name="x"/> is greater than <paramref name="y"/>.
			/// </returns>
			/// <exception cref="T:System.ArgumentException">Neither <paramref name="x"/> nor <paramref name="y"/> implements the <see cref="T:System.IComparable"/> interface.-or- <paramref name="x"/> and <paramref name="y"/> are of different types and neither one can handle comparisons with the other. </exception>
			public int Compare(object x, object y)
			{
				Node left = (Node)x;
				Node right = (Node)y;
				
/*				if ((left.Tag is ProjectFolder) && (right.Tag is Document))
					return -1;

				if ((left.Tag is Document) && (right.Tag is ProjectFolder))
					return 1;

				if (((left.Tag is Document) && (right.Tag is Document)) || ((left.Tag is ProjectFolder) && (right.Tag is ProjectFolder)))
					return string.Compare(left.Text, right.Text);*/

				return 0;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private SortedTreeModel _treeModel = null;							// Tree model.
		private Font _unSavedFont = null;									// Font for unsaved documents.
		private bool _wasSaved = false;										// Flag to indicate that the project was previously saved.
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Click event of the itemExit control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void itemExit_Click(object sender, EventArgs e)
		{
			try
			{				
				Close();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the Click event of the itemResetValue control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void itemResetValue_Click(object sender, EventArgs e)
		{
			try
			{
				if ((propertyItem.SelectedObject == null) || (propertyItem.SelectedGridItem == null))
					return;

				propertyItem.SelectedGridItem.PropertyDescriptor.ResetValue(propertyItem.SelectedObject);
				propertyItem.Refresh();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
		}

		/// <summary>
		/// Handles the Opening event of the popupProperties control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
		private void popupProperties_Opening(object sender, CancelEventArgs e)
		{
			if ((propertyItem.SelectedObject == null) || (propertyItem.SelectedGridItem == null))
			{
				itemResetValue.Enabled = false;
				return;
			}

			itemResetValue.Enabled = (propertyItem.SelectedGridItem.PropertyDescriptor.CanResetValue(propertyItem.SelectedObject));
		}

		/// <summary>
		/// Function to validate the controls on the form.
		/// </summary>
		private void ValidateControls()
		{
		}

		/// <summary>
		/// Function to load content into the interface.
		/// </summary>
		/// <param name="content">Content to load into the interface.</param>
		internal void LoadContentPane<T>()
			where T : ContentObject, new()
		{
			ContentObject result = null;

			if (Program.CurrentContent != null)
			{
				if (!Program.CurrentContent.Close())
				{
					return;
				}
			}

			// Load the content.
			result = new T();
			Control control = result.InitializeContent();

			// If we fail to return a control, then return to the default.
			if (control == null)
			{
				result.Dispose();
				result = null;
				
				result = new DefaultContent();
				control = result.InitializeContent();
			}
						
			control.Dock = DockStyle.Fill;
			
			// Add to our interface.
			splitEdit.Panel1.Controls.Add(control);
			
			Program.CurrentContent = result;

			// If the current content has a renderer, then activate it.
			// Otherwise, turn it off to conserve cycles.
			if (result.HasRenderer)
			{
				Gorgon.ApplicationIdleLoopMethod = Idle;
			}
			else
			{
				Gorgon.ApplicationIdleLoopMethod = null;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"/> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			try
			{
				if ((Program.CurrentContent != null) && (Program.CurrentContent.HasChanges))
				{
					ConfirmationResult result = ConfirmationResult.None;

					result = GorgonDialogs.ConfirmBox(this, "The " + Program.CurrentContent.ContentType + " '" + Program.CurrentContent.Name + "' has changes.  Would you like to save it?", true, false);

					if (result == ConfirmationResult.Yes)
					{
						// TODO:
						// Program.CurrentContent.Save();
					}

					if (result == ConfirmationResult.Cancel)
					{
						e.Cancel = true;
						return;
					}

					// Destroy the current content.
					Program.CurrentContent.Dispose();
					Program.CurrentContent = null;
				}
				
				if (_unSavedFont != null)
				{
					_unSavedFont.Dispose();
					_unSavedFont = null;
				}

				_nodeText.DrawText -= new EventHandler<Aga.Controls.Tree.NodeControls.DrawEventArgs>(_nodeText_DrawText);

				if (this.WindowState != FormWindowState.Minimized)
				{
					Program.Settings.FormState = this.WindowState;
				}

				if (this.WindowState != FormWindowState.Normal)
				{
					Program.Settings.WindowDimensions = this.RestoreBounds;
				}
				else
				{
					Program.Settings.WindowDimensions = this.DesktopBounds;
				}

				Program.Settings.Save();
			}
#if DEBUG
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
#else
			catch
			{
				// Eat this exception if in release.
#endif
			}
		}

		/// <summary>
		/// Function for idle time.
		/// </summary>
		/// <returns>TRUE to continue, FALSE to exit.</returns>
		private bool Idle()
		{
			Program.CurrentContent.Draw();

			return true;
		}

		/// <summary>
		/// Handles the DrawText event of the _nodeText control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="Aga.Controls.Tree.NodeControls.DrawEventArgs"/> instance containing the event data.</param>
		private void _nodeText_DrawText(object sender, Aga.Controls.Tree.NodeControls.DrawEventArgs e)
		{

/*			e.TextColor = Color.White;

			if (document != null)
			{
				if (!document.CanOpen)
					e.TextColor = Color.Black;

				if ((document.NeedsSave) && (document.CanSave))
					e.Font = _unSavedFont;
			}*/
			
		}

		/// <summary>
		/// Function to initialize the files tree.
		/// </summary>
		private void InitializeTree()
		{
			/*_nodeText.DrawText += new EventHandler<Aga.Controls.Tree.NodeControls.DrawEventArgs>(_nodeText_DrawText);
			_treeModel = new SortedTreeModel(new TreeModel());
			_treeModel.Comparer = new FileNodeComparer();
			treeFiles.Model = _treeModel;

			treeFiles.BeginUpdate();
			((TreeModel)_treeModel.InnerModel).Nodes.Add(Program.Project.RootNode);
			treeFiles.EndUpdate();

			treeFiles.Root.Children[0].Expand();*/
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				ToolStripManager.Renderer = new DarkFormsRenderer();

				this.Location = Program.Settings.WindowDimensions.Location;
				this.Size = Program.Settings.WindowDimensions.Size;

				// If this window can't be placed on a monitor, then shift it to the primary.
				if (!Screen.AllScreens.Any(item => item.Bounds.Contains(this.Location)))
				{
					this.Location = Screen.PrimaryScreen.Bounds.Location;
				}

				this.WindowState = Program.Settings.FormState;

				InitializeTree();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				ValidateControls();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formMain"/> class.
		/// </summary>
		public formMain()
		{
			InitializeComponent();

			// Force the splitter width to stay at 4 pixels!
			splitEdit.SplitterWidth = 4;

			_unSavedFont = new Font(this.Font, FontStyle.Bold);
		}
		#endregion
	}
}