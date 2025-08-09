using System.Drawing;
using System.Drawing.Imaging;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
            return;

        foreach (string targetPath in args)
        {
            if (Directory.Exists(targetPath))
                ConvertFolder(targetPath);
            else if (File.Exists(targetPath) && Path.GetExtension(targetPath).ToLower() == ".png")
                ConvertFile(targetPath);
        }
    }

    static void ConvertFolder(string folderPath)
    {
        string[] pngFiles = Directory.GetFiles(folderPath, "*.png");

        foreach (string file in pngFiles)
            ConvertFile(file);
    }

    static void ConvertFile(string filePath)
    {
        try
        {
            using (Image image = Image.FromFile(filePath))
            {
                string outputPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".jpg");
                ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);
                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 90L);

                image.Save(outputPath, jpegEncoder, encoderParams);
            }
        }
        catch { }
    }

    static ImageCodecInfo GetEncoder(ImageFormat format)
    {
        foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
            if (codec.FormatID == format.Guid)
                return codec;
        return null;
    }
}