// (c) Xavalon. All rights reserved.

using System.Collections.Generic;
using System.Xml;
using Xavalon.XamlStyler.MarkupExtensions.Parser;
using Xavalon.XamlStyler.Model;

namespace Xavalon.XamlStyler.MarkupExtensions.Formatter
{
    public class AttributeInfoFactory
    {
        private readonly AttributeOrderRules orderRules;
        private readonly MarkupExtensionParser parser;
        private readonly IList<string> ignoredNamespacesLocalNames;

        public AttributeInfoFactory(MarkupExtensionParser parser, AttributeOrderRules orderRules, IList<string> ignoredNamespacesLocalNames)
        {
            this.parser = parser;
            this.orderRules = orderRules;
            this.ignoredNamespacesLocalNames = ignoredNamespacesLocalNames;
        }

        public AttributeInfo Create(XmlReader xmlReader)
        {
            string attributeName = xmlReader.Name;
            string attributeValue = xmlReader.Value;
            
            AttributeOrderRule orderRule = this.orderRules.GetRuleFor(attributeName);
            MarkupExtension markupExtension = this.ParseMarkupExtension(attributeValue);
            var attributeHasIgnoredNamespace = this.CheckIfAttributeHasIgnoredNamespace(attributeName, out string attributeNameWithoutNamespace);

            return new AttributeInfo(attributeName, attributeValue, attributeHasIgnoredNamespace, attributeNameWithoutNamespace, orderRule, markupExtension);
        }

        private bool CheckIfAttributeHasIgnoredNamespace(string attributeName, out string attributeNameWithoutNamespace)
        {
            attributeNameWithoutNamespace = string.Empty;
            var semicolonIndex = attributeName.IndexOf(':');
            // If semicolon is in the attributeName and is not first nor last.
            if (semicolonIndex > 0 && semicolonIndex < attributeName.Length - 1)
            {
                var localName = attributeName.Substring(0, semicolonIndex);
                if (ignoredNamespacesLocalNames.Contains(localName))
                {
                    attributeNameWithoutNamespace = attributeName.Substring(semicolonIndex + 1);
                    return true;
                }
            }
            return false;
        }

        private MarkupExtension ParseMarkupExtension(string value)
        {
            // Only try to parse if there is a chance that it is a markup extension.
            if (value.IndexOf('{') != -1)
            {
                MarkupExtension markupExtension;
                if (this.parser.TryParse(value, out markupExtension))
                {
                    return markupExtension;
                }
            }

            return null;
        }
    }
}