using System.Drawing;
using System.Drawing.Imaging;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

class Program
{
    static bool DeleteOriginal = false;
    static long Quality = 90L;

    static void Main(string[] args)
    {
        if (args.Length == 0)
            return;

        // --- 追加: quality と delete の解析 ---
        ParseOptions(args);

        foreach (string targetPath in args)
        {
            if (targetPath.StartsWith("--"))
                continue;

            if (Directory.Exists(targetPath))
                ConvertFolder(targetPath);
            else if (File.Exists(targetPath) && Path.GetExtension(targetPath).ToLower() == ".png")
                ConvertFile(targetPath);
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

        try
        {
            Console.WriteLine($"[INFO] Converting: {filePath}");

            using (Image image = Image.FromFile(filePath))
            {
                ImageCodecInfo jpegEncoder = GetEncoder(ImageFormat.Jpeg);
                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, Quality);

                image.Save(outputPath, jpegEncoder, encoderParams);
            }

            Console.WriteLine($"[OK] Saved: {outputPath}");

            if (DeleteOriginal && File.Exists(outputPath))
            {
                File.Delete(filePath);
                Console.WriteLine($"[DEL] Deleted original: {filePath}");
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
