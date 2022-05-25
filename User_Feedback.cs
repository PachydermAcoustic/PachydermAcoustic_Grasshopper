using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grasshopper.Kernel;

namespace PachydermGH
{
    public partial class User_Feedback : Form
    {
        public User_Feedback()
        {
            InitializeComponent();
            this.Focus();
        }

        public void Display(string s)
        {
            text_display.Text = s;
            text_display.Refresh();
            this.Refresh();
            this.Focus();
        }
    }
}
