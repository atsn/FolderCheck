namespace FolderCheck
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("sdada");

            foreach (var item in args)
            {
                System.Console.WriteLine(item);
            }
            if (args.Length > 0)
                CheckForDublicates.FileTjeck(args[0]);
            else
                CheckForDublicates.FileTjeck();
        }
    }
}
