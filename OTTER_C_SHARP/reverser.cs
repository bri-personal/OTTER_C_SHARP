public class HexReverser
{
	//reverses hex output from RARS into something that can be put into website and turned into usable binary
    //go to https://tomeko.net/online_tools/hex_to_file.php?lang=en
    public static void ReverseHex()
    {
        using (FileStream hex = File.Open("hex.mem", FileMode.Open, FileAccess.ReadWrite))
        {
            using (StreamReader hexReader = new StreamReader(hex))
            {
                using (FileStream bin = File.Open("bin.mem", FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (StreamWriter binWriter = new StreamWriter(bin))
                    {
                        string? line;
                        char[] arr;

                        while ((line = hexReader.ReadLine()) is not null)
                        {
                            Console.WriteLine(line);
                            arr = line.ToCharArray();
                            Array.Reverse(arr);

                            for (int i = 0; i < arr.Length - 1; i += 2)
                            {
                                char temp = arr[i];
                                arr[i] = arr[i + 1];
                                arr[i + 1] = temp;
                            }

                            line = new string(arr);
                            Console.WriteLine(line + "\n");
                            binWriter.WriteLine(line);
                        }
                    }
                }
            }
        }
    }
	
}
