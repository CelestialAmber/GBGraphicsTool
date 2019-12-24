using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace GBGraphicsTool
{
    class Program
    {
        public static int rangeStartOffset, rangeLength;

        static void Main(string[] args)
        {
            int width = 128, bitDepth = 0;
            string path;
            Bitmap image;
            bool useFileRange = false;
            string outputFilename = "";

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: GBGraphicsTool <image path> -b -w -r -o");
                Console.WriteLine("Optional arguments:");
                Console.WriteLine("-b: bits per pixel(1 or 2)(e.g. -b2)");
                Console.WriteLine("-w: width(e.g. -w32)");
                Console.WriteLine("-r: file range(start offset, length in bytes)(e.g. -r 0x3f 0x200/512)");
                Console.WriteLine("-o: Output file name (e.g. -o font.png)");
                return;

            }
            else
            {
                path = args[0];
                string extension = Path.GetExtension(path);
                string filename = Path.GetFileNameWithoutExtension(path);
                bitDepth = extension == ".2bpp" ? 2 : extension == ".1bpp" ? 1 : 2;
                if (args.Length > 1)
                {
                    try
                    {
                        for (int i = 1; i < args.Length; i++)
                        {
                            if (args[i].Contains("-b"))
                            {
                                bitDepth = int.Parse(args[i].Replace("-b", ""));
                            }
                            if (args[i].Contains("-w"))
                            {
                                width = int.Parse(args[i].Replace("-w", ""));
                            }
                            if (args[i].Contains("-r"))
                            {
                                useFileRange = true;
                                string startOffsetString = args[i + 1].Replace("0x", "");
                                string lengthString = args[i + 2];
                                rangeStartOffset = Convert.ToInt32(startOffsetString, 16);
                                rangeLength = lengthString.Contains("0x") ? Convert.ToInt32(lengthString, 16) : int.Parse(lengthString);
                                i += 2;
                            }
                            if (args[i].Contains("-o"))
                            {
                                outputFilename = args[i + 1];
                                i++;

                            }
                        }
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        throw new Exception("Invalid syntax");
                    }

                    byte[] imageData = File.ReadAllBytes(path);
                    if (useFileRange)
                    {
                        imageData = imageData.Skip(rangeStartOffset).Take(rangeLength).ToArray();
                    }
                    int height = 8 * (int)Math.Ceiling((float)(imageData.Length) / (float)(width / 8 * 8 * bitDepth));
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
                            int highBit = 0, lowBit = 0, colorVal = 0;
                            for (int x = 0; x < 8; x++)
                            {
                                for (int y = 0; y < 8; y++)
                                {
                                if ((i + y >= imageData.Length && bitDepth == 1) || ((i + 2 * y + 1 >= imageData.Length) && bitDepth == 2)) break;
                                        if (bitDepth == 2)
                                        {
                                            highBit = (imageData[i + 2 * y + 1] >> (7 - x)) & 0x01;
                                            lowBit = (imageData[i + 2 * y] >> (7 - x)) & 0x01;
                                            int value = (highBit << 1) | lowBit;
                                            colorVal = (int)(255f * ((float)(3 - value) / 3f));
                                        }
                                        else
                                        {
                                            int value = ((imageData[i + y] >> (7 - x)) & 0x01);
                                            colorVal = (1-value) * 255;
                                        }
                                        Color color = Color.FromArgb(colorVal, colorVal, colorVal);
                                        image.SetPixel(x + (xPos * 8), y + (yPos * 8), color);
                                }
                            }
                            xPos++;
                            if (xPos >= (width / 8))
                            {
                                xPos = 0;
                                yPos++;
                            }
                    }
                    string outputFilePath = path.Replace(extension, ".png");
                    if (outputFilename != "")
                    {
                        try
                        {
                            string inputDir = Path.GetDirectoryName(outputFilePath);
                            outputFilePath = inputDir + (inputDir == "" ? "" : "/") + outputFilename;
                        }
                        catch (Exception e)
                        {
                            throw new Exception("Invalid file name.");
                        }
                    }
                    image.Save(outputFilePath);
                }
            }
        }
    }

}
