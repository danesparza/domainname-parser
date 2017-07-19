domainname-parser [![Build status](https://ci.appveyor.com/api/projects/status/4i17cgkp978fh6b1?svg=true)](https://ci.appveyor.com/project/danesparza/domainname-parser) [![NuGet](https://img.shields.io/nuget/v/domainname-parser.svg)](https://www.nuget.org/packages/domainname-parser/)
=================

.NET domain name parsing library (uses http://publicsuffix.org rules)

### Overview

A domain name has 3 major parts:

- The 'top level' domain, or TLD (like `.com`, `.net`, `.info`) 
- The 'domain name', or SLD (like `google`, `microsoft`, `ebay`) 
- The subdomain (like `www`, `photos`)

Parsing a domain name into it's 3 major parts sounds easy, but is no trivial task. What happens when you come across hosts like `test.co.uk`? What about hosts like `www.parliament.uk`?

From http://publicsuffix.org :

> "Since there is no algorithmic method of finding the highest level at which a domain may be registered for a particular top-level domain (the policies differ with each registry), the only method is to create a list. This is the aim of the Public Suffix List."

The domain name parsing component uses the list of rules at www.publicsuffix.org to parse a domain name into 3 component parts. 

There are 3 types of rules: 
'Normal' domain rules, 'Wildcard' rules, and 'Exception' rules.

### Quick Start

Download the latest release from the [NuGet repository](http://nuget.org/packages/domainname-parser)

```powershell
Install-Package domainname-parser
```

Download the latest rules file from [https://publicsuffix.org/list/](https://publicsuffix.org/list/) and configure your `app.config` to point to the rules file you just downloaded (see the [sample app.config](https://github.com/danesparza/domainname-parser/blob/master/DomainName.Library/app.config#L10-L13))

### Example

Using the component is simple. Just use the constructor or the static `TryParse` method and pass in the complete host name string. The component will return the parsed domain in a DomainName component. It's as simple as that:

```CSharp
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
```

### Where can I get the latest rules list? 

You can download the latest rules from the Public Suffix site: http://publicsuffix.org/list/ 
