using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

public interface IFileProcessor
{
    byte[] Process(byte[] data);
}

public class GZipCompression : IFileProcessor
{
    public byte[] Process(byte[] data)
    {
        using (var outputStream = new MemoryStream())
        {
            using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                gzipStream.Write(data, 0, data.Length);
            }
            return outputStream.ToArray();
        }
    }
}

public class AesEncryption : IFileProcessor
{
    private readonly byte[] key = Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF");
    private readonly byte[] iv = Encoding.UTF8.GetBytes("ABCDEF0123456789");

    public byte[] Process(byte[] data)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                return encryptor.TransformFinalBlock(data, 0, data.Length);
            }
        }
    }
}

public class FileProcessorContext
{
    private IFileProcessor _strategy;

    public void SetStrategy(IFileProcessor strategy)
    {
        _strategy = strategy;
    }

    public void ProcessFile(string inputFilePath, string outputFilePath)
    {
        if (_strategy == null)
        {
            throw new InvalidOperationException("Strategy is not set.");
        }

        byte[] inputData = File.ReadAllBytes(inputFilePath);
        byte[] processedData = _strategy.Process(inputData);
        File.WriteAllBytes(outputFilePath, processedData);
    }
}

class Program
{
    static void Main()
    {
        var fileProcessor = new FileProcessorContext();

        Console.WriteLine("Choose an option: 1 - Compress, 2 - Encrypt");
        string choice = Console.ReadLine();

        if (choice == "1")
        {
            fileProcessor.SetStrategy(new GZipCompression());
        }
        else if (choice == "2")
        {
            fileProcessor.SetStrategy(new AesEncryption());
        }
        else
        {
            Console.WriteLine("Invalid option.");
            return;
        }

        string inputPath = "input.txt";
        string outputPath = "output.dat";

        fileProcessor.ProcessFile(inputPath, outputPath);
        Console.WriteLine("File processing completed.");
    }
}
