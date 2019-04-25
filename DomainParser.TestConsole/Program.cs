using System;
using System.IO;
using System.Diagnostics;
using DomainParser.Library;
using DomainParser.Tests;

namespace DomainParser.TestConsole {
    class Program {
        static void Main(string[] args) {

            TLDRulesCache.RulesFileLocation = @"d:\" + Path.GetFileName(TLDRulesCache.RulesFileLocation);

            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            DomainTests tests = new DomainTests();

            Debug.WriteLine("-------- ParseNormalDomain");
            tests.ParseNormalDomain();
            Debug.WriteLine("");

            Debug.WriteLine("-------- ParseNormalDomainWhereTLDOccursInDomain");
            tests.ParseNormalDomainWhereTLDOccursInDomain();
            Debug.WriteLine("");

            Debug.WriteLine("-------- ParseWildcardDomain");
            tests.ParseWildcardDomain();
            Debug.WriteLine("");

            Debug.WriteLine("-------- ParseWildcardDomainWhereTLDOccursInDomain");
            tests.ParseWildcardDomainWhereTLDOccursInDomain();
            Debug.WriteLine("");

            Debug.WriteLine("-------- ParseExceptionDomain");
            tests.ParseExceptionDomain();
            Debug.WriteLine("");

            Debug.WriteLine("-------- ParseExceptionDomainWhereTLDOccursInSubdomain");
            tests.ParseExceptionDomainWhereTLDOccursInSubdomain();


            Console.ReadKey();

        }
    }
}
