using WhiteSparrow.Shared.Logging.Core;
using WhiteSparrow.Shared.Logging.Inputs;
using WhiteSparrow.Shared.Logging.Outputs;

namespace WhiteSparrow.Shared.Logging
{
	public enum LogLevel
	{
		Debug = 0,
		Log = 1,
		Info = 2,
		Warning = 3,
		Assert = 4,
		Error = 5,
		Exception = 6
	}

	public static class Chirp
	{
		public const string Version = "0.11.1";

		internal static ChirpImpl Impl { get; private set; }

		public static IChirpChannels Channels => Impl;
		
		private static ChirpStyle s_CachedFallbackStyle;
		public static ChirpStyle Style => Impl?.DefaultStyle ?? (s_CachedFallbackStyle ??= new ChirpStyle());

		static Chirp()
		{
			Impl = new ChirpImpl();
		}

		public static ChirpLogger Logger => Channels.Default;

		public static T AddInput<T>()
			where T : class, IChirpInput, new()
			=> Impl.AddInput<T>();
		
		public static void AddInput(IChirpInput input)
			=> Impl.AddInput(input);
		
		public static T AddOutput<T>()
			where T : class, IChirpOutput, new()
			=> Impl.AddOutput<T>();
		
		
		public static void AddOutput(IChirpOutput output)
			=> Impl.AddOutput(output);
	}
}