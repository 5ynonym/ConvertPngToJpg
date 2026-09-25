using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.VisualBasic.FileIO;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

public static class Program
{
    private static bool _deleteOriginal = false;
    private static long _quality = 90L;
    private static bool _recursive = false;
    private static bool _noPause = false;

    private static void Main(string[] args)
    {
        if (args.Length == 0)
            return;

        ParseOptions(args);

        foreach (string targetPath in args)
        {
            if (targetPath.StartsWith("--"))
                continue;

            if (Directory.Exists(targetPath))
            {
                Console.WriteLine($"\nConvert folder: {targetPath}");
                Console.WriteLine($"Recursive: {_recursive}, Delete original: {_deleteOriginal}, Quality: {_quality}");
                if (!_noPause)
                {
                    Console.Write("Press any key to continue...");
                    Console.ReadKey(true);
                    Console.WriteLine();
                }
                ConvertFolder(targetPath);
            }
            else if (File.Exists(targetPath) && Path.GetExtension(targetPath).ToLower() == ".png")
            {
                Console.WriteLine($"\nConvert file: {targetPath}");
                Console.WriteLine($"Delete original: {_deleteOriginal}, Quality: {_quality}");
                if (!_noPause)
                {
                    Console.Write("Press any key to continue...");
                    Console.ReadKey(true);
                    Console.WriteLine();
                }
                ConvertFile(targetPath);
            }

            if (!_noPause)
            {
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey(true);
            }
        }
    }

    private static void ParseOptions(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--delete" || args[i] == "-d")
            {
                _deleteOriginal = true;
                Console.WriteLine("[INFO] Delete mode enabled");
            }
            else if (args[i] == "--recursive" || args[i] == "-r")
            {
                _recursive = true;
                Console.WriteLine("[INFO] Recursive mode enabled (include subfolders)");
            }
            else if (args[i] == "--no-pause")
            {
                _noPause = true;
                Console.WriteLine("[INFO] Pause prompts disabled");
            }
            else if ((args[i] == "--quality" || args[i] == "-q") && i + 1 < args.Length)
            {
                if (long.TryParse(args[i + 1], out long q))
                {
                    _quality = Math.Clamp(q, 0, 100);
                    Console.WriteLine($"[INFO] Quality set to {_quality}");
                }
            }
        }
    }

    private static void ConvertFolder(string folderPath)
    {
        var opt = _recursive
            ? System.IO.SearchOption.AllDirectories
            : System.IO.SearchOption.TopDirectoryOnly;

        string[] pngFiles = Directory.GetFiles(folderPath, "*.png", opt);

        foreach (string file in pngFiles)
            ConvertFile(file);
    }

    private static void ConvertFile(string filePath)
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
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, _quality);

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

                if (_deleteOriginal)
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
                DeleteFailedOutput(outputPath);
                Console.WriteLine($"[ERR] Save failed (file not readable): {outputPath}");
            }
        }
        catch (Exception ex)
        {
            DeleteFailedOutput(outputPath);
            Console.WriteLine($"[ERR] Failed: {filePath} ({ex.Message})");
        }
    }

    private static void DeleteFailedOutput(string outputPath)
    {
        if (!File.Exists(outputPath))
            return;

        try
        {
            File.Delete(outputPath);
            Console.WriteLine($"[DEL] Removed failed output: {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERR] Could not remove failed output: {outputPath} ({ex.Message})");
        }
    }

    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {
        foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
        {
            if (codec.FormatID == format.Guid)
                return codec;
        }

        return null;
    }
}
