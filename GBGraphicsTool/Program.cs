using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace GBGraphicsTool
{
    class Program
    {
        static void Main(string[] args)
        {
            int width = 128, bitDepth = 0;
            string path;
            Bitmap image;
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: GBGraphicsTool <image path> -b[1|2] -w[num]");
                Console.WriteLine("-b: bit depth (e.g. -b2)");
                Console.WriteLine("-w: width (e.g. -w32");
                return;
            } else {
                path = args[0];
                string extension = path.Substring(path.LastIndexOf("."), path.Length - path.LastIndexOf("."));
                bitDepth = extension == ".2bpp" ? 2 : extension == ".1bpp" ? 1 : 2;
                if (args.Length > 1)
                {
                    for(int i = 1; i < args.Length; i++)
                    {
                        if (args[i].Contains("-b"))
                        {
                            bitDepth = int.Parse(args[i].Replace("-b", ""));
                        }
                        if (args[i].Contains("-w"))
                        {
                            width = int.Parse(args[i].Replace("-w", ""));
                        }
                    }


                }
                byte[] imageData = File.ReadAllBytes(path);
                int height = 8 * (int)MathF.Ceiling((float)(imageData.Length) / (float)(width/8 * 8 * bitDepth));
                image = new Bitmap(width, height);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        image.SetPixel(x, y, Color.White);

                    }
                }
                int xPos = 0;
                int yPos = 0;
                for (int i = 0; i < width * height; i += 8 * bitDepth)
                {
                    if ((i < imageData.Length && bitDepth == 1) || ((i + 1 < imageData.Length) && bitDepth == 2))
                    {
                        int highBit = 0, lowBit = 0, colorVal = 0;
                    for (int x = 0; x < 8; x++)
                    {
                            for (int y = 0; y < 8; y++)
                                {
                                    if (bitDepth == 2)
                                    {
                                        highBit = (imageData[i + 2*y + 1] >> (7 - x))&0x01;
                                        lowBit = (imageData[i + 2*y] >> (7 - x))&0x01;
                                        int value = (highBit << 1) | lowBit;
                                        colorVal = (int)(255f * ((float)(3-value) / 3f));
                                    }
                                    else
                                    {                               
                                        int value = 1 - ((imageData[i + y] >> (7 - x)) & 0x01);
                                        colorVal = value * 255;
                                    }
                                    Color color = Color.FromArgb(colorVal, colorVal, colorVal);
                                    image.SetPixel(x + (xPos * 8),y  + (yPos * 8), color);

                                }
                            
                    }
                    xPos++;
                    if(xPos >= (width / 8))
                    {
                          xPos = 0;
                          yPos++;
                    }
                    }
                }
                image.Save(path.Replace(extension, ".png"));
            }
        }
    }
}
