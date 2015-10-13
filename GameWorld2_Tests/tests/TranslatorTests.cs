using NUnit.Framework;
using System;
using System.Text.RegularExpressions;

namespace GameWorld2_Tests
{
	[TestFixture ()]
	public class TranslatorTests
	{
		[Test ()]
		public void LearnRegex1 ()
		{
			// First we see the input string.
			string input = "/content/alternate-1.aspx";

			// Here we call Regex.Match.
			Match match = Regex.Match(input, @"content/([A-Za-z0-9\-]+)\.aspx$",
				RegexOptions.IgnoreCase);

			// Here we check the Match instance.
			if (match.Success)
			{
				// Finally, we get the Group value and display it.
				string key = match.Groups[1].Value;
				Console.WriteLine(key);
			}
		}

		[Test ()]
		public void LearnRegex2 ()
		{
			// First we see the input string.
			string input = "\"då\" => \"döä!\"";

			// Here we call Regex.Match.
			Match match = Regex.Match(input, "\"(.+)\" => \"(.+)\"");
				
			if (match.Success)
			{
				for (int i = 0; i < match.Groups.Count; i++) {
					string key = match.Groups [i].Value;
					Console.WriteLine ("group " + i + ": " + key);
				}

			}
		}

		//"hej" "pa" "dig"
	}
}

