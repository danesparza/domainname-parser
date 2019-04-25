using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace DomainParser.Library
{
    public class DomainName
    {
        #region Private members

        private string _subDomain = string.Empty;
        private string _domain = string.Empty;
        private string _tld = string.Empty;
        private TLDRule _tldRule = null;

        #endregion

        #region Public properties

        /// <summary>
        /// The subdomain portion
        /// </summary>
        public string SubDomain
        {
            get
            {
                return _subDomain;
            }
        }

        /// <summary>
        /// The domain name portion, without the subdomain or the TLD
        /// </summary>
        public string Domain
        {
            get
            {
                return _domain;
            }
        }

        /// <summary>
        /// The domain name portion, without the subdomain or the TLD
        /// </summary>
        public string SLD
        {
            get
            {
                return _domain;
            }
        }

        /// <summary>
        /// The TLD portion
        /// </summary>
        public string TLD
        {
            get
            {
                return _tld;
            }
        }

        /// <summary>
        /// The matching TLD rule
        /// </summary>
        public TLDRule TLDRule
        {
            get
            {
                return _tldRule;
            }
        }

        #endregion

        #region Construction

        /// <summary>
        /// Constructs a DomainName object from the string representation of a domain. 
        /// </summary>
        /// <param name="domainString"></param>
        public DomainName(string domainString)
        {
            //  If an exception occurs it should bubble up past this
            ParseDomainName(domainString, out _tld, out _domain, out _subDomain, out _tldRule);
        }

        /// <summary>
        /// Constructs a DomainName object from its 3 parts
        /// </summary>
        /// <param name="TLD">The top-level domain</param>
        /// <param name="SLD">The second-level domain</param>
        /// <param name="SubDomain">The subdomain portion</param>
        /// <param name="TLDRule">The rule used to parse the domain</param>
        private DomainName(string TLD, string SLD, string SubDomain, TLDRule TLDRule)
        {
            this._tld = TLD;
            this._domain = SLD;
            this._subDomain = SubDomain;
            this._tldRule = TLDRule;
        }

        #endregion

        #region Parse domain - private static method

        /// <summary>
        /// Converts the string representation of a domain to it's 3 distinct components: 
        /// Top Level Domain (TLD), Second Level Domain (SLD), and subdomain information
        /// </summary>
        /// <param name="domainString">The domain to parse</param>
        /// <param name="TLD"></param>
        /// <param name="SLD"></param>
        /// <param name="SubDomain"></param>
        /// <param name="MatchingRule"></param>
        private static void ParseDomainName(string domainString, out string TLD, out string SLD, out string SubDomain, out TLDRule MatchingRule)
        {
            TLD = string.Empty;
            SLD = string.Empty;
            SubDomain = string.Empty;
            MatchingRule = null;

            //  If the fqdn is empty, we have a problem already
            if (domainString.Trim() == string.Empty)
                throw new ArgumentException("The domain cannot be blank");

            //  Next, find the matching rule:
            MatchingRule = FindMatchingTLDRule(domainString);

            //  At this point, no rules match, we have a problem
            if(MatchingRule == null)
                throw new FormatException("The domain does not have a recognized TLD");

            //  Based on the tld rule found, get the domain (and possibly the subdomain)
            string tempSudomainAndDomain = string.Empty;
            int tldIndex = 0;
            
            //  First, determine what type of rule we have, and set the TLD accordingly
            switch (MatchingRule.Type)
            {
                case TLDRule.RuleType.Normal:
                    tldIndex = domainString.LastIndexOf("." + MatchingRule.Name, StringComparison.OrdinalIgnoreCase);
                    tempSudomainAndDomain = domainString.Substring(0, tldIndex);
                    TLD = domainString.Substring(tldIndex + 1);
                    break;
                case TLDRule.RuleType.Wildcard:
                    //  This finds the last portion of the TLD...
                    tldIndex = domainString.LastIndexOf("." + MatchingRule.Name, StringComparison.OrdinalIgnoreCase);
                    tempSudomainAndDomain = domainString.Substring(0, tldIndex);

                    //  But we need to find the wildcard portion of it:
                    tldIndex = tempSudomainAndDomain.LastIndexOf(".");
                    tempSudomainAndDomain = domainString.Substring(0, tldIndex);
                    TLD = domainString.Substring(tldIndex + 1);
                    break;
                case TLDRule.RuleType.Exception:
                    tldIndex = domainString.LastIndexOf(".");
                    tempSudomainAndDomain = domainString.Substring(0, tldIndex);
                    TLD = domainString.Substring(tldIndex + 1);
                    break;
            }

            //  See if we have a subdomain:
            List<string> lstRemainingParts = new List<string>(tempSudomainAndDomain.Split('.'));

            //  If we have 0 parts left, there is just a tld and no domain or subdomain
            //  If we have 1 part, it's the domain, and there is no subdomain
            //  If we have 2+ parts, the last part is the domain, the other parts (combined) are the subdomain
            if (lstRemainingParts.Count > 0)
            {
                //  Set the domain:
                SLD = lstRemainingParts[lstRemainingParts.Count - 1];

                //  Set the subdomain, if there is one to set:
                if (lstRemainingParts.Count > 1)
                {
                    //  We strip off the trailing period, too
                    SubDomain = tempSudomainAndDomain.Substring(0, tempSudomainAndDomain.Length - SLD.Length - 1);
                }
            }
        }

        #endregion

        #region TryParse method(s)

        /// <summary>
        /// Converts the string representation of a domain to its DomainName equivalent.  A return value
        /// indicates whether the operation succeeded.
        /// </summary>
        /// <param name="domainString"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParse(string domainString, out DomainName result)
        {
            bool retval = false;

            //  Our temporary domain parts:
            string _tld = string.Empty;
            string _sld = string.Empty;
            string _subdomain = string.Empty;
            TLDRule _tldrule = null;
            result = null;

            try
            {
                //  Try parsing the domain name ... this might throw formatting exceptions
                ParseDomainName(domainString, out _tld, out _sld, out _subdomain, out _tldrule);

                //  Construct a new DomainName object and return it
                result = new DomainName(_tld, _sld, _subdomain, _tldrule);

                //  Return 'true'
                retval = true;
            }
            catch
            {
                //  Looks like something bad happened -- return 'false'
                retval = false;
            }

            return retval;
        }

        #endregion

        #region Rule matching
        /// <summary>
        /// Finds matching rule for a domain.  If no rule is found, 
        /// returns a null TLDRule object
        /// </summary>
        /// <param name="domainString"></param>
        /// <returns></returns>
        private static TLDRule FindMatchingTLDRule(string domainString)
        {
            //  Split our domain into parts (based on the '.')
            //  ...Put these parts in a list
            //  ...Make sure these parts are in reverse order (we'll be checking rules from the right-most pat of the domain)
            List<string> lstDomainParts = domainString.Split('.').ToList<string>();
            lstDomainParts.Reverse();

            //  Begin building our partial domain to check rules with:
            string checkAgainst = string.Empty;

            //  Our 'matches' collection:
            List<TLDRule> ruleMatches = new List<TLDRule>();

            foreach (string domainPart in lstDomainParts)
            {
                //  Add on our next domain part:
                checkAgainst = string.Format("{0}.{1}", domainPart, checkAgainst);

                //  If we end in a period, strip it off:
                if (checkAgainst.EndsWith("."))
                    checkAgainst = checkAgainst.Substring(0, checkAgainst.Length - 1);

                var rules = Enum.GetValues(typeof(TLDRule.RuleType)).Cast<TLDRule.RuleType>();
                var ruleList = TLDRulesCache.Instance.GetTLDRuleList();

                foreach (var rule in rules)
                {
                    //  Try to match rule:
                    TLDRule result;
                    if (ruleList[rule].TryGetValue(checkAgainst, out result))
                    {
                        ruleMatches.Add(result);
                    }
                    Debug.WriteLine(string.Format("Domain part {0} matched {1} {2} rules", checkAgainst, result == null ? 0 : 1, rule));
                }
            }

            //  Sort our matches list (longest rule wins, according to :
            var results = from match in ruleMatches
                          orderby match.Name.Length descending
                          select match;

            //  Take the top result (our primary match):
            TLDRule primaryMatch = results.Take(1).SingleOrDefault();

            if (primaryMatch != null)
            {
                Debug.WriteLine(
                    string.Format("Looks like our match is: {0}, which is a(n) {1} rule.", primaryMatch.Name, primaryMatch.Type)
                    );
            }
            else
            {
                Debug.WriteLine(
                    string.Format("No rules matched domain: {0}", domainString)
                    );
            }

            return primaryMatch;
        } 
        #endregion
    }
}
