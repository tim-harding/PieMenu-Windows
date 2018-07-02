namespace PieMenu
{
	using System;
	using Gma.System.MouseKeyHook;
	using WindowsInput;
	using WindowsInput.Native;
	using System.Windows.Forms;
	using System.Runtime.InteropServices;
	using System.Diagnostics;

	interface IBindingsPresentationProvider
	{
		void Present();
		void Recede();
		void Update();
	}

	class BindingsManager
	{
		public IBindingsPresentationProvider provider;

		private readonly IKeyboardMouseEvents hook = Hook.GlobalEvents();
		private readonly InputSimulator simulator = new InputSimulator();
		private Binding[] bindings;
		private uint windowHandle;

		[DllImport("User32.dll")]
		private static extern uint GetWindowThreadProcessId(uint windowHandle, out uint processId);

		[DllImport("User32.dll")]
		private static extern uint GetForegroundWindow();

		public void Bind()
		{
			hook.KeyDown += Hook_KeyDown;
			hook.KeyUp += Hook_KeyUp;
			hook.MouseMove += Hook_MouseMove;
		}

		private void Hook_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F4)
			{
				windowHandle = GetForegroundWindow();
				provider?.Present();
			}
		}

		private void Hook_KeyUp(object sender, KeyEventArgs e)
		{
			provider?.Recede();
		}

		private void Hook_MouseMove(object sender, MouseEventArgs e)
		{
			provider?.Update();
		}

		private string FindExecutable()
		{
			uint processId;
			GetWindowThreadProcessId(windowHandle, out processId);
			try
			{
				Process process = Process.GetProcessById((int)processId);
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
			Console.WriteLine(string.Format("Slice {0} was pressed.", slice));
			switch (slice)
			{
				case 0:
					simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.TAB);
					break;
				case 2:
					simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_W);
					break;
				case 3:
					Console.WriteLine(FindExecutable() ?? "");
					break;
				case 4:
					VirtualKeyCode[] modifiers = new VirtualKeyCode[] { VirtualKeyCode.CONTROL, VirtualKeyCode.SHIFT };
					simulator.Keyboard.ModifiedKeyStroke(modifiers, VirtualKeyCode.TAB);
					break;
				case 6:
					simulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_T);
					break;
				default:
					break;
			}
		}

		public string[] CurrentTitles()
		{
			// Temporary
			return new string[]
			{
				"first",
				"second",
				"third",
				"fourth",
				"fifth",
				"sixth",
				"seventh",
				"eighth"
			};
		}
	}
}
