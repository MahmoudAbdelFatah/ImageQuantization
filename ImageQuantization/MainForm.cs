using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;
        RGBPixel[,] Result;
        RGBPixel[,] ExpectedResult;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);    
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value ;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

        private void Quantize_click(object sender, EventArgs e)
        {
            if (ImageMatrix != null)
            {                 
                int ClustersCount =(int) numericUpDown1.Value;

                Result = ImageOperations.Quantize(ImageMatrix, ClustersCount);
                ImageOperations.DisplayImage(Result, pictureBox2);

                txtWidth.Text = ImageOperations.GetWidth(Result).ToString();
                txtHeight.Text = ImageOperations.GetHeight(Result).ToString();
            }
        }

        private void compare_click(object sender, EventArgs e)
        {
           
            double wright = 0, wrong = 0;
                        
            for (int i = 0; i < ImageOperations.Height; i++)
            {
                for (int k = 0; k < ImageOperations.Width; k++)
                {
                    if (Result[i, k].CompareTo(ExpectedResult[i, k]) == 0)
                    {
                        wright++;
                    }
                    else
                    {
                        wrong++;
                    }
                }
            }
            MessageBox.Show("right = " + wright.ToString() + "\n Wrong = " + wrong.ToString() + "\n");

        }

        private void openTC_result_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ExpectedResult = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ExpectedResult, pictureBox3);                
            }
            //txtWidth.Text = ImageOperations.GetWidth(TC_OutPut).ToString();
            //txtHeight.Text = ImageOperations.GetHeight(TC_OutPut).ToString();
        }

        private void CountColors_Click(object sender, EventArgs e)
        {
            int colorsCount = ImageOperations.CountDisincets(Result);
            MessageBox.Show("the Result has " + colorsCount + " Colors");
        }

       

       

       
       
    }
}