using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DomainName.Library.Properties;
using System.Diagnostics;

namespace DomainName.Library
{
    public sealed class TLDRulesCache
    {
        private static volatile TLDRulesCache _uniqueInstance;
        private static object _syncObj = new object();
        private static object _syncList = new object();
        private List<TLDRule> _lstTLDRules;

        private TLDRulesCache()
        {
            //  Initialize our internal list:
            _lstTLDRules = GetTLDRules();
            _lstTLDRules.Sort();
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
        public List<TLDRule> TLDRuleList
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
        private List<TLDRule> GetTLDRules()
        {
            List<TLDRule> results = new List<TLDRule>();

            //  If the cached suffix rules file exists...
            if (File.Exists(Settings.Default.SuffixRulesFileLocation))
            {
                //  Load the rules from the cached text file
                List<string> lstTLDRuleStrings = File.ReadAllLines(Settings.Default.SuffixRulesFileLocation, Encoding.UTF8).ToList();

                //  Strip out any lines that are:
                //  a.) A comment
                //  b.) Blank
                IEnumerable<TLDRule> lstTLDRules = from ruleString in lstTLDRuleStrings
                                                   where
                                                   !ruleString.StartsWith("//", StringComparison.InvariantCultureIgnoreCase)
                                                   &&
                                                   !(ruleString.Trim().Length == 0)
                                                   select new TLDRule(ruleString);

                //  Transfer this list to the results:
                results = lstTLDRules.ToList();
            }

            //  Return our results:
            Debug.WriteLine(
                   string.Format("Loaded {0} rules into cache.", results.Count)
                   );
            return results;
        }
    }
}
