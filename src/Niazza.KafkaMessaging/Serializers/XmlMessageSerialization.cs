using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Niazza.KafkaMessaging.Serializers
{
    public class XmlMessageSerialization : IMessageSerialization
    {
        private readonly XmlSerializerBuilder _builder;

        public XmlMessageSerialization(Action<IXmlSerializerBuilder> options = null)
        {
            _builder = new XmlSerializerBuilder();
            options?.Invoke(_builder);
        }

        public object Deserialize(string message, Type type)
        {
            var xmlSerializer = _builder.Build(type);
            var settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            

            using (var stringReader = XmlReader.Create(new StringReader(message), settings))
            {
                return xmlSerializer.Deserialize(stringReader);
            }
        }

        public string Serialize(object objectToSerialize)
        {
            var xmlSerializer = _builder.Build(objectToSerialize.GetType());
            using (var textWriter = new StringWriter())
            {
                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                namespaces.Add(string.Empty, string.Empty);
                xmlSerializer.Serialize(textWriter, objectToSerialize, namespaces);
                return textWriter.ToString();
            }
        }
    }

    public class NoNamespaceXmlWriter : XmlTextWriter
    {
        public NoNamespaceXmlWriter(TextWriter output)
            : base(output) { Formatting = System.Xml.Formatting.Indented; }

        public override void WriteStartDocument() { }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            base.WriteStartElement("", localName, "");
        }
    }


    public interface IXmlOptionsSet<in TIn, out TOut>
    {
        TOut Set(TIn parameters);
    }

    public interface ISetDefaultNameSpace
    {
        void Set(string defaultNameSpace);
    }

    public interface IXmlSerializerBuilder: 
        IXmlOptionsSet<XmlAttributeOverrides, 
        IXmlOptionsSet<Type[], 
        IXmlOptionsSet<XmlRootAttribute,
        ISetDefaultNameSpace>>>
    {
    }

    public class XmlSerializerBuilder :
        IXmlSerializerBuilder,
        IXmlOptionsSet<Type[], IXmlOptionsSet<XmlRootAttribute, ISetDefaultNameSpace>>,
        IXmlOptionsSet<XmlRootAttribute, ISetDefaultNameSpace>,
        ISetDefaultNameSpace
    {
        /// <summary>
        /// Allows you to override property, field, and class attributes when you use
        /// the <see cref="T:System.Xml.Serialization.XmlSerializer"></see> to serialize or deserialize an object.
        /// </summary>
        public XmlAttributeOverrides AttributeOverrides { get; private set; }
        /// <summary>
        /// A array of additional object types to serialize.
        /// </summary>
        public Type[] ExtraTypes { get; private set; }
        /// <summary>
        /// The default namespace of all XML elements in the XML document.
        /// </summary>
        public string DefaultNamespace { get; private set; }
        /// <summary>
        /// An that defines the XML root element properties.
        /// </summary>
        public XmlRootAttribute XmlRootAttribute { get; private set; }


        public void Set(string defaultNamespace)
        {
            DefaultNamespace = defaultNamespace;
            
        }

        public ISetDefaultNameSpace Set(XmlRootAttribute parameters)
        {
            XmlRootAttribute = parameters;
            return this;
        }

        public IXmlOptionsSet<XmlRootAttribute, ISetDefaultNameSpace> Set(Type[] parameters)
        {
            ExtraTypes = parameters;
            return this;
        }

        public IXmlOptionsSet<Type[], IXmlOptionsSet<XmlRootAttribute, ISetDefaultNameSpace>> Set(XmlAttributeOverrides parameters)
        {
            AttributeOverrides = parameters;
            return this;
        }

        public XmlSerializer Build(Type type)
        {
            return AttributeOverrides is null ? new XmlSerializer(type) :
                ExtraTypes is null ? new XmlSerializer(type, AttributeOverrides) :
                new XmlSerializer(type, AttributeOverrides, ExtraTypes, XmlRootAttribute, DefaultNamespace);
        }
    }
}
