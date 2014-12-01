using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Legion_Tactical_Launcher
{

    [XmlRoot("launcher-slides")]
    public class Slides
    {
        [XmlElement("content-root")]
        public string contentRoot { get; set; }

        [XmlElement("slide")]
        public ImageSlides[] imageSlides { get; set; }

        public Slides() { imageSlides = null; }

    }


    [Serializable]
    public class ImageSlides
    {
        [XmlElement("img-uri")]
        public string imgUri { get; set; }

        [XmlElement("target-uri")]
        public string targetUri { get; set; }

    }
}
