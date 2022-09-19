#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using System.IO;
using System.Xml;

namespace nv.editor
{
    public static class BitmapFontImporter
    {
        [MenuItem(Consts.Menu.ROOT + "/Assets/Generate Bitmap Font")]
        public static void GenerateFont()
        {
            TextAsset selected = (TextAsset)Selection.activeObject;
            string rootPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(selected));

            Texture2D texture = AssetDatabase.LoadAssetAtPath(rootPath + "/" + selected.name + ".png", typeof(Texture2D)) as Texture2D;
            if(!texture) throw new UnityException("Texture2d asset doesn't exist for " + selected.name);

            string exportPath = rootPath + "/" + Path.GetFileNameWithoutExtension(selected.name);

            Work(selected, exportPath, texture);
        }


        private static void Work(TextAsset import, string exportPath, Texture2D texture)
        {
            if(!import) throw new UnityException(import.name + "is not a valid font-xml file");

            Font font = new Font();

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(import.text);

            XmlNode info = xml.GetElementsByTagName("info")[0];
            //XmlNode common = xml.GetElementsByTagName( "common" )[ 0 ];        
            XmlNodeList chars = xml.GetElementsByTagName("chars")[0].ChildNodes;

            float texW = texture.width;
            float texH = texture.height;

            CharacterInfo[] charInfos = new CharacterInfo[chars.Count];
            Rect r;

            for(int i = 0; i < chars.Count; i++)
            {
                XmlNode charNode = chars[i];
                CharacterInfo charInfo = new CharacterInfo();

                charInfo.index = ToInt(charNode, "id");
                charInfo.advance = ToInt(charNode, "xadvance");
                //charInfo.flipped = false;

                r = new Rect();
                r.x = ((float)ToInt(charNode, "x")) / texW;
                r.y = ((float)ToInt(charNode, "y")) / texH;
                r.width = ((float)ToInt(charNode, "width")) / texW;
                r.height = ((float)ToInt(charNode, "height")) / texH;
                r.y = 1f - r.y - r.height;
                //charInfo.uv = r;
                charInfo.uvBottomLeft = r.min;
                charInfo.uvTopRight = r.max;


                r = new Rect();
                r.x = (float)ToInt(charNode, "xoffset");
                r.y = (float)ToInt(charNode, "yoffset");
                r.width = (float)ToInt(charNode, "width");
                r.height = (float)ToInt(charNode, "height");
                r.y = -r.y;
                r.height = -r.height;
                //charInfo.vert = r;
                charInfo.minX = (int)r.xMin;
                charInfo.minY = (int)r.yMin;
                charInfo.maxX = (int)r.xMax;
                charInfo.maxY = (int)r.yMax;

                charInfos[i] = charInfo;
            }

            // Create material
            Shader shader = Shader.Find("UI/Default");
            Material material = new Material(shader);
            material.mainTexture = texture;
            AssetDatabase.CreateAsset(material, exportPath + ".mat");

            // Create font
            font.material = material;
            font.name = info.Attributes.GetNamedItem("face").InnerText;
            font.characterInfo = charInfos;
            AssetDatabase.CreateAsset(font, exportPath + ".fontsettings");
        }

        private static int ToInt(XmlNode node, string name)
        {
            return Convert.ToInt32(node.Attributes.GetNamedItem(name).InnerText);
        }
    }
}
#endif