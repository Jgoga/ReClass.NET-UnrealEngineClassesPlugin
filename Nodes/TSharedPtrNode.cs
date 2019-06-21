﻿using System;
using System.Drawing;
using ReClassNET;
using ReClassNET.Memory;
using ReClassNET.Nodes;
using ReClassNET.UI;

namespace UnrealEngineClassesPlugin.Nodes
{
	public class TSharedPtrNode : BaseWrapperNode
	{
		private readonly MemoryBuffer memory = new MemoryBuffer();

		public override int MemorySize => IntPtr.Size * 2;

		protected override bool PerformCycleCheck => false;

		public override void GetUserInterfaceInfo(out string name, out Image icon)
		{
			name = "TSharedPtr";
			icon = null;
		}

		public override bool CanChangeInnerNodeTo(BaseNode node)
		{
			switch (node)
			{
				case ClassNode _:
				case VirtualMethodNode _:
					return false;
			}

			return true;
		}

		public override Size Draw(ViewInfo view, int x, int y)
		{
			if (IsHidden && !IsWrapped)
			{
				return DrawHidden(view, x, y);
			}

			var origX = x;
			var origY = y;

			AddSelection(view, x, y, view.Font.Height);

			if (InnerNode != null)
			{
				x = AddOpenCloseIcon(view, x, y);
			}
			else
			{
				x += TextPadding;
			}
			x = AddIcon(view, x, y, Icons.Pointer, -1, HotSpotType.None);

			var tx = x;
			x = AddAddressOffset(view, x, y);

			x = AddText(view, x, y, view.Settings.TypeColor, HotSpot.NoneId, "TSharedPtr") + view.Font.Width;
			if (!IsWrapped)
			{
				x = AddText(view, x, y, view.Settings.NameColor, HotSpot.NameId, Name) + view.Font.Width;
			}
			if (InnerNode == null)
			{
				x = AddText(view, x, y, view.Settings.ValueColor, HotSpot.NoneId, "<void>") + view.Font.Width;
			}
			x = AddIcon(view, x, y, Icons.Change, 4, HotSpotType.ChangeWrappedType) + view.Font.Width;

			var ptr = view.Memory.ReadIntPtr(Offset);

			x = AddText(view, x, y, view.Settings.OffsetColor, HotSpot.NoneId, "->") + view.Font.Width;
			x = AddText(view, x, y, view.Settings.ValueColor, 0, "0x" + ptr.ToString(Constants.AddressHexFormat)) + view.Font.Width;

			x = AddComment(view, x, y);

			DrawInvalidMemoryIndicatorIcon(view, y);
			AddContextDropDownIcon(view, y);
			AddDeleteIcon(view, y);

			y += view.Font.Height;

			var size = new Size(x - origX, y - origY);

			if (LevelsOpen[view.Level] && InnerNode != null)
			{
				memory.Size = InnerNode.MemorySize;
				memory.UpdateFrom(view.Process, ptr);

				var v = view.Clone();
				v.Address = ptr;
				v.Memory = memory;

				var innerSize = InnerNode.Draw(v, tx, y);

				size.Width = Math.Max(size.Width, innerSize.Width + tx - origX);
				size.Height += innerSize.Height;
			}

			return size;
		}

		public override int CalculateDrawnHeight(ViewInfo view)
		{
			if (IsHidden && !IsWrapped)
			{
				return HiddenHeight;
			}

			var height = view.Font.Height;
			if (LevelsOpen[view.Level] && InnerNode != null)
			{
				height += InnerNode.CalculateDrawnHeight(view);
			}
			return height;
		}
	}
}
