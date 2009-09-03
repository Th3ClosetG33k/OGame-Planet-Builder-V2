using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Text;

namespace OGPB
{
    public class Translate
    {
        ResourceManager english = new ResourceManager("OGPB.language.en", Assembly.GetExecutingAssembly());
        ResourceManager translation;

        public Translate(string language)
        {
            if (language == "en" || language == "")
            {
                translation = english;
            }
            else
            {
                translation = new ResourceManager("OGPB.language." + language, Assembly.GetExecutingAssembly());
            }
        }

        public string GetString(string toTranslate)
        {
            string translated = translation.GetString(toTranslate);
            if (translated == "")
            {
                translated = english.GetString(toTranslate);
            }
            return translated;
        }
    }
}
