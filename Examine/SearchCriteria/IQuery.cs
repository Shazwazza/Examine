using System;
using System.Collections.Generic;
namespace UmbracoExamine.Core.SearchCriteria
{
    public interface IQuery
    {
        IBooleanOperation Id(int id);
        IBooleanOperation NodeName(string nodeName);
        IBooleanOperation NodeName(IExamineValue nodeName);
        IBooleanOperation NodeTypeAlias(string nodeTypeAlias);
        IBooleanOperation NodeTypeAlias(IExamineValue nodeTypeAlias);
        IBooleanOperation ParentId(int id);
        IBooleanOperation Field(string fieldName, string fieldValue);
        IBooleanOperation Field(string fieldName, IExamineValue fieldValue);
        IBooleanOperation MultipleFields(IEnumerable<string> fieldNames, string fieldValue);
        IBooleanOperation MultipleFields(IEnumerable<string> fieldNames, IExamineValue fieldValue);
        IBooleanOperation Range(string fieldName, DateTime start, DateTime end);
        IBooleanOperation Range(string fieldName, DateTime start, DateTime end, bool inclusive);
        IBooleanOperation Range(string fieldName, int start, int end);
        IBooleanOperation Range(string fieldName, int start, int end, bool inclusive);
        IBooleanOperation Range(string fieldName, string start, string end);
        IBooleanOperation Range(string fieldName, string start, string end, bool inclusive);
    }
}
