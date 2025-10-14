using Spectre.Console;
using Spectre.Console.Rendering;

namespace AppSage.Run.Utilities
{
    internal class SpectreRender
    {
        public static string ToAnsi(IRenderable renderable, int width = 100)
        {
            var writer = new StringWriter();
            var console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.Detect,
                ColorSystem = ColorSystemSupport.Detect,
                Out = new AnsiConsoleOutput(writer),
                Interactive = InteractionSupport.No,
            });

            console.Profile.Width = width;
            console.Write(renderable);
            return writer.ToString();
        }
    }
}
