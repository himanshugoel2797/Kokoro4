using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Kokoro.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL;

namespace Kokoro.Engine.Graphics
{
    public class TextureStreamer
    {
        public class TextureStream
        {
            private TextureStreamer owner;
            internal int pboId;
            internal Fence uploadFence;
            internal int curLevel;
            internal ITextureSource src;
            private int sz, dims;

            public bool IsDone { get; internal set; }
            public Texture TargetTexture { get; internal set; }


            public TextureStream(TextureStreamer owner)
            {
                pboId = GL.GenBuffer();
                uploadFence = new Fence();
                IsDone = true;
                this.owner = owner;

                EngineManager.RegisterBackgroundTask(UpdateProgress);
                GraphicsDevice.Cleanup += Dispose;
            }

            private void Upload()
            {
                GPUStateMachine.BindBuffer(BufferTarget.PixelUnpackBuffer, pboId);
                GL.BufferData(BufferTarget.PixelUnpackBuffer, sz >> (int)System.Math.Pow(curLevel, dims), src.GetPixelData(curLevel), BufferUsageHint.StreamDraw);

                switch (dims)
                {
                    case 1:
                        GL.TextureSubImage1D(TargetTexture.id, curLevel, 0, TargetTexture.Width >> curLevel, (OpenTK.Graphics.OpenGL.PixelFormat)src.GetFormat(), (OpenTK.Graphics.OpenGL.PixelType)src.GetPixelType(), IntPtr.Zero);
                        break;
                    case 2:
                        GL.TextureSubImage2D(TargetTexture.id, curLevel, 0, 0, TargetTexture.Width >> curLevel, TargetTexture.Height >> curLevel, (OpenTK.Graphics.OpenGL.PixelFormat)src.GetFormat(), (OpenTK.Graphics.OpenGL.PixelType)src.GetPixelType(), IntPtr.Zero);
                        break;
                    case 3:
                        GL.TextureSubImage3D(TargetTexture.id, curLevel, 0, 0, 0, TargetTexture.Width >> curLevel, TargetTexture.Height >> curLevel, TargetTexture.Depth >> curLevel, (OpenTK.Graphics.OpenGL.PixelFormat)src.GetFormat(), (OpenTK.Graphics.OpenGL.PixelType)src.GetPixelType(), IntPtr.Zero);
                        break;
                }
                uploadFence.PlaceFence();

                GPUStateMachine.UnbindBuffer(BufferTarget.PixelUnpackBuffer);
            }

            internal void Setup(ITextureSource src)
            {
                this.src = src;
                IsDone = false;
                curLevel = src.GetLevels() - 1;
                dims = src.GetDimensions();
                owner.pending.Add(this);

                TargetTexture = new Texture();
                TargetTexture.SetData(src, curLevel);

                int pixelSize = 4;

                switch (dims)
                {
                    case 1:
                        sz = TargetTexture.Width * pixelSize;
                        break;
                    case 2:
                        sz = (TargetTexture.Width * TargetTexture.Height) * pixelSize;
                        break;
                    case 3:
                        sz = (TargetTexture.Width * TargetTexture.Height * TargetTexture.Depth) * pixelSize;
                        break;
                }

                //Copy the data over, uploading the highest mipmap normally and the others asynchronously

                curLevel--;

                //Setup the pbo for further uploads
                if (curLevel >= 0)
                {
                    Upload();
                }
                else
                {
                    IsDone = true;
                    owner.pending.Remove(this);
                    owner.buffers.Enqueue(this);
                }
            }

            private void UpdateProgress()
            {
                if (IsDone) return;

                if (uploadFence.Raised(1))
                {
                    //Proceed to the next upload
                    curLevel--;
                    if (curLevel >= 0)
                        Upload();
                    else
                    {
                        //Finished and update buffers appropriately
                        IsDone = true;
                        owner.pending.Remove(this);
                        owner.buffers.Enqueue(this);
                    }
                }
            }

            private void Dispose()
            {
                GL.DeleteBuffer(pboId);
            }
        }

        private Queue<TextureStream> buffers;
        private HashSet<TextureStream> pending;

        public TextureStreamer(int poolSize)
        {
            buffers = new Queue<TextureStream>();
            pending = new HashSet<TextureStream>();


            for (int i = 0; i < poolSize; i++)
            {
                TextureStream t = new TextureStream(this);
                buffers.Enqueue(t);
            }
        }

        public TextureStream UploadTexture(ITextureSource src)
        {
            //Wait for an available texture stream
            if (!EngineManager.ExecuteBackgroundTasksUntil(() => (buffers.Count != 0)))
            {
                throw new UploadTextureException("No streams available for upload.");
            }

            TextureStream tStream = buffers.Dequeue();
            tStream.Setup(src);
            return tStream;
        }

        [Serializable]
        private class UploadTextureException : Exception
        {
            public UploadTextureException()
            {
            }

            public UploadTextureException(string message) : base(message)
            {
            }

            public UploadTextureException(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected UploadTextureException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }
    }
}
