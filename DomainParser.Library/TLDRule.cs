using System;

namespace DomainParser.Library
{
    /// <summary>
    /// Meta information class for an individual TLD rule
    /// </summary>
    public class TLDRule : IComparable<TLDRule>
    {
        /// <summary>
        /// The rule name
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// The rule type
        /// </summary>
        public RuleType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Construct a TLDRule based on a single line from
        /// the www.publicsuffix.org list
        /// </summary>
        /// <param name="RuleInfo"></param>
        public TLDRule(string RuleInfo)
        {
            //  Parse the rule and set properties accordingly:
            if (RuleInfo.StartsWith("*"))
            {
                this.Type = RuleType.Wildcard;
                this.Name = RuleInfo.Substring(2);
            }
            else if (RuleInfo.StartsWith("!"))
            {
                this.Type = RuleType.Exception;
                this.Name = RuleInfo.Substring(1);
            }
            else
            {
                this.Type = RuleType.Normal;
                this.Name = RuleInfo;
            }
        }

        #region IComparable<TLDRule> Members

        public int CompareTo(TLDRule other)
        {
            if (other == null)
                return -1;

            return Name.CompareTo(other.Name);
        }

        #endregion

        #region RuleType enum
        
        /// <summary>
        /// TLD Rule type
        /// </summary>
        public enum RuleType
        {
            /// <summary>
            /// A normal rule
            /// </summary>
            Normal,

            /// <summary>
            /// A wildcard rule, as defined by www.publicsuffix.org
            /// </summary>
            Wildcard,

            /// <summary>
            /// An exception rule, as defined by www.publicsuffix.org
            /// </summary>
            Exception
        } 

        #endregion
    }
}
