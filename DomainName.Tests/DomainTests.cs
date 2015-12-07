using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace DomainName.Tests
{
    /// <summary>
    /// Summary description for DomainTests
    /// </summary>
    [TestClass]
    public class DomainTests
    {
        private DomainName.Library.DomainName outDomain = null;

        public DomainTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void ParseNormalDomain()
        {
            //  Try parsing a 'normal' domain:
            DomainName.Library.DomainName.TryParse("photos.totallycool.com", out outDomain);

            //  The domain should be parsed as 'totallycool'
            Assert.AreEqual<string>("totallycool", outDomain.Domain);

            Debug.WriteLine(
                string.Format("Looks like the parsed domain part is: {0}", outDomain.Domain)
                );
        }

        [TestMethod]
        public void ParseWildcardDomain()
        {
            //  Try parsing a 'wildcard' domain
            if (DomainName.Library.DomainName.TryParse("photos.verybritish.co.uk", out outDomain))
            {
                //  The domain should be parsed as 'verybritish'
                Assert.AreEqual<string>("verybritish", outDomain.Domain);

                //  The TLD is 'co.uk'
                Assert.AreEqual<string>("co.uk", outDomain.TLD);

                //  The SLD is just an alias for 'Domain':
                Assert.AreEqual<string>(outDomain.Domain, outDomain.SLD);

                //  The subdomain is everything else to the left of the domain:
                Assert.AreEqual<string>("photos", outDomain.SubDomain);
            }
            else
            {
                Debug.WriteLine("Apparently, we couldn't parse photos.verybritish.co.uk");
            }
        }

        [TestMethod]
        public void ParseExceptionDomain()
        {
            //  Try parsing an 'exception' domain
            DomainName.Library.DomainName.TryParse("photos.wishlist.parliament.uk", out outDomain);

            //  The domain should be parsed as 'parliament'
            Assert.AreEqual<string>("parliament", outDomain.Domain);

            Debug.WriteLine(
                string.Format("Looks like the parsed domain part is: {0}", outDomain.Domain)
                );
        }
        
        [TestMethod]
        public void ParseNormalDomainWhereTLDOccursInDomain()
        {
            //  Try parsing a 'normal' domain where the TLD part also occurs in the domain part
            DomainName.Library.DomainName.TryParse("russian.cntv.cn", out outDomain);

            //  The domain should be parsed as 'cntv'
            Assert.AreEqual<string>("cntv", outDomain.Domain);

            Debug.WriteLine(
                string.Format("Looks like the parsed domain part is: {0}", outDomain.Domain)
                );
        }

        [TestMethod]
        public void ParseWildcardDomainWhereTLDOccursInDomain()
        {
            //  Try parsing a 'wildcard' domain where the TLD part also occurs in the domain part
            DomainName.Library.DomainName.TryParse("com.er.com.er", out outDomain);

            //  The domain should be parsed as 'er'
            Assert.AreEqual<string>("er", outDomain.Domain);

            Debug.WriteLine(
                string.Format("Looks like the parsed domain part is: {0}", outDomain.Domain)
                );
        }

        [TestMethod]
        public void ParseExceptionDomainWhereTLDOccursInSubdomain()
        {
            //  Try parsing an 'exception' domain where the TLD part also occurs in the subdomain part
            DomainName.Library.DomainName.TryParse("www.ck.www.ck", out outDomain);

            //  The domain should be parsed as 'www'
            Assert.AreEqual<string>("www", outDomain.Domain);

            Debug.WriteLine(
                string.Format("Looks like the parsed domain part is: {0}", outDomain.Domain)
                );
        }
    }
}
