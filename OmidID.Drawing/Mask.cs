using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace OmidID.Drawing
{
    public static class Mask
    {

        public unsafe static Bitmap MaskTo(this Bitmap Input, Bitmap Mask)
        {
            Bitmap output = new Bitmap(Input.Width, Input.Height, PixelFormat.Format32bppArgb);

            var rect = new Rectangle(0, 0, Input.Width, Input.Height);
            var bitsMask = Mask.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bitsInput = Input.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bitsOutput = output.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                for (int y = 0; y < Input.Height; y++)
                {
                    byte* ptrMask = (byte*)bitsMask.Scan0 + y * bitsMask.Stride;
                    byte* ptrInput = (byte*)bitsInput.Scan0 + y * bitsInput.Stride;
                    byte* ptrOutput = (byte*)bitsOutput.Scan0 + y * bitsOutput.Stride;

                    for (int x = 0; x < Input.Width; x++)
                    {
                        ptrOutput[4 * x] = ptrInput[4 * x];
                        ptrOutput[4 * x + 1] = ptrInput[4 * x + 1];
                        ptrOutput[4 * x + 2] = ptrInput[4 * x + 2];
                        ptrOutput[4 * x + 3] = ptrMask[4 * x];
                    }
                }
            
            Mask.UnlockBits(bitsMask);
            Input.UnlockBits(bitsInput);
            output.UnlockBits(bitsOutput);

            return output;
        }

    }
}
