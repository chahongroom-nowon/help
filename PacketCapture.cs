using System;
using System.Collections.Generic;
using System.Linq;
using SharpPcap;
using SharpPcap.LibPcap;

namespace MobiMeter;

public class PacketCapture
{
	private LibPcapLiveDevice[] devices;

	private List<List<byte>> common_sequences = new List<List<byte>>();

	public PacketCapture()
	{
		List<LibPcapLiveDevice> devices = LibPcapLiveDeviceList.Instance.ToList();
		devices = devices.FindAll(delegate(LibPcapLiveDevice dev)
		{
			string text = dev.Description.ToLower();
			return (!text.Contains("loopback") || !text.Contains("capture")) ? true : false;
		});
		for (int i = 0; i < devices.Count; i++)
		{
			devices[i].Addresses.Select((PcapAddress a) => a.Addr);
		}
		this.devices = devices.ToArray();
	}

	private void AnalyzePacket(byte[] bytes)
	{
		if (bytes.Length <= 32)
		{
			return;
		}
		List<byte> data = bytes.Skip(32).ToList();
		if (common_sequences.Count == 0)
		{
			for (int i = 0; i < data.Count; i++)
			{
				for (int j = 1; j <= data.Count - i; j++)
				{
					List<byte> slice = data.GetRange(i, j);
					common_sequences.Add(slice);
				}
			}
			common_sequences = (from x in common_sequences
				group x by BitConverter.ToString(x.ToArray()) into x
				select x.First()).ToList();
			return;
		}
		common_sequences = common_sequences.Where((List<byte> subsequence) => ContainsSubsequence(data, subsequence)).ToList();
		common_sequences = common_sequences.Where((List<byte> source) => !source.All((byte b) => b == 0)).ToList();
		if (common_sequences.Count >= 10000)
		{
			return;
		}
		List<List<byte>> list = common_sequences.OrderByDescending((List<byte> s) => s.Count).ToList();
		List<List<byte>> top = new List<List<byte>>();
		foreach (List<byte> seq in list)
		{
			if (!top.Any((List<byte> existing) => ContainsSubsequence(existing, seq)))
			{
				top.Add(seq);
				if (top.Count == 5)
				{
					break;
				}
			}
		}
		for (int i2 = 0; i2 < top.Count; i2++)
		{
			BitConverter.ToString(top[i2].ToArray()).Replace("-", " ");
		}
		static bool ContainsSubsequence(List<byte> source, List<byte> subsequence)
		{
			if (subsequence.Count == 0 || source.Count < subsequence.Count)
			{
				return false;
			}
			for (int k = 0; k <= source.Count - subsequence.Count; k++)
			{
				bool match = true;
				for (int l = 0; l < subsequence.Count; l++)
				{
					if (source[k + l] != subsequence[l])
					{
						match = false;
						break;
					}
				}
				if (match)
				{
					return true;
				}
			}
			return false;
		}
	}

	public void Start()
	{
		LibPcapLiveDevice[] array = devices;
		foreach (LibPcapLiveDevice _device in array)
		{
			_device.Open(DeviceModes.Promiscuous, 100);
			byte[] triggerDamage = new byte[8] { 81, 0, 0, 53, 0, 0, 0, 0 };
			byte[][] triggerHeals = new byte[2][]
			{
				new byte[8] { 79, 0, 0, 24, 0, 0, 0, 0 },
				new byte[8] { 40, 0, 0, 16, 0, 0, 0, 0 }
			};
			_ = new byte[9] { 180, 39, 0, 0, 24, 0, 0, 0, 0 };
			_ = new byte[8] { 137, 1, 0, 24, 0, 0, 0, 0 };
			int segLen = 71;
			_device.OnPacketArrival += delegate(object sender, SharpPcap.PacketCapture e)
			{
				byte[] data = e.GetPacket().Data;
				for (int j = 54; j < data.Length - segLen; j++)
				{
					byte[] array2 = triggerHeals[0];
					if (data.Skip(j).Take(array2.Length).SequenceEqual(array2))
					{
						int num = j;
						ulong num2 = 0uL;
						ulong num3 = 0uL;
						ulong toID = 0uL;
						for (int k = 0; k < 24; k += 8)
						{
							int num4 = k / 8;
							int count = num + array2.Length + k;
							byte[] array3 = data.Skip(count).Take(8).ToArray();
							ulong num5 = BitConverter.ToUInt64(array3, 0);
							_ = num5 + " (" + BitConverter.ToString(array3).Replace("-", " ") + ")";
							switch (num4)
							{
							case 1:
								num3 = num5;
								break;
							case 0:
								toID = num5;
								break;
							case 2:
								num2 = num5;
								break;
							}
						}
						if (num2 == 0L || num2 == ulong.MaxValue || num3 != 8192)
						{
							continue;
						}
						DamageMeter.RecordHeal(num2, num3, toID);
					}
					if (data.Skip(j).Take(triggerDamage.Length).SequenceEqual(triggerDamage))
					{
						int num6 = j;
						ulong num7 = 0uL;
						ulong fromID = 0uL;
						ulong toID2 = 0uL;
						for (int l = 0; l < 24; l += 8)
						{
							int num8 = l / 8;
							int count2 = num6 + triggerDamage.Length + l;
							ulong num9 = BitConverter.ToUInt64(data.Skip(count2).Take(8).ToArray(), 0);
							num9.ToString();
							switch (num8)
							{
							case 0:
								fromID = num9;
								break;
							case 1:
								toID2 = num9;
								break;
							case 2:
								num7 = num9;
								break;
							}
						}
						if (num7 != 0L && num7 != ulong.MaxValue && !((double)num7 < Form1.optionData.minDamage))
						{
							DamageMeter.RecordDamage(num7, fromID, toID2);
						}
					}
				}
			};
			_device.StartCapture();
		}
	}

	private bool ByteArrayEquals(byte[] data, int offset, byte[] pattern)
	{
		if (offset + pattern.Length > data.Length)
		{
			return false;
		}
		for (int i = 0; i < pattern.Length; i++)
		{
			if (data[offset + i] != pattern[i])
			{
				return false;
			}
		}
		return true;
	}

	public void Stop()
	{
		LibPcapLiveDevice[] array = devices;
		foreach (LibPcapLiveDevice obj in array)
		{
			obj?.StopCapture();
			obj?.Close();
		}
	}
}
