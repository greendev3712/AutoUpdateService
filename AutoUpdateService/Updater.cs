using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AutoUpdateService
{
    static class Updater
    {
        static readonly Lazy<IDictionary<Version, Uri>> _lazyVersionUrls =
            new Lazy<IDictionary<Version, Uri>>(() => GetVersionUrls());
        private static IDictionary<Version, Uri> _versionUrls
        {
            get
            {
                return _lazyVersionUrls.Value;
            }
        }

        public static string GitHubRepo { get; set; }
        public static bool HasUpdate { 
            get 
            {
                var v = Assembly.GetEntryAssembly().GetName().Version;
                foreach (var e in _versionUrls)
                {
                    if (e.Key > v) return true;
                }
                return false;
            }
        }

        public static bool AutoUpdate(string[] args)
        {
            if (HasUpdate)
            {
                Update(args);
                return true;
            }
            return false;
        }

        private static void Update(string[] args)
        {
            throw new NotImplementedException();
        }

        static IDictionary<Version, Uri> GetVersionUrls()
        {
            string pattern = string.Concat(
                Regex.Escape(GitHubRepo),
                @"\/releases\/download\/Update.v[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+.*\.zip");

            Regex urlMatcher = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.Compiled);
            var result = new Dictionary<Version, Uri>();
            WebRequest wrq = WebRequest.Create(string.Concat(@"https://github.com", GitHubRepo, "/releases/latest"));
            WebResponse wrs;

            try
            {
                wrs = wrq.GetResponse();
            } catch(WebException ex)
            {
                Debug.WriteLine("Error fetching repo: " + ex.Message);
                return result;
            }
            using (var sr = new StreamReader(wrs.GetResponseStream()))
            {
                string line;
                while((line = sr.ReadLine()) != null)
                {
                    var match = urlMatcher.Match(line);
                    if (match.Success)
                    {
                        var uri = new Uri(string.Concat(@"http://github.com", match.Value));
                        var vs = match.Value.LastIndexOf("/Update.v");
                        var sa = match.Value.Substring(vs + 10).Split('.', '/');
                        var v = new Version(int.Parse(sa[0]), int.Parse(sa[1]), int.Parse(sa[2]), int.Parse(sa[3]));
                        result.Add(v, uri);
                    }
                }
            }

            return result;
        }
    }
}
