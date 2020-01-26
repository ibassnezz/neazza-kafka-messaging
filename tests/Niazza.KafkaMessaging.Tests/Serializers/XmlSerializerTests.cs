using System;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Niazza.KafkaMessaging.Serializers;

namespace Niazza.KafkaMessaging.Tests.Serializers
{
    [TestClass]
    public class XmlSerializerTests
    {
        [TestMethod]
        public void SerializeDeserializeTest()
        {
            var serializer = new XmlMessageSerialization();
            var xmlObject =  new ExchangeInfo
            { 
                    DBName = "db",
                    ExchangeTypeID = "ExchangeTypeID",
                    EntityID = "EntityID",
                    InitialHandle = "InitialHandle",
                    ProcSet = "ProcSet",
                    SendMoment = DateTime.Now
                
            };
            var str = serializer.Serialize(xmlObject);
            var deserialized = (ExchangeInfo)serializer.Deserialize(str, typeof(ExchangeInfo));

            Assert.AreEqual(xmlObject.DBName, deserialized.DBName);
            Assert.AreEqual(xmlObject.ExchangeTypeID, deserialized.ExchangeTypeID);
            Assert.AreEqual(xmlObject.EntityID, deserialized.EntityID);
            Assert.AreEqual(xmlObject.InitialHandle, deserialized.InitialHandle);
            Assert.AreEqual(xmlObject.ProcSet, deserialized.ProcSet);
            Assert.AreEqual(xmlObject.SendMoment, deserialized.SendMoment);
            

        }

        [TestMethod]
        public void DeserializeRezonTest()
        {
            var message = @"<ExchangeInfo><ExchangeTypeID>1000500</ExchangeTypeID><EntityID>693864</EntityID><ProcSet>sbExchangeHistorySet</ProcSet><ServerName>SQLDB02Z1\BO</ServerName><DBName>Rezon</DBName><SendMoment>2019-09-29T18:10:57.523</SendMoment><InitialHandle>91158252-CBE2-E911-9115-A0369FF80230</InitialHandle></ExchangeInfo>";
            var serializer = new XmlMessageSerialization();
            var xmlObject = new ExchangeInfo
                {
                    DBName = "Rezon",
                    ExchangeTypeID = "1000500",
                    EntityID = "693864",
                    InitialHandle = "91158252-CBE2-E911-9115-A0369FF80230",
                    ProcSet = "sbExchangeHistorySet",
                    SendMoment = DateTime.Parse("2019-09-29T18:10:57.523"),
                    ServerName = @"SQLDB02Z1\BO",
            };
            
            var deserialized = (ExchangeInfo)serializer.Deserialize(message, typeof(ExchangeInfo));
            
            Assert.AreEqual(xmlObject.DBName, deserialized.DBName);
            Assert.AreEqual(xmlObject.ExchangeTypeID, deserialized.ExchangeTypeID);
            Assert.AreEqual(xmlObject.EntityID, deserialized.EntityID);
            Assert.AreEqual(xmlObject.InitialHandle, deserialized.InitialHandle);
            Assert.AreEqual(xmlObject.ProcSet, deserialized.ProcSet);
            Assert.AreEqual(xmlObject.SendMoment, deserialized.SendMoment);

        }


        [XmlRoot(ElementName = "ExchangeInfo")]
        public class ExchangeInfo
        {
            [XmlElement(ElementName = "ExchangeTypeID")]
            public string ExchangeTypeID { get; set; }

            [XmlElement(ElementName = "EntityID")]
            public string EntityID { get; set; }

            [XmlElement(ElementName = "ProcSet")]
            public string ProcSet { get; set; }

            [XmlElement(ElementName = "DBName")]
            public string DBName { get; set; }

            [XmlElement(ElementName = "SendMoment")]
            public DateTime SendMoment { get; set; }

            [XmlElement(ElementName = "InitialHandle")]
            public string InitialHandle { get; set; }

            [XmlElement(ElementName = "ServerName")]
            public string ServerName { get; set; }

          
        }
    }
}