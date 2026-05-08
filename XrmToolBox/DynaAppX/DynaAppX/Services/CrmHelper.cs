using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DynaAppX.Services
{
    public static class CrmHelper
    {
        public static string EscapeXml(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
        }

        public static bool IsGuid(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return Guid.TryParse(value.Replace("{", "").Replace("}", ""), out _);
        }

        public static string BuildRecordSearchFetchXml(EntityWrapper entityWrapper, string searchText)
        {
            var conditions = new List<string>();

            if (IsGuid(searchText))
            {
                conditions.Add($"<condition attribute='{entityWrapper.PrimaryIdAttribute}' operator='eq' value='{searchText}'/>");
            }
            else if (!string.IsNullOrWhiteSpace(searchText))
            {
                if (entityWrapper.Attributes != null)
                {
                    var stringAttrs = entityWrapper.Attributes
                        .Where(a => a.AttributeOf == null &&
                                    a.AttributeType == AttributeTypeCode.String &&
                                    (a.LogicalName.ToLowerInvariant().Contains("code") ||
                                     a.LogicalName.ToLowerInvariant().Contains("name") ||
                                     a.LogicalName.ToLowerInvariant().Contains("number")))
                        .ToList();

                    foreach (var attr in stringAttrs)
                    {
                        conditions.Add($"<condition attribute='{attr.LogicalName}' operator='like' value='%{EscapeXml(searchText)}%'/>");
                    }
                }

                if (!string.IsNullOrEmpty(entityWrapper.PrimaryNameAttribute))
                {
                    conditions.Add($"<condition attribute='{entityWrapper.PrimaryNameAttribute}' operator='like' value='%{EscapeXml(searchText)}%'/>");
                }
            }

            var conditionXml = conditions.Count > 0
                ? $"<filter type='or'>{string.Join("", conditions)}</filter>"
                : "";

            return $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='30'>
                <entity name='{entityWrapper.LogicalName}'>
                    <attribute name='{entityWrapper.PrimaryIdAttribute}'/>
                    <attribute name='{entityWrapper.PrimaryNameAttribute}'/>
                    <order attribute='modifiedon' descending='true'/>
                    {conditionXml}
                </entity>
            </fetch>";
        }
    }
}
