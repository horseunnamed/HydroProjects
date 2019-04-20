using System.Drawing;
using Core.Channels;

namespace Core
{
    class Program
    {
        static void Main(string[] args)
        {
            var channelsTree = CgInteraction.ReadChannelsTreeFromCg(Dir.Data("channels.cg"));
            CgInteraction.WriteChannelsTreeToCg(Dir.Data("channels_copy.cg"), channelsTree);
            var bitmap = Drawing.DrawBitmap(944, 944, g =>
            {
                Drawing.DrawChannels(g, channelsTree.GetAllChannels(), new SolidBrush(Color.Black));
            });
            bitmap.Save(Dir.Data("image1.png"));
        }
    }
}
