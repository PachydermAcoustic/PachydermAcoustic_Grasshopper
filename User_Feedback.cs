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
        private Label text_display;

        public User_Feedback()
        {
            this.text_display = new Label();
            this.Title = "Working...";
            this.Content = text_display;
            this.Focus();
        }

        public void Display(string s, string title)
        {
            this.Title = title;
            text_display.Text = s;
            this.Invalidate();
            this.Focus();
        }
    }
}
