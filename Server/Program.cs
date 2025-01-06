namespace Server;

public sealed class Program
{
    public static void Main(string[] args)
    {
        using var application = new Application(args);
        application.Initialize();
        application.Run(5);
    }
}