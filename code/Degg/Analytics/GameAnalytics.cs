﻿using Sandbox;


namespace Degg.Degg.Analytics
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
		}

		public static async void InitialisePlayer( string userId, string platform = "default", string os_version = "default" )
		{
			InitialiseBody data = new InitialiseBody();
			data.user_id = userId;
			data.platform = platform;
			data.os_version = os_version;
			data.build = GameAnalytics.Build;

			var url = $"{DevPublicEndpoint}/remote_configs/v1/init?game_key={Key}";





		
		}

		public static void ConfigureBuild(string build)
		{
			Build = build;
		}



	}
}