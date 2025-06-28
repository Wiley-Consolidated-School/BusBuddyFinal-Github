using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("BusBuddy Diagnostics Tool");
        Console.WriteLine("========================");
        Console.WriteLine($"Current Directory: {Environment.CurrentDirectory}");
        Console.WriteLine($"OS Version: {Environment.OSVersion}");
        Console.WriteLine($"Is 64-bit Process: {Environment.Is64BitProcess}");
        Console.WriteLine($"Is Interactive: {Environment.UserInteractive}");
        Console.WriteLine($".NET Runtime Version: {Environment.Version}");

        try
        {
            Console.WriteLine("\nChecking if Windows Forms is available...");
            var form = new System.Windows.Forms.Form();
            form.Text = "BusBuddy Test Form";
            Console.WriteLine("✅ Windows Forms is available");

            Console.WriteLine("\nChecking if BusBuddy.exe exists...");
            var exePath = System.IO.Path.Combine(Environment.CurrentDirectory, "bin", "Debug", "net8.0-windows", "BusBuddy.exe");
            if (System.IO.File.Exists(exePath))
            {
                Console.WriteLine($"✅ Found at: {exePath}");
            }
            else
            {
                Console.WriteLine($"❌ Not found at: {exePath}");
                Console.WriteLine("Searching for BusBuddy.exe...");
                var files = System.IO.Directory.GetFiles(Environment.CurrentDirectory, "BusBuddy.exe", System.IO.SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    Console.WriteLine($"- Found: {file}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
