using System;
using OpenCvSharp;
using Windows.Graphics.Imaging;

namespace Labeling
{
    public sealed class OpenCV : IDisposable
    {
        private BackgroundSubtractorMOG2 mog2;

        public OpenCV()
        {
            mog2 = BackgroundSubtractorMOG2.Create();
        }

        public void Dispose()
        {
            mog2.Dispose();
        }

        public void Blur(SoftwareBitmap input, SoftwareBitmap output)
        {
            Mat mInput = SoftwareBitmap2Mat(input);
            Mat mOutput = new Mat(mInput.Rows, mInput.Cols, MatType.CV_8UC4);

            //set size
            Cv2.Blur(mInput, mOutput,Size.Zero);
            Mat2SoftwareBitmap(mOutput, output);
        }

            
        public void HoughLines(SoftwareBitmap input, SoftwareBitmap output, Algorithm algorithm)
        {
                    Mat mInput = SoftwareBitmap2Mat(input);
                    Mat mOutput = new Mat(mInput.Rows, mInput.Cols, MatType.CV_8UC4);
                    mInput.CopyTo(mOutput);
                    Mat gray = mInput.CvtColor(ColorConversionCodes.BGRA2GRAY);
                    Mat edges = gray.Canny(50, 200);

            var res = Cv2.HoughLinesP(edges, 0, 0, 0);
                        /*
                        rho: (double)algorithm.AlgorithmProperties[0].CurrentValue,
                        theta: (double)algorithm.AlgorithmProperties[1].CurrentValue / 100.0,
                        threshold: (int)algorithm.AlgorithmProperties[2].CurrentValue,
                        minLineLength: (double)algorithm.AlgorithmProperties[3].CurrentValue,
                        maxLineGap: (double)algorithm.AlgorithmProperties[4].CurrentValue
                        */

            for (int i = 0; i < res.Length; i++)
            {
                Cv2.Line(mOutput, res[i].P1, res[i].P2, Scalar.AliceBlue);
                    /*
                    color: (Scalar)algorithm.AlgorithmProperties[5].CurrentValue,
                    thickness: (int)algorithm.AlgorithmProperties[6].CurrentValue,
                    lineType: (LineTypes)algorithm.AlgorithmProperties[7].CurrentValue
                    */
            }

            Mat2SoftwareBitmap(mOutput, output);
        }

        public async void Contours(SoftwareBitmap input, SoftwareBitmap output, Algorithm algorithm)
        {
            Mat mInput = SoftwareBitmap2Mat(input);
            Mat mOutput = new Mat(mInput.Rows, mInput.Cols, MatType.CV_8UC4);
            mInput.CopyTo(mOutput);
            Mat gray = mInput.CvtColor(ColorConversionCodes.BGRA2GRAY);
            Mat edges = gray.Canny(0, 0);
            //(double)algorithm.AlgorithmProperties[6].CurrentValue, (double)algorithm.AlgorithmProperties[7].CurrentValue);

            Cv2.FindContours(
                image: edges,
                contours: out OpenCvSharp.Point[][] contours,
                hierarchy: out HierarchyIndex[] outputArray,
                RetrievalModes.CComp,
                ContourApproximationModes.ApproxNone);
                /*
                mode: (RetrievalModes)algorithm.AlgorithmProperties[0].CurrentValue,
                method: (ContourApproximationModes)algorithm.AlgorithmProperties[1].CurrentValue,
                offset: (Point)algorithm.AlgorithmProperties[2].CurrentValue);
                */

            int maxLen = 0;
            int maxIdx = -1;

            for (int i = 0; i < contours.Length; i++)
            {
                if (contours[i].Length > maxLen)
                {
                    maxIdx = i;
                    maxLen = contours[i].Length;
                }

                if (contours[i].Length > 10)
                {
                    Cv2.DrawContours(
                        mOutput,
                        contours,
                        contourIdx: i,
                        Scalar.AliceBlue);
                        /*
                        color: (Scalar)algorithm.AlgorithmProperties[3].CurrentValue,
                        thickness: (int)algorithm.AlgorithmProperties[4].CurrentValue,
                        lineType: (LineTypes)algorithm.AlgorithmProperties[5].CurrentValue,
                        hierarchy: outputArray,
                        maxLevel: 0);
                        */

                }
            }
            if (maxIdx != -1)
            {
                var res = Cv2.ApproxPolyDP(contours[maxIdx], 1, true);
                //Cv2.DrawContours(
                //    mOutput,
                //    contours,
                //    maxIdx,
                //    (Scalar)algorithm.algorithmProperties[3].CurrentValue,
                //    (int)algorithm.algorithmProperties[4].CurrentValue,
                //    (LineTypes)algorithm.algorithmProperties[5].CurrentValue,
                //    outputArray,
                //    0);
                ////return Cv2.ContourArea(res);
            }

            Mat2SoftwareBitmap(mOutput, output);

            // Must run on UI thread.  The winrt container also needs to be set.
            /*
            if (App.container != null)
            {
                await App.container.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Cv2.ImShow("Contours", mOutput);
                });
            }
            */
        }

        public void Canny(SoftwareBitmap input, SoftwareBitmap output, Algorithm algorithm)
        {
            Mat mInput = SoftwareBitmap2Mat(input);
            Mat mOutput = new Mat(mInput.Rows, mInput.Cols, MatType.CV_8UC4);
            Mat intermediate = new Mat(mInput.Rows, mInput.Cols, MatType.CV_8UC4);

            Cv2.Canny(mInput, intermediate,0,0);
            /*
                threshold1: (double)algorithm.AlgorithmProperties[0].CurrentValue,
                threshold2: (double)algorithm.AlgorithmProperties[1].CurrentValue,
                apertureSize: (int)algorithm.AlgorithmProperties[2].CurrentValue);
            */

            Cv2.CvtColor(intermediate, mOutput, ColorConversionCodes.GRAY2BGRA);

            Mat2SoftwareBitmap(mOutput, output);
        }

        public void MotionDetector(SoftwareBitmap input, SoftwareBitmap output, Algorithm algorithm)
        {
            Mat mInput = SoftwareBitmap2Mat(input);
            Mat mOutput = new Mat(mInput.Rows, mInput.Cols, MatType.CV_8UC4);
            Mat fgMaskMOG2 = new Mat(mInput.Rows, mInput.Cols, MatType.CV_8UC4);
            Mat temp = new Mat(mInput.Rows, mInput.Cols, MatType.CV_8UC4);

            mog2.Apply(mInput, fgMaskMOG2);
                //, (double)algorithm.AlgorithmProperties[0].CurrentValue);
            Cv2.CvtColor(fgMaskMOG2, temp, ColorConversionCodes.GRAY2BGRA);

            Mat element = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
            Cv2.Erode(temp, temp, element);
            temp.CopyTo(mOutput);
            Mat2SoftwareBitmap(mOutput, output);
        }

        public void clipImage(SoftwareBitmap input, int left, int top, int width, int height, string path)
        {
            Mat mInput = SoftwareBitmap2Mat(input);
            Mat clipedMat = mInput.Clone(new Rect(left, top, width, height));
            if (!clipedMat.SaveImage(path))
            {
                Console.WriteLine("Can't save");
            }
        }

        public void clipImage(SoftwareBitmap input, RectangleItem rectItem, string path)
        {
            this.clipImage(input, (int)rectItem.ActualLeft, (int)rectItem.ActualTop, (int)rectItem.ActualWidth, (int)rectItem.ActualHeight, path);
        }

        public SoftwareBitmap getClippedSoftwareBitmap(SoftwareBitmap input, int left, int top, int width, int height)
        {
            Mat mInput = SoftwareBitmap2Mat(input);
            Mat clipedMat = mInput.Clone(new Rect(left, top, width, height));
            SoftwareBitmap outputBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8, clipedMat.Width, clipedMat.Height, BitmapAlphaMode.Premultiplied);
            Mat2SoftwareBitmap(clipedMat, outputBitmap);
            return outputBitmap;
        }

        public SoftwareBitmap getClippedSoftwareBitmap(SoftwareBitmap input, RectangleItem rectItem)
        {
            return getClippedSoftwareBitmap(input, (int)rectItem.ActualLeft, (int)rectItem.ActualTop, (int)rectItem.ActualWidth, (int)rectItem.ActualHeight);
        }

        public void ChangeColor2Gray(SoftwareBitmap input, SoftwareBitmap output)
        {
            Mat mInput = SoftwareBitmap2Mat(input);
            Mat mOutput = new Mat(mInput.Rows, mInput.Cols, MatType.CV_8UC4);
            Cv2.CvtColor(mInput, mOutput, ColorConversionCodes.BGRA2GRAY);
            Cv2.CvtColor(mOutput, mOutput, ColorConversionCodes.GRAY2BGRA);
            Mat2SoftwareBitmap(mOutput, output);
        }

        public static unsafe Mat SoftwareBitmap2Mat(SoftwareBitmap softwareBitmap)
        {
            using (BitmapBuffer buffer = softwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
            {
                using (var reference = buffer.CreateReference())
                {
                    ((IMemoryBufferByteAccess)reference).GetBuffer(out var dataInBytes, out var capacity);

                    Mat outputMat = new Mat(softwareBitmap.PixelHeight, softwareBitmap.PixelWidth, MatType.CV_8UC4, (IntPtr)dataInBytes);
                    return outputMat;
                }
            }
        }

        public static unsafe void Mat2SoftwareBitmap(Mat input, SoftwareBitmap output)
        {
            using (BitmapBuffer buffer = output.LockBuffer(BitmapBufferAccessMode.ReadWrite))
            {
                using (var reference = buffer.CreateReference())
                {
                    ((IMemoryBufferByteAccess)reference).GetBuffer(out var dataInBytes, out var capacity);
                    BitmapPlaneDescription bufferLayout = buffer.GetPlaneDescription(0);

                    for (int i = 0; i < bufferLayout.Height; i++)
                    {
                        for (int j = 0; j < bufferLayout.Width; j++)
                        {
                            dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 0] =
                                input.DataPointer[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 0];
                            dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 1] =
                                input.DataPointer[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 1];
                            dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 2] =
                                input.DataPointer[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 2];
                            dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 3] =
                                input.DataPointer[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 3];
                        }
                    }
                }
            }
        }
    }
}
