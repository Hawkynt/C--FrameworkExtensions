#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY

#endregion

using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using Guard;

namespace System.Windows.Forms;

public static partial class TreeViewExtensions {
  /// <summary>
  ///   This class handles the drag and drop functionality.
  /// </summary>
  private sealed class DragDropInstance : IDisposable {
    #region consts

    /// <summary>
    ///   The initial size of the tree-map
    /// </summary>
    private const int _INITIAL_NODEMAP_SIZE = 128;

    /// <summary>
    ///   The delimiter used for concatting the node-ids
    /// </summary>
    private const char _NODEMAP_DELIMITER = '|';

    private const int _SCROLL_PIXEL_RANGE = 30;

    #endregion

    #region readonly fields

    /// <summary>
    ///   The predicate which determines if a given node is a folder node.
    ///   Note: If this is <c>null</c>, no folder nodes are possible.
    /// </summary>
    private readonly Predicate<TreeNode> _folderSelector;

    /// <summary>
    ///   The TreeView this instance is bound to.
    /// </summary>
    private readonly TreeView _treeView;

    /// <summary>
    ///   A value indicating whether root nodes can be moved or not.
    /// </summary>
    private readonly bool _canDragRootNodes;

    /// <summary>
    ///   Gets called whenever a node is moved.
    /// </summary>
    private readonly Action<TreeNode, TreeNode, int> _OnNodeMove;

    private readonly ImageList _dragImageList;
    private readonly Timer _dragScrollTimer;

    #endregion

    #region node-map

    private StringBuilder NewNodeMap = new(_INITIAL_NODEMAP_SIZE);
    private string NodeMap;

    #endregion

    private TreeNode _draggedNode;

    public DragDropInstance(TreeView treeView, Predicate<TreeNode> folderSelector, bool canDragRootNodes, Action<TreeNode, TreeNode, int> onNodeMove) {
      Against.ArgumentIsNull(treeView);
      Against.True(treeView.InvokeRequired);

      this._treeView = treeView;
      this._folderSelector = folderSelector;
      this._canDragRootNodes = canDragRootNodes;
      this._OnNodeMove = onNodeMove;

      this._dragImageList = new();
      this._dragScrollTimer = new() { Interval = 200, Enabled = false };

      this._dragScrollTimer.Tick += this._Tick;
      treeView.DragOver += this._DragOver;
      treeView.DragEnter += this._DragEnter;
      treeView.DragLeave += this._DragLeave;
      treeView.ItemDrag += this._ItemDrag;
      treeView.DragDrop += this._Drop;
      treeView.GiveFeedback += this._GiveFeedback;
    }

    #region Implementation of IDisposable

    public void Dispose() {
      var treeView = this._treeView;

      treeView.DragLeave -= this._DragLeave;
      treeView.DragOver -= this._DragOver;
      treeView.DragEnter -= this._DragEnter;
      treeView.ItemDrag -= this._ItemDrag;
      treeView.DragDrop -= this._Drop;
      treeView.GiveFeedback -= this._GiveFeedback;

      this._dragScrollTimer.Dispose();
      this._dragImageList.Dispose();
    }

    #endregion

    #region event handler

    /// <summary>
    ///   Gets called when the timer ticks.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _Tick(object sender, EventArgs e) {
      var treeView = this._treeView;

      // get node at mouse position
      var pt = treeView.PointToClient(Control.MousePosition);
      var node = treeView.GetNodeAt(pt);

      if (node == null)
        return;

      // if mouse is near to the top, scroll up
      if (pt.Y < _SCROLL_PIXEL_RANGE) {
        // set actual node to the upper one
        if (node.PrevVisibleNode == null)
          return;

        node = node.PrevVisibleNode;

        // hide drag image
        NativeMethods.ImageList_DragShowNolock(false);

        // scroll and refresh
        node.EnsureVisible();
        this._Draw();
      }
      // if mouse is near to the bottom, scroll down
      else if (pt.Y > treeView.Size.Height - _SCROLL_PIXEL_RANGE) {
        if (node.NextVisibleNode == null)
          return;

        node = node.NextVisibleNode;

        NativeMethods.ImageList_DragShowNolock(false);
        node.EnsureVisible();
        this._Draw();
      }
    }

    private void _GiveFeedback(object sender, GiveFeedbackEventArgs e) {
      if (sender is not TreeView treeView || treeView != this._treeView)
        return;

      if (e.Effect == DragDropEffects.Move) {
        // Show pointer cursor while dragging
        e.UseDefaultCursors = false;
        treeView.Cursor = Cursors.Default;
      } else
        e.UseDefaultCursors = true;
    }

    /// <summary>
    ///   Gets called when an item starts to be dragged.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _ItemDrag(object sender, ItemDragEventArgs e) {
      if (sender is not TreeView treeView || treeView != this._treeView)
        return;

      // do not allow root nodes to be dragged
      if (!this._canDragRootNodes && treeView.Nodes.Contains((TreeNode)e.Item))
        return;

      // Get drag node and select it
      var treeNode = (TreeNode)e.Item;
      this._draggedNode = treeNode;
      treeView.SelectedNode = this._draggedNode;

      // Reset image list used for drag image
      var imageList = this._dragImageList;
      imageList.Images.Clear();

      var nodeImage = treeNode.GetImage();

      imageList.ImageSize = new(
        Math.Min(256, treeNode.Bounds.Size.Width + (nodeImage?.Width + 1 ?? 0)),
        Math.Min(256, treeNode.Bounds.Height)
      );

      // Create new bitmap
      // This bitmap will contain the tree node image to be dragged
      var bmp = new Bitmap(imageList.ImageSize.Width, imageList.ImageSize.Height);

      // Get graphics from bitmap
      var gfx = Graphics.FromImage(bmp);

      // Draw node icon into the bitmap
      if (nodeImage != null)
        gfx.DrawImage(nodeImage, 0, 0);

      // Draw node label into bitmap
      gfx.DrawString(treeNode.Text, treeView.Font, new SolidBrush(treeView.ForeColor), nodeImage?.Width + 1 ?? 0, 1.0f);

      // Add bitmap to imagelist
      imageList.Images.Add(bmp);

      // Compute hotspot
      const int dx = 16;
      const int dy = 16;

      // Begin dragging image
      if (!NativeMethods.ImageList_BeginDrag(imageList.Handle, 0, -dx, -dy))
        return;

      treeView.DoDragDrop(treeNode, DragDropEffects.Move);

      // End dragging image
      NativeMethods.ImageList_EndDrag();
    }

    /// <summary>
    ///   Gets called when the mouse enters the control while dragging.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _DragEnter(object sender, DragEventArgs e) {
      if (sender is not TreeView treeView || treeView != this._treeView)
        return;

      if (!e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false) || (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode") != this._draggedNode) {
        e.Effect = DragDropEffects.None;
        return;
      }

      var screen = new Point(e.X, e.Y);
      var client = treeView.PointToClient(screen);
      var window = client;
      NativeMethods.ImageList_DragEnter(treeView.Handle, window.X, window.Y);

      // Enable timer for scrolling dragged item
      this._dragScrollTimer.Enabled = true;

      e.Effect = DragDropEffects.Move;
    }

    /// <summary>
    ///   Gets called when the mouse moves with a dragged item.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _DragOver(object sender, DragEventArgs e) {
      if (sender is not TreeView treeView || treeView != this._treeView)
        return;

      var currentScreenPoint = new Point(e.X, e.Y);

      // Compute drag position and move image
      var form = treeView.FindForm();
      if (form == null)
        NativeMethods.ImageList_DragShowNolock(false);
      else {
        var tvP = treeView.GetLocationOnForm();
        var formP = form.PointToClient(currentScreenPoint);
        formP.Offset(-tvP.X, -tvP.Y);
        NativeMethods.ImageList_DragMove(formP.X, formP.Y);
      }

      var hoveredNode = treeView.GetNodeAt(treeView.PointToClient(currentScreenPoint));
      if (hoveredNode == null)
        return;

      var draggedNode = this._draggedNode;

      #region If hoveredNode is a child of the dragged node then cancel

      if (hoveredNode.IsChildOf(draggedNode)) {
        this.NodeMap = string.Empty;
        return;
      }

      #endregion

      // A bit long, but to summarize, process the following code only if the nodeover is null
      // and either the nodeover is not the same thing as nodemoving UNLESSS nodeover happens
      // to be the last node in the branch (so we can allow drag & drop below a parent branch)
      if (hoveredNode == draggedNode && (hoveredNode.Parent == null || hoveredNode.Index != hoveredNode.Parent.Nodes.Count - 1))
        return;

      var offsetY = treeView.PointToClient(Cursor.Position).Y - hoveredNode.Bounds.Top;

      this._ResetDraw();

      if (!this._IsFolderNode(hoveredNode)) {
        #region Standard Node

        if (offsetY < hoveredNode.Bounds.Height / 2) {
          //this.lblDebug.Text = "top";

          #region Store the placeholder info into a pipe delimited string

          this.SetNewNodeMap(hoveredNode, false);
          if (this.SetMapsEqual())
            return;

          #endregion

          this._SetAndDraw(PlaceHolderType.LeafTop, hoveredNode);
        } else {
          //this.lblDebug.Text = "bottom";

          #region Allow drag drop to parent branches

          TreeNode ParentDragDrop = null;
          // If the node the mouse is over is the last node of the branch we should allow
          // the ability to drop the "nodemoving" node BELOW the parent node
          if (hoveredNode.Parent != null && hoveredNode.Index == hoveredNode.Parent.Nodes.Count - 1) {
            var XPos = treeView.PointToClient(Cursor.Position).X;
            if (XPos < hoveredNode.Bounds.Left) {
              ParentDragDrop = hoveredNode.Parent;

              var image = ParentDragDrop.GetImage();
              if (XPos < ParentDragDrop.Bounds.Left - (image?.Size.Width ?? 0))
                if (ParentDragDrop.Parent != null)
                  ParentDragDrop = ParentDragDrop.Parent;
            }
          }

          #endregion

          #region Store the placeholder info into a pipe delimited string

          // Since we are in a special case here, use the ParentDragDrop node as the current "nodeover"
          this.SetNewNodeMap(ParentDragDrop ?? hoveredNode, true);
          if (this.SetMapsEqual())
            return;

          #endregion

          this._SetAndDraw(PlaceHolderType.LeafBottom, hoveredNode, ParentDragDrop);
        }

        #endregion
      } else {
        #region Folder Node

        if (offsetY < hoveredNode.Bounds.Height / 3) {
          //this.lblDebug.Text = "folder top";

          #region Store the placeholder info into a pipe delimited string

          this.SetNewNodeMap(hoveredNode, false);
          if (this.SetMapsEqual())
            return;

          #endregion

          this._SetAndDraw(PlaceHolderType.FolderTop, hoveredNode);
        } else if (hoveredNode.Parent != null && hoveredNode.Index == 0 && offsetY > hoveredNode.Bounds.Height - hoveredNode.Bounds.Height / 3) {
          //this.lblDebug.Text = "folder bottom";

          #region Store the placeholder info into a pipe delimited string

          this.SetNewNodeMap(hoveredNode, true);
          if (this.SetMapsEqual())
            return;

          #endregion

          this._SetAndDraw(PlaceHolderType.FolderTop, hoveredNode);
        } else {
          //this.lblDebug.Text = "folder over";

          if (hoveredNode.Nodes.Count > 0) {
            NativeMethods.ImageList_DragShowNolock(false);
            hoveredNode.Expand();

            this._SetAndDraw(PlaceHolderType.AddToFolder, hoveredNode);
          } else {
            #region Prevent the node from being dragged onto itself

            if (draggedNode == hoveredNode)
              return;

            #endregion

            #region If hoveredNode is a child then cancel

            if (hoveredNode.IsChildOf(draggedNode)) {
              this.NodeMap = string.Empty;
              return;
            }

            #endregion

            #region Store the placeholder info into a pipe delimited string

            this.SetNewNodeMap(hoveredNode, false);
            this.NewNodeMap = this.NewNodeMap.Insert(this.NewNodeMap.Length, _NODEMAP_DELIMITER + "0");

            if (this.SetMapsEqual())
              return;

            #endregion

            this._SetAndDraw(PlaceHolderType.AddToFolder, hoveredNode);
          }
        }

        #endregion
      }
    }

    /// <summary>
    ///   Gets called when a dragged item leaves the control.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _DragLeave(object sender, EventArgs e) {
      if (sender is not TreeView treeView || treeView != this._treeView)
        return;

      // remove drag/drop image
      NativeMethods.ImageList_DragLeave(treeView.Handle);

      // Disable timer for scrolling dragged item
      this._dragScrollTimer.Enabled = false;
    }

    /// <summary>
    ///   Gets called when the item is dropped.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void _Drop(object sender, DragEventArgs e) {
      if (sender is not TreeView treeView || treeView != this._treeView)
        return;

      // Unlock updates
      NativeMethods.ImageList_DragShowNolock(false);
      treeView.Refresh();

      if (this._draggedNode != null && this.NodeMap != "") {
        var movingNode = this._draggedNode;
        var NodeIndexes = this.NodeMap.Split(_NODEMAP_DELIMITER);
        var InsertCollection = treeView.Nodes;
        TreeNode newParent = null;
        for (var i = 0; i < NodeIndexes.Length - 1; i++) {
          var index = int.Parse(NodeIndexes[i]);
          if (index > InsertCollection.Count)
            index = InsertCollection.Count;
          InsertCollection = (newParent = InsertCollection[index]).Nodes;
        }

        var insertIndex = int.Parse(NodeIndexes[^1]);

        // special case: we're inserting the node into the same tree again
        var oldIndex = InsertCollection.IndexOf(movingNode);
        // we need to decrement the stored index, because after removing the node, all following elements' indexes will be decremented
        if (oldIndex <= insertIndex && movingNode.Parent == newParent)
          --insertIndex;

        // prepare move
        this._OnNodeMove?.Invoke(movingNode, newParent, insertIndex);

        // move
        movingNode.Remove();
        InsertCollection.Insert(insertIndex, movingNode);

        var index2 = int.Parse(NodeIndexes[^1]);
        if (index2 < InsertCollection.Count)
          treeView.SelectedNode = InsertCollection[index2];

        this._draggedNode = null;
        this._dragScrollTimer.Enabled = false;
      }
    }

    #endregion

    #region utilities

    /// <summary>
    ///   Determines whether the given node is a folder node or not.
    /// </summary>
    /// <param name="treeNode">The TreeNode for which we want to know the detail.</param>
    /// <returns>
    ///   <c>true</c> if the given node is handled as a folder node, which means we can drag items into it; otherwise,
    ///   <c>false</c>.
    /// </returns>
    private bool _IsFolderNode(TreeNode treeNode) {
      Against.ArgumentIsNull(treeNode);

      var folderSelector = this._folderSelector;
      return folderSelector != null && folderSelector(treeNode);
    }

    #region node map stuff

    private void SetNewNodeMap(TreeNode tnNode, bool boolBelowNode) {
      this.NewNodeMap.Length = 0;

      if (boolBelowNode)
        this.NewNodeMap.Insert(0, tnNode.Index + 1);
      else
        this.NewNodeMap.Insert(0, tnNode.Index);

      var tnCurNode = tnNode;

      while ((tnCurNode = tnCurNode.Parent) != null)
        if (this.NewNodeMap.Length == 0 && boolBelowNode)
          this.NewNodeMap.Insert(0, tnCurNode.Index + 1 + _NODEMAP_DELIMITER.ToString());
        else
          this.NewNodeMap.Insert(0, tnCurNode.Index + _NODEMAP_DELIMITER.ToString());

    }

    private bool SetMapsEqual() {
      if (this.NewNodeMap.ToString() == this.NodeMap)
        return true;

      this.NodeMap = this.NewNodeMap.ToString();
      return false;
    }

    #endregion

    #endregion

    #region drawing routines

    private enum PlaceHolderType {
      None,
      LeafTop,
      LeafBottom,
      FolderTop,
      AddToFolder
    }

    private PlaceHolderType _placeHolder;
    private TreeNode _nodeOver;
    private TreeNode _parentDragDrop;

    private void _ResetDraw() {
      this._placeHolder = PlaceHolderType.None;
      this._nodeOver = null;
      this._parentDragDrop = null;
    }

    private void _SetAndDraw(PlaceHolderType placeHolder, TreeNode nodeOver, TreeNode parentDragDrop = null) {
      this._placeHolder = placeHolder;
      this._nodeOver = nodeOver;
      this._parentDragDrop = parentDragDrop;
      this._Draw();
    }

    private void _Draw() {
      // hide image
      NativeMethods.ImageList_DragShowNolock(false);

      // paint treeview
      this._treeView.Refresh();

      // paint placeholder
      var hoveredNode = this._nodeOver;
      if (hoveredNode is { TreeView: not null })
        switch (this._placeHolder) {
          case PlaceHolderType.LeafTop: {
            _DrawLeafTopPlaceholders(hoveredNode);
            break;
          }
          case PlaceHolderType.LeafBottom: {
            _DrawLeafBottomPlaceholders(hoveredNode, this._parentDragDrop);
            break;
          }
          case PlaceHolderType.AddToFolder: {
            _DrawAddToFolderPlaceholder(hoveredNode);
            break;
          }
          case PlaceHolderType.FolderTop: {
            _DrawFolderTopPlaceholders(hoveredNode);
            break;
          }
          case PlaceHolderType.None: {
            break;
          }
          default: {
            throw new NotImplementedException("Unknown place holder type");
          }
        }

      // paint image
      NativeMethods.ImageList_DragShowNolock(true);
    }

    private static void _DrawLeafTopPlaceholders(TreeNode hoveredNode) {
      var treeView = hoveredNode.TreeView;
      var g = treeView.CreateGraphics();

      var nodeImage = hoveredNode.GetImage();
      var imageWidth = nodeImage == null ? 0 : nodeImage.Size.Width + 8;
      var leftPos = hoveredNode.Bounds.Left - imageWidth;
      var rightPos = treeView.Width - 4;

      var leftTriangle = new[] {
        new Point(leftPos, hoveredNode.Bounds.Top - 4),
        new Point(leftPos, hoveredNode.Bounds.Top + 4),
        new Point(leftPos + 4, hoveredNode.Bounds.Y),
        new Point(leftPos + 4, hoveredNode.Bounds.Top - 1),
        new Point(leftPos, hoveredNode.Bounds.Top - 5)
      };

      var rightTriangle = new[] {
        new Point(rightPos, hoveredNode.Bounds.Top - 4),
        new Point(rightPos, hoveredNode.Bounds.Top + 4),
        new Point(rightPos - 4, hoveredNode.Bounds.Y),
        new Point(rightPos - 4, hoveredNode.Bounds.Top - 1),
        new Point(rightPos, hoveredNode.Bounds.Top - 5)
      };
      
      g.FillPolygon(Brushes.Black, leftTriangle);
      g.FillPolygon(Brushes.Black, rightTriangle);
      g.DrawLine(new(Color.Black, 2), new(leftPos, hoveredNode.Bounds.Top), new(rightPos, hoveredNode.Bounds.Top));
    }

    private static void _DrawLeafBottomPlaceholders(TreeNode hoveredNode, TreeNode parentNodeDragDrop) {
      var treeView = hoveredNode.TreeView;
      var g = treeView.CreateGraphics();

      var nodeImage = hoveredNode.GetImage();
      var imageWidth = nodeImage == null ? 0 : nodeImage.Size.Width + 8;
      // Once again, we are not dragging to node over, draw the placeholder using the ParentDragDrop bounds
      var leftPos = parentNodeDragDrop != null
          ? parentNodeDragDrop.Bounds.Left - (parentNodeDragDrop.GetImage().Size.Width + 8)
          : hoveredNode.Bounds.Left - imageWidth
        ;

      var rightPos = treeView.Width - 4;

      var leftTriangle = new[] {
        new Point(leftPos, hoveredNode.Bounds.Bottom - 4),
        new Point(leftPos, hoveredNode.Bounds.Bottom + 4),
        new Point(leftPos + 4, hoveredNode.Bounds.Bottom),
        new Point(leftPos + 4, hoveredNode.Bounds.Bottom - 1),
        new Point(leftPos, hoveredNode.Bounds.Bottom - 5)
      };

      var rightTriangle = new[] {
        new Point(rightPos, hoveredNode.Bounds.Bottom - 4),
        new Point(rightPos, hoveredNode.Bounds.Bottom + 4),
        new Point(rightPos - 4, hoveredNode.Bounds.Bottom),
        new Point(rightPos - 4, hoveredNode.Bounds.Bottom - 1),
        new Point(rightPos, hoveredNode.Bounds.Bottom - 5)
      };
      
      g.FillPolygon(Brushes.Black, leftTriangle);
      g.FillPolygon(Brushes.Black, rightTriangle);
      g.DrawLine(
        new(Color.Black, 2),
        new(leftPos, hoveredNode.Bounds.Bottom),
        new(rightPos, hoveredNode.Bounds.Bottom)
      );
    }

    private static void _DrawFolderTopPlaceholders(TreeNode hoveredNode) {
      var treeView = hoveredNode.TreeView;

      var g = treeView.CreateGraphics();
      var nodeImage = hoveredNode.GetImage();
      var imageWidth = nodeImage == null ? 0 : nodeImage.Size.Width + 8;

      var leftPos = hoveredNode.Bounds.Left - imageWidth;
      var rightPos = treeView.Width - 4;

      var leftTriangle = new[] {
        new Point(leftPos, hoveredNode.Bounds.Top - 4),
        new Point(leftPos, hoveredNode.Bounds.Top + 4),
        new Point(leftPos + 4, hoveredNode.Bounds.Y),
        new Point(leftPos + 4, hoveredNode.Bounds.Top - 1),
        new Point(leftPos, hoveredNode.Bounds.Top - 5)
      };

      var rightTriangle = new[] {
        new Point(rightPos, hoveredNode.Bounds.Top - 4),
        new Point(rightPos, hoveredNode.Bounds.Top + 4),
        new Point(rightPos - 4, hoveredNode.Bounds.Y),
        new Point(rightPos - 4, hoveredNode.Bounds.Top - 1),
        new Point(rightPos, hoveredNode.Bounds.Top - 5)
      };

      g.FillPolygon(Brushes.Black, leftTriangle);
      g.FillPolygon(Brushes.Black, rightTriangle);
      g.DrawLine(new(Color.Black, 2), new(leftPos, hoveredNode.Bounds.Top), new(rightPos, hoveredNode.Bounds.Top));
    }

    private static void _DrawAddToFolderPlaceholder(TreeNode hoveredNode) {
      var treeView = hoveredNode.TreeView;

      var g = treeView.CreateGraphics();
      var rightPos = hoveredNode.Bounds.Right + 6;
      var halfHeight = hoveredNode.Bounds.Height / 2;
      var y = hoveredNode.Bounds.Y;
      var rightTriangle = new[] {
        new Point(rightPos, y + halfHeight + 4),
        new Point(rightPos, y + halfHeight + 4),
        new Point(rightPos - 4, y + halfHeight),
        new Point(rightPos - 4, y + halfHeight - 1),
        new Point(rightPos, y + halfHeight - 5)
      };

      g.FillPolygon(Brushes.Black, rightTriangle);
    }

    #endregion

    #region internal class for displaying dragged items

    private static class NativeMethods {
      [DllImport("comctl32.dll")]
      private static extern bool InitCommonControls();

      [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
      public static extern bool ImageList_BeginDrag(IntPtr himlTrack, int iTrack, int dxHotspot, int dyHotspot);

      [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
      public static extern bool ImageList_DragMove(int x, int y);

      [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
      public static extern void ImageList_EndDrag();

      [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
      public static extern bool ImageList_DragEnter(IntPtr hwndLock, int x, int y);

      [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
      public static extern bool ImageList_DragLeave(IntPtr hwndLock);

      /// <summary>
      ///   Shows or hides the drag image.
      /// </summary>
      /// <param name="fShow">if set to <c>true</c> the image will be shown; otherwise, it will be hidden.</param>
      /// <returns></returns>
      [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
      public static extern bool ImageList_DragShowNolock(bool fShow);

      static NativeMethods() => InitCommonControls();
    }

    #endregion
  }
}
