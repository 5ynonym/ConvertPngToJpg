using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.VisualBasic.FileIO;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

class Program
{
    static bool DeleteOriginal = false;
    static long Quality = 90L;

    static void Main(string[] args)
    {
        if (args.Length == 0)
            return;

        ParseOptions(args);

        foreach (string targetPath in args)
        {
            if (targetPath.StartsWith("--"))
                continue;

            if (Directory.Exists(targetPath))
                ConvertFolder(targetPath);
            else if (File.Exists(targetPath) && Path.GetExtension(targetPath).ToLower() == ".png")
                ConvertFile(targetPath);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);
        }
    }

    static void ParseOptions(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--delete")
            {
                DeleteOriginal = true;
                Console.WriteLine("[INFO] Delete mode enabled");
            }
            else if (args[i] == "--quality" && i + 1 < args.Length)
            {
                if (long.TryParse(args[i + 1], out long q))
                {
                    Quality = Math.Clamp(q, 0, 100);
                    Console.WriteLine($"[INFO] Quality set to {Quality}");
                }
            }
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
        string outputPath = Path.Combine(
            Path.GetDirectoryName(filePath),
            Path.GetFileNameWithoutExtension(filePath) + ".jpg"
        );

        bool saved = false;

        try
        {
            using (Image image = Image.FromFile(filePath))
            {
                ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);
                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, Quality);

                image.Save(outputPath, jpegEncoder, encoderParams);
            }

            try
            {
                using (var test = Image.FromFile(outputPath))
                {
                    saved = true;
                }
            }
            catch
            {
                saved = false;
            }

            if (saved)
            {
                Console.WriteLine($"[OK] Saved: {outputPath}");

                if (DeleteOriginal)
                {
                    FileSystem.DeleteFile(
                        filePath,
                        UIOption.OnlyErrorDialogs,
                        RecycleOption.SendToRecycleBin
                    );
                    Console.WriteLine($"[DEL] Sent to Recycle Bin: {filePath}");
                }
            }
            else
            {
                Console.WriteLine($"[ERR] Save failed (file not readable): {outputPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERR] Failed: {filePath} ({ex.Message})");
        }
    }

    static ImageCodecInfo GetEncoder(ImageFormat format)
    {
        foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
            if (codec.FormatID == format.Guid)
                return codec;
        return null;
    }
}
