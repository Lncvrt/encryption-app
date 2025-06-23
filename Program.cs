using System.Text;
using System.Security.Cryptography;

class Program {
    static readonly string[] options = ["Encryption", "Decryption", "Key Generation", "Exit"];
    static int selected = 0;

    static void Main()
    {
        Console.CursorVisible = false;
        DrawMenu();

        while (true)
        {
            while (true)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.LeftArrow)
                {
                    selected = (selected == 0) ? options.Length - 1 : selected - 1;
                }
                else if (key == ConsoleKey.RightArrow)
                {
                    selected = (selected + 1) % options.Length;
                }
                else if (key == ConsoleKey.Enter)
                {
                    break;
                }
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop);
                DrawMenu();
            }

            Console.CursorVisible = true;
            Console.Clear();
            if (selected == 0)
            {
                Console.Write("Enter encryption key (16, 24, or 32 bytes): ");
                var inputKey = Console.ReadLine() ?? throw new ArgumentNullException();
                Console.Write("Enter data to encrypt: ");
                var inputData = Console.ReadLine() ?? throw new ArgumentNullException();

                var outputData = Convert.ToBase64String(Encrypt(inputData, inputKey));
                Console.WriteLine("Encrypted: " + outputData);
            }
            else if (selected == 1)
            {
                Console.Write("Enter decryption key (16, 24, or 32 bytes): ");
                var inputKey = Console.ReadLine() ?? throw new ArgumentNullException();
                Console.Write("Enter data to decrypt: ");
                var inputData = Console.ReadLine() ?? throw new ArgumentNullException();

                var outputData = Decrypt(Convert.FromBase64String(inputData), inputKey);
                Console.WriteLine("Decrypted: " + outputData);
            }
            else if (selected == 2)
            {
                var key = new byte[16];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(key);
                Console.WriteLine("Generated Key (hex): " + Convert.ToHexStringLower(key));
            }
            else if (selected == 3)
            {
                break;
            }
            Console.CursorVisible = false;
            Console.WriteLine("\nPress any key to return to the menu...");
            Console.ReadKey();
            DrawMenu();
        }
    }

    static void DrawMenu()
    {
        Console.Clear();
        for (int i = 0; i < options.Length; i++)
        {
            if (i > 0) Console.Write(" - ");
            if (i == selected)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write(options[i]);
                Console.ResetColor();
            }
            else
            {
                Console.Write(options[i]);
            }
        }
    }
    
    static byte[] Encrypt(string data, string key)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();
        using var enc = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(data);
        var encBytes = enc.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        var combined = new byte[aes.IV.Length + encBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
        Buffer.BlockCopy(encBytes, 0, combined, aes.IV.Length, encBytes.Length);
        return combined;
    }
    

    static string? Decrypt(byte[] data, string key)
    {
        if (data.Length < 17) return null;
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        var iv = new byte[16];
        Buffer.BlockCopy(data, 0, iv, 0, 16);
        aes.IV = iv;
        var enc = new byte[data.Length - 16];
        Buffer.BlockCopy(data, 16, enc, 0, enc.Length);
        using var dec = aes.CreateDecryptor();
        var decBytes = dec.TransformFinalBlock(enc, 0, enc.Length);
        return Encoding.UTF8.GetString(decBytes);
    }
}
