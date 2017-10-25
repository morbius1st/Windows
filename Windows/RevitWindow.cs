using System;
using System.Drawing;

namespace Windows
{
	internal class RevitWindow
	{
		internal int sequence;
		internal IntPtr handle;
		internal string docTitle;
		internal string winTitle;
		internal bool IsMinimized;
		internal bool IsValid = true;
		internal Rectangle current;
		internal Rectangle proposed;

		internal RevitWindow Clone()
		{
			RevitWindow rwn = new RevitWindow();
			rwn.sequence = this.sequence;
			rwn.handle = this.handle;
			rwn.docTitle = this.docTitle;
			rwn.winTitle = this.winTitle;
			rwn.IsMinimized = this.IsMinimized;
			rwn.IsValid = this.IsValid;
			rwn.current = this.current;
			rwn.proposed = this.proposed;

			return rwn;

		}
	}
}