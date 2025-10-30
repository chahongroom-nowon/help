using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;

namespace MobiMeter.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("MobiMeter.Properties.Resources", typeof(Resources).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static UnmanagedMemoryStream Beep => ResourceManager.GetStream("Beep", resourceCulture);

	internal static Bitmap HP_Bar_8 => (Bitmap)ResourceManager.GetObject("HP_Bar_8", resourceCulture);

	internal static Bitmap StartButton1 => (Bitmap)ResourceManager.GetObject("StartButton1", resourceCulture);

	internal static Bitmap StartButton2 => (Bitmap)ResourceManager.GetObject("StartButton2", resourceCulture);

	internal static Bitmap StartButton3 => (Bitmap)ResourceManager.GetObject("StartButton3", resourceCulture);

	internal Resources()
	{
	}
}
