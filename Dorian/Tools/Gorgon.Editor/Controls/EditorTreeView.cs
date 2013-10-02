﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Wednesday, March 13, 2013 7:29:54 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using GorgonLibrary.Editor.Properties;
using GorgonLibrary.IO;
using GorgonLibrary.Math;

namespace GorgonLibrary.Editor
{
	/// <summary>
	/// Tree node editing state.
	/// </summary>
	enum NodeEditState
	{
		/// <summary>
		/// No state.
		/// </summary>
		None = 0,
		/// <summary>
		/// Creating a directory.
		/// </summary>
		CreateDirectory = 1,
		/// <summary>
		/// Renaming a directory.
		/// </summary>
		RenameDirectory = 2,
		/// <summary>
		/// Creating a file.
		/// </summary>
		CreateFile = 3,
		/// <summary>
		/// Renaming a file.
		/// </summary>
		RenameFile = 4
	}

	/// <summary>
	/// Extension method for the tree node collection.
	/// </summary>
	static class TreeNodeCollectionExtension
	{
		/// <summary>
		/// Function to return all nodes under this collection.
		/// </summary>
		/// <param name="nodes">Source collection.</param>
		/// <returns>An enumerator for the nodes.</returns>
		public static IEnumerable<EditorTreeNode> AllNodes(this TreeNodeCollection nodes)
		{
			foreach (EditorTreeNode node in nodes.Cast<TreeNode>().Where(nodeItem => nodeItem is EditorTreeNode))
			{
				yield return node;

				// Skip if there aren't any children.
				if (node.Nodes.Count == 0)
				{
					continue;
				}

				// Gather the children for this node.
				foreach (EditorTreeNode childNode in AllNodes(node.Nodes))
				{
					yield return childNode;
				}
			}
		}

		/// <summary>
		/// Function to add a directory to the tree.
		/// </summary>
		/// <param name="nodes">Source collection.</param>
		/// <param name="directory">Directory to add.</param>
		/// <returns>The new node.</returns>
		public static TreeNodeDirectory AddDirectory(this TreeNodeCollection nodes, GorgonFileSystemDirectory directory)
		{
			if (directory == null)
			{
				throw new ArgumentNullException("directory");
			}

            // Find check to ensure the node is unique.
		    TreeNodeDirectory result = (from node in nodes.Cast<TreeNode>()
		                                let dirNode = node as TreeNodeDirectory
		                                where dirNode != null
		                                    && string.Equals(node.Name, directory.FullPath, StringComparison.OrdinalIgnoreCase)
		                                select dirNode).FirstOrDefault();

		    if (result != null)
		    {
		        return result;
		    }

			result = new TreeNodeDirectory(directory);

			if ((directory.Directories.Count > 0) || (directory.Files.Count > 0))
			{
				// Add a dummy node to indicate that there are children.
				result.Nodes.Add("DummyNode");
			}

			nodes.Add(result);

			return result;
		}

		/// <summary>
		/// Function to add a file to the tree.
		/// </summary>
		/// <param name="nodes">Source collection.</param>
		/// <param name="file">File to add.</param>
		/// <returns>The new node.</returns>
		public static TreeNodeFile AddFile(this TreeNodeCollection nodes, GorgonFileSystemFileEntry file)
		{
			if (file == null)
			{
				throw new ArgumentNullException("file");
			}

            // Find check to ensure the node is unique.
		    TreeNodeFile result = (from node in nodes.Cast<TreeNode>()
		                           let dirNode = node as TreeNodeFile
		                           where dirNode != null
		                                 && string.Equals(node.Name, file.FullPath, StringComparison.OrdinalIgnoreCase)
		                           select dirNode).FirstOrDefault();

		    if (result != null)
		    {
		        return result;
		    }

			result = new TreeNodeFile(file);

			// TODO: Eventually file nodes may have children (associated files, like sprites are associated with an image).  
			//		 Add that code here when we need that functionality.

			nodes.Add(result);

			return result;
		}
	}

    /// <summary>
    /// Custom treeview for the editor.
    /// </summary>
    class EditorTreeView
        : TreeView
    {
        #region Variables.
        private bool _disposed;							// Flag to indicate that the object was disposed.
        private Font _openContent;						// Font used for open content items.
        private Brush _selectBrush;						// Brush used for selection background.
        private Pen _focusPen;							// Pen used for focus.
		private TextBox _renameBox;						// Text box used to rename a node.
		private EditorTreeNode _editNode;				// Node being edited.
	    private ImageAttributes _fadeAttributes;		// Attributes for faded items.	    
		private formMain _parentForm;					// The parent form for this control.
        #endregion

		#region Properties.
		/// <summary>
        /// Gets or sets the mode in which the control is drawn.
        /// </summary>
        /// <returns>One of the <see cref="T:System.Windows.Forms.TreeViewDrawMode" /> values. The default is <see cref="F:System.Windows.Forms.TreeViewDrawMode.Normal" />.</returns>
        ///   <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   </PermissionSet>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new TreeViewDrawMode DrawMode
        {
            get
            {
                return TreeViewDrawMode.OwnerDrawAll;
            }
        }
        #endregion

        #region Methods.
		/// <summary>
		/// Handles the LostFocus event of the _renameBox control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		private void _renameBox_LostFocus(object sender, EventArgs e)
		{
			HideRenameBox(false);
		}

		/// <summary>
		/// Handles the KeyDown event of the _renameBox control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		private void _renameBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Enter)
			{
				return;
			}

			HideRenameBox(false);
			e.Handled = true;
		}

	    /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.TreeView" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
					if (_fadeAttributes != null)
					{
						_fadeAttributes.Dispose();
						_fadeAttributes = null;
					}

					if (_renameBox != null)
					{
						_renameBox.KeyDown -= _renameBox_KeyDown;
						_renameBox.Dispose();
					}

                    if (_selectBrush != null)
                    {
                        _selectBrush.Dispose();
                    }

                    if (_focusPen != null)
                    {
                        _focusPen.Dispose();
                    }

                    if (_openContent != null)
                    {
                        _openContent.Dispose();
                    }
                }

                _selectBrush = null;
                _focusPen = null;
                _openContent = null;
                _disposed = true;
            }

            base.Dispose(disposing);
        }

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			var node = GetNodeAt(e.Location) as EditorTreeNode;

			if ((node != null) && (node != SelectedNode))
			{
				SelectedNode = node;
			}
		}

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.FontChanged" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            if (_openContent != null)
            {
                _openContent.Dispose();
                _openContent = null;
            }

            _openContent = new Font(Font, FontStyle.Bold);
        }
            	
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.TreeView.DrawNode" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.DrawTreeNodeEventArgs" /> that contains the event data.</param>
        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            Image currentImage;
            Image plusMinusImage;
	        var node = e.Node as EditorTreeNode;
	        Point position = e.Bounds.Location;
	        ImageAttributes attribs = null;

            if ((e.Bounds.Width == 0) || (e.Bounds.Height == 0) && ((e.Bounds.X == 0) && (e.Bounds.Y == 0)))
            {
                return;
            }

            if (node == null)
            {
                e.DrawDefault = true;
                return;
            }

	        if (_parentForm == null)
	        {
		        _parentForm = (formMain)FindForm();
	        }

            // Create graphics resources.
            if (_selectBrush == null)
            {
                _selectBrush = new SolidBrush(DarkFormsRenderer.MenuHilightBackground);
            }

            if (_focusPen == null)
            {
                _focusPen = new Pen(DarkFormsRenderer.BorderColor)
                            {
	                            DashStyle = DashStyle.DashDot
                            };
            }

			// Use parent font if no font is assigned.
            Font font = node.NodeFont ?? Font;

	        // Shift the position.
            position.X = position.X + (e.Node.Level * 16);

            if (node.IsExpanded)
            {
                currentImage = node.ExpandedImage;
                plusMinusImage = Resources.tree_expand_16x16;

                if (currentImage == null)
                {
                    currentImage = node.CollapsedImage;
                }
            }
            else
            {
                plusMinusImage = Resources.tree_collapse_16x16;
                currentImage = node.CollapsedImage;
            }

            var nodeFile = e.Node as TreeNodeFile;

			if (node.IsCut)
			{
				attribs = _fadeAttributes;
			}

            if ((ContentManagement.Current != null) && (_parentForm != null) && (nodeFile != null) && (_parentForm.CurrentOpenFile == nodeFile.File))
            {
                // Create the open content font if it's been changed or doesn't exist.
                if ((_openContent == null) 
					|| (_openContent.FontFamily.Name != font.FontFamily.Name) 
					|| (!_openContent.Size.EqualsEpsilon(font.Size)))
                {
                    if (_openContent != null)
                    {
                        _openContent.Dispose();
                    }

                    _openContent = new Font(font, FontStyle.Bold);
                }

                font = _openContent;
            }

            // Draw selection rectangle.
            if ((e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected)
            {
                e.Graphics.FillRectangle(_selectBrush, e.Bounds);
            }

            // Draw a focus rectangle only when focused, not when selected.
            if (e.State == TreeNodeStates.Focused)
            {
                var rect = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
                e.Graphics.DrawRectangle(_focusPen, rect);
            }			

            // Check for child nodes.
            position.X = position.X + 8;

            if (node.Nodes.Count > 0)
            {
                e.Graphics.DrawImage(plusMinusImage, new Rectangle(position, plusMinusImage.Size));
            }

            position.X = position.X + 16;
			
            // Draw the icon.
            e.Graphics.DrawImage(currentImage, new Rectangle(position, currentImage.Size), 0, 0, currentImage.Width, currentImage.Height, GraphicsUnit.Pixel, attribs);

            // Offset.
			if (currentImage != null)
			{
				position.X = position.X + currentImage.Width + 2;
			}

            // Do not re-draw text when in focus mode only (it looks awful).
            if ((e.State != TreeNodeStates.Focused) && (_editNode != node))
            {
                TextRenderer.DrawText(e.Graphics, node.Text, font, position, node.ForeColor);
            }
        }

		/// <summary>
		/// Processes a command key.
		/// </summary>
		/// <param name="msg">A <see cref="T:System.Windows.Forms.Message" />, passed by reference, that represents the window message to process.</param>
		/// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys" /> values that represents the key to process.</param>
		/// <returns>
		/// true if the character was processed by the control; otherwise, false.
		/// </returns>
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if ((_renameBox == null) || (!_renameBox.Visible) || (!_renameBox.Focused))
			{
				return base.ProcessCmdKey(ref msg, keyData);
			}

			switch (keyData)
			{
				case Keys.Escape:
					HideRenameBox(true);
					return true;
				case Keys.Delete:
					int selectionStart = _renameBox.SelectionStart;
					int selectionLength = _renameBox.SelectionLength == 0 ? 1 : _renameBox.SelectionLength;

					if (selectionStart < _renameBox.TextLength)
					{
						_renameBox.Text = _renameBox.Text.Remove(selectionStart, selectionLength);
						_renameBox.SelectionStart = selectionStart;
					}
					return true;
				default:
					return base.ProcessCmdKey(ref msg, keyData);
			}
		}

		/// <summary>
		/// Function to show the rename text box.
		/// </summary>
		/// <param name="node">Node to edit.</param>
		public void ShowRenameBox(EditorTreeNode node)
		{
			if (node == null)
			{
				return;
			}

			if (_renameBox == null)
			{
				_renameBox = new TextBox
				             {
					             Name = Name + "_EditBox",
					             Visible = false,
					             BorderStyle = BorderStyle.None,
					             BackColor = Color.White,
					             ForeColor = Color.Black,
					             Height = node.Bounds.Height,
					             AcceptsTab = false,
					             Anchor = AnchorStyles.Left | AnchorStyles.Right
				             };
				Controls.Add(_renameBox);
			}

			// Wipe out the background.
			using (var g = CreateGraphics())
			{
				// Create graphics resources.
				if (_selectBrush == null)
				{
					_selectBrush = new SolidBrush(DarkFormsRenderer.MenuHilightBackground);
				}

				g.FillRectangle(_selectBrush, new Rectangle(0, node.Bounds.Y, ClientSize.Width, node.Bounds.Height));
			}

			Point nodePosition = node.Bounds.Location;
			nodePosition.X += node.Level * 16 + 24;
			nodePosition.Y++;
			if (node.CollapsedImage != null)
			{
				nodePosition.X += node.CollapsedImage.Width + 2;
			}

			_renameBox.Location = nodePosition;
			_renameBox.Width = ClientSize.Width - nodePosition.X;
			_renameBox.Text = node.Text;

			var editArgs = new NodeLabelEditEventArgs(node, node.Text)
			               {
				               CancelEdit = false
			               };

			OnBeforeLabelEdit(editArgs);

			if (editArgs.CancelEdit)
			{
				return;
			}

			_editNode = node;
			_editNode.Redraw();
			_renameBox.Visible = true;
			_renameBox.Focus();

			if (node.Text.Length > 0)
			{
				_renameBox.Select(0, node.Text.Length);
			}

			_renameBox.KeyDown += _renameBox_KeyDown;
			_renameBox.LostFocus += _renameBox_LostFocus;
		}

		/// <summary>
		/// Function to hide the rename box.
		/// </summary>
		/// <param name="canceled">TRUE if the edit was canceled, FALSE if not.</param>
		public void HideRenameBox(bool canceled)
		{
			if (_renameBox == null)
			{
				return;
			}

			var editNode = _editNode;

			_renameBox.KeyDown -= _renameBox_KeyDown;
			_renameBox.LostFocus -= _renameBox_LostFocus;
			_renameBox.Visible = false;
			_editNode = null;

			var eventArgs = new NodeLabelEditEventArgs(editNode, canceled ? editNode.Text : _renameBox.Text);
			OnAfterLabelEdit(eventArgs);

			if (!eventArgs.CancelEdit)
			{
				editNode.Text = eventArgs.Label;
			}
		}

		/// <summary>
		/// Function to return an iterator that will search through all nodes in the tree.
		/// </summary>
		/// <returns>An enumerator for all the nodes in the tree.</returns>
	    public IEnumerable<EditorTreeNode> AllNodes()
		{
			return Nodes.AllNodes();
	    }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorTreeView"/> class.
        /// </summary>
        public EditorTreeView()
        {
	        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            base.DrawMode = TreeViewDrawMode.OwnerDrawAll;

	        var fadeMatrix = new ColorMatrix(new []
	                                         {
		                                         new[]
		                                         {
			                                         1.0f, 0.0f, 0.0f, 0.0f, 0.0f
		                                         },
		                                         new[]
		                                         {
			                                         0.0f, 1.0f, 0.0f, 0.0f, 0.0f
		                                         },
		                                         new[]
		                                         {
			                                         0.0f, 0.0f, 1.0f, 0.0f, 0.0f
		                                         },
		                                         new[]
		                                         {
			                                         0.0f, 0.0f, 0.0f, 0.25f, 0.0f
		                                         },
		                                         new[]
		                                         {
			                                         0.0f, 0.0f, 0.0f, 0.0f, 1.0f
		                                         }
	                                         });

			_fadeAttributes = new ImageAttributes();
			_fadeAttributes.SetColorMatrix(fadeMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        }
        #endregion
    }
}
