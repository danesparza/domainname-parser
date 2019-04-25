using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Net.Http;

namespace DomainParser.Library
{
    public sealed class TLDRulesCache
    {
        private const int DefaultSuffixRulesExpireDays = 1;
        private const string DefaultSuffixRulesFileLocation = "publicsuffix.txt";
        private const string DefaultSuffixRulesUrl = "https://publicsuffix.org/list/effective_tld_names.dat";

        private static volatile TLDRulesCache _uniqueInstance;
        private static object _syncObj = new object();
        private static object _syncList = new object();
        private static object _syncData = new object();


        /// <summary>
        /// Added the settings as static properties to be set or overridden by the application at runtime
        /// </summary>
        private static string _rulesFileLocation;
        private static int _rulesExpireDays = -1;
        private static string _rulesUrl;

        /// <summary>
        /// Using this property/setting a cache file will be created at this location.
        /// When the property consists of only a filename, it is combined with the current working directory.
        /// </summary>
        public static string RulesFileLocation {
            get {
                if (_rulesFileLocation == null)
                {
                    _rulesFileLocation = DefaultSuffixRulesFileLocation;
                }
                return _rulesFileLocation;
            }
            set {
                _rulesFileLocation = value;
                Reset();
            }
        }
        /// <summary>
        /// Defines an expiration period in days before the rules are reloaded.
        /// If a cache file is used, then the file is also renewed.
        /// To disable expiration, set the expiration period to zero (0).
        /// </summary>
        public static int RulesExpireDays {
            get {
                if (_rulesExpireDays == -1)
                {
                    _rulesExpireDays = DefaultSuffixRulesExpireDays;
                }
                return _rulesExpireDays;
            }
            set {
                _rulesExpireDays = value;
                Reset();
            }
        }

        public static string RulesUrl {
            get {
                if (_rulesUrl == null)
                {
                    _rulesUrl = DefaultSuffixRulesUrl;
                }
                return _rulesUrl;
            }
            set {
                _rulesUrl = value;
            }
        }


        private IDictionary<TLDRule.RuleType, IDictionary<string, TLDRule>> _lstTLDRules = null;
		private DateTime? _expires = null;

        private TLDRulesCache()
        {
            //  Initialize our internal list:
            // moved to the CheckRuleList method  GetTLDRules();
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
        /// Checks the availability and/or expiration of the rulelist
        /// </summary>
        public void CheckRuleList() {
            if (_lstTLDRules == null || (RulesExpireDays > 0 && (_expires == null || _expires < DateTime.Now))) {
                _lstTLDRules = GetTLDRules();
            }
        }

        /// <summary>
        /// List of TLD rules
        /// </summary>
        public IDictionary<TLDRule.RuleType, IDictionary<string, TLDRule>> TLDRuleLists
        {
            get
            {
                if (_lstTLDRules == null) {
                    _lstTLDRules = GetTLDRules();
                }
                return _lstTLDRules;
            }
            set
            {
                _lstTLDRules = value;
            }
        }

        /// <summary>
        /// Preferable method to get the TLDRuleList. This method checks the availability and/or expiration of the rule list.
        /// </summary>
        /// <returns></returns>
        public IDictionary<TLDRule.RuleType, IDictionary<string, TLDRule>> GetTLDRuleList() {
 
            CheckRuleList();
            return _lstTLDRules;

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
                results[rule] = new Dictionary<string, TLDRule>(StringComparer.OrdinalIgnoreCase);
            }

            var ruleStrings = ReadRulesData();           

            //  Strip out any lines that are:
            //  a.) A comment
            //  b.) Blank
            foreach (var ruleString in ruleStrings.Where(ruleString => !ruleString.StartsWith("//") && ruleString.Trim().Length != 0))
            {
                var result = new TLDRule(ruleString);
                results[result.Type][result.Name] = result;
            }

            //  Return our results:
            Debug.WriteLine(string.Format("Loaded {0} rules into cache.", results.Values.Sum(r => r.Values.Count)));

            return results;
        }

        private IEnumerable<string> ReadRulesData()
        {
            // Allow for non cachable rules
            if (!string.IsNullOrEmpty(RulesFileLocation)) {

                Debug.WriteLine(string.Format("CurrentDirectory is {0}.", Directory.GetCurrentDirectory()));

                DateTime? expireDate = null;
                string fileLocation = RulesFileLocation;

                if (string.IsNullOrEmpty(Path.GetDirectoryName(fileLocation))) {

                    // Filename without directory. Use the current directory.
                    fileLocation = Path.Combine(Directory.GetCurrentDirectory(), fileLocation);

                }

                Debug.WriteLine(string.Format("Cache file location is {0}.", fileLocation));

                if (File.Exists(fileLocation)) {

                    Debug.WriteLine("Cache file exists.");

                    // Allow for non expiring rules
                    if (RulesExpireDays > 0) {

                        expireDate = File.GetLastWriteTime(fileLocation).AddDays(RulesExpireDays);

                        Debug.WriteLine(string.Format("Cache file expires on {0}. Its's now {1}.", expireDate, DateTime.Now));

                        if (expireDate < DateTime.Now) {

                            lock (_syncData) {

                                // We have to check again. The file might have been rotated by another process.
                                expireDate = File.GetLastWriteTime(fileLocation).AddDays(RulesExpireDays);
                                if (expireDate < DateTime.Now) {

                                    GetAndSaveRulesData();

                                }

                            }

                        }

                    }

                } else {

                    Debug.WriteLine("Cache file does not exist (yet).");

                    lock (_syncData) {

                        if (!File.Exists(fileLocation)) {

                            GetAndSaveRulesData();

                        }

                    }

                }

                if (expireDate == null && RulesExpireDays > 0) {
                    expireDate = File.GetLastWriteTime(fileLocation).AddDays(RulesExpireDays);
                }

                _expires = expireDate;
                Debug.WriteLine(string.Format("The rulelist expires on {0}. Its's now {1}.", _expires, DateTime.Now));

                //  Load the rules from the cached text file
                foreach (var line in File.ReadAllLines(fileLocation, Encoding.UTF8))
                    yield return line;
            }
            else
            {

                Debug.WriteLine("Get the rules directly from the web.");

                if (RulesExpireDays > 0) {

                    _expires = DateTime.Now.AddDays(RulesExpireDays);

                    Debug.WriteLine(string.Format("The rulelist expires on {0}. Its's now {1}.", _expires, DateTime.Now));

                }

				// read the files from the web directly.
				using (var datFile = new HttpClient().GetStreamAsync(RulesUrl).Result)
				using (var reader = new StreamReader(datFile)) {
					string line;
					while ((line = reader.ReadLine()) != null)
						yield return line;
				}
            }
        }

        private void GetAndSaveRulesData() {

            string fileLocation = RulesFileLocation;
            if (string.IsNullOrEmpty(Path.GetDirectoryName(fileLocation))) {

                // Filename without directory. Use the current directory.
                fileLocation = Path.Combine(Directory.GetCurrentDirectory(), fileLocation);

            }

            try {
                File.Delete(fileLocation);
            } catch { }

            using (var datStream = new HttpClient().GetStreamAsync(RulesUrl).Result)
            using (var datFile = new FileStream(fileLocation, FileMode.Create, FileAccess.Write)) {
                datStream.CopyTo(datFile);
            }

            Debug.WriteLine(string.Format("Cache file successfully saved to {0}.", fileLocation));

        }
    }
}
