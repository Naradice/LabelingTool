using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace Labeling
{
    class FileUtils
    {

        private Windows.Storage.Pickers.FolderPicker folderPicker = new Windows.Storage.Pickers.FolderPicker();
        private string  LABEL_PATH = "label";
        private string LABEL_FILENAME = "label.txt";
        private string DEFAULT_LABEL_NAME = "label name";
        private string CLIP_FOLDER_NAME = "clip";

        public string Path = "";

        public FileUtils()
        {
            folderPicker.FileTypeFilter.Add(".png");
            folderPicker.FileTypeFilter.Add(".jpg");
            folderPicker.FileTypeFilter.Add(".bmp");
        }

        /// <summary>
        /// Save image based on image class. If you want to save an original image after converting the file like Color2Gray, set original softwarebitmap instead.
        /// </summary>
        /// <param name="image">image</param>
        /// <param name="fileSuffix">filename suffix. finame is created by OriginalFilename_Suffix</param>
        /// <returns></returns>
        public async Task<bool> saveImage(Images image, string fileSuffix)
        {

            var outputFile = await getClipFilePath(image, fileSuffix);
            SaveSoftwareBitmapToFile(image.softwareBitmap, image.encoderId, outputFile);
            return true;
        }

        /// <summary>
        /// save image from softwarebitmap
        /// </summary>
        /// <param name="image">softwarebitmap</param>
        /// <param name="enoder">Guid defined in Bitmapencoder</param>
        /// <param name="imagePath">Clipped image is saved in subfolder of this path.</param>
        /// <param name="fileSuffix">filename suffix. finame is created by OriginalFilename_Suffix</param>
        /// <returns></returns>
        public async Task<bool> saveImage(SoftwareBitmap image, Guid enoder, string imagePath, string fileSuffix)
        {

            var outputFile = await getClipFilePath(imagePath, fileSuffix);
            SaveSoftwareBitmapToFile(image, enoder, outputFile);
            return true;
        }

        /// <summary>
        /// save image
        /// </summary>
        /// <param name="softwareBitmap"></param>
        /// <param name="enoder"></param>
        /// <param name="outputFile"></param>
        private async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap, Guid enoder, StorageFile outputFile)
        {
            using (IRandomAccessStream stream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Create an encoder with the desired format
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(enoder, stream);

                // Set the software bitmap
                encoder.SetSoftwareBitmap(softwareBitmap);

                // Set additional encoding parameters, if needed
                //encoder.BitmapTransform.ScaledWidth = 320;
                //encoder.BitmapTransform.ScaledHeight = 240;
                //encoder.BitmapTransform.Rotation = Windows.Graphics.Imaging.BitmapRotation.Clockwise90Degrees;
                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
                encoder.IsThumbnailGenerated = true;

                try
                {
                    await encoder.FlushAsync();
                }
                catch (Exception err)
                {
                    const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);
                    switch (err.HResult)
                    {
                        case WINCODEC_ERR_UNSUPPORTEDOPERATION:
                            // If the encoder does not support writing a thumbnail, then try again
                            // but disable thumbnail generation.
                            encoder.IsThumbnailGenerated = false;
                            break;
                        default:
                            throw;
                    }
                }

                if (encoder.IsThumbnailGenerated == false)
                {
                    await encoder.FlushAsync();
                }


            }
        }

        private async Task<Windows.Storage.StorageFile> getClipFilePath(Images image, string fileSuffix)
        {
            return await getClipFilePath(image.ImageLocation, fileSuffix);
        }

        private async Task<Windows.Storage.StorageFile> getClipFilePath(string imagePath, string fileSuffix)
        {
            string path = "";
            string filename = "";
            try
            {
                path = System.IO.Path.GetDirectoryName(imagePath);
                Windows.Storage.StorageFolder storageFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(path);
                storageFolder = await storageFolder.CreateFolderAsync(CLIP_FOLDER_NAME, Windows.Storage.CreationCollisionOption.OpenIfExists);
                filename = System.IO.Path.GetFileNameWithoutExtension(imagePath) + "_" + fileSuffix + System.IO.Path.GetExtension(imagePath);
                return await storageFolder.CreateFileAsync(filename, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Debug.WriteLine($"Error::: ImageViewModel:: cannot open the file {filename} of {path} in getLabelFilePath");
                return null;
            }

        }


        public async Task<bool> loadImageFiles(IList<string> labels, IList<Windows.Storage.StorageFile> files)
        {
            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("label", folder);
                //need ex handling
                try
                {
                    IReadOnlyList<Windows.Storage.StorageFile> tempFiles = await folder.GetFilesAsync();
                    CopyList2List(tempFiles, files);
                    Windows.Storage.StorageFolder labelFolder = await folder.CreateFolderAsync(LABEL_PATH, Windows.Storage.CreationCollisionOption.OpenIfExists);
                    Windows.Storage.StorageFile file = await labelFolder.CreateFileAsync(LABEL_FILENAME, Windows.Storage.CreationCollisionOption.OpenIfExists);
                    IList<string> tempLabels = await Windows.Storage.FileIO.ReadLinesAsync(file, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                    CopyList2List(tempLabels, labels);
                    if (labels.Count == 0)
                        labels.Add(DEFAULT_LABEL_NAME);
                    this.Path = labelFolder.Path;
                    return true;
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    Debug.WriteLine("file not found:" + ex.ToString());
                    return false;
                }
                catch (System.IO.IOException ex)
                {
                    Debug.WriteLine("file exeption:" + ex.ToString());
                    return false;
                }
                catch (System.UnauthorizedAccessException ex)
                {
                    Debug.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private void CopyList2List(IReadOnlyList<Windows.Storage.StorageFile> source, IList<Windows.Storage.StorageFile> target)
        {
            if(source !=  null && target != null)
            {
                foreach(var item in source)
                {
                    target.Add(item);
                }
            }
        }

        private void CopyList2List(IList<string> source, IList<string> target)
        {
            if (source != null && target != null)
            {
                foreach (var item in source)
                {
                    target.Add(item);
                }
            }
        }
    }
}
