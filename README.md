domainname-parser
=================

.NET domain name parsing library (uses http://publicsuffix.org rules)

Overview
-----------
A domain name has 3 major parts:

- The 'top level' domain, or TLD (like .com, .net, .info) 
- The 'domain name', or SLD (like google, microsoft, ebay) 
- The subdomain (like www, photos)

Parsing a domain name into it's 3 major parts sounds easy, but is no trivial task. What happens when you come across hosts like test.co.uk? What about hosts like www.parliament.uk?

From http://publicsuffix.org :

"Since there is no algorithmic method of finding the highest level at which a domain may be registered for a particular top-level domain (the policies differ with each registry), the only method is to create a list. This is the aim of the Public Suffix List."

The domain name parsing component uses the list of rules at www.publicsuffix.org to parse a domain name into 3 component parts. 

There are 3 types of rules: 
'Normal' domain rules, 'Wildcard' rules, and 'Exception' rules.

Quick Start
-----------

- Download the latest release from the NuGet repository (or compiled from the source, here)
- Download the latest rules file: http://publicsuffix.org/list/
- Configure your app.config in the to point to the rules file you just downloaded (see the sample app.config included in the .zip) 
- Look at the included unit tests to see how easy the component is to use 

Usage
-----------

Using the component is simple. Just use the constructor or the static 'TryParse' method and pass in the complete host name string. The component will return the parsed domain in a DomainName component. It's as simple as that:

    // Try parsing a 'wildcard' domain 
    if (DomainName.TryParse("photos.verybritish.co.uk", out outDomain)) 
    { 
      // The domain should be parsed as 'verybritish' 
      Assert.AreEqual("verybritish", outDomain.Domain);
      
      // The TLD is 'co.uk' 
      Assert.AreEqual("co.uk", outDomain.TLD);
      
      // The SLD is just an alias for 'Domain': 
      Assert.AreEqual(outDomain.Domain, outDomain.SLD);
      
      // The subdomain is everything else to the left of the domain: 
      Assert.AreEqual("photos", outDomain.SubDomain); 
    } 
    else 
    { 
      Debug.WriteLine("Apparently, we couldn't parse photos.verybritish.co.uk"); 
    }

Where can I get the latest rules list?
-----------
You can download the latest rules from the Public Suffix site.
