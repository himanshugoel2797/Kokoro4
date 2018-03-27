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
            private int pboId;
            private Fence uploadFence;
            private int curLevel, maxLevels;
            private ITextureSource src;
            private int sz, dims;

            public bool IsDone { get; private set; }
            public Texture TargetTexture { get; private set; }
            public TextureSampler TargetSampler { get; private set; }

            public TextureStream(TextureStreamer owner)
            {
                GL.CreateBuffers(1, out pboId);
                uploadFence = new Fence();
                IsDone = true;
                this.owner = owner;

                EngineManager.RegisterBackgroundTask(UpdateProgress);
                GraphicsDevice.Cleanup.Add(Dispose);
            }

            private void Upload()
            {
                GPUStateMachine.BindBuffer((OpenTK.Graphics.OpenGL.BufferTarget)BufferTarget.PixelUnpackBuffer, pboId);
                GL.BufferData((OpenTK.Graphics.OpenGL.BufferTarget) BufferTarget.PixelUnpackBuffer, sz >> (int)System.Math.Pow(curLevel, dims), src.GetPixelData(curLevel), BufferUsageHint.StreamDraw);

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
                GPUStateMachine.UnbindBuffer((OpenTK.Graphics.OpenGL.BufferTarget)BufferTarget.PixelUnpackBuffer);
                uploadFence.PlaceFence();

                TargetSampler = new TextureSampler
                {
                    MinLOD = curLevel + 1,
                    MaxLOD = maxLevels
                };
            }

            internal void Setup(ITextureSource src)
            {
                this.src = src;
                IsDone = false;
                maxLevels = curLevel = src.GetLevels() - 1;
                dims = src.GetDimensions();
                owner.pending.Add(this);

                //Copy the data over, uploading the highest mipmap normally and the others asynchronously
                TargetTexture = new Texture();
                TargetTexture.SetData(src, curLevel);
                TargetSampler = new TextureSampler
                {
                    MinLOD = curLevel,
                    MaxLOD = curLevel
                };

                int pixelSize = src.GetBpp();

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


                curLevel--;

                //Setup the pbo for further uploads
                if (curLevel >= 0)
                {
                    Upload();
                }
                else
                {
                    IsDone = true;
                    TargetSampler = new TextureSampler
                    {
                        MinLOD = 0,
                        MaxLOD = maxLevels
                    };
                }
            }

            private void UpdateProgress()
            {
                if (IsDone) return;

                if (uploadFence.Raised(10))
                {
                    //Proceed to the next upload
                    curLevel--;
                    if (curLevel >= 0)
                        Upload();
                    else
                    {
                        //Finished and update buffers appropriately
                        IsDone = true;
                        TargetSampler = new TextureSampler
                        {
                            MinLOD = 0,
                            MaxLOD = maxLevels
                        };
                    }
                }
            }

            public void Free()
            {
                TargetSampler = null;
                TargetTexture = null;
                owner.pending.Remove(this);
                owner.buffers.Enqueue(this);
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
