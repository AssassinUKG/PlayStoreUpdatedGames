using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace PlayStoreUpdatedGames
{
   partial class  AppDisplay : UserControl
    {
        public AppDisplay()
        {
            InitializeComponent();
            this.Click += (IgotClicked);
        }


        public AppStoreClass aPc { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {

            get { return _isSelected; }
            set { _isSelected = value; Invalidate(); }
        }


        public string LabelText
        {
            get { return LabelText; }
            set {  label1.Text = value; }
        }

        //could be expanded to include a image cache
        public void SetImage(string imageLink)
        {
            pictureBox1.Image = Image.FromStream(new MemoryStream(new System.Net.WebClient().DownloadData(imageLink)));
        }

        public void SetImageLocalFile(string filename)
        {
            pictureBox1.Image = Image.FromFile(filename);
        }

        private void AppDisplay_Load(object sender, EventArgs e)
        {
            
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (Graphics G = e.Graphics)
            {
                if (IsSelected)
                {
                    G.FillRectangle(Brushes.LightBlue, new Rectangle(0, 0, Width - 1, Height - 1));
                    G.DrawRectangle(Pens.DarkGray, new Rectangle(0, 0, Width - 1, Height - 1));
                    
                }
            }





                base.OnPaint(e);
        }

        private void IgotClicked(object sender, EventArgs e)
        {
            if(aPc != null)
            {


                Debug.Print("Clicked");
                RaiseOnDoubleClicked(aPc);
            }
        }

        public delegate void RaiseDoubleClickedEventHandler(AppStoreClass apc);

        public event RaiseDoubleClickedEventHandler DoubleClick_Clicked;

        protected virtual void RaiseOnDoubleClicked(AppStoreClass apc)
        {

            DoubleClick_Clicked?.Invoke(apc);


        }

        

        #region "mouse"
        protected override void OnMouseEnter(EventArgs e)
        {
            IsSelected = true;
            base.OnMouseEnter(e);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            IsSelected = false;
            base.OnMouseLeave(e);
        }


# endregion  




    }
}
