using Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace Infrastructure.Services
{
    public class XmlValidator : IXmlValidator
    {
        public void Validate(string xmlString, string xsdPath)
        {
            var schema = new XmlSchemaSet();
            schema.Add(null, xsdPath);

            var doc = new XmlDocument();
            doc.LoadXml(xmlString);
            doc.Schemas.Add(schema);

            doc.Validate((s, e) =>
            {
                if (e.Severity == XmlSeverityType.Error)
                    throw new Exception($"XSD Error: {e.Message}");
            });
        }
    }

}
