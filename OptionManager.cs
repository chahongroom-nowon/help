using System.IO;
using System.Text.Json;

namespace MobiMeter;

public static class OptionManager
{
	private static string configPath = "configPath.json";

	public static void Save(OptionData data)
	{
		string json = JsonSerializer.Serialize(data);
		File.WriteAllText(configPath, json);
	}

	public static OptionData Load()
	{
		if (!File.Exists(configPath))
		{
			return new OptionData();
		}
		return JsonSerializer.Deserialize<OptionData>(File.ReadAllText(configPath));
	}
}
