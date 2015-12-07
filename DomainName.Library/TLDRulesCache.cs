using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DomainName.Library.Properties;
using System.Diagnostics;
using System.Net.Http;

namespace DomainName.Library
{
    public sealed class TLDRulesCache
    {
        private static volatile TLDRulesCache _uniqueInstance;
        private static object _syncObj = new object();
        private static object _syncList = new object();

        private IDictionary<TLDRule.RuleType, IDictionary<string, TLDRule>> _lstTLDRules;

        private TLDRulesCache()
        {
            //  Initialize our internal list:
            _lstTLDRules = GetTLDRules();
        }

        /// <summary>
        /// Returns the singleton instance of the class
        /// </summary>
        public static TLDRulesCache Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_syncObj)
                    {
                        if (_uniqueInstance == null)
                            _uniqueInstance = new TLDRulesCache();
                    }
                }
                return (_uniqueInstance);
            }
        }

        /// <summary>
        /// List of TLD rules
        /// </summary>
        public IDictionary<TLDRule.RuleType, IDictionary<string, TLDRule>> TLDRuleLists
        {
            get
            {
                return _lstTLDRules;
            }
            set
            {
                _lstTLDRules = value;
            }
        }

        /// <summary>
        /// Resets the singleton class and flushes all the cached 
        /// values so they will be re-cached the next time they are requested
        /// </summary>
        public static void Reset()
        {
            lock (_syncObj)
            {
                _uniqueInstance = null;
            }
        }

        /// <summary>
        /// Gets the list of TLD rules from the cache
        /// </summary>
        /// <returns></returns>
        private IDictionary<TLDRule.RuleType, IDictionary<string, TLDRule>> GetTLDRules()
        {
            var results = new Dictionary<TLDRule.RuleType, IDictionary<string, TLDRule>>();
            var rules = Enum.GetValues(typeof(TLDRule.RuleType)).Cast<TLDRule.RuleType>();
            foreach (var rule in rules)
            {
                results[rule] = new Dictionary<string, TLDRule>(StringComparer.InvariantCultureIgnoreCase);
            }

            var ruleStrings = ReadRulesData();           

            //  Strip out any lines that are:
            //  a.) A comment
            //  b.) Blank
            foreach (var ruleString in ruleStrings.Where(ruleString => !ruleString.StartsWith("//", StringComparison.InvariantCultureIgnoreCase) && ruleString.Trim().Length != 0))
            {
                var result = new TLDRule(ruleString);
                results[result.Type][result.Name] = result;
            }

            //  Return our results:
            Debug.WriteLine(string.Format("Loaded {0} rules into cache.", results.Count));
            return results;
        }

        private IEnumerable<string> ReadRulesData()
        {
            if (File.Exists(Settings.Default.SuffixRulesFileLocation))
            {
                //  Load the rules from the cached text file
                foreach (var line in File.ReadAllLines(Settings.Default.SuffixRulesFileLocation, Encoding.UTF8))
                    yield return line;
            }
            else
            {
                // read the files from the web directly.
                var datFile = new HttpClient().GetStreamAsync("https://publicsuffix.org/list/effective_tld_names.dat").Result;
                using (var reader = new StreamReader(datFile))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                        yield return line;
                }
            }
        }
    }
}
