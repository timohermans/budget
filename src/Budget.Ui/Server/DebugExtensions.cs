using System.Text.Json;
using System.Text.Json.Serialization;

namespace Budget.Ui.Server;

public static class DebugExtensions
{
    public static void Dump(this object obj, ILogger logger)
    {
        logger.LogInformation(obj.Dump());
    }

    public static string Dump(this object obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            WriteIndented = true,
        });
    }
}
