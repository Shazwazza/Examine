---
# Feel free to add content and custom Front Matter to this file.
# To modify the layout, see https://jekyllrb.com/docs/themes/#overriding-theme-defaults

layout: default
---

_[...Back to home](index)_

Configuration
===

An index can be configured in many ways including different configurations per field such as how those values are analyzed, indexed, tokenized ... basically how the data is stored and retrieved. 

The `LuceneIndex` constructor has several **optional** parameters that can be supplied to configure the index:

* __fieldDefinitions__ _`FieldDefinitionCollection`_ - Manages the mappings between a field name and it's index value type
* __analyzer__ _`Analyzer`_ - The default Lucene Analyzer to use for each field (default = `StandardAnalyzer`)
* __validator__ _`IValueSetValidator`_ - Used to validate a value set to be indexed, if validation fails it will not be indexed
* __indexValueTypesFactory__ _`IReadOnlyDictionary<string, IFieldValueTypeFactory>`_ - Allows you to define custom data types, basically how a field value is stored and retrieved. 

## Default value types

These are the default value types provided:

* `FieldDefinitionTypes.FullText` - Default. The field will be indexed with the index's default Analyzer without any fancy type storage or sortability. Generally this is fine for normal text searching.
* `FieldDefinitionTypes.FullTextSortable` - Will be indexed with FullText but also enable sorting on this field for search results.
* `FieldDefinitionTypes.Integer` - Stored as a numerical structure.
* `FieldDefinitionTypes.Float` - Stored as a numerical structure.
* `FieldDefinitionTypes.Double` - Stored as a numerical structure.
* `FieldDefinitionTypes.Long` - Stored as a numerical structure.
* `FieldDefinitionTypes.DateTime` - Stored as a DateTime, represented by a numerical structure.
* `FieldDefinitionTypes.DateYear` - Just like DateTime but with precision only to the year.
* `FieldDefinitionTypes.DateMonth` - Just like DateTime but with precision only to the month.
* `FieldDefinitionTypes.DateDay` - Just like DateTime but with precision only to the day.
* `FieldDefinitionTypes.DateHour` - Just like DateTime but with precision only to the hour.
* `FieldDefinitionTypes.DateMinute` - Just like DateTime but with precision only to the minute.
* `FieldDefinitionTypes.EmailAddress` - Uses custom analyzers for dealing with email address searching.
* `FieldDefinitionTypes.InvariantCultureIgnoreCase` - Uses custom analyzers for dealing with text so it can be searched on regardless of the culture/casing.
* `FieldDefinitionTypes.Raw` - Will be indexed without analysis, searching will only match with an exact value.

## Custom field definitions

By default any field that is indexed that is not mapped to an explicit definition will be mapped to the default: `FieldDefinitionTypes.FullText`. If you need to map a field to a specific value type, you can specify this via the constructor, or you can modify the definitions at runtime.

### Via constructor

```cs
//Create and add a new index to the manager
var myIndex = examineManager.AddIndex(
    new LuceneIndex(            
        "MyIndex",              
        new SimpleFSDirectory(new DirectoryInfo("C:\\TestIndexes")),
        // Pass in a custom field definition collection
        new FieldDefinitionCollection(            
            // Set the "Price" field to map to the 'Double' value type.
            // All values for this field will be stored numerically in the index (not as strings).
            new FieldDefinition("Price", FieldDefinitionTypes.Double))));
```

### At runtime

You can modify the field definitions for an index at runtime by using any of the following methods:

* `myIndex.FieldDefinitionCollection.TryAdd`
* `myIndex.FieldDefinitionCollection.AddOrUpdate`
* `myIndex.FieldDefinitionCollection.GetOrAdd`