namespace PieMenu
{
	using System;
	using Gma.System.MouseKeyHook;
	using WindowsInput;
	using System.Windows.Forms;
	using System.Runtime.InteropServices;
	using System.Diagnostics;
	using System.Collections.Generic;

	class BindingsManager
	{
		private readonly IKeyboardMouseEvents hook = Hook.GlobalEvents();
		private readonly InputSimulator simulator = new InputSimulator();
		private Config config = Config.FromSettings();
		private List<SliceOptions> currentOptions = new List<SliceOptions>();

		public delegate void PresentHandler();
		public event PresentHandler beginSelection;
		public delegate void RecedeHandler();
		public event RecedeHandler endSelection;
		public delegate void UpdateHandler();
		public event UpdateHandler updateSelection;

		[DllImport("User32.dll")]
		private static extern uint GetWindowThreadProcessId(uint windowHandle, out uint processId);

		[DllImport("User32.dll")]
		private static extern uint GetForegroundWindow();

		public BindingsManager()
		{
			hook.KeyDown += Hook_KeyDown;
			hook.KeyUp += Hook_KeyUp;
		}

		private void Hook_KeyDown(object sender, KeyEventArgs e)
		{
			foreach (var binding in config.bindings)
			{
				if (e.KeyCode == binding.trigger)
				{
					string exe = FindExecutable(GetForegroundWindow());
					List<SliceOptions> options;
					if (binding.apps.TryGetValue(exe, out options))
					{
						hook.MouseMove += Hook_MouseMove;
						currentOptions = options;
						beginSelection?.Invoke();
					}
					break;
				}
			}
		}

		private void Hook_KeyUp(object sender, KeyEventArgs e)
		{
			hook.MouseMove -= Hook_MouseMove;
			endSelection?.Invoke();
		}

		private void Hook_MouseMove(object sender, MouseEventArgs e)
		{
			updateSelection?.Invoke();
		}

		private string FindExecutable(uint windowHandle)
		{
			uint processId;
			GetWindowThreadProcessId(windowHandle, out processId);
			try
			{
				var process = Process.GetProcessById((int)processId);
				return process?.MainModule.FileName;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return null;
			}
		}

		public void PerformSliceAction(int slice)
		{
			if (slice < currentOptions.Count && slice > -1)
			{
				SliceOptions options = currentOptions[slice];
				simulator.Keyboard.ModifiedKeyStroke(options.modifiers, options.trigger);
			}
		}

		public int SliceCount()
		{
			return currentOptions.Count;
		}

		public string TitleForSlice(int slice)
		{
			return currentOptions[slice].title;
		}
	}
}
