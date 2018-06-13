using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gma.System.MouseKeyHook;

namespace PieMenu
{
    public partial class MainWindow : Window
    {
        IKeyboardEvents GlobalHook;

        public MainWindow()
        {
            InitializeComponent();

            this.Hide();
            this.ShowActivated = false; // Just pop over, don't steal focus

            GlobalHook = Hook.GlobalEvents();
            GlobalHook.KeyDown += GlobalHook_KeyDown;
            GlobalHook.KeyUp += GlobalHook_KeyUp;
        }


        private void GlobalHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D)
            {
                this.Show();
                this.Topmost = true;

                Matrix tx = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
                System.Drawing.Point pos = System.Windows.Forms.Control.MousePosition;
                Point mouse = tx.Transform(new Point(pos.X, pos.Y));
                Left = mouse.X - ActualWidth / 2.0;
                Top = mouse.Y - ActualHeight / 2.0;
            }
        }

        private void GlobalHook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D)
            {
                this.Hide();
            }
        }
    }
}
