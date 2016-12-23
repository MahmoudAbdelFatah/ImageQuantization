using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.IO;

using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Collections.ObjectModel;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel : IComparable<RGBPixel>
    {
        public byte red, green, blue;

        public RGBPixel(byte r, byte g, byte b)
        {
            this.red = r;
            this.blue = b;
            this.green = g;

        }
        public int CompareTo(RGBPixel that)
        {

            if (this.red == that.red && this.blue == that.blue && this.green == that.green)
                return 0;
            if (this.red > that.red || (this.green > that.green && (this.red == that.red))
                || (this.blue > that.blue && (this.green == that.green) && (this.red == that.red)))
            {
                return 1;
            }

            return -1;
        }

    }



    public struct Vertex
    {

        public double weight;
        public RGBPixel Color;
        public int ClusterID;

        public static int BaseClusterID = 1;

        //public Vertex()
        //{
        //    this.weight = 0;
        //    this.Color = new RGBPixel();
        //    this.ClusterID = 0;
        //}

        public Vertex(RGBPixel rgb)
        {
            this.weight = 0;
            this.Color = rgb;
            this.ClusterID = 0;
        }

        public Vertex(double w, RGBPixel rgb)
        {
            this.weight = w;
            this.Color = rgb;
            this.ClusterID = 0;
        }
        public Vertex(double w, RGBPixel rgb, int ClusterID)
        {
            this.weight = w;
            this.Color = rgb;
            this.ClusterID = ClusterID;
        }

        public void SetWeight(double Weight)
        {
            this = new Vertex(Weight, this.Color, this.ClusterID);
            //this.weight = Weight;
        }
        public void TerminateDistination()
        {
            this = new Vertex(0, this.Color, this.ClusterID);
            //this.weight = 0;
        }
        public bool hasDistination()
        {
            return weight == 0 ? false : true;
        }
        public int AssignNew_ClusterID()
        {
            this = new Vertex(this.weight, this.Color, BaseClusterID);
            //ClusterID = BaseClusterID;
            BaseClusterID++;
            return ClusterID;
        }

        public static void ResetBaseID()
        {
            BaseClusterID = 1;
        }

        public bool hasClusterID()
        {
            return ClusterID == 0 ? false : true;
        }
        public int getClusterID()
        {
            return ClusterID;
        }
        public void SetClusterID(int CID)
        {
            this = new Vertex(this.weight, this.Color, CID);
            //this.ClusterID = CID;

        }
        public String ToString()
        {
            return this.Color.red + "," + Color.green + "," + Color.blue + "\t  w = " + weight.ToString() + "\t ID = " + ClusterID;
        }


        // to solv the List of Struct Problem
        public static Vertex SetWeight(double weight, Vertex v)
        {
            return new Vertex(weight, v.Color, v.ClusterID);
        }
        public static Vertex SetClusterID(int CID, Vertex v)
        {
            return new Vertex(v.weight, v.Color, CID);
        }

    }

    public class Cluster
    {
        public int ClusterID;
        public int red, green, blue;
        public int Count;

        private static int BaseID = 1;
        public Cluster()
        {
            ClusterID = BaseID;
            BaseID++;
            red = blue = green = Count = 0;
        }

        public RGBPixel GetAVG()
        {
            return new RGBPixel((byte)(red / Count), (byte)(green / Count), (byte)(blue / Count));
        }
    }






    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        /// 
        public static Vertex[, ,] mst3D;

        public static List<Vertex> distinct;


        public static System.DateTime t1;
        public static System.DateTime t2;
        public static System.DateTime t3;
        public static System.DateTime t4;
        public static String Out = "";
        public static int Height;
        public static int Width;


        public static RGBPixel[,] OpenImage(string ImagePath)
        {




            Bitmap original_bm = new Bitmap(ImagePath);
            Height = original_bm.Height;
            Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }


            return Buffer;

        }

        public static RGBPixel[,] Quantize(RGBPixel[,] img ,int ClustersCount)
        {
            ConstructMST(img);
            Cluster[] Clusters = Cluster(ClustersCount);
            if (Clusters != null)
            {
                for (int i = 0; i < Width; i++)
                {
                    for (int k = 0; k < Height; k++)
                    {
                        // mst3D [r,g,b].CusterID 
                        // Clusters[ClusterID-1] . AVG 
                        int r, g, b;
                        r = img[i, k].red;
                        g = img[i, k].green;
                        b = img[i, k].blue;
                        img[i, k] = Clusters[mst3D[r, g, b].ClusterID - 1].GetAVG();
                    }
                }
            }          
            return img;
        }


        public static void ConstructMST(RGBPixel[,] img)
        {

            mst3D = new Vertex[256, 256, 256];
            t1 = t2 = t3 = t4 = DateTime.Now;

            distinct = new List<Vertex>();

            Boolean[, ,] distinctColors = new Boolean[256, 256, 256];

            int Count = 0;
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    if (!distinctColors[img[i, j].blue, img[i, j].green, img[i, j].red])
                    {
                        distinct.Add(new Vertex(new RGBPixel(img[i, j].red, img[i, j].green, img[i, j].blue)));
                        distinctColors[img[i, j].blue, img[i, j].green, img[i, j].red] = true;
                        Count++;
                    }
                }
            }


            int index = 0; double minWeight = 0;
            int currentIndex = 0;
            int Cnt = distinct.Count;
            for (int i = 1; i < Cnt; i++)
            {
                double current_weight = getWeight(distinct[0].Color, distinct[i].Color);
                if (i == 1)
                {
                    minWeight = current_weight;
                    index = 1;
                }
                else
                    if (current_weight < minWeight)
                    {
                        minWeight = current_weight;
                        index = i;
                    }
                distinct[i] = new Vertex(current_weight, distinct[i].Color);

                mst3D[distinct[i].Color.red, distinct[i].Color.green, distinct[i].Color.blue] =
                     new Vertex(distinct[i].weight, distinct[0].Color);

            }

            Cnt--;
            if (Cnt == index)
                index = currentIndex;
            swap(Cnt, currentIndex);

            while (Cnt != 0)
            {
                currentIndex = index;

                for (int j = 0; j < Cnt; j++)
                {

                    {
                        if (currentIndex != j)
                        {

                            double current_weight = getWeight(distinct[currentIndex].Color, distinct[j].Color);

                            if (j == 0 || currentIndex == 0 && j == 1)
                            {
                                minWeight = current_weight;
                                index = j;
                            }
                            else
                                if (current_weight < minWeight)
                                {
                                    minWeight = current_weight;
                                    index = j;
                                }
                            double vertexweigt = distinct[j].weight;
                            if (vertexweigt > current_weight)
                            {

                                mst3D[distinct[j].Color.red, distinct[j].Color.green, distinct[j].Color.blue] =
                                     new Vertex(current_weight, distinct[currentIndex].Color);
                                distinct[j] = new Vertex(current_weight, distinct[j].Color);
                            }
                            else
                            {
                                if (minWeight > vertexweigt)
                                {
                                    minWeight = vertexweigt;
                                    index = j;
                                }
                            }
                        }
                    }
                }

                Cnt--;
                if (Cnt == index)
                    index = currentIndex;
                swap(Cnt, currentIndex);


            }


            t2 = DateTime.Now;
            //Check the Out in MessageBox
            Out += "    MST   \t" + (t2 - t1).ToString() + "\n";
                         
        }


        // Assignes for Each Color in mst3D a ClusterID , Calculate the AVG Color for each Cluster
        /*
         * Return Null OR Cluster[]
         */
        public static Cluster[] Cluster(int ClusersCount)
        {
            //the clusters Count Can NOT be greater than the Colors Count or == 0
            if (ClusersCount <= 0 || ClusersCount >= distinct.Count)
                return null;


            //// get the 3D vertices into List
            //distinct contain all the colors

            t3 = DateTime.Now;

            ////cutting max # weights

            //the cuts = ClusterCount - 1
            for (int i = 0; i < ClusersCount - 1; i++)                         //--- O(K)*O(D) = O(K*D)
            {
                //find the max weight
                double maxWeight = -1;
                int maxWeightIndex = -1;
                for (int k = 0; k < distinct.Count; k++)                    //--- O(D)*O(1) = O(D)
                {
                    // MST has only one Vertixe as a destination but NOT a Source to any other
                    if (!distinct[k].hasDistination() && !distinct[k].hasClusterID())                    //--- O(1)
                    {
                        //Initiate it in the mst3D AND assign it with a New ClusetID 
                        mst3D[distinct[k].Color.red, distinct[k].Color.green, distinct[k].Color.blue] = new Vertex(0, new RGBPixel());//--- O(1)

                        int cID = mst3D[distinct[k].Color.red, distinct[k].Color.green, distinct[k].Color.blue].AssignNew_ClusterID();//--- O(1)

                        distinct[k] = Vertex.SetClusterID(cID, distinct[k]);

                    }
                    else if (distinct[k].weight > maxWeight)                //--- O(1)
                    {
                        maxWeight = distinct[k].weight;                     //--- O(1)
                        maxWeightIndex = k;                                 //--- O(1)
                    }
                }
                distinct[maxWeightIndex] = Vertex.SetWeight(-1, distinct[maxWeightIndex]);              //--- O(1)

                int r, g, b;
                r = distinct[maxWeightIndex].Color.red;                     //--- O(1)
                g = distinct[maxWeightIndex].Color.green;                   //--- O(1)
                b = distinct[maxWeightIndex].Color.blue;                    //--- O(1)
                //make it has no destination
                mst3D[r, g, b].TerminateDistination();                      //--- O(1)
            }


            //// Accumelate each Cluster Colors, Calculate each Cluster AVG
            Cluster[] Clusters = new Cluster[Math.Max(1, ClusersCount)];
            for (int i = 0; i < Clusters.GetLength(0); i++)
                Clusters[i] = new Cluster();


            //assign the Cluster ID :- loopes throught the distinct(s) assigning  ClusterIDs
            for (int i = 0; i < distinct.Count; i++)  //--- O(D)
            {
                //Assign
                int r, g, b;
                r = distinct[i].Color.red;
                g = distinct[i].Color.green;
                b = distinct[i].Color.blue;
                int C_ID = AssignClusterID(r, g, b);
                distinct[i] = Vertex.SetClusterID(C_ID, distinct[i]);

                //to calculate the AVG Color of Each Cluster
                Clusters[C_ID - 1].red += r;
                Clusters[C_ID - 1].green += g;
                Clusters[C_ID - 1].blue += b;
                Clusters[C_ID - 1].Count++;
            }

            t4 = DateTime.Now;


            ////Check for Clustering
            Out += "Clustering\t" + (t4 - t3).ToString() + "\n" + Environment.NewLine;
            Out += "Tootal    \t" + (t4 - t1).ToString() + "\n" + Environment.NewLine;

            Out += "Clusters Count = " + (ClusersCount).ToString() + "\n\n" + Environment.NewLine;

            //for (int i = 0; i < Clusters.GetLength(0) ; i++)
            //{
            //    Out += "Cluster # " + (i + 1).ToString() + '\n' + Environment.NewLine;
            //    Out += "SUM(red) = " + Clusters[i].red + " SUM(green) = " + Clusters[i].green + " Sum(blue) = " + Clusters[i].blue + "\n" + Environment.NewLine;
            //    Out += "Cout = " + Clusters[i].Count + "\n AVG = " +
            //        Clusters[i].red / Clusters[i].Count + ',' +
            //        Clusters[i].green / Clusters[i].Count + ',' +
            //        Clusters[i].blue / Clusters[i].Count + "\n\n" + Environment.NewLine;

            //}



            //string path = @"d:\MyTest.txt";
            //// This text is added only once to the file.
            //if (!File.Exists(path))
            //{
            //    // Create a file to write to.
            //    string createText = "Hello and Welcome" + Environment.NewLine;
            //    File.WriteAllText(path, createText);
            //}
            //// This text is always added, making the file longer over time
            //// if it is not deleted.
            //File.AppendAllText(path, Out);


            MessageBox.Show(Out);
            //clear
            Vertex.ResetBaseID();
            Out = "";

            return Clusters;
        }


        // Assigns a ClusterId to the rgb color into the mst3D
        public static int AssignClusterID(int r, int g, int b)
        {

            if (mst3D[r, g, b].hasClusterID())
            {
                return mst3D[r, g, b].getClusterID(); //--- O(1)
            }
            else
            {
                if (!mst3D[r, g, b].hasDistination())
                {
                    return mst3D[r, g, b].AssignNew_ClusterID(); //--- O(1)
                }
                else
                {
                    RGBPixel distination = mst3D[r, g, b].Color;
                    AssignClusterID((byte)distination.red, (byte)distination.green, (byte)distination.blue);
                    int distClusterID = mst3D[distination.red, distination.green, distination.blue].getClusterID();
                    mst3D[r, g, b].SetClusterID(distClusterID);

                    return distClusterID;
                }
            }
        }


        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }
        private static double getWeight(RGBPixel rgb1, RGBPixel rgb2)
        {
            double rgb = (rgb1.red - rgb2.red) * (rgb1.red - rgb2.red) +
                                   (rgb1.green - rgb2.green) * (rgb1.green - rgb2.green) +
                                   (rgb1.blue - rgb2.blue) * (rgb1.blue - rgb2.blue);
            return rgb;
        }
        public static void swap(int p1, int p2)
        {
            Vertex tmp = distinct[p1];
            distinct[p1] = distinct[p2];
            distinct[p2] = tmp;

        }
        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public struct RGBPixelD
        {
            public double red, green, blue;
        }
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }


    }
}
