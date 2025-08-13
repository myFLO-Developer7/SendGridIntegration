
class Program
{
    static void Main(string[] args)
    {
        try
        {
            if (args.Length > 0)
            {
                var command = args[0];
                if (command == "Test")
                {
                    Console.WriteLine("Test");
                }
                else
                {
                    throw new Exception($"No command such as {command}");
                }
            }
            else
            {
                throw new Exception($"No command input");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}