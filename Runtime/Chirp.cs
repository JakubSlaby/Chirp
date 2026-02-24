using WhiteSparrow.Shared.Logging.Core;

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
		public const string Version = "0.12.0";

		internal static ChirpImpl Impl { get; private set; }

		public static IChirpChannels Channels => Impl;
		
		private static ChirpStyle s_CachedFallbackStyle;
		public static ChirpStyle Style => Impl?.DefaultStyle ?? (s_CachedFallbackStyle ??= new ChirpStyle());

		static Chirp()
		{
			Impl = new ChirpImpl();
		}

		public static ChirpLogger Logger => Channels.Default;

		public static T AddPlugin<T>()
			where T : class, IChirpPlugin, new()
			=> Impl.AddPlugin<T>();
		
		public static void AddPlugin(IChirpPlugin plugin)
			=> Impl.AddPlugin(plugin);
		
		
		public static void RemovePlugin(IChirpPlugin plugin)
			=> Impl.RemovePlugin(plugin);
	}
}