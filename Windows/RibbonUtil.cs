using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

using static RevitWindows.Ribbon;

namespace RevitWindows
{
	class RibbonUtil
	{
		private const string NAMESPACE_PREFIX = "RevitWindows.Resources.Images";

		// load an image from embeded resource
		public static BitmapImage GetBitmapImage(string imageName)
		{
			Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(NAMESPACE_PREFIX + "." + imageName);

			BitmapImage img = new BitmapImage();

			img.BeginInit();
			img.StreamSource = s;
			img.EndInit();

			return img;
		}
	}
}
