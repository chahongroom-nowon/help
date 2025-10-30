using System;
using System.Collections.Generic;
using System.Linq;

namespace MobiMeter;

internal static class DamageMeter
{
	internal class Meter
	{
		private class DamageEntryRaw
		{
			public double Damage { get; set; }

			public ulong FromID { get; set; }

			public ulong ToID { get; set; }

			public DateTime Timestamp { get; set; }

			public DamageEntryRaw(double damage, ulong fromID, ulong toID)
			{
				Damage = damage;
				FromID = fromID;
				ToID = toID;
				Timestamp = DateTime.Now;
			}

			public override string ToString()
			{
				return $"[{Timestamp:HH:mm:ss.fff}] {Damage} damage";
			}
		}

		private class DamageEntry
		{
			public List<DamageEntryRaw> DamageEntryRaws = new List<DamageEntryRaw>();

			public double totalDamage;

			public DamageEntry(List<DamageEntryRaw> DamageEntryRaws, ulong fromID, ulong toID)
			{
				this.DamageEntryRaws = DamageEntryRaws;
			}

			public void Record(List<DamageEntryRaw> DamageEntryRaws)
			{
				if (DamageEntryRaws.Count != 0)
				{
					this.DamageEntryRaws.AddRange(DamageEntryRaws);
					totalDamage += DamageEntryRaws.Sum((DamageEntryRaw x) => x.Damage);
				}
			}

			public void Record(DamageEntry damageEntry)
			{
				Record(damageEntry.DamageEntryRaws);
			}
		}

		public DateTime recodeStartTime = DateTime.MaxValue;

		public DateTime recodeEndTime = DateTime.MaxValue;

		public double totalDamage;

		private List<DamageEntryRaw> _damageLogNew = new List<DamageEntryRaw>();

		private List<DamageEntry> damageLogFroms = new List<DamageEntry>();

		private List<DamageEntry> damageLogTos = new List<DamageEntry>();

		public ulong topID;

		public double[] barsFrom = new double[7];

		public double[] barsTo = new double[7];

		public double BattleTime()
		{
			return Math.Max((LastAttackTime() - recodeStartTime).TotalSeconds, 0.0);
		}

		public DateTime LastAttackTime()
		{
			if (Form1.optionData.useLastAttack)
			{
				return recodeEndTime;
			}
			return DateTime.Now;
		}

		private List<DamageEntryRaw> GetDamageLog()
		{
			List<DamageEntryRaw> records = new List<DamageEntryRaw>();
			lock (_damageLogNew)
			{
				foreach (DamageEntryRaw e in _damageLogNew)
				{
					records.Add(new DamageEntryRaw(e.Damage, e.FromID, e.ToID)
					{
						Timestamp = e.Timestamp
					});
				}
			}
			foreach (DamageEntry entry in damageLogFroms)
			{
				records.AddRange(entry.DamageEntryRaws);
			}
			if (records.Count == 0)
			{
				return null;
			}
			return records;
		}

		private List<DamageEntryRaw> GetDamageLog(ulong targetID)
		{
			return GetDamageLog(targetID, Form1.optionData.multiTarget);
		}

		private List<DamageEntryRaw> GetDamageLog(ulong targetID, bool fromOnly)
		{
			return ((!fromOnly) ? GetDamageLogByToID(targetID) : GetDamageLogByFromID(targetID))?.DamageEntryRaws;
		}

		private DamageEntry? GetDamageLogByFromID(ulong fromID)
		{
			List<DamageEntryRaw> records = new List<DamageEntryRaw>();
			lock (_damageLogNew)
			{
				foreach (DamageEntryRaw e in _damageLogNew)
				{
					if (e.FromID == fromID)
					{
						records.Add(new DamageEntryRaw(e.Damage, e.FromID, e.ToID)
						{
							Timestamp = e.Timestamp
						});
					}
				}
			}
			foreach (DamageEntry entry in damageLogFroms)
			{
				if (entry.DamageEntryRaws.Count > 0 && entry.DamageEntryRaws[0].FromID == fromID)
				{
					records.AddRange(entry.DamageEntryRaws);
				}
			}
			if (records.Count == 0)
			{
				return null;
			}
			return new DamageEntry(records, fromID, 0uL);
		}

		private DamageEntry? GetDamageLogByToID(ulong toID)
		{
			List<DamageEntryRaw> records = new List<DamageEntryRaw>();
			lock (_damageLogNew)
			{
				foreach (DamageEntryRaw e in _damageLogNew)
				{
					if (e.ToID == toID)
					{
						records.Add(new DamageEntryRaw(e.Damage, e.FromID, e.ToID)
						{
							Timestamp = e.Timestamp
						});
					}
				}
			}
			foreach (DamageEntry entry in damageLogTos)
			{
				if (entry.DamageEntryRaws.Count > 0 && entry.DamageEntryRaws[0].ToID == toID)
				{
					records.AddRange(entry.DamageEntryRaws);
				}
			}
			if (records.Count == 0)
			{
				return null;
			}
			return new DamageEntry(records, 0uL, toID);
		}

		public void RecordDamage(double damage, ulong fromID, ulong toID)
		{
			if (Form1.running)
			{
				DamageEntryRaw damageEntry = new DamageEntryRaw(damage, fromID, toID);
				lock (_damageLogNew)
				{
					_damageLogNew.Add(damageEntry);
				}
				totalDamage += damage;
				if (recodeStartTime > DateTime.Now)
				{
					recodeStartTime = DateTime.Now;
				}
				recodeEndTime = DateTime.Now;
			}
		}

		public double DPS()
		{
			return DPS(topID);
		}

		public double DPS(ulong targetID)
		{
			return Math.Floor(DPSCalcu(Form1.optionData.dpsCheckTime, targetID));
		}

		public double HPS()
		{
			GetDamageLog();
			return Math.Floor(DPSCalcu(Form1.optionData.dpsCheckTime, topID));
		}

		public void Reset()
		{
			totalDamage = 0.0;
			_damageLogNew.Clear();
			damageLogFroms.Clear();
			damageLogTos.Clear();
			recodeStartTime = DateTime.MaxValue;
			recodeEndTime = DateTime.MaxValue;
		}

		private double DPSCalcu(List<DamageEntryRaw> _damageLog, double retentionSeconds, ulong targetID)
		{
			List<DamageEntryRaw> _damageLogClone = _damageLog;
			double dps = 0.0;
			if (_damageLogClone == null)
			{
				return 0.0;
			}
			DateTime cutoff = LastAttackTime().AddSeconds(0.0 - retentionSeconds);
			_damageLogClone = _damageLogClone.FindAll((DamageEntryRaw x) => x.Timestamp > cutoff);
			if (_damageLogClone.Count > 0)
			{
				dps = _damageLogClone.Sum((DamageEntryRaw x) => x.Damage);
			}
			double second = retentionSeconds;
			int longTime = 15;
			if (second > (double)longTime && recodeStartTime < DateTime.Now && recodeStartTime > cutoff)
			{
				second = Math.Max(BattleTime(), longTime);
			}
			second = Math.Max(1.5, second);
			return dps / second;
		}

		private double DPSCalcu(double retentionSeconds, ulong targetID)
		{
			List<DamageEntryRaw> _damageLogClone = GetDamageLog(targetID);
			return DPSCalcu(_damageLogClone, retentionSeconds, targetID);
		}

		public void UpdateCache()
		{
			DateTime now = DateTime.Now;
			DateTime currentSecond = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
			List<DamageEntryRaw> _damageLogTemp = new List<DamageEntryRaw>();
			lock (_damageLogNew)
			{
				foreach (var group in (from e in _damageLogNew
					group e by new
					{
						TimeKey = new DateTime(e.Timestamp.Year, e.Timestamp.Month, e.Timestamp.Day, e.Timestamp.Hour, e.Timestamp.Minute, e.Timestamp.Second),
						FromID = e.FromID,
						ToID = e.ToID
					}).ToList())
				{
					if (group.Key.TimeKey < currentSecond)
					{
						DamageEntryRaw merged = new DamageEntryRaw(group.Sum((DamageEntryRaw e) => e.Damage), group.Key.FromID, group.Key.ToID)
						{
							Timestamp = group.Key.TimeKey
						};
						_damageLogTemp.Add(merged);
						_damageLogNew.RemoveAll((DamageEntryRaw e) => new DateTime(e.Timestamp.Year, e.Timestamp.Month, e.Timestamp.Day, e.Timestamp.Hour, e.Timestamp.Minute, e.Timestamp.Second) == group.Key.TimeKey && e.FromID == group.Key.FromID && e.ToID == group.Key.ToID);
					}
				}
			}
			if (_damageLogTemp.Count > 0)
			{
				foreach (IGrouping<ulong, DamageEntryRaw> group2 in from e in _damageLogTemp
					group e by e.FromID into g
					orderby g.Key
					select g)
				{
					List<DamageEntryRaw> records = group2.OrderBy((DamageEntryRaw e) => e.Timestamp).ToList();
					DamageEntry existing = damageLogFroms.FirstOrDefault((DamageEntry x) => x.DamageEntryRaws.Count > 0 && x.DamageEntryRaws[0].FromID == group2.Key);
					if (existing != null)
					{
						existing.Record(records);
						continue;
					}
					DamageEntry newEntry = new DamageEntry(new List<DamageEntryRaw>(), group2.Key, 0uL);
					newEntry.Record(records);
					damageLogFroms.Add(newEntry);
				}
				foreach (IGrouping<ulong, DamageEntryRaw> group3 in from e in _damageLogTemp
					group e by e.ToID into g
					orderby g.Key
					select g)
				{
					List<DamageEntryRaw> records2 = group3.OrderBy((DamageEntryRaw e) => e.Timestamp).ToList();
					DamageEntry existing2 = damageLogTos.FirstOrDefault((DamageEntry x) => x.DamageEntryRaws.Count > 0 && x.DamageEntryRaws[0].ToID == group3.Key);
					if (existing2 != null)
					{
						existing2.Record(records2);
						continue;
					}
					DamageEntry newEntry2 = new DamageEntry(new List<DamageEntryRaw>(), 0uL, group3.Key);
					newEntry2.Record(records2);
					damageLogTos.Add(newEntry2);
				}
			}
			double retentionSeconds = Form1.optionData.dpsCheckTime;
			DateTime cutoff = LastAttackTime().AddSeconds(0.0 - retentionSeconds);
			foreach (DamageEntry damageLogFrom in damageLogFroms)
			{
				damageLogFrom.DamageEntryRaws = damageLogFrom.DamageEntryRaws.Where((DamageEntryRaw record) => record.Timestamp > cutoff).ToList();
			}
			foreach (DamageEntry damageLogTo in damageLogTos)
			{
				damageLogTo.DamageEntryRaws = damageLogTo.DamageEntryRaws.Where((DamageEntryRaw record) => record.Timestamp > cutoff).ToList();
			}
			damageLogFroms = damageLogFroms.Where((DamageEntry entry) => entry.DamageEntryRaws.Count > 0).ToList();
			damageLogTos = damageLogTos.Where((DamageEntry entry) => entry.DamageEntryRaws.Count > 0).ToList();
		}

		public void Update()
		{
			UpdateCache();
			damageLogFroms.Sort((DamageEntry a, DamageEntry b) => b.totalDamage.CompareTo(a.totalDamage));
			damageLogTos.Sort((DamageEntry a, DamageEntry b) => b.totalDamage.CompareTo(a.totalDamage));
			ulong topIDFrom = 0uL;
			ulong topIDTo = 0uL;
			if (damageLogFroms.Count > 0)
			{
				topIDFrom = damageLogFroms.First().DamageEntryRaws.First().FromID;
			}
			if (damageLogTos.Count > 0)
			{
				topIDTo = damageLogTos.First().DamageEntryRaws.First().ToID;
			}
			if (Form1.optionData.multiTarget)
			{
				topID = topIDFrom;
			}
			else
			{
				topID = topIDTo;
			}
			for (int i = 0; i < barsFrom.Length; i++)
			{
				barsFrom[i] = 0.0;
				if (i < damageLogFroms.Count)
				{
					barsFrom[i] = damageLogFroms[i].totalDamage;
				}
			}
			for (int i2 = 0; i2 < barsTo.Length; i2++)
			{
				barsTo[i2] = 0.0;
				if (i2 < damageLogTos.Count)
				{
					barsTo[i2] = damageLogTos[i2].totalDamage;
				}
			}
		}
	}

	public static double dps = 0.0;

	public static double hps = 0.0;

	public static double sps = 0.0;

	public static Meter damageMeter = new Meter();

	public static Meter healMeter = new Meter();

	public static Meter shieldMeter = new Meter();

	public static double totalDamage = 0.0;

	public static double totalHeal = 0.0;

	public static double totalShield = 0.0;

	public static double[] barsFrom = new double[6];

	public static double[] barsTo = new double[6];

	public static Meter MainTarget
	{
		get
		{
			if (Form1.optionData.dpmMode)
			{
				return healMeter;
			}
			return damageMeter;
		}
	}

	public static void Reset()
	{
		totalDamage = 0.0;
		totalHeal = 0.0;
		totalShield = 0.0;
		damageMeter.Reset();
		healMeter.Reset();
		shieldMeter.Reset();
	}

	public static void Update()
	{
		damageMeter.Update();
		healMeter.Update();
		shieldMeter.Update();
		dps = damageMeter.DPS();
		hps = healMeter.DPS();
		sps = shieldMeter.DPS();
		hps += sps;
		if (Form1.optionData.dpmMode)
		{
			totalDamage = healMeter.totalDamage;
		}
		else
		{
			totalDamage = damageMeter.totalDamage;
		}
	}

	public static DateTime LastAttackTime()
	{
		DateTime attack = damageMeter.LastAttackTime();
		DateTime heal = healMeter.LastAttackTime();
		DateTime shield = shieldMeter.LastAttackTime();
		DateTime[] times = new DateTime[3] { attack, heal, shield };
		times = times.Where((DateTime x) => x != DateTime.MaxValue).ToArray();
		if (times.Length == 0)
		{
			return DateTime.MaxValue;
		}
		return times.Max();
	}

	public static double BattleTime()
	{
		if (Form1.optionData.dpmMode)
		{
			return healMeter.BattleTime();
		}
		return damageMeter.BattleTime();
	}

	public static void RecordShield(double damage, ulong fromID, ulong toID)
	{
		shieldMeter.RecordDamage(damage, fromID, toID);
	}

	public static void RecordHeal(double damage, ulong fromID, ulong toID)
	{
		healMeter.RecordDamage(damage, fromID, toID);
	}

	public static void RecordDamage(double damage, ulong fromID, ulong toID)
	{
		damageMeter.RecordDamage(damage, fromID, toID);
	}
}
