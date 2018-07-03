using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Levelset
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Image tmpImage;
        Bitmap oriimage;//originale Image
        Bitmap oriimage0;
        Bitmap oriimage1;
        Size tmpsiz;
        string Filename;
        bool mousedown1 = false;
        int x;
        Point pt;
        double[,] Mphi;
        Graphics graf = null;
        double[,] MDirac;
        double[,] MHeaviside;
        double[,] MCurv;
        double[,] MPenalize;
        double[,] dx, dy, ddx, ddy;
        double[,] Kontur;
        double c1, c2;
        int iterNum;
        double nu, mu, lamda, timestep;
        private void button1_Click(object sender, EventArgs e)
        {
            
            OpenFileDialog opd = new OpenFileDialog();
            opd.DefaultExt = ".bmp";
            opd.Filter = "Image Files(*.bmp,*.jpg,*.png,*.TIF)|*.bmp;*.jpg;*.png;*.TIF||";
            if (DialogResult.OK != opd.ShowDialog(this))
            {
                return;
            }
            Filename = opd.FileName;
            textBox1.AppendText(opd.FileName);
            loadimage();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            x = Convert.ToInt32(textBox2.Text);
            if (mousedown1 == false)
            {
                mousedown1 = true;
                button2.Enabled = false;
            }
            else
            {
                return;
            }
            textBox1.AppendText("Koordinate: " + Convert.ToDouble(pt.X) + "," + Convert.ToDouble(pt.Y) + "\n");
        }
        private void button3_Click(object sender, EventArgs e)
        {
            iterNum = Convert.ToInt16(textBox3.Text);
            nu = Convert.ToDouble(textBox4.Text);
            mu = Convert.ToDouble(textBox5.Text);
            lamda = Convert.ToDouble(textBox6.Text);
            timestep = Convert.ToDouble(textBox7.Text);
            for (int itr = 0; itr < iterNum; itr++)
            {
                Heaviside();
                Dirac();
                curv();
                BrGray();
                //Sobel();
                for (int i = 0; i < oriimage1.Width; i++)
                {
                    for (int j = 0; j < oriimage1.Height; j++)
                    {
                        Mphi[i, j] = Mphi[i, j] + timestep * MDirac[i, j] * (nu * MCurv[i, j] - Math.Pow(oriimage1.GetPixel(i, j).R - c1, 2) + lamda * Math.Pow(oriimage1.GetPixel(i, j).R - c2, 2));
                       
                        //double lengthtr = nu * MDirac[i, j] * MCurv[i, j];
                        //double penalizetr = mu * (MPenalize[i, j] - MCurv[i, j]);
                        //double areatr = MDirac[i, j] *  (Math.Pow(oriimage1.GetPixel(i, j).R - c1, 2) - lamda * Math.Pow(oriimage1.GetPixel(i, j).R - c2, 2));
                        //Mphi[i, j] = Mphi[i, j] + timestep * MDirac[i, j]*(lengthtr + areatr);
                    }
                }
                plot1();
                
            }
            
        }
        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (mousedown1 == true)
            {
                pt = new Point(e.Location.X, e.Location.Y);
                Initialisierung();
                mousedown1 = false;
            }
        }
        public void loadimage()
        {
            tmpImage = Image.FromFile(Filename);
            Size pansiz = panel1.ClientSize;
            tmpsiz = tmpImage.Size;
            if (tmpsiz.Width > pansiz.Width || tmpsiz.Height > pansiz.Height)
            {
                double rImage = tmpsiz.Width * 1.0 / tmpsiz.Height;
                double rWnd = pansiz.Width * 1.0 / pansiz.Height;
                if (rImage < rWnd) // image more high
                {

                    tmpsiz.Height = pansiz.Height;
                    tmpsiz.Width = (int)(tmpsiz.Height * rImage);
                }
                else //image is more wide
                {

                    tmpsiz.Width = pansiz.Width;
                    tmpsiz.Height = (int)(pansiz.Width / rImage);
                }
            }
            panel1.Size = tmpsiz;
            oriimage = new Bitmap(tmpImage, tmpsiz);
            Color pixcolor = Color.FromArgb(0);
            double pixval = 0;
            for (int i = 0; i < oriimage.Height; i++)
            {
                for (int j = 0; j < oriimage.Width; j++)
                {
                    pixcolor = oriimage.GetPixel(j, i);
                    pixval = pixcolor.R * 0.3 + pixcolor.G * 0.59 + pixcolor.B * 0.11;
                    oriimage.SetPixel(j, i, Color.FromArgb(Convert.ToInt32(pixval), Convert.ToInt32(pixval), Convert.ToInt32(pixval)));
                }
            }
            oriimage1 = oriimage;
            //Grauss();//useless
            panel1.BackgroundImage = oriimage1;
            textBox1.AppendText("Image load" + "\n");
            panel1.Refresh();
            oriimage0 = new Bitmap(oriimage1, tmpsiz);
            //oriimage=>grau;oriimage1=>nach gaussian;oriimage0=>copy von oriimage1
        }
        public void Initialisierung()
        {
            graf = Graphics.FromImage(oriimage0);
            Pen pen = new Pen(Color.Red);
            graf.DrawEllipse(pen, pt.X - x, pt.Y - x, 2 * x, 2 * x);
            graf.Save();
            panel1.Refresh();
            panel1.BackgroundImage = oriimage0;
            panel1.Refresh();
            button2.Enabled = true;
            Mphi = new double[oriimage1.Width, oriimage1.Height];
            for (int i = 0; i < oriimage1.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    double c;
                    c = Math.Sqrt((Math.Pow(i - pt.X, 2) + Math.Pow(j - pt.Y, 2)));
                    if (c >= x )
                    {
                        Mphi[i, j] = x-c;
                    }
                    else
                    {
                        Mphi[i, j] = x-c;
                    }
                }
            }
        }
        public void Dirac()
        {
            MDirac = new double[oriimage1.Width, oriimage1.Height];
            dx = new double[oriimage1.Width, oriimage1.Height];
            dy = new double[oriimage1.Width, oriimage1.Height];
            for (int i = 0; i < oriimage1.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    if (i == 0 || j == 0 || i == (oriimage1.Width - 1) || j == (oriimage1.Height - 1))
                    {
                        dx[i, j] = 0;
                        dy[i, j] = 0;
                    }
                    else
                    {
                        dx[i, j] = 0.5 * (MHeaviside[i + 1, j] - MHeaviside[i, j]) / Math.Sqrt(Math.Pow(MHeaviside[i + 1, j] - MHeaviside[i, j], 2) + Math.Pow((MHeaviside[i, j + 1] - MHeaviside[i, j - 1]) / 2, 2) + 0.0000000001) + 0.5 * (MHeaviside[i, j] - MHeaviside[i - 1, j]) / Math.Sqrt(Math.Pow(MHeaviside[i, j] - MHeaviside[i - 1, j], 2) + Math.Pow((MHeaviside[i, j + 1] - MHeaviside[i, j - 1]) / 2, 2) + 0.0000000001);
                        dy[i, j] = 0.5 * (MHeaviside[i, j + 1] - MHeaviside[i, j]) / Math.Sqrt(Math.Pow((MHeaviside[i + 1, j] - MHeaviside[i - 1, j]) / 2, 2) + Math.Pow(MHeaviside[i, j + 1] - MHeaviside[i, j], 2) + 0.0000000001) + 0.5 * (MHeaviside[i, j] - MHeaviside[i, j - 1]) / Math.Sqrt(Math.Pow((MHeaviside[i + 1, j] - MHeaviside[i - 1, j]) / 2, 2) + Math.Pow(MHeaviside[i, j] - MHeaviside[i, j - 1], 2) + 0.0000000001);
                    }
                    if (Math.Sqrt(Math.Pow(dx[i, j], 2) + Math.Pow(dy[i, j], 2)) != 0)
                    {
                        MDirac[i, j] = 1;
                    }
                    else
                    {
                        MDirac[i, j] = 0;
                    }
                }
            }
        }
        public void Heaviside()
        {
            MHeaviside = new double[oriimage1.Width, oriimage1.Height];
            for (int i = 0; i < oriimage1.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    if (Mphi[i, j] >= 0)
                    {
                        MHeaviside[i, j] = 1;
                    }
                    else
                    {
                        MHeaviside[i, j] = 0;
                    }

                      
                }
            }
        }
        public void BrGray()
        {
            double sumfgval = 0;
            double sumbkval = 0;
            int fgn = 0;
            int bkn = 0;
            for (int i = 1; i < oriimage1.Width; i++)
            {
                for (int j = 1; j < oriimage1.Height; j++)
                {
                    if (Mphi[i, j] == 1)
                    {
                        sumfgval = sumfgval + oriimage1.GetPixel(i, j).R;
                        fgn++;
                        

                    }
                    else
                    {
                        sumbkval = sumbkval + oriimage1.GetPixel(i, j).R;
                        bkn++;
                        
                    }
                }
            }
            c1 = sumfgval / fgn;
            c2 = sumbkval / bkn;
            
        }
        public void curv()
        {
            MCurv = new double[oriimage1.Width, oriimage1.Height];
            dx = new double[oriimage1.Width, oriimage1.Height];
            dy = new double[oriimage1.Width, oriimage1.Height];
            ddx = new double[oriimage1.Width, oriimage1.Height];
            ddy = new double[oriimage1.Width, oriimage1.Height];
            for (int i = 0; i < oriimage1.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    if (i == 0 || j == 0 || i == (oriimage1.Width - 1) || j == (oriimage1.Height - 1))
                    {
                        dx[i, j] = 0;
                        dy[i, j] = 0;
                    }
                    else
                    {
                        dx[i, j] = (Mphi[i + 1, j] - Mphi[i, j]) / Math.Sqrt(Math.Pow(Mphi[i + 1, j] - Mphi[i, j], 2) + Math.Pow((Mphi[i, j + 1] - Mphi[i, j - 1]) / 2, 2) + 0.0000000001);
                        dy[i, j] = (Mphi[i, j + 1] - Mphi[i, j]) / Math.Sqrt(Math.Pow((Mphi[i + 1, j] - Mphi[i - 1, j]) / 2, 2) + Math.Pow(Mphi[i, j + 1] - Mphi[i, j], 2) + 0.0000000001);
                    }
                }
            }
            //ddx,ddy
            for (int i = 0; i < oriimage1.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    if (i == 0 || j == 0 || i == (oriimage1.Width - 1) || j == (oriimage1.Height - 1))
                    {
                        dx[i, j] = 0;
                        dy[i, j] = 0;
                    }
                    else
                    {
                        ddx[i, j] = dx[i, j] - dx[i - 1, j];
                        ddy[i, j] = dy[i, j] - dy[i, j - 1];
                        MCurv[i, j] = ddx[i, j] + ddy[i, j];
                    }
                }
            }
        }
        public void Sobel()
        {
            /* 0.5 1 0.5
             *  1 -6 1
             * 0.5 1 0.5
             */
            MPenalize = new double[oriimage1.Width, oriimage1.Height];
            for (int i = 0; i < oriimage1.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    if (i == 0 || j == 0 || i == (oriimage1.Width - 1) || j == (oriimage1.Height - 1))
                    {
                        MPenalize[i, j] = 0;
                    }
                    else
                    {
                        MPenalize[i, j] = 0.5 * oriimage1.GetPixel(i - 1, j - 1).R + oriimage1.GetPixel(i, j - 1).R + 0.5 * oriimage1.GetPixel(i + 1, j - 1).R
                                      + oriimage1.GetPixel(i - 1, j).R - 6 * oriimage1.GetPixel(i, j).R + oriimage1.GetPixel(i + 1, j).R
                                      + 0.5 * oriimage1.GetPixel(i - 1, j + 1).R + oriimage1.GetPixel(i, j + 1).R + 0.5 * oriimage1.GetPixel(i + 1, j + 1).R;
                        if (MPenalize[i, j] > 255)
                        {
                            MPenalize[i, j] = 255;
                        }
                        if (MPenalize[i, j] < 0)
                        {
                            MPenalize[i, j] = 0;
                        }
                    }
                }
            }
        }
        public void plot1()
        {
            oriimage0 = new Bitmap(oriimage1, tmpsiz); ;

            double au;
            Kontur = new double[oriimage1.Width, oriimage1.Height];
            for (int i = 0; i < oriimage1.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    if (i == 0 || j == 0 || i == (oriimage1.Width - 1) || j == (oriimage1.Height - 1))
                    {
                        dx[i, j] = 0;
                        dy[i, j] = 0;
                    }
                    else
                    {
                        dx[i, j] = (MHeaviside[i + 1, j] - MHeaviside[i, j]) / Math.Sqrt(Math.Pow(MHeaviside[i + 1, j] - MHeaviside[i, j], 2) + Math.Pow((MHeaviside[i, j + 1] - MHeaviside[i, j - 1]) / 2, 2) + 0.0000000001);
                        dy[i, j] = (MHeaviside[i, j + 1] - MHeaviside[i, j]) / Math.Sqrt(Math.Pow((MHeaviside[i + 1, j] - MHeaviside[i - 1, j]) / 2, 2) + Math.Pow(MHeaviside[i, j + 1] - MHeaviside[i, j], 2) + 0.0000000001);
                    }
                    au = Math.Sqrt(Math.Pow(dx[i, j], 2) + Math.Pow(dy[i, j], 2));
                    if (au != 0)
                    {
                        Kontur[i, j] = 1;
                    }
                    else
                    {
                        Kontur[i, j] = 0;
                    }
                }
            }
            for (int i = 0; i < oriimage1.Width; i++)
            {
                for (int j = 0; j < oriimage1.Height; j++)
                {
                    if (Kontur[i, j] == 1)
                    {
                        oriimage0.SetPixel(i, j, Color.Red);
                    }
                }
            }
            panel1.BackgroundImage = oriimage0;
            panel1.Refresh();
        }
    }
}
