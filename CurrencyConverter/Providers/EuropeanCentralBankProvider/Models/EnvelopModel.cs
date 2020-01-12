using System.Xml.Serialization;

namespace CurrencyConverter.Providers.EuropeanCentralBankProvider.Models
{
    [XmlRoot(ElementName = "Envelope", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
    public class Envelope
    {
        [XmlElement(ElementName = "Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
        public RootCube Cube { get; set; }

        [XmlAttribute(AttributeName = "gesmes", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Gesmes { get; set; }

        [XmlElement(ElementName = "Sender", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
        public Sender Sender { get; set; }

        [XmlElement(ElementName = "subject", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
        public string Subject { get; set; }

        [XmlAttribute(AttributeName = "xmlns", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xmlns { get; set; }
    }

    [XmlRoot(ElementName = "Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
    public class RootCube
    {
        [XmlElement(ElementName = "Cube")]
        public TimeCube[] Cubes { get; set; }
    }

    [XmlRoot(ElementName = "Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
    public class TimeCube
    {
        [XmlElement(ElementName = "Cube")]
        public Cube[] Cubes { get; set; }

        [XmlAttribute(AttributeName = "time")]
        public string Time { get; set; }
    }

    [XmlRoot(ElementName = "Cube", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
    public class Cube
    {
        [XmlAttribute(AttributeName = "currency")]
        public string Currency { get; set; }
        [XmlAttribute(AttributeName = "rate")]
        public string Rate { get; set; }
    }

    [XmlRoot(ElementName = "sender", Namespace = "http://www.ecb.int/vocabulary/2002-08-01/eurofxref")]
    public class Sender
    {
        [XmlElement(ElementName = "name", Namespace = "http://www.gesmes.org/xml/2002-08-01")]
        public string Name { get; set; }
    }
}
