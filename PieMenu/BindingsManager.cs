namespace PieMenu
{
	using System;
	using Gma.System.MouseKeyHook;
	using WindowsInput;
	using WindowsInput.Native;
	using System.Windows.Forms;

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

		public void Bind()
		{
			hook.KeyDown += Hook_KeyDown;
			hook.KeyUp += Hook_KeyUp;
			hook.MouseMove += Hook_MouseMove;
		}

		private void Hook_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.D)
			{
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
