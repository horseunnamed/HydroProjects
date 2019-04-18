using Core.IO;
using System.Drawing;

namespace Core
{
    class Program
    {
        static void Main(string[] args)
        {
            var channelsTree1 = CgInteraction.ReadChannelsTreeFromCg(Dir.Data("channels.cg"));
            CgInteraction.WriteChannelsTreeToCg(Dir.Data("channels_copy.cg"), channelsTree1);
            var channelsTree2 = CgInteraction.ReadChannelsTreeFromCg(Dir.Data("channels_copy.cg"));
            CgInteraction.WriteChannelsTreeToCg(Dir.Data("channels_copy2.cg"), channelsTree2);
            var bitmap = Drawing.DrawBitmap(944, 944, g =>
            {
                Drawing.DrawChannels(g, channelsTree1.GetAllChannels(), new SolidBrush(Color.Black));
            });
            bitmap.Save(Dir.Data("image1.png"));
            var bitmap2 = Drawing.DrawBitmap(944, 944, g =>
            {
                Drawing.DrawChannels(g, channelsTree2.GetAllChannels(), new SolidBrush(Color.Black));
            });
            bitmap2.Save(Dir.Data("image2.png"));
        }
    }
}
