using Sandbox;
using System;

namespace Degg.Analytics
{
	public class InitialiseBody
	{
		public string user_id { get; set; }
		public string platform { get; set; }
		public string os_version { get; set; }
		public string sdk_version = "rest api v2";
		public string random_salt { get; set; }
		public string build { get; set; }
	}

	public class AnlyticsEvent {
		public string Event { get; set; }
		public string UserId { get; set; }
		public float Time { get; set; }

		public float? Metric { get; set; }
		public string Tags { get; set; }
	}

	public partial class GameAnalytics
	{
		public const string PublicEndpoint = "api.gameanalytics.com";
		public const string DevPublicEndpoint = "sandbox-api.gameanalytics.com";
		

		public static string Build { get; set; } = "default";
		public static string Key { get; set; }
		public static string Secret { get; set; }

		public static void Initialise(string key, string secret)
		{
			Key = key;
			Secret = secret;
			Backend.DeggBackend.Initialise( key, secret );
		}

		public static void InitialisePlayer( string userId, string platform = "default", string os_version = "default" )
		{
			InitialiseBody data = new InitialiseBody();
			data.user_id = userId;
			data.platform = platform;
			data.os_version = os_version;
			data.build = GameAnalytics.Build;

			var url = $"{DevPublicEndpoint}/remote_configs/v1/init?game_key={Key}";		
		}

		public static void TriggerEvent(string userId, string e, float? metric = null, string tags = null, float? time = null)
		{
			var aEvent = new AnlyticsEvent();
			aEvent.UserId = userId;
			aEvent.Event = e;
			aEvent.Metric = metric;
			aEvent.Time = time.GetValueOrDefault( DateTime.UtcNow.GetEpoch() );
			aEvent.Tags = tags;

			Backend.DeggBackend.SendEvent( "analytics", aEvent );
		}

		public static void ConfigureBuild(string build)
		{
			Build = build;
		}



	}
}
