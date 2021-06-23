using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using SharpDX;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SoftEngine
{
    class Device
    {
        private byte[] backBuffer;
        private WriteableBitmap bmp;

        public Device(WriteableBitmap bmp)
        {
            this.bmp = bmp;
            // 后台缓冲区大小值是要绘制的像素
            // 屏幕(width*heigth)*4 (R,G,B&Alpha)
            backBuffer = new byte[bmp.PixelWidth * bmp.PixelHeight * 4];
        }
        // 清楚后台缓冲区为制定颜色
        public void Clear(byte r, byte g, byte b, byte a)
        {
            for( var index = 0;index < backBuffer.Length;index += 4)
            {
                // win使用bgra
                backBuffer[index] = b;
                backBuffer[index + 1] = g;
                backBuffer[index + 2] = r;
                backBuffer[index + 3] = a;
            }
        }
        //当一切准备就绪时，我们就可以刷新后台缓冲区到前台缓冲区
        public void Present()
        {
            using (var stream = bmp.PixelBuffer.AsStream())
            {
                // 将我们的byte[]后台缓冲写入到WriteableBitmap流
                stream.Write(backBuffer, 0, backBuffer.Length);
            }

            //请求整个位图重绘
            //bmp.InvalidateProperty();
            bmp.Invalidate();
        }
        // 调用此方法把一个像素绘制到制定的x,y坐标上
        public void PutPixel(int x, int y, Colors color)
        {
            var index = (x + y * bmp.PixelWidth) * 4;//着重理解

            backBuffer[index] = (byte)(color.Blue * 255);
            backBuffer[index + 1] = (byte)(color.Green * 255);
            backBuffer[index + 2] = (byte)(color.Red * 255);
            backBuffer[index + 3] = (byte)(color.Alpha * 255);
        }

        // 将三维坐标和变换矩阵转换成二维坐标
        public Vector2 Project(Vector3 coord, Matrix transMat)
        {
            var point = Vector3.TransformCoordinate(coord, transMat);//x,y的值似乎是[0,1]

            //变换后的坐标起始点是坐标系的中心点
            //但是，在屏幕上，我们以左上角为起始点
            // 我们需要重新计算使他们的起始点变为左上角
            var x = point.X * bmp.PixelWidth + bmp.PixelWidth / 2.0f;
            var y = -point.Y * bmp.PixelHeight + bmp.PixelHeight / 2.0f;

            return (new Vector2(x,y));
        }
        //如果二维坐标在可是范围内则绘制
        public void DrawPoint(Vector2 point)
        {
            if( point.X >= 0 && point.Y >= 0 && point.X < bmp.PixelWidth && point.Y < bmp.PixelHeight )
            {
                //绘制一个黄点
                PutPixel((int)point.X, (int)point.Y, new Color4(1.0f, 1.0f, 1.0f, 1.0f));
            }
        }
        //主体循环，每一帧都会重新计算投射的顶点
        public void Render(Camera camera, params Mesh[] meshes)
        {
            //获得视图矩阵，参数为摄像机的位置，摄像机观察的对象位置，摄像机的上下方
            var viewMatrix = Matrix.LookAtLH(camera.Position, camera.Target, Vector3.UnitY);
            // 投影矩阵(参数为投影视角,0.78为π/4)
            var projectionMatrix = Matrix.PerspectiveFovRH(0.78f, (float)bmp.PixelWidth / bmp.PixelHeight, 0.01f, 1.0f) ;

            foreach( Mesh mesh in meshes)
            {
                // 请注意，在平移前先旋转
                var worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z)*
                    Matrix.Translation(mesh.Position);

                var transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

                foreach( var vertex in mesh.Vertices)
                {
                    var point = Project(vertex, transformMatrix);

                    DrawPoint(point);
                }
            }
        }
    }
}
