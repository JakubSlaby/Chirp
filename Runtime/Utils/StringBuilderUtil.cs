using System.Text;

namespace WhiteSparrow.Shared.Logging
{
	internal static class StringBuilderUtil
	{
		// StringBuilder.Append(int) routes through int.ToString() on Unity's Mono BCL and
		// allocates a string per call; emitting digits manually keeps numeric appends free.
		internal static void AppendInt(StringBuilder sb, int value)
		{
			if (value < 0)
			{
				sb.Append('-');
				AppendUInt(sb, (uint)-(long)value);
				return;
			}

			AppendUInt(sb, (uint)value);
		}

		private static void AppendUInt(StringBuilder sb, uint value)
		{
			if (value >= 10)
				AppendUInt(sb, value / 10);
			sb.Append((char)('0' + (int)(value % 10)));
		}

		internal static void TrimEndLineBreaks(StringBuilder sb)
		{
			int length = sb.Length;
			while (length > 0)
			{
				char c = sb[length - 1];
				if (c != '\n' && c != '\r')
					break;
				length--;
			}

			sb.Length = length;
		}
	}
}
