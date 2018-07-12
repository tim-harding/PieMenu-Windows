namespace PieMenu
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Windows.Forms;
	using WindowsInput.Native;

	[Serializable]
	public class Config
	{
		public List<Binding> bindings = new List<Binding>();

		private Config() { }

		public static Config FromSettings()
		{
			using (var stream = new MemoryStream(Convert.FromBase64String(Properties.Settings.Default.Config)))
			{
				var formatter = new BinaryFormatter();
				try
				{
					return (Config)formatter.Deserialize(stream);
				}
				catch (Exception)
				{
					var options1 = new SliceOptions() { title = "New Tab", modifiers = new List<VirtualKeyCode>() { VirtualKeyCode.CONTROL }, trigger = VirtualKeyCode.VK_T };
					var options2 = new SliceOptions() { title = "Tab Right", modifiers = new List<VirtualKeyCode>() { VirtualKeyCode.CONTROL, VirtualKeyCode.SHIFT }, trigger = VirtualKeyCode.TAB };
					var options3 = new SliceOptions() { title = "Close Tab", modifiers = new List<VirtualKeyCode>() { VirtualKeyCode.CONTROL }, trigger = VirtualKeyCode.VK_W };
					var options4 = new SliceOptions() { title = "Tab Left", modifiers = new List<VirtualKeyCode>() { VirtualKeyCode.CONTROL }, trigger = VirtualKeyCode.TAB };
					var options5 = new SliceOptions() { title = "New Tab", modifiers = new List<VirtualKeyCode>() { VirtualKeyCode.CONTROL }, trigger = VirtualKeyCode.VK_T };
					var options6 = new SliceOptions() { title = "Tab Right", modifiers = new List<VirtualKeyCode>() { VirtualKeyCode.CONTROL, VirtualKeyCode.SHIFT }, trigger = VirtualKeyCode.TAB };
					var options7 = new SliceOptions() { title = "Close Tab", modifiers = new List<VirtualKeyCode>() { VirtualKeyCode.CONTROL }, trigger = VirtualKeyCode.VK_W };
					var options8 = new SliceOptions() { title = "Tab Left", modifiers = new List<VirtualKeyCode>() { VirtualKeyCode.CONTROL }, trigger = VirtualKeyCode.TAB };
					var bind = new Binding() { trigger = Keys.F4, apps = new Dictionary<string, List<SliceOptions>> { { "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe", new List<SliceOptions>() { options1, options2, options3, options4, options5, options6, options7 } } } };
					return new Config() { bindings = new List<Binding>() { bind } };
				}
			}
		}

		public void Save()
		{
			using (var stream = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(stream, this);
				stream.Position = 0;
				byte[] buffer = new byte[stream.Length];
				stream.Read(buffer, 0, buffer.Length);
				Properties.Settings.Default.Config = Convert.ToBase64String(buffer);
				Properties.Settings.Default.Save();
			}
		}
	}

	[Serializable]
	public class Binding
	{
		public Keys trigger;
		public Dictionary<string, List<SliceOptions>> apps = new Dictionary<string, List<SliceOptions>>();
	}

	[Serializable]
	public class SliceOptions
	{
		public string title;
		public List<VirtualKeyCode> modifiers = new List<VirtualKeyCode>();
		public VirtualKeyCode trigger;
	}
}
