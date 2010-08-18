See the web.config file included for configuratoin docs.

*** IMPORTANT ***
If you are using Examine in Umbraco currently, this version is NOT 100% backwards compatible.
Some of the underlying framework has been reorganized for the better and is more abstract
which has produced the new Examine.LuceneEngine.DLL. 

You don't need all of the DLLs, just depends on what you need:
- Definitly need Examine.dll
- If you using Umbraco, you need both Examine.LuceneEngine.dll & UmbracoExamine.dll
- If you just want to index your own data, you'll just need Examine.LuceneEngine.dll
- If you want PDF indexing, you'll also need itextsharp.dll

If you are upgrading from previous versions:
- backup your files (of course)
- replace all DLLs with the ones included in this package
- (You don't have to do this, but you should...) upgrade your web.config to use the new objects:
UmbracoExamine.Config.ExamineLuceneIndexes should now be: Examine.LuceneEngine.Config.IndexSets
LuceneExamineSearcher should now be: UmbracoExamineSearcher
LuceneExamineIndexer should now be: UmbracoExamineIndexer
- TEST! ... if you had created your own providers, you will most likely have to change some of your
code to support the new code base.



More documentation references:

http://examine.codeplex.com/documentation
http://farmcode.org/search.aspx?q=Examine


