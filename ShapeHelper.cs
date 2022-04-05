using Newtonsoft.Json;
using Riches.Visio.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Riches.Visio
{
    public class DoUnit
    {
        public Stack<KeyValuePair<IShape, object>> Units = new Stack<KeyValuePair<IShape, object>>();
    }
    public class MyImg
    {
        public Image Image { get; set; }
        public int I { get; set; }
    }
    public class ImageConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Bitmap);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var m = new MemoryStream(Convert.FromBase64String((string)reader.Value));
            return (Bitmap)Bitmap.FromStream(m);
        }

        //public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        //{
        //    MyImg myImg = new MyImg();
        //    var m = new MemoryStream(Convert.FromBase64String((string)reader.Value));
        //    BinaryReader binaryReader = new BinaryReader(m);
        //    var len=binaryReader.ReadInt32();
        //    var buffer=binaryReader.ReadBytes(len);
        //    myImg.Image = Bitmap.FromStream(new MemoryStream(buffer));
        //    myImg.I = binaryReader.ReadInt32();
        //    return myImg;
        //}
        //public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        //{
        //    MyImg myImg = (MyImg)value;
        //    MemoryStream m = new MemoryStream();
        //    myImg.Image.Save(m, System.Drawing.Imaging.ImageFormat.Jpeg);

        //    MemoryStream bm = new MemoryStream();
        //    BinaryWriter binary = new BinaryWriter(bm);
        //    var buffer = m.ToArray();
        //    binary.Write(buffer.Length);
        //    binary.Write(buffer);
        //    binary.Write(myImg.I);
        //    m.Dispose();
        //    bm.Dispose();
        //    writer.WriteValue(Convert.ToBase64String(bm.ToArray()));
        //}

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Bitmap bmp = (Bitmap)value;
            MemoryStream m = new MemoryStream();
            bmp.Save(m, System.Drawing.Imaging.ImageFormat.Jpeg);
            writer.WriteValue(Convert.ToBase64String(m.ToArray()));
        }
    }
}
