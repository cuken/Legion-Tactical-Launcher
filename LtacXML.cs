using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace Legion_Tactical_Launcher
{
    public class LtacXML
    {
       public static List<string> GetXML(string URL)
        {
           List<string> imagePaths = new List<string>();
           XmlDocument doc = new XmlDocument();
           doc.Load(URL);
           XmlElement root = doc.DocumentElement;
           XmlNodeList nodes = root.SelectNodes("slide");

           foreach (XmlNode node in nodes)
           {
               imagePaths.Add(node.InnerXml.ToString());            
           }

           return imagePaths;

        }

       public static Slides getImageSlides(string url)
       {
           try
           {               
               XmlSerializer xmlSerial = new XmlSerializer(typeof(Slides));
               object ImageSlide = null;
               string xmlFile;

               using (var client = new WebClient())
               {
                    xmlFile = client.DownloadString(url);
               }

               using (TextReader reader = new StringReader(xmlFile))
               {
                   ImageSlide = xmlSerial.Deserialize(reader);
               }

               return (Slides)ImageSlide;
           }
           catch
           {
               return null;
           }
            
       }

    }
}
