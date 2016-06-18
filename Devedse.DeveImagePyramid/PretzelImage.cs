namespace Devedse.DeveImagePyramid
{
    public class PretzelImage
    {
        public byte[] Data { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public PretzelImage(byte[] data, int width, int height)
        {
            Data = data;
            Width = width;
            Height = height;
        }
    }
}
