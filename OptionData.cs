namespace MobiMeter;

public class OptionData
{
	public double dpmCheckTime { get; set; } = 15.0;

	public double dpsCheckTime { get; set; } = 3.0;

	public bool dpmMode { get; set; }

	public bool overlayAlways { get; set; }

	public bool dpmStopWatch { get; set; }

	public bool useLastAttack { get; set; } = true;

	public bool multiTarget { get; set; } = true;

	public double minDamage { get; set; }
}
