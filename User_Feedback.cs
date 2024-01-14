using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eto.Forms;
using Eto.Drawing;

namespace PachydermGH
{
    public partial class User_Feedback : Eto.Forms.FloatingForm
    {
        private System.Windows.Forms.Label text_display;

        public User_Feedback()
        {
            this.text_display = new System.Windows.Forms.Label();
            this.Title = "Working...";
            this.Focus();
        }

        public void Display(string s, string title)
        {
            this.Title = title;
            text_display.Text = s;
            //text_display.Refresh();
            this.Invalidate();
            this.Focus();
        }
    }
}
